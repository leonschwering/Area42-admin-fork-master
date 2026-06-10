namespace Area42_1.Web.Models;

/// <summary>
/// Admin rank system - defines role hierarchy and permissions
/// Based on security compliance document requirements
/// </summary>
public enum AdminRank
{
    // Tier 1 - Super Admin & Admin
    SuperAdmin = 11,          // 1.1 - System control, kill switch, audit logs
    Admin = 12,                // 1.2 - User management, security policy
    SeniorManager = 13,        // 1.3 - Staff management, financial reports

    // Tier 2 - Managers
    PropertyManager = 21,      // 2.1 - Property CRUD
    BookingManager = 22,       // 2.2 - Booking management
    CustomerSupport = 23,      // 2.3 - Support replies (read-only booking view)

    // Tier 3 - Interns
    SeniorIntern = 31,         // 3.1 - Read access + limited tasks
    Intern = 32,               // 3.2 - Read-only dashboard
    InternAdmin = 99           // IA - Supervised, pending approvals
}

public enum HRRank
{
    HRManager = 1,             // HR.1 - Full employee data CRUD, BSN+salary access
    HREmployee = 2,            // HR.2 - Read/write employee records (no sensitive)
    HRIntern = 3               // HR.3 - Read-only basic info
}

public class AdminUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Rank claim - mutually exclusive with HRRank
    public AdminRank? Rank { get; set; }
    public HRRank? HRRank { get; set; }

    // Account state
    public bool IsEnabled { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public DateTime? LockoutUntil { get; set; }

    // For intern accounts
    public DateTime? InternshipEndDate { get; set; }

    // MFA
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    // Session timeout (30 min for admins)
    public DateTime? SessionExpiresAt { get; set; }
}

/// <summary>
/// Kill switch request - two-person quorum for security incidents
/// </summary>
public class KillSwitchRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? TargetUserId { get; set; }      // Null for KS-2/KS-3
    public string InitiatorUserId { get; set; } = string.Empty;
    public string? ConfirmerUserId { get; set; }
    public KillSwitchStatus Status { get; set; } = KillSwitchStatus.Pending;
    public KillSwitchLevel Level { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }        // CreatedAt + 10 min
    public DateTime? ExecutedAt { get; set; }
    public string? ExecutionLog { get; set; }      // JSON summary
}

public enum KillSwitchStatus { Pending, Confirmed, Rejected, Expired, Executed }
public enum KillSwitchLevel { AccountLock, AdminDomainLock, FullSystemLock }

/// <summary>
/// Immutable audit log - all admin actions tracked
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string? UserRank { get; set; }
    public string Action { get; set; } = string.Empty;    // e.g. "User.Create", "Booking.Cancel"
    public string EntityType { get; set; } = string.Empty; // e.g. "User", "Booking"
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; }                  // JSON
    public string? NewValues { get; set; }                  // JSON
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// Security flag from SIEM rules - behavioral anomaly detection
/// </summary>
public class SecurityFlag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RuleId { get; set; } = string.Empty;     // e.g. "BR-01"
    public string TriggeredByUserId { get; set; } = string.Empty;
    public string AffectedEntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FlagStatus Status { get; set; } = FlagStatus.Pending;
    public string? ReviewedByAdminId { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}

public enum FlagStatus { Pending, Dismissed, Escalated }

/// <summary>
/// Financial audit log - dual approval tracking for transactions
/// </summary>
public class FinancialAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InitiatorUserId { get; set; } = string.Empty;
    public string? ApproverUserId { get; set; }
    public string OperationType { get; set; } = string.Empty;  // e.g. "Refund"
    public string EntityId { get; set; } = string.Empty;        // Booking or Invoice ID
    public decimal AmountBefore { get; set; }
    public decimal AmountAfter { get; set; }
    public string? EncryptedPaymentRef { get; set; }            // AES-256
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public bool MfaConfirmed { get; set; }
}

/// <summary>
/// Pending action from Intern Admin - queued for approval
/// </summary>
public class PendingAdminAction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InternUserId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;      // e.g. "Booking.Cancel"
    public string EntityId { get; set; } = string.Empty;
    public string SerializedPayload { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime InitialExpiresAt { get; set; }              // SubmittedAt + 24h
    public bool ExtensionGranted { get; set; }
    public DateTime? ExtendedExpiresAt { get; set; }            // +24h if extended
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByAdminId { get; set; }
    public string? ReviewNote { get; set; }
    public PendingActionStatus Status { get; set; } = PendingActionStatus.Pending;
}

public enum PendingActionStatus { Pending, Extended, Approved, Rejected, AutoRejected }

/// <summary>
/// GDPR erasure request tracking
/// </summary>
public class GdprErasureRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime TargetCompletionDate { get; set; }  // RequestedAt + 30 days
    public GdprErasureStatus Status { get; set; } = GdprErasureStatus.Pending;
    public string? CompletedByAdminId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public enum GdprErasureStatus { Pending, InProgress, Completed, Failed }

/// <summary>
/// Consent record - tracks user opt-in/opt-out for GDPR Art. 7
/// </summary>
public class ConsentLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public ConsentType Type { get; set; }
    public bool Consented { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? PrivacyVersionAccepted { get; set; }
}

public enum ConsentType { Marketing, Analytics, Cookies, TermsOfService }
