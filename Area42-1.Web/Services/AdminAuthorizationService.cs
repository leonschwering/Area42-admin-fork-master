using Area42_1.Web.Models;

namespace Area42_1.Web.Services;

/// <summary>
/// Admin authorization service - manages rank-based permissions and policies
/// Implements the permission matrix from security compliance document
/// </summary>
public class AdminAuthorizationService
{
    private readonly ILogger<AdminAuthorizationService> _logger;

    public AdminAuthorizationService(ILogger<AdminAuthorizationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Check if user has permission for an action
    /// </summary>
    public bool HasPermission(AdminRank? rank, string permission)
    {
        if (!rank.HasValue)
            return false;

        // Permission matrix implementation
        return (rank, permission) switch
        {
            // Super Admin (1.1) - full access
            (AdminRank.SuperAdmin, _) => true,

            // Admin (1.2) - all except system config
            (AdminRank.Admin, "system_config") => false,
            (AdminRank.Admin, _) => true,

            // Senior Manager (1.3) - staff, financial, override
            (AdminRank.SeniorManager, "kill_switch_initiate") => false,
            (AdminRank.SeniorManager, "system_config") => false,
            (AdminRank.SeniorManager, "user_management") => false,
            (AdminRank.SeniorManager, "staff_management") => true,
            (AdminRank.SeniorManager, "financial_reports") => true,
            (AdminRank.SeniorManager, "financial_override_2000") => true,
            (AdminRank.SeniorManager, _) => true,

            // Property Manager (2.1) - properties only
            (AdminRank.PropertyManager, "property_crud") => true,
            (AdminRank.PropertyManager, _) => false,

            // Booking Manager (2.2) - bookings only
            (AdminRank.BookingManager, "booking_edit") => true,
            (AdminRank.BookingManager, "booking_view") => true,
            (AdminRank.BookingManager, "customer_support_reply") => false,
            (AdminRank.BookingManager, _) => false,

            // Customer Support (2.3) - view bookings, reply only
            (AdminRank.CustomerSupport, "booking_view") => true,
            (AdminRank.CustomerSupport, "customer_support_reply") => true,
            (AdminRank.CustomerSupport, _) => false,

            // Senior Intern (3.1) - read + supervised tasks
            (AdminRank.SeniorIntern, "dashboard_view") => true,
            (AdminRank.SeniorIntern, "customer_support_reply") => true,
            (AdminRank.SeniorIntern, _) => false,

            // Intern (3.2) - read-only
            (AdminRank.Intern, "dashboard_view") => true,
            (AdminRank.Intern, _) => false,

            // Intern Admin (IA) - read + queued writes
            (AdminRank.InternAdmin, "dashboard_view") => true,
            (AdminRank.InternAdmin, "action_queue_submit") => true,
            (AdminRank.InternAdmin, _) => false,

            _ => false
        };
    }

    /// <summary>
    /// Check financial transaction approval requirements
    /// Returns required approver rank and MFA requirement
    /// </summary>
    public (AdminRank? requiredApproverRank, bool mfaRequired, decimal autonomousLimit) 
        GetFinancialApprovalPolicy(AdminRank rank, decimal amount)
    {
        return rank switch
        {
            AdminRank.CustomerSupport => amount <= 150
                ? (null, false, 150)  // Can process autonomously
                : (AdminRank.SeniorManager, false, 150),  // Need Rank 1.3+ approval

            AdminRank.BookingManager => amount <= 500
                ? (null, false, 500)
                : (AdminRank.SeniorManager, false, 500),

            AdminRank.SeniorManager => amount > 2000
                ? (AdminRank.Admin, true, 2000)  // Need 2× Rank 1.2+ approval
                : (null, amount > 500, 2000),

            AdminRank.Admin => amount > 2000
                ? (AdminRank.SuperAdmin, true, 2000)  // Need 2× Rank 1.2+ approval
                : (null, amount > 500, 2000),

            AdminRank.SuperAdmin => amount > 2000
                ? (AdminRank.Admin, true, 2000)  // Cannot self-approve, need 2× others
                : (null, amount > 500, 2000),

            _ => (null, false, 0)  // No financial access
        };
    }

    /// <summary>
    /// Check if user can kill switch
    /// Only Rank 1.1 and 1.2 can initiate/confirm
    /// </summary>
    public bool CanInitiateKillSwitch(AdminRank? rank)
        => rank == AdminRank.SuperAdmin || rank == AdminRank.Admin;

    /// <summary>
    /// Check if user has data export permission
    /// Rank 3 (interns) cannot export
    /// </summary>
    public bool CanExportData(AdminRank? rank)
        => rank is not (AdminRank.Intern or AdminRank.InternAdmin);

    /// <summary>
    /// Check if user can view audit logs
    /// Rank 1.2+ only
    /// </summary>
    public bool CanViewAuditLogs(AdminRank? rank)
        => rank == AdminRank.SuperAdmin || rank == AdminRank.Admin;

    /// <summary>
    /// Check if user can view security flags
    /// Rank 1.2+ only
    /// </summary>
    public bool CanViewSecurityFlags(AdminRank? rank)
        => rank == AdminRank.SuperAdmin || rank == AdminRank.Admin;

    /// <summary>
    /// Check if user can manage admins
    /// Rank 1.2+ only
    /// </summary>
    public bool CanManageAdmins(AdminRank? rank)
        => rank == AdminRank.SuperAdmin || rank == AdminRank.Admin;

    /// <summary>
    /// Check if user can view GDPR tools
    /// Rank 1.2+ only (for compliance oversight)
    /// </summary>
    public bool CanAccessGdprTools(AdminRank? rank)
        => rank == AdminRank.SuperAdmin || rank == AdminRank.Admin;

    /// <summary>
    /// Get the rank label in English/Dutch format
    /// </summary>
    public (string englishLabel, string dutchLabel) GetRankLabel(AdminRank rank)
    {
        return rank switch
        {
            AdminRank.SuperAdmin => ("Super Admin", "Systeembeheerder"),
            AdminRank.Admin => ("Admin", "Beheerder"),
            AdminRank.SeniorManager => ("Senior Manager", "Senior Manager"),
            AdminRank.PropertyManager => ("Property Manager", "Beheerder Accommodaties"),
            AdminRank.BookingManager => ("Booking Manager", "Reserveringsbeheerder"),
            AdminRank.CustomerSupport => ("Customer Support", "Klantenondersteuning"),
            AdminRank.SeniorIntern => ("Senior Intern", "Senior Stagiair"),
            AdminRank.Intern => ("Intern", "Stagiair"),
            AdminRank.InternAdmin => ("Intern Admin", "Stagiair Admin"),
            _ => ("Unknown", "Onbekend")
        };
    }

    /// <summary>
    /// Get dashboard sections visible to this rank
    /// </summary>
    public string[] GetVisibleDashboardSections(AdminRank? rank)
    {
        if (!rank.HasValue)
            return Array.Empty<string>();

        return rank switch
        {
            AdminRank.SuperAdmin => new[] {
                "overview", "kpis", "booking_stats", "revenue", "security_flags",
                "audit_logs", "kill_switch", "staff_management", "financial_reports",
                "gdpr_tools", "security_dashboard"
            },

            AdminRank.Admin => new[] {
                "overview", "kpis", "booking_stats", "revenue", "security_flags",
                "audit_logs", "kill_switch", "staff_management", "gdpr_tools"
            },

            AdminRank.SeniorManager => new[] {
                "overview", "kpis", "booking_stats", "revenue", "financial_reports",
                "staff_management"
            },

            AdminRank.PropertyManager => new[] {
                "overview", "kpis", "properties"
            },

            AdminRank.BookingManager => new[] {
                "overview", "kpis", "bookings", "booking_stats"
            },

            AdminRank.CustomerSupport => new[] {
                "overview", "bookings_readonly", "support_inbox"
            },

            AdminRank.SeniorIntern => new[] {
                "overview", "bookings_readonly", "support_inbox"
            },

            AdminRank.Intern => new[] {
                "overview", "bookings_readonly"
            },

            AdminRank.InternAdmin => new[] {
                "overview", "bookings_readonly", "pending_approvals"
            },

            _ => Array.Empty<string>()
        };
    }
}
