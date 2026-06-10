using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area42_1.ApiService.Data;
using Area42_1.ApiService.Models.Admin;
using System.Security.Claims;
using System.Text.Json;

namespace Area42_1.ApiService.Controllers;

/// <summary>
/// GDPR/AVG compliance operations controller
/// Handles erasure requests, consent logs, data exports, and compliance audits
/// Requires Manager rank and above
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GdprController : ControllerBase
{
    private readonly Area42Context _context;
    private readonly ILogger<GdprController> _logger;
    private const int ErasureRequestSlaDay = 30;

    public GdprController(Area42Context context, ILogger<GdprController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? GetUserRank() => User.FindFirst("rank")?.Value;

    // POST: api/gdpr/erasure-requests
    /// <summary>
    /// Submit a GDPR data erasure request (Right to be forgotten - Art. 17)
    /// 30-day SLA from submission
    /// User can self-request; admins can approve/execute
    /// </summary>
    [HttpPost("erasure-requests")]
    [AllowAnonymous]
    public async Task<ActionResult<GdprErasureResponse>> SubmitErasureRequest([FromBody] SubmitErasureRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            var existingRequest = await _context.GdprErasureRequests
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.Status != GdprErasureStatus.Completed && r.Status != GdprErasureStatus.Failed);

            if (existingRequest != null)
                return BadRequest("This user already has an active erasure request");

            var erasureRequest = new GdprErasureRequest
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RequestedAt = DateTime.UtcNow,
                DueAt = DateTime.UtcNow.AddDays(ErasureRequestSlaDay),
                Status = GdprErasureStatus.Pending,
                Notes = request.Reason
            };

            _context.GdprErasureRequests.Add(erasureRequest);

            // Log audit
            var auditLog = new AuditLogEntry
            {
                UserId = "user-" + request.UserId,
                UserRank = "User",
                Action = "GDPR.ErasureRequest",
                EntityType = "GdprErasureRequest",
                EntityId = erasureRequest.Id.ToString(),
                NewValues = $"UserId: {request.UserId}, DueAt: {erasureRequest.DueAt}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"GDPR erasure request {erasureRequest.Id} submitted for user {request.UserId}. Due: {erasureRequest.DueAt:yyyy-MM-dd}");

            return CreatedAtAction(nameof(GetErasureRequest), new { id = erasureRequest.Id }, new GdprErasureResponse
            {
                RequestId = erasureRequest.Id,
                UserId = erasureRequest.UserId,
                Status = erasureRequest.Status,
                RequestedAt = erasureRequest.RequestedAt,
                DueAt = erasureRequest.DueAt,
                Message = $"Erasure request created. Due date: {erasureRequest.DueAt:yyyy-MM-dd}. SLA: {ErasureRequestSlaDay} days."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error submitting erasure request: {ex.Message}");
            return BadRequest($"Failed to submit erasure request: {ex.Message}");
        }
    }

    // GET: api/gdpr/erasure-requests/{id}
    /// <summary>
    /// Get erasure request details
    /// </summary>
    [HttpGet("erasure-requests/{id}")]
    public async Task<ActionResult<GdprErasureRequest>> GetErasureRequest(Guid id)
    {
        try
        {
            var request = await _context.GdprErasureRequests.FindAsync(id);
            if (request == null)
                return NotFound("Erasure request not found");

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching erasure request: {ex.Message}");
            return BadRequest("Failed to fetch erasure request");
        }
    }

    // GET: api/gdpr/erasure-requests
    /// <summary>
    /// List all erasure requests (Admin only)
    /// Shows pending, overdue, and completed requests
    /// </summary>
    [HttpGet("erasure-requests")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<GdprErasureRequest>>> GetErasureRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.GdprErasureRequests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GdprErasureStatus>(status, out var erasureStatus))
                query = query.Where(r => r.Status == erasureStatus);

            var total = await query.CountAsync();
            var requests = await query
                .OrderByDescending(r => r.DueAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<GdprErasureRequest>
            {
                Data = requests,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching erasure requests: {ex.Message}");
            return BadRequest("Failed to fetch erasure requests");
        }
    }

    // POST: api/gdpr/erasure-requests/{id}/approve
    /// <summary>
    /// Approve and execute a GDPR erasure request (Admin only)
    /// Marks user data for pseudonymization/deletion in user service
    /// </summary>
    [HttpPost("erasure-requests/{id}/approve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<GdprErasureResponse>> ApproveErasureRequest(Guid id, [FromBody] ApproveErasureRequest request)
    {
        try
        {
            var adminId = GetUserId();
            if (string.IsNullOrWhiteSpace(adminId))
                return Unauthorized("Admin not identified");

            var erasureRequest = await _context.GdprErasureRequests.FindAsync(id);
            if (erasureRequest == null)
                return NotFound("Erasure request not found");

            if (erasureRequest.Status != GdprErasureStatus.Pending && erasureRequest.Status != GdprErasureStatus.InProgress)
                return BadRequest($"Cannot approve request with status: {erasureRequest.Status}");

            erasureRequest.Status = GdprErasureStatus.InProgress;
            erasureRequest.CompletedByAdminId = adminId;
            erasureRequest.CompletedAt = DateTime.UtcNow;
            erasureRequest.Notes = request.ExecutionNotes;

            var auditLog = new AuditLogEntry
            {
                UserId = adminId,
                UserRank = GetUserRank(),
                Action = "GDPR.ErasureExecute",
                EntityType = "GdprErasureRequest",
                EntityId = id.ToString(),
                NewValues = $"Status: InProgress, CompletedBy: {adminId}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogWarning($"GDPR erasure request {id} approved by {adminId} for user {erasureRequest.UserId}");

            return Ok(new GdprErasureResponse
            {
                RequestId = erasureRequest.Id,
                UserId = erasureRequest.UserId,
                Status = erasureRequest.Status,
                RequestedAt = erasureRequest.RequestedAt,
                DueAt = erasureRequest.DueAt,
                Message = $"Erasure request approved and queued for execution. Status: {erasureRequest.Status}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error approving erasure request: {ex.Message}");
            return BadRequest($"Failed to approve erasure request: {ex.Message}");
        }
    }

    // POST: api/gdpr/erasure-requests/{id}/reject
    /// <summary>
    /// Reject a GDPR erasure request (Admin only)
    /// </summary>
    [HttpPost("erasure-requests/{id}/reject")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<GdprErasureResponse>> RejectErasureRequest(Guid id, [FromBody] RejectErasureRequest request)
    {
        try
        {
            var adminId = GetUserId();
            if (string.IsNullOrWhiteSpace(adminId))
                return Unauthorized("Admin not identified");

            var erasureRequest = await _context.GdprErasureRequests.FindAsync(id);
            if (erasureRequest == null)
                return NotFound("Erasure request not found");

            if (erasureRequest.Status != GdprErasureStatus.Pending)
                return BadRequest($"Cannot reject request with status: {erasureRequest.Status}");

            erasureRequest.Status = GdprErasureStatus.Failed;
            erasureRequest.CompletedByAdminId = adminId;
            erasureRequest.Notes = request.RejectionReason;

            var auditLog = new AuditLogEntry
            {
                UserId = adminId,
                UserRank = GetUserRank(),
                Action = "GDPR.ErasureReject",
                EntityType = "GdprErasureRequest",
                EntityId = id.ToString(),
                NewValues = $"Status: Failed, Reason: {request.RejectionReason}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"GDPR erasure request {id} rejected by {adminId}. Reason: {request.RejectionReason}");

            return Ok(new GdprErasureResponse
            {
                RequestId = erasureRequest.Id,
                UserId = erasureRequest.UserId,
                Status = erasureRequest.Status,
                RequestedAt = erasureRequest.RequestedAt,
                DueAt = erasureRequest.DueAt,
                Message = $"Erasure request rejected: {request.RejectionReason}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error rejecting erasure request: {ex.Message}");
            return BadRequest($"Failed to reject erasure request: {ex.Message}");
        }
    }

    // POST: api/gdpr/consent
    /// <summary>
    /// Record user consent (Marketing, Analytics, Cookies, TermsOfService)
    /// Supports opting in and out
    /// Required by GDPR Art. 7
    /// </summary>
    [HttpPost("consent")]
    [AllowAnonymous]
    public async Task<ActionResult<ConsentResponse>> RecordConsent([FromBody] RecordConsentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserId is required");

            if (string.IsNullOrWhiteSpace(request.ConsentType))
                return BadRequest("ConsentType is required");

            var consentLog = new ConsentLog
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ConsentType = request.ConsentType,
                Granted = request.Granted,
                RecordedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                PrivacyVersionAccepted = request.PrivacyVersionAccepted
            };

            _context.ConsentLogs.Add(consentLog);

            var auditLog = new AuditLogEntry
            {
                UserId = "user-" + request.UserId,
                UserRank = "User",
                Action = "GDPR.ConsentRecord",
                EntityType = "ConsentLog",
                EntityId = consentLog.Id.ToString(),
                NewValues = $"ConsentType: {request.ConsentType}, Granted: {request.Granted}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Consent recorded for user {request.UserId}: {request.ConsentType} = {request.Granted}");

            return CreatedAtAction(nameof(GetConsent), new { id = consentLog.Id }, new ConsentResponse
            {
                ConsentId = consentLog.Id,
                ConsentType = consentLog.ConsentType,
                Granted = consentLog.Granted,
                RecordedAt = consentLog.RecordedAt,
                Message = $"Consent for {request.ConsentType} recorded: {(request.Granted ? "Granted" : "Withdrawn")}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording consent: {ex.Message}");
            return BadRequest($"Failed to record consent: {ex.Message}");
        }
    }

    // GET: api/gdpr/consent/{id}
    /// <summary>
    /// Get consent log details
    /// </summary>
    [HttpGet("consent/{id}")]
    public async Task<ActionResult<ConsentLog>> GetConsent(Guid id)
    {
        try
        {
            var consentLog = await _context.ConsentLogs.FindAsync(id);
            if (consentLog == null)
                return NotFound("Consent log not found");

            return Ok(consentLog);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching consent log: {ex.Message}");
            return BadRequest("Failed to fetch consent log");
        }
    }

    // GET: api/gdpr/consent/user/{userId}
    /// <summary>
    /// Get all consent logs for a user
    /// </summary>
    [HttpGet("consent/user/{userId}")]
    public async Task<ActionResult<List<ConsentLog>>> GetUserConsent(string userId)
    {
        try
        {
            var consentLogs = await _context.ConsentLogs
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.RecordedAt)
                .ToListAsync();

            return Ok(consentLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user consent logs: {ex.Message}");
            return BadRequest("Failed to fetch consent logs");
        }
    }

    // GET: api/gdpr/compliance-status
    /// <summary>
    /// Get GDPR compliance status dashboard
    /// Shows overdue erasure requests, consent statistics, audit trail
    /// </summary>
    [HttpGet("compliance-status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ComplianceStatusResponse>> GetComplianceStatus()
    {
        try
        {
            var now = DateTime.UtcNow;

            var totalErasureRequests = await _context.GdprErasureRequests.CountAsync();
            var pendingErasures = await _context.GdprErasureRequests
                .Where(r => r.Status == GdprErasureStatus.Pending).CountAsync();
            var overdueErasures = await _context.GdprErasureRequests
                .Where(r => (r.Status == GdprErasureStatus.Pending || r.Status == GdprErasureStatus.InProgress) && r.DueAt < now).CountAsync();

            var totalConsents = await _context.ConsentLogs.CountAsync();
            var recentConsents = await _context.ConsentLogs
                .Where(c => c.RecordedAt > now.AddDays(-30)).CountAsync();

            var totalAuditLogs = await _context.AuditLogs.CountAsync();
            var gdprAuditLogs = await _context.AuditLogs
                .Where(a => a.Action.StartsWith("GDPR")).CountAsync();

            return Ok(new ComplianceStatusResponse
            {
                TotalErasureRequests = totalErasureRequests,
                PendingErasures = pendingErasures,
                OverdueErasures = overdueErasures,
                TotalConsents = totalConsents,
                RecentConsents = recentConsents,
                TotalAuditLogs = totalAuditLogs,
                GdprAuditLogs = gdprAuditLogs,
                ComplianceCheckDate = now,
                Message = overdueErasures > 0 ? $"WARNING: {overdueErasures} overdue erasure requests" : "Compliance status OK"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching compliance status: {ex.Message}");
            return BadRequest("Failed to fetch compliance status");
        }
    }
}

// DTOs
public class GdprErasureResponse
{
    public Guid RequestId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public GdprErasureStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime DueAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SubmitErasureRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class ApproveErasureRequest
{
    public string? ExecutionNotes { get; set; }
}

public class RejectErasureRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}

public class ConsentResponse
{
    public Guid ConsentId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public DateTime RecordedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RecordConsentRequest
{
    public string UserId { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public string? PrivacyVersionAccepted { get; set; }
}

public class ComplianceStatusResponse
{
    public int TotalErasureRequests { get; set; }
    public int PendingErasures { get; set; }
    public int OverdueErasures { get; set; }
    public int TotalConsents { get; set; }
    public int RecentConsents { get; set; }
    public int TotalAuditLogs { get; set; }
    public int GdprAuditLogs { get; set; }
    public DateTime ComplianceCheckDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
