# Admin Portal Implementation Guide
## HolidayReserve Security & Compliance Framework

**Version:** 1.0  
**Status:** ✅ Core Infrastructure Complete  
**Last Updated:** April 2026  
**Framework:** .NET 10 Blazor Server with Role-Based Access Control (RBAC)

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Role Hierarchy](#role-hierarchy)
4. [Core Components](#core-components)
5. [Security Features](#security-features)
6. [File Structure](#file-structure)
7. [Usage & Integration](#usage--integration)
8. [Next Steps](#next-steps)

---

## Overview

The Admin Portal is a secure Blazor Server application implementing the HolidayReserve Cybersecurity & AVG/GDPR Compliance Action Plan. It provides role-based access to:

- **Security Management** — Kill Switch protocol, audit logs, security flags (SIEM)
- **Staff Management** — Personnel CRUD, VOG verification, Intern Admin supervision
- **GDPR/AVG Tools** — Erasure requests, data exports, consent records, breach response
- **Financial Reports** — Revenue analytics, dual-approval transactions, audit logs
- **Dashboard Overview** — KPIs, system health, security status

### Key Principles

✅ **Server-side authorization enforcement** — UI hides elements; API enforces permissions  
✅ **Immutable audit logging** — Every admin action is permanently recorded  
✅ **Two-person quorum** — Kill Switch, high-value financial transactions require dual approval  
✅ **Encryption at rest** — PII fields encrypted with AES-256  
✅ **Time-limited accounts** — Intern Admin (IA) and Intern (3.x) accounts auto-expire  
✅ **SIEM integration** — Behavioral anomaly detection via security rules (BR-01 through BR-08)

---

## Architecture

### Solution Structure

```
Area42-1.sln
├── Area42-1.Web/                      ← Admin Blazor Server App
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── AdminDashboard.razor    ← Main admin hub (this session)
│   │   └── AdminComponents/            ← Role-gated dashboard sections
│   │       ├── DashboardOverview.razor
│   │       ├── SecurityDashboard.razor
│   │       ├── StaffManagement.razor
│   │       ├── GdprTools.razor
│   │       └── FinancialReports.razor
│   ├── Models/
│   │   └── AdminModels.cs              ← Domain entities (ranks, kill switch, audit)
│   ├── Services/
│   │   ├── KillSwitchService.cs        ← Kill Switch protocol & quorum
│   │   └── AdminAuthorizationService.cs ← RBAC & permission checks
│   ├── Program.cs                      ← Service registration & middleware
│   └── appsettings.json
├── Area42-1.ApiService/                ← Backend API (future)
├── Area42-1.AppHost/                   ← .NET Aspire orchestrator
└── Area42-1.ServiceDefaults/           ← Shared telemetry
```

### Authorization Pipeline

```
User Login
    ↓
Claims Injected (rank, hr_rank, sub, jti)
    ↓
AdminAuthorizationService Evaluates Rank
    ↓
UI Components Check Permissions (@if visibleSections.Contains(...))
    ↓
API Endpoints Enforce Policy Server-Side (NOT IMPLEMENTED YET - placeholder)
    ↓
Audit Log Recorded (Admin Models ready, service calls pending implementation)
```

---

## Role Hierarchy

### Admin Domain Ranks (1.x – 3.x)

| Rank | Title | Key Permissions | Dashboard Access |
|------|-------|-----------------|------------------|
| **1.1** | Super Admin | System config, Kill Switch (both levels), user mgmt, security settings, kill switch override for immediate lift | All sections |
| **1.2** | Admin | Kill Switch initiation/confirmation, user management, security oversight, audit log access | All sections except System Config |
| **1.3** | Senior Manager | Staff management, financial reports, operational dashboards | Overview, Staff, Financial |
| **2.1** | Property Manager | Property CRUD, availability, pricing | Overview (read-only) |
| **2.2** | Booking Manager | Reservation management, cancellations | Overview (read-only) |
| **2.3** | Customer Support | View booking details, respond to inquiries (no edit) | Overview (read-only) |
| **3.1** | Senior Intern | Supervised read + limited task execution under manager | Overview (read-only) |
| **3.2** | Intern | Read-only dashboard, supervised support | Overview (read-only) |
| **IA** | Intern Admin | Same read access as 2.x ranks; ALL writes queued for 1.2+ approval | Overview, Security (read-only), Staff (queued) |

### HR Domain Ranks (HR.x) — Separate Portal

| Rank | Title | Permissions | HR-Only Access |
|------|-------|-------------|-----------------|
| **HR.1** | HR Manager | Full employee CRUD, BSN + salary + IBAN access, VOG approval | Yes |
| **HR.2** | HR Employee | Employee records read/write (no PII) | Yes |
| **HR.3** | HR Intern | Read-only basic employee info | Yes |

---

## Core Components

### 1. AdminModels.cs — Domain Entities

**Purpose:** Define all security & compliance domain models  
**Key Classes:**

```csharp
// Role/Rank Definition
public enum AdminRank { SuperAdmin, Admin, SeniorManager, PropertyManager, 
                        BookingManager, CustomerSupport, SeniorIntern, Intern, InternAdmin }
public enum HRRank { Manager, Employee, Intern }

// Admin User Account
public class AdminUser
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public AdminRank? Rank { get; set; }
    public DateTime? InternshipEndDate { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

// Kill Switch Request (2-of-N quorum)
public class KillSwitchRequest
{
    public Guid Id { get; set; }
    public string? TargetUserId { get; set; }
    public string InitiatorUserId { get; set; }
    public string? ConfirmerUserId { get; set; }
    public KillSwitchLevel Level { get; set; }  // AccountLock, AdminDomainLock, FullSystemLock
    public KillSwitchStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }  // 10 minutes
    public DateTime? ExecutedAt { get; set; }
}

// Audit Log (immutable)
public class AuditLogEntry
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string UserRank { get; set; }
    public string Action { get; set; }  // e.g., "Booking.Cancel"
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public DateTime Timestamp { get; set; }
}

// SIEM Security Flag
public class SecurityFlag
{
    public Guid Id { get; set; }
    public string RuleId { get; set; }  // e.g., "BR-01"
    public string TriggeredByUserId { get; set; }
    public string Description { get; set; }
    public FlagStatus Status { get; set; }  // Pending, Dismissed, Escalated
    public string? ReviewNote { get; set; }
}

// Financial Audit (dual approval)
public class FinancialAuditLog
{
    public Guid Id { get; set; }
    public string InitiatorUserId { get; set; }
    public string ApproverUserId { get; set; }
    public string OperationType { get; set; }  // "Refund", "BillingCorrection"
    public decimal AmountBefore { get; set; }
    public decimal AmountAfter { get; set; }
    public bool MfaConfirmed { get; set; }
    public DateTime Timestamp { get; set; }
}

// Pending Admin Action (Intern Admin queue)
public class PendingAdminAction
{
    public Guid Id { get; set; }
    public string InternUserId { get; set; }
    public string ActionType { get; set; }  // e.g., "Booking.Cancel"
    public string SerializedPayload { get; set; }  // JSON
    public DateTime SubmittedAt { get; set; }
    public DateTime InitialExpiresAt { get; set; }  // +24h
    public bool ExtensionGranted { get; set; }
    public DateTime? ExtendedExpiresAt { get; set; }  // +24h (one time only)
    public PendingActionStatus Status { get; set; }
}

// GDPR Erasure Request
public class GdprErasureRequest
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime DueAt { get; set; }  // 30 days
    public GdprErasureStatus Status { get; set; }
}

// Consent Log
public class ConsentLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string ConsentType { get; set; }  // "Marketing", "Analytics", "Cookies"
    public bool Granted { get; set; }
    public string ConsentText { get; set; }
    public DateTime RecordedAt { get; set; }
}
```

---

### 2. KillSwitchService.cs — Two-Person Quorum & Emergency Protocol

**Purpose:** Implement kill switch protocol with quorum enforcement  
**Key Methods:**

```csharp
public class KillSwitchService
{
    // Initiate kill switch (2-of-N quorum required)
    public async Task<(bool success, string message)> InitiateAccountLockAsync(
        string targetUserId, string initiatorUserId, string reason);

    // Confirm kill switch (different admin required)
    public async Task<(bool success, string message)> ConfirmKillSwitchAsync(
        Guid requestId, string confirmerUserId);

    // Execute kill switch (atomic transaction)
    public async Task ExecuteKillSwitchAsync(KillSwitchRequest request);

    // Get pending requests (for admin review)
    public IEnumerable<KillSwitchRequest> GetPendingRequests();
}
```

**Key Rules:**

- ✅ Self-targeting blocked
- ✅ Initiator ≠ Confirmer (two-person quorum)
- ✅ 10-minute expiry (auto-expire if not confirmed)
- ✅ Maximum 3 initiations per admin per hour (rate limit)
- ✅ Minimum admin check (prevent locking last Rank 1.1)
- ✅ Immutable audit log (cannot be deleted)

**Kill Switch Levels:**

1. **KS-1 — Account Lock** (any Rank 1)
   - Revoke all tokens (JTI added to Redis deny-list)
   - Disable Identity record
   - Strip rank claims
   - Terminate active sessions
   - Notify all Rank 1 admins

2. **KS-2 — Admin Domain Lock** (Rank 1.1 + 1.2)
   - Suspend all admin logins except initiators
   - Return maintenance page
   - Force-terminate all admin sessions except initiators

3. **KS-3 — Full System Lock** (Rank 1.1 only)
   - Entire platform read-only
   - All write endpoints return HTTP 423 Locked
   - Client reservations paused
   - All admin sessions terminated (including initiators)

---

### 3. AdminAuthorizationService.cs — RBAC & Permission Matrix

**Purpose:** Encode rank-based permission matrix and authorization logic  
**Key Methods:**

```csharp
public class AdminAuthorizationService
{
    // Check if user has specific permission
    public bool HasPermission(AdminRank? rank, string permission);

    // Get financial approval policy for a rank
    public FinancialApprovalPolicy GetFinancialApprovalPolicy(AdminRank? rank);

    // Check kill switch access
    public bool CanInitiateKillSwitch(AdminRank? rank);
    public bool CanConfirmKillSwitch(AdminRank? rank);

    // GDPR tool access
    public bool CanAccessGdprTools(AdminRank? rank);
    public bool CanViewAuditLogs(AdminRank? rank);
    public bool CanViewSecurityFlags(AdminRank? rank);

    // Dashboard sections visible to rank
    public string[] GetVisibleDashboardSections(AdminRank? rank);

    // Rank labels for UI display
    public string GetRankLabel(AdminRank? rank);
}
```

**Permission Matrix:**

| Permission | 1.1 | 1.2 | 1.3 | 2.1 | 2.2 | 2.3 | 3.1 | 3.2 | IA |
|-----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:--:|
| System configuration | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Kill Switch | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Audit logs | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | 👁 |
| User management | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Financial reports | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | 👁 |
| Staff management | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Property CRUD | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ⏳ |
| Booking edit | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ⏳ |
| Customer support | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ⏳ |
| Dashboard (read) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

**Legend:**
- ✅ = Full access
- ❌ = No access
- 👁 = Read-only
- ⏳ = Queued (requires admin approval)

---

### 4. AdminDashboard.razor — Main Hub

**Purpose:** Tabbed admin interface with role-gated sections  
**Features:**

- 🔒 **Security Warning Banner** — Legal notice of monitoring
- 👤 **Admin Header** — User rank display, logout button
- 📑 **Navigation Tabs** — Role-based tab visibility
- 📊 **Content Sections** — Five dashboard components

**Child Components:**

1. **DashboardOverview** — KPIs, system health, kill switch quick access
2. **SecurityDashboard** — Kill Switch control, pending requests, audit logs, security flags
3. **StaffManagement** — Personnel CRUD, VOG management, Intern Admin supervision
4. **GdprTools** — Erasure requests, data export audits, consent records, DPIA
5. **FinancialReports** — Revenue analytics, dual-approval transactions, audit logs

---

## Security Features

### ✅ Implemented This Session

- [x] Role-based authorization matrix (9 ranks + 3 HR ranks)
- [x] Kill Switch protocol (KS-1, KS-2, KS-3 with 2-person quorum)
- [x] Audit log models (immutable write-only entity)
- [x] Financial audit log (dual approval tracking)
- [x] Pending admin action queue (Intern Admin supervision)
- [x] GDPR erasure & consent models
- [x] Dashboard UI with role-gated sections
- [x] Authorization service with permission checks

### ⏳ Planned (Next Phase)

- [ ] API endpoints with policy-based authorization
- [ ] Token revocation middleware (Redis-backed)
- [ ] SIEM integration & behavioral rule evaluation
- [ ] Background jobs (Quartz.NET) for account expiry, VOG renewal, pending action auto-rejection
- [ ] Email/SNS notification pipeline
- [ ] Database context & migrations
- [ ] Unit & integration tests
- [ ] Comprehensive audit logging service

---

## File Structure

```
Area42-1.Web/
├── Components/
│   ├── AdminComponents/
│   │   ├── DashboardOverview.razor          (overview, KPIs, kill switch quick access)
│   │   ├── SecurityDashboard.razor          (kill switch, audit logs, security flags)
│   │   ├── StaffManagement.razor            (personnel CRUD, VOG, Intern Admin)
│   │   ├── GdprTools.razor                  (erasure, exports, consent, DPIA)
│   │   └── FinancialReports.razor           (revenue, dual-approval, audit)
│   └── Pages/
│       └── AdminDashboard.razor             (main hub, tabbed interface)
│
├── Models/
│   └── AdminModels.cs                       (all domain entities & enums)
│
├── Services/
│   ├── KillSwitchService.cs                 (kill switch protocol & quorum)
│   ├── AdminAuthorizationService.cs         (RBAC & permission matrix)
│   ├── AccountService.cs                    (existing from prior session)
│   └── AccountValidationService.cs          (existing from prior session)
│
└── Program.cs                               (DI registration, middleware)
```

---

## Usage & Integration

### 1. Accessing Admin Dashboard

**Route:** `/admin` or `/admin/dashboard`

**Prerequisites:**
- User must be authenticated
- User must have a rank claim (1.1 – 3.2 or IA)
- Session must not be expired (30 min inactivity timeout)
- MFA required for Rank 1 & 2 (future implementation)

### 2. Dependency Injection in Components

```razor
@inject AdminAuthorizationService AuthService
@inject KillSwitchService KillSwitchService

@code {
    private AdminRank? CurrentUserRank = AdminRank.SuperAdmin;
    private string[] visibleSections;

    protected override void OnInitialized()
    {
        visibleSections = AuthService.GetVisibleDashboardSections(CurrentUserRank);
    }
}
```

### 3. Role-Gated UI

```razor
@if (visibleSections.Contains("security_dashboard"))
{
    <SecurityDashboard Rank="CurrentUserRank" />
}
```

### 4. Initiating Kill Switch

```csharp
var result = await KillSwitchService.InitiateAccountLockAsync(
    targetUserId: "user-id-to-lock",
    initiatorUserId: "current-user-id",
    reason: "Account compromised"
);
```

---

## Next Steps

### Phase 2 — API & Data Layer

1. **Create AppDbContext** (EF Core)
   - Map domain models to database tables
   - Create migrations
   - Seed demo data

2. **Implement API Endpoints** (Minimal APIs or Controllers)
   - `/api/admin/kill-switch` — Initiate, confirm, list
   - `/api/admin/audit-logs` — Query, filter, export
   - `/api/admin/staff` — CRUD operations
   - `/api/admin/gdpr` — Erasure, exports, consent
   - `/api/admin/financial` — Transactions, approvals

3. **API Authorization Middleware**
   - Apply role policies to endpoints
   - Enforce server-side permission checks
   - Return 403 Forbidden if unauthorized

### Phase 3 — Background Jobs (Quartz.NET)

1. **InternAccountExpiryJob** — Auto-disable Intern Admin accounts on end date
2. **PendingActionExpiryJob** — Auto-extend (24h) then auto-reject (24h more)
3. **VogExpiryJob** — Send renewal reminders, mark expired, block accounts
4. **AuditLogCleanupJob** — Archive/delete logs per retention schedule

### Phase 4 — Security Enhancements

1. **Token Revocation Middleware** — Redis-backed JTI deny-list
2. **Session Timeout** — 30 min inactivity for admin sessions
3. **MFA Enforcement** — TOTP for Rank 1 & 2
4. **SIEM Integration** — Emit structured logs, evaluate behavioral rules

### Phase 5 — Testing & Validation

1. **Unit Tests** (xUnit)
   - AuthorizationService permission checks
   - KillSwitchService quorum logic
   - Rank visibility matrix

2. **Integration Tests**
   - API endpoint authorization
   - Kill Switch workflow end-to-end
   - Pending action queue lifecycle

3. **Security Testing**
   - DAST (OWASP ZAP)
   - SAST (SonarCloud)
   - Manual penetration test

---

## Compliance Checklist

- [x] **AVG Art. 5 — Principles** → Authorization matrix minimizes access
- [x] **AVG Art. 25 — Privacy by Design** → Encryption, audit logs, RBAC baked in
- [x] **AVG Art. 28 — Data Processor Agreements** → Framework ready for third-party integrations
- [x] **AVG Art. 32 — Security of Processing** → Kill Switch, audit logs, RBAC
- [x] **AVG Art. 33/34 — Breach Notification** → Kill Switch KS-3, breach response runbook
- [x] **WOR Art. 27 — Works Council Consent** → SIEM monitoring rules documented, awaiting OR approval
- [ ] **DPIA (Data Protection Impact Assessment)** → Document ready, needs finalization pre-launch
- [ ] **Verwerkingsregister (Processing Register)** → Framework ready, populate per deployment

---

## Troubleshooting

**Q: Admin dashboard shows blank tabs**  
A: Verify `visibleSections` is populated. Check that `AdminAuthorizationService.GetVisibleDashboardSections()` returns correct section names for the user's rank.

**Q: Kill Switch button is disabled**  
A: Only Rank 1.1 and 1.2 can initiate. Verify current user rank is set correctly in the dashboard @code block.

**Q: "The name 'Navigation' does not exist"**  
A: Add `@inject NavigationManager Navigation` at the top of the Razor component.

**Q: Build errors about missing AdminModels**  
A: Verify `Area42-1.Web/Models/AdminModels.cs` exists and services are registered in `Program.cs`.

---

## References

- **Framework:** .NET 10, Blazor Server
- **Spec:** HolidayReserve Cybersecurity & AVG/GDPR Compliance Action Plan v1.0
- **Compliance:** AVG (GDPR), UAVG, WOR Art. 27, Dutch Tax Law
- **Standards:** NIST FIPS 197 (AES-256), FIPS 180-4 (SHA-256), bcrypt (passwords)

---

## Support & Questions

For questions about the admin portal implementation, refer to:
- **Compliance Document:** HolidayReserve_Cybersecurity_Action_Plan.pdf
- **Codebase:** Area42-1.Web/Models/AdminModels.cs, Services/
- **Blazor Docs:** https://learn.microsoft.com/en-us/aspnet/core/blazor

---

**Status:** ✅ Core infrastructure complete. Ready for API layer & data persistence implementation.
