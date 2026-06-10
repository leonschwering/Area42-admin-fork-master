using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area42_1.ApiService.Data;
using Area42_1.ApiService.Models.Admin;
using System.Security.Claims;

namespace Area42_1.ApiService.Controllers;

/// <summary>
/// Admin user management and operations controller
/// Requires admin rank (SuperAdmin or Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly Area42Context _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(Area42Context context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? GetUserRank() => User.FindFirst("rank")?.Value;

    private static string HashPassword(string password)
    {
        // Simple hash - in production, use proper bcrypt
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    // GET: api/admin/users
    /// <summary>
    /// List all admin users (paginated)
    /// Only accessible by SuperAdmin and Admin ranks
    /// </summary>
    [HttpGet("users")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<AdminUser>>> GetAdminUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var total = await _context.AdminUsers.CountAsync();
            var users = await _context.AdminUsers
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUser
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Rank = u.Rank,
                    IsEnabled = u.IsEnabled,
                    IsLocked = u.IsLocked,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation($"Admin user listing requested by {GetUserId()}");

            return Ok(new PagedResult<AdminUser>
            {
                Data = users,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching admin users: {ex.Message}");
            return BadRequest("Failed to fetch admin users");
        }
    }

    // GET: api/admin/users/{id}
    /// <summary>
    /// Get a specific admin user by ID
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AdminUser>> GetAdminUser(string id)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null)
                return NotFound("Admin user not found");

            _logger.LogInformation($"Admin user {id} retrieved by {GetUserId()}");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching admin user: {ex.Message}");
            return BadRequest("Failed to fetch admin user");
        }
    }

    // POST: api/admin/users
    /// <summary>
    /// Create a new admin user (SuperAdmin only)
    /// </summary>
    [HttpPost("users")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<AdminUser>> CreateAdminUser([FromBody] CreateAdminUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Email and FullName are required");

            var existingUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                return Conflict("Admin user with this email already exists");

            var newUser = new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FullName = request.FullName,
                Rank = request.Rank,
                PasswordHash = request.PasswordHash ?? HashPassword("DefaultPassword"),
                IsEnabled = request.IsEnabled ?? true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdminUsers.Add(newUser);

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = GetUserId() ?? "system",
                UserRank = GetUserRank(),
                Action = "AdminUser.Create",
                EntityType = "AdminUser",
                EntityId = newUser.Id,
                NewValues = System.Text.Json.JsonSerializer.Serialize(newUser),
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"New admin user {newUser.Id} created by {GetUserId()}");

            // Return user without password hash
            return CreatedAtAction(nameof(GetAdminUser), new { id = newUser.Id }, newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating admin user: {ex.Message}");
            return BadRequest($"Failed to create admin user: {ex.Message}");
        }
    }

    // PUT: api/admin/users/{id}
    /// <summary>
    /// Update an admin user (SuperAdmin only, Admin can update self if not changing rank)
    /// </summary>
    [HttpPut("users/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AdminUser>> UpdateAdminUser(string id, [FromBody] UpdateAdminUserRequest request)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null)
                return NotFound("Admin user not found");

            var currentUserId = GetUserId();
            var isSuperAdmin = GetUserRank() == AdminRank.SuperAdmin.ToString();

            // Only SuperAdmin can change rank, or user can update self (non-rank fields)
            if (request.Rank.HasValue && request.Rank != user.Rank && !isSuperAdmin)
                return Forbid("Only SuperAdmin can change user rank");

            var oldValues = System.Text.Json.JsonSerializer.Serialize(user);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (request.Rank.HasValue && isSuperAdmin)
                user.Rank = request.Rank;

            if (request.IsEnabled.HasValue)
                user.IsEnabled = request.IsEnabled.Value;

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = currentUserId ?? "system",
                UserRank = GetUserRank(),
                Action = "AdminUser.Update",
                EntityType = "AdminUser",
                EntityId = id,
                OldValues = oldValues,
                NewValues = System.Text.Json.JsonSerializer.Serialize(user),
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Admin user {id} updated by {GetUserId()}");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating admin user: {ex.Message}");
            return BadRequest($"Failed to update admin user: {ex.Message}");
        }
    }

    // DELETE: api/admin/users/{id}
    /// <summary>
    /// Soft-delete an admin user (SuperAdmin only)
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult> DeleteAdminUser(string id)
    {
        try
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user == null)
                return NotFound("Admin user not found");

            user.IsEnabled = false;
            user.DeactivatedAt = DateTime.UtcNow;

            var auditLog = new AuditLogEntry
            {
                UserId = GetUserId() ?? "system",
                UserRank = GetUserRank(),
                Action = "AdminUser.Delete",
                EntityType = "AdminUser",
                EntityId = id,
                NewValues = $"IsEnabled: false, DeactivatedAt: {DateTime.UtcNow}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Admin user {id} deactivated by {GetUserId()}");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting admin user: {ex.Message}");
            return BadRequest($"Failed to delete admin user: {ex.Message}");
        }
    }

    // GET: api/admin/audit-logs
    /// <summary>
    /// Get audit logs (Admin rank and above)
    /// </summary>
    [HttpGet("audit-logs")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<AuditLogEntry>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<AuditLogEntry>
            {
                Data = logs,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching audit logs: {ex.Message}");
            return BadRequest("Failed to fetch audit logs");
        }
    }

    // GET: api/admin/security-flags
    /// <summary>
    /// Get security flags (Admin rank and above)
    /// </summary>
    [HttpGet("security-flags")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<PagedResult<SecurityFlag>>> GetSecurityFlags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.SecurityFlags.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FlagStatus>(status, out var flagStatus))
                query = query.Where(f => f.Status == flagStatus);

            var total = await query.CountAsync();
            var flags = await query
                .OrderByDescending(f => f.TriggeredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<SecurityFlag>
            {
                Data = flags,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching security flags: {ex.Message}");
            return BadRequest("Failed to fetch security flags");
        }
    }

    // PUT: api/admin/security-flags/{id}
    /// <summary>
    /// Update security flag status (dismiss/escalate)
    /// </summary>
    [HttpPut("security-flags/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SecurityFlag>> UpdateSecurityFlag(Guid id, [FromBody] UpdateSecurityFlagRequest request)
    {
        try
        {
            var flag = await _context.SecurityFlags.FindAsync(id);
            if (flag == null)
                return NotFound("Security flag not found");

            flag.Status = request.Status;
            flag.ReviewedByAdminId = GetUserId();
            flag.ReviewNote = request.ReviewNote;
            flag.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Security flag {id} updated by {GetUserId()}");
            return Ok(flag);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating security flag: {ex.Message}");
            return BadRequest($"Failed to update security flag: {ex.Message}");
        }
    }
}

// DTOs
public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AdminRank? Rank { get; set; }
    public string? PasswordHash { get; set; }
    public bool? IsEnabled { get; set; }
}

public class UpdateAdminUserRequest
{
    public string? FullName { get; set; }
    public AdminRank? Rank { get; set; }
    public bool? IsEnabled { get; set; }
}

public class UpdateSecurityFlagRequest
{
    public FlagStatus Status { get; set; }
    public string? ReviewNote { get; set; }
}

