using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Area42_1.ApiService.Data;
using Area42_1.ApiService.Models.Admin;
using System.Security.Claims;

namespace Area42_1.ApiService.Controllers;

/// <summary>
/// Financial operations controller - Transaction approvals and audit
/// Tiered approval system: €150 (Support), €500 (Booking), €2000 (Manager/Admin)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly Area42Context _context;
    private readonly ILogger<FinancialController> _logger;

    public FinancialController(Area42Context context, ILogger<FinancialController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string? GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string? GetUserRank() => User.FindFirst("rank")?.Value;

    // GET: api/financial/approval-thresholds
    /// <summary>
    /// Get financial approval thresholds by rank
    /// Public endpoint for UI reference
    /// </summary>
    [HttpGet("approval-thresholds")]
    [AllowAnonymous]
    public ActionResult<ApprovalThresholdsResponse> GetApprovalThresholds()
    {
        return Ok(new ApprovalThresholdsResponse
        {
            Thresholds = new[]
            {
                new ThresholdInfo { Role = "CustomerSupport", AutonomousLimit = 150, RequiresApprovalAbove = 150 },
                new ThresholdInfo { Role = "BookingManager", AutonomousLimit = 500, RequiresApprovalAbove = 500 },
                new ThresholdInfo { Role = "PropertyManager", AutonomousLimit = 0, RequiresApprovalAbove = 0 },
                new ThresholdInfo { Role = "SeniorManager", AutonomousLimit = 2000, RequiresApprovalAbove = 2000 },
                new ThresholdInfo { Role = "Admin", AutonomousLimit = 2000, RequiresApprovalAbove = 2000 },
                new ThresholdInfo { Role = "SuperAdmin", AutonomousLimit = 2000, RequiresApprovalAbove = 2000 }
            }
        });
    }

    // POST: api/financial/transactions
    /// <summary>
    /// Submit a financial transaction (refund, adjustment, etc.)
    /// Returns 202 if awaiting approval or 200 if auto-approved
    /// </summary>
    [HttpPost("transactions")]
    [Authorize(Policy = "FinancialAccessOnly")]
    public async Task<ActionResult<FinancialTransactionResponse>> SubmitTransaction([FromBody] SubmitTransactionRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User not identified");

            if (request.Amount <= 0)
                return BadRequest("Transaction amount must be positive");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest("Transaction reason is required");

            // Get user rank
            var admin = await _context.AdminUsers.FindAsync(userId);
            if (admin?.Rank == null)
                return BadRequest("User rank not found");

            // Determine if transaction needs approval
            var (isAutoApproved, threshold) = GetApprovalThreshold(admin.Rank.Value);
            bool needsApproval = request.Amount > threshold;

            var transaction = new FinancialAuditLog
            {
                Id = Guid.NewGuid(),
                InitiatorUserId = userId,
                OperationType = request.OperationType,
                EntityId = request.EntityId,
                AmountBefore = request.AmountBefore,
                AmountAfter = request.AmountAfter,
                Reason = request.Reason,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                MfaConfirmed = request.MfaConfirmed ?? false
            };

            // Check if MFA required for high amounts (>€2000)
            if (request.Amount > 2000 && !request.MfaConfirmed.HasValue)
                return BadRequest("MFA confirmation required for transactions over €2,000");

            _context.FinancialAuditLogs.Add(transaction);

            // Create audit log
            var auditLog = new AuditLogEntry
            {
                UserId = userId,
                UserRank = admin.Rank.ToString(),
                Action = needsApproval ? "FinancialTransaction.SubmitForApproval" : "FinancialTransaction.AutoApprove",
                EntityType = "FinancialAuditLog",
                EntityId = transaction.Id.ToString(),
                NewValues = $"Amount: {request.Amount}, Type: {request.OperationType}, RequiresApproval: {needsApproval}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            var responseStatus = needsApproval ? StatusCodes.Status202Accepted : StatusCodes.Status200OK;

            _logger.LogInformation($"Financial transaction {transaction.Id} submitted by {userId}. Amount: €{request.Amount}, RequiresApproval: {needsApproval}");

            return StatusCode(responseStatus, new FinancialTransactionResponse
            {
                TransactionId = transaction.Id,
                Amount = request.Amount,
                Status = needsApproval ? "PendingApproval" : "Approved",
                Message = needsApproval
                    ? $"Transaction requires approval from {(request.Amount > 2000 ? "two senior admins" : "an approver")}"
                    : "Transaction auto-approved",
                RequiresMfa = request.Amount > 2000
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error submitting financial transaction: {ex.Message}");
            return BadRequest($"Failed to submit transaction: {ex.Message}");
        }
    }

    // GET: api/financial/transactions
    /// <summary>
    /// Get financial audit log (SeniorManager and above only)
    /// </summary>
    [HttpGet("transactions")]
    [Authorize(Policy = "FinancialAuditOnly")]
    public async Task<ActionResult<PagedResult<FinancialAuditLog>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? operationType = null,
        [FromQuery] bool? requiresApproval = null)
    {
        try
        {
            var query = _context.FinancialAuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(operationType))
                query = query.Where(f => f.OperationType == operationType);

            if (requiresApproval.HasValue)
            {
                if (requiresApproval.Value)
                    query = query.Where(f => f.ApproverUserId == null);
                else
                    query = query.Where(f => f.ApproverUserId != null);
            }

            var total = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(f => f.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<FinancialAuditLog>
            {
                Data = transactions,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching transactions: {ex.Message}");
            return BadRequest("Failed to fetch transactions");
        }
    }

    // GET: api/financial/transactions/{id}
    /// <summary>
    /// Get a specific financial transaction
    /// </summary>
    [HttpGet("transactions/{id}")]
    [Authorize(Policy = "FinancialAuditOnly")]
    public async Task<ActionResult<FinancialAuditLog>> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await _context.FinancialAuditLogs.FindAsync(id);
            if (transaction == null)
                return NotFound("Transaction not found");

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching transaction: {ex.Message}");
            return BadRequest("Failed to fetch transaction");
        }
    }

    // POST: api/financial/transactions/{id}/approve
    /// <summary>
    /// Approve a pending financial transaction (Manager and above only)
    /// For amounts >€2000, requires SuperAdmin
    /// </summary>
    [HttpPost("transactions/{id}/approve")]
    [Authorize(Policy = "FinancialApprovalOnly")]
    public async Task<ActionResult<FinancialTransactionResponse>> ApproveTransaction(Guid id, [FromBody] ApproveTransactionRequest request)
    {
        try
        {
            var approverId = GetUserId();
            if (string.IsNullOrWhiteSpace(approverId))
                return Unauthorized("User not identified");

            var transaction = await _context.FinancialAuditLogs.FindAsync(id);
            if (transaction == null)
                return NotFound("Transaction not found");

            if (transaction.ApproverUserId != null)
                return BadRequest("Transaction already approved");

            var amount = Math.Abs(transaction.AmountAfter - transaction.AmountBefore);

            // Verify approver rank for high amounts
            if (amount > 2000)
            {
                var approverRank = GetUserRank();
                if (approverRank != AdminRank.SuperAdmin.ToString())
                    return Forbid("Only SuperAdmin can approve transactions over €2,000");

                // Check MFA requirement
                if (!request.MfaConfirmed)
                    return BadRequest("MFA confirmation required for approvals over €2,000");
            }

            // Prevent self-approval for high amounts
            if (amount > 2000 && transaction.InitiatorUserId == approverId)
                return BadRequest("Cannot self-approve transactions over €2,000");

            transaction.ApproverUserId = approverId;
            transaction.MfaConfirmed = request.MfaConfirmed;

            var auditLog = new AuditLogEntry
            {
                UserId = approverId,
                UserRank = GetUserRank(),
                Action = "FinancialTransaction.Approve",
                EntityType = "FinancialAuditLog",
                EntityId = id.ToString(),
                NewValues = $"ApproverUserId: {approverId}, MfaConfirmed: {request.MfaConfirmed}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Financial transaction {id} approved by {approverId}. Amount: €{amount}");

            return Ok(new FinancialTransactionResponse
            {
                TransactionId = id,
                Amount = amount,
                Status = "Approved",
                Message = $"Transaction approved by {approverId}",
                RequiresMfa = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error approving transaction: {ex.Message}");
            return BadRequest($"Failed to approve transaction: {ex.Message}");
        }
    }

    // POST: api/financial/transactions/{id}/reject
    /// <summary>
    /// Reject a pending financial transaction
    /// </summary>
    [HttpPost("transactions/{id}/reject")]
    [Authorize(Policy = "FinancialApprovalOnly")]
    public async Task<ActionResult<FinancialTransactionResponse>> RejectTransaction(Guid id, [FromBody] RejectTransactionRequest request)
    {
        try
        {
            var rejectorId = GetUserId();
            if (string.IsNullOrWhiteSpace(rejectorId))
                return Unauthorized("User not identified");

            var transaction = await _context.FinancialAuditLogs.FindAsync(id);
            if (transaction == null)
                return NotFound("Transaction not found");

            if (transaction.ApproverUserId != null)
                return BadRequest("Transaction already processed");

            var auditLog = new AuditLogEntry
            {
                UserId = rejectorId,
                UserRank = GetUserRank(),
                Action = "FinancialTransaction.Reject",
                EntityType = "FinancialAuditLog",
                EntityId = id.ToString(),
                NewValues = $"Reason: {request.Reason}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Financial transaction {id} rejected by {rejectorId}. Reason: {request.Reason}");

            return Ok(new FinancialTransactionResponse
            {
                TransactionId = id,
                Amount = Math.Abs(transaction.AmountAfter - transaction.AmountBefore),
                Status = "Rejected",
                Message = $"Transaction rejected: {request.Reason}",
                RequiresMfa = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error rejecting transaction: {ex.Message}");
            return BadRequest($"Failed to reject transaction: {ex.Message}");
        }
    }

    // Helper method to get approval threshold by rank
    private (bool autoApproved, decimal threshold) GetApprovalThreshold(AdminRank rank)
    {
        return rank switch
        {
            AdminRank.CustomerSupport => (true, 150),
            AdminRank.BookingManager => (true, 500),
            AdminRank.PropertyManager => (true, 0),
            AdminRank.SeniorManager => (true, 2000),
            AdminRank.Admin => (true, 2000),
            AdminRank.SuperAdmin => (true, 2000),
            _ => (false, 0)
        };
    }
}

// DTOs
public class ApprovalThresholdsResponse
{
    public ThresholdInfo[] Thresholds { get; set; } = Array.Empty<ThresholdInfo>();
}

public class ThresholdInfo
{
    public string Role { get; set; } = string.Empty;
    public decimal AutonomousLimit { get; set; }
    public decimal RequiresApprovalAbove { get; set; }
}

public class SubmitTransactionRequest
{
    public string OperationType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountBefore { get; set; }
    public decimal AmountAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool? MfaConfirmed { get; set; }
}

public class ApproveTransactionRequest
{
    public bool MfaConfirmed { get; set; }
}

public class RejectTransactionRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class FinancialTransactionResponse
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool RequiresMfa { get; set; }
}
