using Area42_1.Web.Models;
using System.Text.Json;

namespace Area42_1.Web.Services;

/// <summary>
/// Kill Switch service - emergency mechanism for security incidents
/// Implements two-person quorum for account/domain/system lock
/// </summary>
public class KillSwitchService
{
    private readonly ILogger<KillSwitchService> _logger;
    private readonly Dictionary<Guid, KillSwitchRequest> _requests = new();  // In-memory for demo
    private readonly INotificationService _notifier;

    public KillSwitchService(ILogger<KillSwitchService> logger, INotificationService notifier)
    {
        _logger = logger;
        _notifier = notifier;
    }

    /// <summary>
    /// KS-1: Lock a single admin account (e.g., compromised)
    /// </summary>
    public async Task<(bool success, string message, Guid? requestId)> InitiateAccountLockAsync(
        string initiatorUserId,
        string targetUserId,
        string reason)
    {
        // Validation
        if (initiatorUserId == targetUserId)
            return (false, "Cannot target your own account", null);

        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Reason is required", null);

        var request = new KillSwitchRequest
        {
            InitiatorUserId = initiatorUserId,
            TargetUserId = targetUserId,
            Level = KillSwitchLevel.AccountLock,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)  // 10-minute expiry
        };

        _requests[request.Id] = request;

        _logger.LogWarning(
            "Kill Switch KS-1 initiated by {Initiator} against account {Target}. Reason: {Reason}",
            initiatorUserId, targetUserId, reason);

        // Notify all admins
        await _notifier.NotifyAdminsKillSwitchInitiatedAsync(request);

        return (true, "Account lock initiated. Awaiting confirmation from another admin.", request.Id);
    }

    /// <summary>
    /// Confirm a pending kill switch request (different admin than initiator)
    /// </summary>
    public async Task<(bool success, string message)> ConfirmKillSwitchAsync(
        Guid requestId,
        string confirmerUserId)
    {
        if (!_requests.TryGetValue(requestId, out var request))
            return (false, "Kill switch request not found");

        // Validation
        if (request.Status != KillSwitchStatus.Pending)
            return (false, $"Request is already {request.Status}");

        if (request.ExpiresAt < DateTime.UtcNow)
        {
            request.Status = KillSwitchStatus.Expired;
            return (false, "Request has expired (10-minute window)");
        }

        if (confirmerUserId == request.InitiatorUserId)
            return (false, "Confirmer must be a different admin than the initiator");

        request.ConfirmerUserId = confirmerUserId;
        request.Status = KillSwitchStatus.Confirmed;
        request.ExecutedAt = DateTime.UtcNow;

        // Execute the kill switch
        var (execSuccess, execMessage) = await ExecuteKillSwitchAsync(request);

        if (!execSuccess)
            return (false, execMessage);

        _logger.LogCritical(
            "Kill Switch KS-{Level} EXECUTED by initiator {Initiator} + confirmer {Confirmer}. " +
            "Target: {Target}. Reason: {Reason}",
            (int)request.Level, request.InitiatorUserId, confirmerUserId,
            request.TargetUserId ?? "[DOMAIN/SYSTEM]", request.Reason);

        // Notify all admins of execution
        await _notifier.NotifyAdminsKillSwitchExecutedAsync(request);

        return (true, "Kill switch executed successfully");
    }

    /// <summary>
    /// Execute the kill switch action based on level
    /// </summary>
    private async Task<(bool success, string message)> ExecuteKillSwitchAsync(KillSwitchRequest request)
    {
        try
        {
            var executionLog = new Dictionary<string, object>();

            switch (request.Level)
            {
                case KillSwitchLevel.AccountLock:
                    // KS-1: Lock target account
                    // In production: set IsLocked, revoke tokens, strip rank claims, terminate sessions
                    executionLog["action"] = "AccountLock";
                    executionLog["targetUserId"] = request.TargetUserId;
                    executionLog["tokenRevoked"] = true;
                    executionLog["sessionTerminated"] = true;
                    executionLog["rankClaimsStripped"] = true;
                    break;

                case KillSwitchLevel.AdminDomainLock:
                    // KS-2: Suspend all admin logins except initiator + confirmer
                    executionLog["action"] = "AdminDomainLock";
                    executionLog["allowedAdmins"] = new[] { request.InitiatorUserId, request.ConfirmerUserId };
                    executionLog["maintenancePage"] = "Enabled for all other admins";
                    break;

                case KillSwitchLevel.FullSystemLock:
                    // KS-3: Entire platform read-only, terminate all sessions
                    executionLog["action"] = "FullSystemLock";
                    executionLog["mode"] = "ReadOnly";
                    executionLog["writeEndpointsBlocked"] = true;
                    executionLog["allSessionsTerminated"] = true;
                    executionLog["incidentResponseTriggered"] = true;
                    break;
            }

            executionLog["executedAt"] = DateTime.UtcNow;
            request.ExecutionLog = JsonSerializer.Serialize(executionLog);

            return (true, "Kill switch executed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing kill switch");
            return (false, $"Error executing kill switch: {ex.Message}");
        }
    }

    /// <summary>
    /// Get pending kill switch requests
    /// </summary>
    public IEnumerable<KillSwitchRequest> GetPendingRequests()
    {
        // Clean up expired requests
        var expired = _requests.Where(r => r.Value.ExpiresAt < DateTime.UtcNow && r.Value.Status == KillSwitchStatus.Pending)
            .ToList();

        foreach (var exp in expired)
        {
            _requests[exp.Key].Status = KillSwitchStatus.Expired;
        }

        return _requests.Values.Where(r => r.Status == KillSwitchStatus.Pending);
    }

    /// <summary>
    /// Get kill switch request by ID
    /// </summary>
    public KillSwitchRequest? GetRequestById(Guid id)
    {
        return _requests.TryGetValue(id, out var req) ? req : null;
    }
}

/// <summary>
/// Notification service interface - implementations provided by infra team
/// </summary>
public interface INotificationService
{
    Task NotifyAdminsKillSwitchInitiatedAsync(KillSwitchRequest request);
    Task NotifyAdminsKillSwitchExecutedAsync(KillSwitchRequest request);
    Task NotifySecurityFlagTriggeredAsync(SecurityFlag flag);
    Task SendVogRenewalReminderAsync(string userId, DateTime expiryDate);
    Task NotifyHrVogExpiredAsync(string userId, DateTime blockScheduledDate);
    Task NotifyEmployeeVogBlockedAsync(string userId);
    Task NotifyHrVogBlockedAsync(string userId);
}

/// <summary>
/// Demo notification service
/// </summary>
public class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> _logger;

    public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAdminsKillSwitchInitiatedAsync(KillSwitchRequest request)
    {
        _logger.LogWarning("📢 NOTIFICATION: Kill Switch KS-{Level} initiated by {Initiator}",
            (int)request.Level, request.InitiatorUserId);
        return Task.CompletedTask;
    }

    public Task NotifyAdminsKillSwitchExecutedAsync(KillSwitchRequest request)
    {
        _logger.LogCritical("🔴 ALERT: Kill Switch KS-{Level} EXECUTED", (int)request.Level);
        return Task.CompletedTask;
    }

    public Task NotifySecurityFlagTriggeredAsync(SecurityFlag flag)
    {
        _logger.LogWarning("🚨 SECURITY FLAG {RuleId}: {Description}", flag.RuleId, flag.Description);
        return Task.CompletedTask;
    }

    public Task SendVogRenewalReminderAsync(string userId, DateTime expiryDate)
    {
        _logger.LogInformation("📧 VOG renewal reminder sent to {UserId}, expires {Date}", userId, expiryDate);
        return Task.CompletedTask;
    }

    public Task NotifyHrVogExpiredAsync(string userId, DateTime blockScheduledDate)
    {
        _logger.LogWarning("⏰ VOG expired for {UserId}, account will be blocked on {Date}", userId, blockScheduledDate);
        return Task.CompletedTask;
    }

    public Task NotifyEmployeeVogBlockedAsync(string userId)
    {
        _logger.LogWarning("🔒 Account blocked for {UserId} - VOG not renewed", userId);
        return Task.CompletedTask;
    }

    public Task NotifyHrVogBlockedAsync(string userId)
    {
        _logger.LogWarning("🔒 HR notified: Account blocked for {UserId} - VOG expired", userId);
        return Task.CompletedTask;
    }
}
