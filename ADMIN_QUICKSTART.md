# Admin Portal Quick Start Guide

**For:** .NET 10 Blazor Developers  
**Last Updated:** April 2026  
**Status:** ✅ Ready for Data Layer Integration

---

## 🚀 Quick Start (5 Minutes)

### 1. Clone & Open
```powershell
cd C:\Users\LAURE\source\repos\Area42-1
git pull origin master
```

Open `Area42-1.sln` in Visual Studio 2026.

### 2. Run the Application
```powershell
# In Visual Studio: Press F5 or Ctrl+F5
# Or from terminal:
dotnet run --project Area42-1.Web/Area42-1.Web.csproj
```

### 3. Navigate to Admin Dashboard
- **URL:** `http://localhost:5000/admin`
- **Expected:** Tabbed admin interface with security warning banner

---

## 📁 Key Files Reference

| File | Purpose | Status |
|------|---------|--------|
| `Components/Pages/AdminDashboard.razor` | Main admin hub (tabs, sections) | ✅ Complete |
| `Components/AdminComponents/*.razor` | Role-gated dashboard sections | ✅ Complete (5 components) |
| `Models/AdminModels.cs` | All domain entities & enums | ✅ Complete |
| `Services/AdminAuthorizationService.cs` | RBAC & permission matrix | ✅ Complete |
| `Services/KillSwitchService.cs` | Kill Switch protocol & quorum | ✅ Complete (in-memory) |
| `Program.cs` | Service registration & DI | ✅ Updated |
| `ADMIN_PORTAL_IMPLEMENTATION.md` | Full technical documentation | ✅ Complete |

---

## 🔐 Role Hierarchy Cheat Sheet

```
Tier 1 (System Security)
├─ 1.1 Super Admin        [All permissions]
├─ 1.2 Admin              [All except system config]
└─ 1.3 Senior Manager     [Staff, Financial, Overview]

Tier 2 (Operational)
├─ 2.1 Property Manager   [Property CRUD]
├─ 2.2 Booking Manager    [Booking management]
└─ 2.3 Customer Support   [Support + limited financial]

Tier 3 (Supervised)
├─ 3.1 Senior Intern      [Read + supervised tasks]
├─ 3.2 Intern             [Read-only]
└─ IA  Intern Admin       [Like 2.x but writes queued]

HR Domain (Separate Portal)
├─ HR.1 HR Manager        [Full employee CRUD + sensitive data]
├─ HR.2 HR Employee       [Employee CRUD - no sensitive]
└─ HR.3 HR Intern         [Read-only - supervised]
```

---

## 🧪 Testing the Admin Portal

### 1. Test Role Visibility

**As SuperAdmin (1.1):**
```csharp
@code {
    private AdminRank? CurrentUserRank = AdminRank.SuperAdmin;
    // Expected: All tabs visible (Overview, Security, Staff, GDPR, Financial)
}
```

**As CustomerSupport (2.3):**
```csharp
@code {
    private AdminRank? CurrentUserRank = AdminRank.CustomerSupport;
    // Expected: Only Overview tab visible
}
```

### 2. Test Kill Switch Initiation

```csharp
var killSwitchService = new KillSwitchService();
var result = await killSwitchService.InitiateAccountLockAsync(
    targetUserId: "user-123",
    initiatorUserId: "admin-456",
    reason: "Account compromised"
);

// result.success should be true
// A pending request should appear in SecurityDashboard
```

### 3. Test Kill Switch Confirmation

```csharp
// From pending requests list, confirm with different admin
var result = await killSwitchService.ConfirmKillSwitchAsync(
    requestId: Guid.Parse("xxx"),
    confirmerUserId: "admin-789"  // MUST differ from initiator
);

// result.success should be true
// Request status should change to "Confirmed" then "Executed"
```

### 4. Test Authorization Checks

```csharp
var authService = new AdminAuthorizationService();

// SuperAdmin can access security dashboard
bool canAccess = authService.CanViewSecurityFlags(AdminRank.SuperAdmin);
// Expected: true

// CustomerSupport cannot
canAccess = authService.CanViewSecurityFlags(AdminRank.CustomerSupport);
// Expected: false
```

---

## 🔗 Integration Checklist

### Before API Implementation

- [ ] Add EF Core DbContext with domain models
- [ ] Create database migrations
- [ ] Seed demo data (test users, ranks)
- [ ] Configure connection string in appsettings.json

### API Endpoints to Create

```
GET  /api/admin/dashboard/summary              → KPI data for overview
GET  /api/admin/kill-switch/pending            → Pending requests for security dashboard
POST /api/admin/kill-switch/initiate           → Start kill switch
POST /api/admin/kill-switch/{id}/confirm       → Confirm with second admin
GET  /api/admin/audit-logs                     → Fetch & filter audit logs
POST /api/admin/staff                          → Create staff member
GET  /api/admin/staff/{id}                     → Get staff details
PUT  /api/admin/staff/{id}                     → Update staff
DELETE /api/admin/staff/{id}                   → Deactivate staff
GET  /api/admin/financial/pending-approvals    → Transactions awaiting approval
POST /api/admin/financial/{id}/approve         → Approve financial transaction
GET  /api/admin/gdpr/erasure-requests          → List erasure requests
POST /api/admin/gdpr/erasure/{id}/process      → Process erasure (30-day SLA)
```

### Middleware to Add

- **TokenRevocationMiddleware** — Check JTI against Redis deny-list
- **AdminSessionTimeoutMiddleware** — Enforce 30-min inactivity
- **AuditLoggingMiddleware** — Capture all admin actions
- **AuthorizationPolicyMiddleware** — Enforce role policies per endpoint

### Background Jobs to Register (Quartz.NET)

```csharp
// In Program.cs
services.AddQuartz(q =>
{
    q.AddJob<InternAccountExpiryJob>(opts => opts.WithIdentity("InternExpiry"))
        .AddTrigger(t => t.ForJob("InternExpiry").WithCronSchedule("0 0 * * *")); // Daily at midnight

    q.AddJob<PendingActionExpiryJob>(opts => opts.WithIdentity("ActionExpiry"))
        .AddTrigger(t => t.ForJob("ActionExpiry").WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever()));

    q.AddJob<VogExpiryJob>(opts => opts.WithIdentity("VogExpiry"))
        .AddTrigger(t => t.ForJob("VogExpiry").WithCronSchedule("0 6 * * *")); // Daily at 6 AM
});
```

---

## 🛡️ Security Reminders

### ✅ DO

- ✅ Always enforce authorization **server-side** on API endpoints (not just UI)
- ✅ Log every admin action to immutable audit log
- ✅ Use two-person quorum for high-risk operations (Kill Switch, financial >€2000)
- ✅ Encrypt PII fields (AES-256) before storing in database
- ✅ Validate all inputs (FluentValidation) before processing
- ✅ Use parameterized queries (EF Core only) — no raw SQL
- ✅ Revoke tokens immediately on account lock (Redis deny-list)
- ✅ Auto-expire Intern Admin accounts on internship end date
- ✅ Notify all Rank 1 admins of security events (Kill Switch, suspicious activity)

### ❌ DON'T

- ❌ Trust client-side authorization (UI visibility is not security)
- ❌ Store secrets in appsettings.json or version control
- ❌ Log sensitive data (passwords, BSN, IBAN) — use masking
- ❌ Allow Super Admin to bypass dual-approval for financial transactions >€2000
- ❌ Store plaintext passwords — always use bcrypt (cost factor 12+)
- ❌ Make authorization decisions based on HTTP headers alone
- ❌ Disable MFA for admin accounts
- ❌ Create accounts without VOG verification (for employees)
- ❌ Ignore Rank 3 (Intern) time-limit expiry

---

## 🐛 Debugging Tips

### Enable Detailed Logging

```csharp
// In Program.cs
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
```

### Check Rank in Browser Console

```javascript
// In Blazor component
@code {
    protected override void OnInitialized()
    {
        Console.WriteLine($"Current rank: {CurrentUserRank}");
        Console.WriteLine($"Visible sections: {string.Join(", ", visibleSections)}");
    }
}
```

### Inspect Kill Switch Request

```csharp
var requests = KillSwitchService.GetPendingRequests();
foreach (var req in requests)
{
    Console.WriteLine($"ID: {req.Id}, Level: {req.Level}, Status: {req.Status}, Expires: {req.ExpiresAt}");
}
```

---

## 📚 Learning Path

1. **Understand Role Hierarchy** → Read `AdminModels.cs` (AdminRank enum)
2. **Study Authorization Logic** → Read `AdminAuthorizationService.cs`
3. **Understand Kill Switch** → Read `KillSwitchService.cs` + "KS-1/KS-2/KS-3" section in IMPLEMENTATION.md
4. **Explore UI Components** → Check `Components/AdminComponents/` for role-gated sections
5. **Review Compliance Spec** → Read HolidayReserve_Cybersecurity_Action_Plan.pdf (Section 2.3, 3, 4, 5)

---

## 🚨 Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Admin tabs all invisible | `visibleSections` is empty | Verify `AuthService.GetVisibleDashboardSections()` is called in OnInitialized |
| "Navigation" not found | Missing `@inject NavigationManager` | Add `@inject NavigationManager Navigation` to Razor component |
| Kill Switch button throws exception | In-memory service has no data persistence | Data will be persisted to database in next phase |
| CSS not applying | Bootstrap classes not loaded | Verify Bootstrap is included in MainLayout or _Host.html |
| Rank claim not populated | Authentication not configured | Authentication setup is out of scope for this implementation; demo uses hardcoded rank in @code |

---

## 🎯 Success Criteria

After completing the admin portal implementation, verify:

- [x] Admin dashboard loads at `/admin`
- [x] All 5 dashboard sections render with correct content
- [x] Tabs only show for authorized ranks
- [x] Security warning banner displays
- [x] Kill Switch service returns pending requests
- [x] Authorization checks return correct permissions per rank
- [x] Build succeeds with no warnings
- [x] No JavaScript console errors

---

## 📞 Next Phase: Data Layer

The admin portal is **UI-complete** and **service-ready**. The next developer should:

1. Create `AppDbContext` with EF Core
2. Implement API endpoints with role-based authorization
3. Add database persistence to Kill Switch, Audit Log, Financial Audit
4. Implement token revocation middleware
5. Add Quartz.NET background jobs

**Estimated effort:** 2–3 weeks for full integration + testing.

---

**Happy coding! 🚀**
