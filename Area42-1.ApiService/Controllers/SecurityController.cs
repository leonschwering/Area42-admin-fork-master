using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area42_1.ApiService.Data;
using Area42_1.ApiService.Models.Admin;
using System.Security.Claims;

namespace Area42_1.ApiService.Controllers;

/// <summary>
/// Security operations controller - Kill switch protocol
/// Requires SuperAdmin or Admin rank
/// Two-person quorum required for execution
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly Area42Context _context;
    private readonly ILogger<SecurityController> _logger;
    private const int KillSwitchExpiryMinutes = 10;

    public SecurityController(Area42Context context, ILogger<SecurityController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? GetUserRank() => User.FindFirst("rank")?.Value;

    // POST: api/security/kill-switch/initiate
    /// <summary>
    /// Initiate a kill switch request (KS-1: Account Lock, KS-2: Admin Domain, KS-3: Full System)
    /// Returns pending request ID; requires confirmation by different admin within 10 minutes
    /// </summary>
    [HttpPost("kill-switch/initiate")]
    [Authorize(Policy = "SecurityAdminOnly")]
    public async Task<ActionResult<KillSwitchResponse>> InitiateKillSwitch([FromBody] InitiateKillSwitchRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User not identified");

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("Kill switch reason is required");

            // Check for existing pending requests from this user in last hour
            var recentRequests = await _context.KillSwitchRequests
                .Where(r => r.InitiatorUserId == userId 
                    && r.Status == KillSwitchStatus.Pending
                    && r.CreatedAt > DateTime.UtcNow.AddHours(-1))
                .CountAsync();

            if (recentRequests >= 3)
                return BadRequest("Rate limit exceeded: Maximum 3 pending kill switch requests per hour");

            // Create kill switch request
            var killSwitch = new KillSwitchRequest
            {
                Id = Guid.NewGuid(),
                TargetUserId = request.TargetUserId,
                InitiatorUserId = userId,
                Level = request.Level,
                Reason = request.Reason,
                Status = KillSwitchStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(KillSwitchExpiryMinutes)
            };

            _context.KillSwitchRequests.Add(killSwitch);

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = userId,
                UserRank = GetUserRank(),
                Action = "KillSwitch.Initiate",
                EntityType = "KillSwitchRequest",
                EntityId = killSwitch.Id.ToString(),
                NewValues = $"Level: {killSwitch.Level}, Reason: {request.Reason}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogWarning($"Kill switch {killSwitch.Level} initiated by {userId}: {request.Reason}");

            return CreatedAtAction(nameof(GetKillSwitchRequest), new { id = killSwitch.Id }, new KillSwitchResponse
            {
                Id = killSwitch.Id,
                Level = killSwitch.Level,
                Status = killSwitch.Status,
                ExpiresAt = killSwitch.ExpiresAt,
                Message = $"Kill switch {killSwitch.Level} initiated. Awaiting confirmation from another admin. Expires in {KillSwitchExpiryMinutes} minutes."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error initiating kill switch: {ex.Message}");
            return BadRequest($"Failed to initiate kill switch: {ex.Message}");
        }
    }

    // GET: api/security/kill-switch/{id}
    /// <summary>
    /// Get kill switch request details
    /// </summary>
    [HttpGet("kill-switch/{id}")]
    [Authorize(Policy = "SecurityAdminOnly")]
    public async Task<ActionResult<KillSwitchRequest>> GetKillSwitchRequest(Guid id)
    {
        try
        {
            var request = await _context.KillSwitchRequests.FindAsync(id);
            if (request == null)
                return NotFound("Kill switch request not found");

            // Check if expired
            if (request.Status == KillSwitchStatus.Pending && DateTime.UtcNow > request.ExpiresAt)
            {
                request.Status = KillSwitchStatus.Expired;
                await _context.SaveChangesAsync();
            }

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching kill switch request: {ex.Message}");
            return BadRequest("Failed to fetch kill switch request");
        }
    }

    // GET: api/security/kill-switch/pending
    /// <summary>
    /// Get all pending kill switch requests
    /// </summary>
    [HttpGet("kill-switch/pending")]
    [Authorize(Policy = "SecurityAdminOnly")]
    public async Task<ActionResult<List<KillSwitchRequest>>> GetPendingKillSwitches()
    {
        try
        {
            var now = DateTime.UtcNow;
            var pending = await _context.KillSwitchRequests
                .Where(r => r.Status == KillSwitchStatus.Pending && r.ExpiresAt > now)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(pending);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching pending kill switches: {ex.Message}");
            return BadRequest("Failed to fetch pending kill switches");
        }
    }

    // POST: api/security/kill-switch/{id}/confirm
    /// <summary>
    /// Confirm a kill switch request (two-person quorum)
    /// Confirmer MUST be different from initiator
    /// Confirmer MUST be SuperAdmin or Admin rank
    /// </summary>
    [HttpPost("kill-switch/{id}/confirm")]
    [Authorize(Policy = "SecurityAdminOnly")]
    public async Task<ActionResult<KillSwitchResponse>> ConfirmKillSwitch(Guid id)
    {
        try
        {
            var confirmerId = GetUserId();
            if (string.IsNullOrWhiteSpace(confirmerId))
                return Unauthorized("User not identified");

            var killSwitch = await _context.KillSwitchRequests.FindAsync(id);
            if (killSwitch == null)
                return NotFound("Kill switch request not found");

            // Verify status
            if (killSwitch.Status != KillSwitchStatus.Pending)
                return BadRequest($"Kill switch is {killSwitch.Status}. Only pending requests can be confirmed.");

            // Check expiry
            if (DateTime.UtcNow > killSwitch.ExpiresAt)
            {
                killSwitch.Status = KillSwitchStatus.Expired;
                await _context.SaveChangesAsync();
                return BadRequest("Kill switch request has expired");
            }

            // Verify two-person quorum
            if (killSwitch.InitiatorUserId == confirmerId)
                return BadRequest("Kill switch must be confirmed by a different admin (two-person quorum)");

            // Verify confirmer rank
            var confirmerRank = GetUserRank();
            if (confirmerRank != AdminRank.SuperAdmin.ToString() && confirmerRank != AdminRank.Admin.ToString())
                return Forbid("Only SuperAdmin or Admin can confirm kill switches");

            // Confirm
            killSwitch.ConfirmerUserId = confirmerId;
            killSwitch.ConfirmedAt = DateTime.UtcNow;
            killSwitch.Status = KillSwitchStatus.Confirmed;

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = confirmerId,
                UserRank = confirmerRank,
                Action = "KillSwitch.Confirm",
                EntityType = "KillSwitchRequest",
                EntityId = id.ToString(),
                NewValues = $"Status: Confirmed, ConfirmedAt: {DateTime.UtcNow}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogWarning($"Kill switch {killSwitch.Level} confirmed by {confirmerId}. Ready for execution.");

            return Ok(new KillSwitchResponse
            {
                Id = killSwitch.Id,
                Level = killSwitch.Level,
                Status = killSwitch.Status,
                ExpiresAt = killSwitch.ExpiresAt,
                Message = $"Kill switch {killSwitch.Level} confirmed by {confirmerId}. Ready for execution."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error confirming kill switch: {ex.Message}");
            return BadRequest($"Failed to confirm kill switch: {ex.Message}");
        }
    }

    // POST: api/security/kill-switch/{id}/execute
    /// <summary>
    /// Execute a confirmed kill switch request
    /// Only SuperAdmin can execute (requires 2FA in production)
    /// </summary>
    [HttpPost("kill-switch/{id}/execute")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<KillSwitchResponse>> ExecuteKillSwitch(Guid id)
    {
        try
        {
            var executorId = GetUserId();
            if (string.IsNullOrWhiteSpace(executorId))
                return Unauthorized("User not identified");

            var killSwitch = await _context.KillSwitchRequests.FindAsync(id);
            if (killSwitch == null)
                return NotFound("Kill switch request not found");

            // Verify status
            if (killSwitch.Status != KillSwitchStatus.Confirmed)
                return BadRequest("Only confirmed kill switch requests can be executed");

            // Execute based on level
            switch (killSwitch.Level)
            {
                case KillSwitchLevel.AccountLock:
                    // Lock target user account
                    if (!string.IsNullOrWhiteSpace(killSwitch.TargetUserId))
                    {
                        var targetUser = await _context.AdminUsers.FindAsync(killSwitch.TargetUserId);
                        if (targetUser != null)
                        {
                            targetUser.IsLocked = true;
                            targetUser.LockoutUntil = DateTime.UtcNow.AddHours(24);
                            killSwitch.ExecutionLog = $"User {killSwitch.TargetUserId} locked until {targetUser.LockoutUntil}";
                        }
                    }
                    break;

                case KillSwitchLevel.AdminDomainLock:
                    // Lock all admin accounts except executor and initiator
                    var adminsToLock = await _context.AdminUsers
                        .Where(u => u.Id != executorId && u.Id != killSwitch.InitiatorUserId && u.IsEnabled)
                        .ToListAsync();
                    foreach (var admin in adminsToLock)
                    {
                        admin.IsLocked = true;
                        admin.LockoutUntil = DateTime.UtcNow.AddHours(24);
                    }
                    killSwitch.ExecutionLog = $"Locked {adminsToLock.Count} admin accounts";
                    break;

                case KillSwitchLevel.FullSystemLock:
                    // Mark all sessions as expired (in production: revoke all tokens)
                    var allAdmins = await _context.AdminUsers.Where(u => u.IsEnabled).ToListAsync();
                    foreach (var admin in allAdmins)
                    {
                        admin.SessionExpiresAt = DateTime.UtcNow;
                    }
                    killSwitch.ExecutionLog = "Full system lock initiated. All sessions expired.";
                    break;
            }

            killSwitch.Status = KillSwitchStatus.Executed;
            killSwitch.ExecutedAt = DateTime.UtcNow;

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = executorId,
                UserRank = GetUserRank(),
                Action = "KillSwitch.Execute",
                EntityType = "KillSwitchRequest",
                EntityId = id.ToString(),
                NewValues = $"Status: Executed, ExecutedAt: {DateTime.UtcNow}, Level: {killSwitch.Level}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogError($"KILL SWITCH EXECUTED: {killSwitch.Level} by {executorId}. Message: {killSwitch.ExecutionLog}");

            return Ok(new KillSwitchResponse
            {
                Id = killSwitch.Id,
                Level = killSwitch.Level,
                Status = killSwitch.Status,
                ExpiresAt = killSwitch.ExpiresAt,
                Message = $"Kill switch {killSwitch.Level} executed. {killSwitch.ExecutionLog}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing kill switch: {ex.Message}");
            return BadRequest($"Failed to execute kill switch: {ex.Message}");
        }
    }

    // POST: api/security/kill-switch/{id}/reject
    /// <summary>
    /// Reject a pending kill switch request
    /// </summary>
    [HttpPost("kill-switch/{id}/reject")]
    [Authorize(Policy = "SecurityAdminOnly")]
    public async Task<ActionResult<KillSwitchResponse>> RejectKillSwitch(Guid id, [FromBody] RejectKillSwitchRequest request)
    {
        try
        {
            var killSwitch = await _context.KillSwitchRequests.FindAsync(id);
            if (killSwitch == null)
                return NotFound("Kill switch request not found");

            if (killSwitch.Status != KillSwitchStatus.Pending)
                return BadRequest("Only pending requests can be rejected");

            killSwitch.Status = KillSwitchStatus.Rejected;
            killSwitch.ExecutionLog = request.Reason;

            var auditLog = new AuditLogEntry
            {
                UserId = GetUserId() ?? "system",
                UserRank = GetUserRank(),
                Action = "KillSwitch.Reject",
                EntityType = "KillSwitchRequest",
                EntityId = id.ToString(),
                NewValues = $"Status: Rejected, Reason: {request.Reason}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Kill switch request {id} rejected by {GetUserId()}");

            return Ok(new KillSwitchResponse
            {
                Id = killSwitch.Id,
                Level = killSwitch.Level,
                Status = killSwitch.Status,
                ExpiresAt = killSwitch.ExpiresAt,
                Message = $"Kill switch request rejected: {request.Reason}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error rejecting kill switch: {ex.Message}");
            return BadRequest($"Failed to reject kill switch: {ex.Message}");
        }
    }
}

// DTOs
public class KillSwitchResponse
{
    public Guid Id { get; set; }
    public KillSwitchLevel Level { get; set; }
    public KillSwitchStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class InitiateKillSwitchRequest
{
    public string? TargetUserId { get; set; }
    public KillSwitchLevel Level { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectKillSwitchRequest
{
    public string Reason { get; set; } = string.Empty;
}
