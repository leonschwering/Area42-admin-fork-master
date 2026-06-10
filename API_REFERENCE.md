# Admin Portal - API Reference Documentation

## Overview

This document provides a comprehensive reference for all admin portal API endpoints implemented in the `Area42-1.ApiService` project. All endpoints require JWT authentication and role-based authorization via the admin rank system.

**Base URL:** `https://localhost:7000/api/`

**Authentication:** JWT Bearer Token (passed in `Authorization: Bearer <token>` header)

---

## Authentication & Claims

### JWT Token Claims

The JWT token issued during admin login must include:

```json
{
  "sub": "admin-user-id",
  "rank": "AdminRank enum value (e.g., 'SuperAdmin', 'Admin')",
  "email": "admin@example.com",
  "iat": 1234567890,
  "exp": 1234571490
}
```

### Authorization Policies

| Policy | Ranks Allowed | Purpose |
|--------|---------------|---------|
| `AdminOnly` | SuperAdmin, Admin, SeniorManager | Admin dashboard and user management |
| `SuperAdminOnly` | SuperAdmin | System-level operations (kill switch execute, user creation) |
| `SecurityAdminOnly` | SuperAdmin, Admin | Kill switch and security operations |
| `FinancialAccessOnly` | All operational ranks | Submit financial transactions |
| `FinancialAuditOnly` | SeniorManager, Admin, SuperAdmin | View financial audit logs |
| `FinancialApprovalOnly` | SeniorManager, Admin, SuperAdmin | Approve/reject financial transactions |

---

## Admin Controller (`/api/admin`)

### User Management

#### GET `/users`
**Authorization:** `AdminOnly`

List all admin users with pagination.

**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Results per page

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "fullName": "John Admin",
      "email": "john@example.com",
      "rank": "Admin",
      "isEnabled": true,
      "isLocked": false,
      "lastLoginAt": "2024-01-15T10:30:00Z",
      "createdAt": "2024-01-01T09:00:00Z"
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 20
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions

---

#### GET `/users/{id}`
**Authorization:** `AdminOnly`

Get a specific admin user by ID.

**Path Parameters:**
- `id` (string) - Admin user ID

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fullName": "John Admin",
  "email": "john@example.com",
  "rank": "Admin",
  "isEnabled": true,
  "isLocked": false,
  "lastLoginAt": "2024-01-15T10:30:00Z",
  "createdAt": "2024-01-01T09:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Admin user does not exist
- `401 Unauthorized` - Missing or invalid token

---

#### POST `/users`
**Authorization:** `SuperAdminOnly`

Create a new admin user.

**Request Body:**
```json
{
  "email": "newadmin@example.com",
  "fullName": "Jane Doe",
  "rank": "Admin",
  "passwordHash": "hashed-password-or-null-for-default",
  "isEnabled": true
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "fullName": "Jane Doe",
  "email": "newadmin@example.com",
  "rank": "Admin",
  "isEnabled": true,
  "createdAt": "2024-01-16T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Email or FullName missing
- `409 Conflict` - Email already exists
- `403 Forbidden` - Only SuperAdmin can create users

---

#### PUT `/users/{id}`
**Authorization:** `AdminOnly`

Update an admin user (SuperAdmin can change rank, others can only update own non-rank fields).

**Path Parameters:**
- `id` (string) - Admin user ID

**Request Body:**
```json
{
  "fullName": "Jane Doe Updated",
  "rank": "SeniorManager",
  "isEnabled": true
}
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "fullName": "Jane Doe Updated",
  "email": "newadmin@example.com",
  "rank": "SeniorManager",
  "isEnabled": true,
  "lastLoginAt": "2024-01-15T10:30:00Z",
  "createdAt": "2024-01-16T10:00:00Z"
}
```

**Error Responses:**
- `403 Forbidden` - Non-SuperAdmin trying to change rank
- `404 Not Found` - User does not exist

---

#### DELETE `/users/{id}`
**Authorization:** `SuperAdminOnly`

Soft-delete (deactivate) an admin user.

**Path Parameters:**
- `id` (string) - Admin user ID

**Response (204 No Content)**

**Error Responses:**
- `404 Not Found` - User does not exist
- `403 Forbidden` - Only SuperAdmin can delete

---

### Audit Logs

#### GET `/audit-logs`
**Authorization:** `AdminOnly`

Retrieve immutable audit logs with optional filtering.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 50)
- `entityType` (string, optional) - Filter by entity type (e.g., "User", "Booking")
- `action` (string, optional) - Filter by action (e.g., "User.Create", "Booking.Cancel")

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "userRank": "Admin",
      "action": "AdminUser.Create",
      "entityType": "AdminUser",
      "entityId": "550e8400-e29b-41d4-a716-446655440001",
      "oldValues": null,
      "newValues": "{\"id\":\"...\",\"fullName\":\"Jane Doe\"}",
      "timestamp": "2024-01-16T10:00:00Z",
      "ipAddress": "192.168.1.100"
    }
  ],
  "total": 1250,
  "page": 1,
  "pageSize": 50
}
```

**Audit Log Actions:**
- `AdminUser.Create` - New admin created
- `AdminUser.Update` - Admin profile updated
- `AdminUser.Delete` - Admin deactivated
- `KillSwitch.Initiate` - Kill switch requested
- `KillSwitch.Confirm` - Kill switch confirmed
- `KillSwitch.Execute` - Kill switch executed
- `FinancialTransaction.SubmitForApproval` - Transaction submitted
- `FinancialTransaction.Approve` - Transaction approved
- `GDPR.ErasureRequest` - Data erasure requested

---

### Security Flags

#### GET `/security-flags`
**Authorization:** `AdminOnly`

List security flags with pagination and optional status filter.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `status` (string, optional) - Filter by status: "Pending", "Dismissed", "Escalated"

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "ruleId": "BR-01",
      "triggeredByUserId": "user-123",
      "description": "Multiple failed login attempts detected",
      "status": "Pending",
      "reviewedByAdminId": null,
      "reviewNote": null,
      "triggeredAt": "2024-01-16T09:45:00Z",
      "resolvedAt": null
    }
  ],
  "total": 15,
  "page": 1,
  "pageSize": 20
}
```

---

#### PUT `/security-flags/{id}`
**Authorization:** `AdminOnly`

Update security flag status (dismiss or escalate).

**Path Parameters:**
- `id` (Guid) - Security flag ID

**Request Body:**
```json
{
  "status": "Dismissed",
  "reviewNote": "False alarm - user was traveling internationally"
}
```

**Response (200 OK):**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "ruleId": "BR-01",
  "status": "Dismissed",
  "reviewedByAdminId": "550e8400-e29b-41d4-a716-446655440000",
  "reviewNote": "False alarm - user was traveling internationally",
  "resolvedAt": "2024-01-16T10:00:00Z"
}
```

---

## Security Controller (`/api/security`)

### Kill Switch Operations

#### POST `/kill-switch/initiate`
**Authorization:** `SecurityAdminOnly`

Initiate a kill switch request (KS-1: Account Lock, KS-2: Admin Domain, KS-3: Full System).

**Request Body:**
```json
{
  "targetUserId": "user-456",
  "level": "AccountLock",
  "reason": "Account compromised - unauthorized access detected"
}
```

**Kill Switch Levels:**
- `AccountLock` (KS-1) - Lock single user account
- `AdminDomainLock` (KS-2) - Lock all admin accounts except initiator
- `FullSystemLock` (KS-3) - Platform read-only, all sessions end

**Response (201 Created):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "level": "AccountLock",
  "status": "Pending",
  "expiresAt": "2024-01-16T10:10:00Z",
  "message": "Kill switch AccountLock initiated. Awaiting confirmation from another admin. Expires in 10 minutes."
}
```

**Error Responses:**
- `400 Bad Request` - Reason missing or rate limit exceeded (max 3 per hour)
- `401 Unauthorized` - User not identified

---

#### GET `/kill-switch/{id}`
**Authorization:** `SecurityAdminOnly`

Get kill switch request details.

**Path Parameters:**
- `id` (Guid) - Kill switch request ID

**Response (200 OK):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "targetUserId": "user-456",
  "initiatorUserId": "550e8400-e29b-41d4-a716-446655440000",
  "confirmerUserId": null,
  "level": "AccountLock",
  "status": "Pending",
  "reason": "Account compromised",
  "createdAt": "2024-01-16T10:00:00Z",
  "expiresAt": "2024-01-16T10:10:00Z",
  "confirmedAt": null,
  "executedAt": null,
  "executionLog": null
}
```

---

#### GET `/kill-switch/pending`
**Authorization:** `SecurityAdminOnly`

Get all pending kill switch requests.

**Response (200 OK):**
```json
[
  {
    "id": "880e8400-e29b-41d4-a716-446655440000",
    "level": "AccountLock",
    "status": "Pending",
    "createdAt": "2024-01-16T10:00:00Z",
    "expiresAt": "2024-01-16T10:10:00Z"
  }
]
```

---

#### POST `/kill-switch/{id}/confirm`
**Authorization:** `SecurityAdminOnly`

Confirm a kill switch request (two-person quorum - different from initiator).

**Path Parameters:**
- `id` (Guid) - Kill switch request ID

**Response (200 OK):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "level": "AccountLock",
  "status": "Confirmed",
  "expiresAt": "2024-01-16T10:10:00Z",
  "message": "Kill switch AccountLock confirmed by admin-xyz. Ready for execution."
}
```

**Error Responses:**
- `400 Bad Request` - Kill switch already expired or same user as initiator (quorum violation)
- `404 Not Found` - Request does not exist

---

#### POST `/kill-switch/{id}/execute`
**Authorization:** `SuperAdminOnly`

Execute a confirmed kill switch request.

**Path Parameters:**
- `id` (Guid) - Kill switch request ID

**Response (200 OK):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "level": "AccountLock",
  "status": "Executed",
  "expiresAt": "2024-01-16T10:10:00Z",
  "message": "Kill switch AccountLock executed. User user-456 locked until 2024-01-17T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Kill switch not in Confirmed state
- `403 Forbidden` - Only SuperAdmin can execute

---

#### POST `/kill-switch/{id}/reject`
**Authorization:** `SecurityAdminOnly`

Reject a pending kill switch request.

**Path Parameters:**
- `id` (Guid) - Kill switch request ID

**Request Body:**
```json
{
  "reason": "False alarm - issue resolved by user"
}
```

**Response (200 OK):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440000",
  "level": "AccountLock",
  "status": "Rejected",
  "message": "Kill switch request rejected: False alarm - issue resolved by user"
}
```

---

## Financial Controller (`/api/financial`)

### Approval Thresholds

#### GET `/approval-thresholds`
**Authorization:** `None` (public endpoint)

Get financial approval thresholds by rank for UI reference.

**Response (200 OK):**
```json
{
  "thresholds": [
    {
      "role": "CustomerSupport",
      "autonomousLimit": 150,
      "requiresApprovalAbove": 150
    },
    {
      "role": "BookingManager",
      "autonomousLimit": 500,
      "requiresApprovalAbove": 500
    },
    {
      "role": "PropertyManager",
      "autonomousLimit": 0,
      "requiresApprovalAbove": 0
    },
    {
      "role": "SeniorManager",
      "autonomousLimit": 2000,
      "requiresApprovalAbove": 2000
    },
    {
      "role": "Admin",
      "autonomousLimit": 2000,
      "requiresApprovalAbove": 2000
    },
    {
      "role": "SuperAdmin",
      "autonomousLimit": 2000,
      "requiresApprovalAbove": 2000
    }
  ]
}
```

---

### Transactions

#### POST `/transactions`
**Authorization:** `FinancialAccessOnly`

Submit a financial transaction (refund, adjustment, etc.).

**Request Body:**
```json
{
  "operationType": "Refund",
  "entityId": "booking-789",
  "amount": 250.00,
  "amountBefore": 1000.00,
  "amountAfter": 750.00,
  "reason": "Guest requested cancellation",
  "mfaConfirmed": false
}
```

**Response:**
- **202 Accepted** (if approval required):
```json
{
  "transactionId": "990e8400-e29b-41d4-a716-446655440000",
  "amount": 250.00,
  "status": "PendingApproval",
  "message": "Transaction requires approval from an approver",
  "requiresMfa": false
}
```

- **200 OK** (if auto-approved):
```json
{
  "transactionId": "990e8400-e29b-41d4-a716-446655440000",
  "amount": 250.00,
  "status": "Approved",
  "message": "Transaction auto-approved",
  "requiresMfa": false
}
```

**Error Responses:**
- `400 Bad Request` - Amount ≤ 0, reason missing, or MFA required for amounts > €2,000
- `401 Unauthorized` - User not identified

---

#### GET `/transactions`
**Authorization:** `FinancialAuditOnly`

Get financial audit log with pagination and filtering.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 50)
- `operationType` (string, optional) - Filter by operation (e.g., "Refund")
- `requiresApproval` (bool, optional) - Filter by approval status

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440000",
      "initiatorUserId": "550e8400-e29b-41d4-a716-446655440023",
      "approverUserId": "550e8400-e29b-41d4-a716-446655440024",
      "operationType": "Refund",
      "entityId": "booking-789",
      "amountBefore": 1000.00,
      "amountAfter": 750.00,
      "reason": "Guest requested cancellation",
      "timestamp": "2024-01-16T10:00:00Z",
      "ipAddress": "192.168.1.100",
      "mfaConfirmed": false
    }
  ],
  "total": 523,
  "page": 1,
  "pageSize": 50
}
```

---

#### POST `/transactions/{id}/approve`
**Authorization:** `FinancialApprovalOnly`

Approve a pending financial transaction.

**Path Parameters:**
- `id` (Guid) - Transaction ID

**Request Body:**
```json
{
  "mfaConfirmed": true
}
```

**Response (200 OK):**
```json
{
  "transactionId": "990e8400-e29b-41d4-a716-446655440000",
  "amount": 250.00,
  "status": "Approved",
  "message": "Transaction approved by admin-xyz",
  "requiresMfa": false
}
```

**Error Responses:**
- `400 Bad Request` - Transaction already processed, MFA required for amounts > €2,000
- `403 Forbidden` - Non-SuperAdmin approving transaction > €2,000, or self-approval of > €2,000

---

#### POST `/transactions/{id}/reject`
**Authorization:** `FinancialApprovalOnly`

Reject a pending financial transaction.

**Path Parameters:**
- `id` (Guid) - Transaction ID

**Request Body:**
```json
{
  "reason": "Insufficient documentation"
}
```

**Response (200 OK):**
```json
{
  "transactionId": "990e8400-e29b-41d4-a716-446655440000",
  "amount": 250.00,
  "status": "Rejected",
  "message": "Transaction rejected: Insufficient documentation",
  "requiresMfa": false
}
```

---

## GDPR Controller (`/api/gdpr`)

### Erasure Requests

#### POST `/erasure-requests`
**Authorization:** `None` (public endpoint)

Submit a GDPR data erasure request (Right to be forgotten - Art. 17).

**Request Body:**
```json
{
  "userId": "user-456",
  "reason": "User wants to delete personal data"
}
```

**Response (201 Created):**
```json
{
  "requestId": "aa0e8400-e29b-41d4-a716-446655440000",
  "userId": "user-456",
  "status": "Pending",
  "requestedAt": "2024-01-16T10:00:00Z",
  "dueAt": "2024-02-15T10:00:00Z",
  "message": "Erasure request created. Due date: 2024-02-15. SLA: 30 days."
}
```

**SLA:** 30 days from submission (GDPR Art. 12(3))

---

#### GET `/erasure-requests/{id}`
**Authorization:** `None` (public endpoint)

Get erasure request details.

**Path Parameters:**
- `id` (Guid) - Erasure request ID

**Response (200 OK):**
```json
{
  "id": "aa0e8400-e29b-41d4-a716-446655440000",
  "userId": "user-456",
  "requestedAt": "2024-01-16T10:00:00Z",
  "dueAt": "2024-02-15T10:00:00Z",
  "status": "Pending",
  "completedByAdminId": null,
  "completedAt": null,
  "notes": null
}
```

---

#### GET `/erasure-requests`
**Authorization:** `AdminOnly`

List all GDPR erasure requests with optional status filtering.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `status` (string, optional) - Filter by status: "Pending", "InProgress", "Completed", "Failed"

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440000",
      "userId": "user-456",
      "requestedAt": "2024-01-16T10:00:00Z",
      "dueAt": "2024-02-15T10:00:00Z",
      "status": "Pending",
      "completedByAdminId": null,
      "completedAt": null,
      "notes": null
    }
  ],
  "total": 12,
  "page": 1,
  "pageSize": 20
}
```

---

#### POST `/erasure-requests/{id}/approve`
**Authorization:** `AdminOnly`

Approve and execute a GDPR erasure request.

**Path Parameters:**
- `id` (Guid) - Erasure request ID

**Request Body:**
```json
{
  "executionNotes": "Pseudonymized user records and soft-deleted PII"
}
```

**Response (200 OK):**
```json
{
  "requestId": "aa0e8400-e29b-41d4-a716-446655440000",
  "userId": "user-456",
  "status": "InProgress",
  "requestedAt": "2024-01-16T10:00:00Z",
  "dueAt": "2024-02-15T10:00:00Z",
  "message": "Erasure request approved and queued for execution. Status: InProgress"
}
```

---

#### POST `/erasure-requests/{id}/reject`
**Authorization:** `AdminOnly`

Reject a GDPR erasure request.

**Path Parameters:**
- `id` (Guid) - Erasure request ID

**Request Body:**
```json
{
  "rejectionReason": "User identity not verified"
}
```

**Response (200 OK):**
```json
{
  "requestId": "aa0e8400-e29b-41d4-a716-446655440000",
  "userId": "user-456",
  "status": "Failed",
  "requestedAt": "2024-01-16T10:00:00Z",
  "dueAt": "2024-02-15T10:00:00Z",
  "message": "Erasure request rejected: User identity not verified"
}
```

---

### Consent Logs

#### POST `/consent`
**Authorization:** `None` (public endpoint)

Record user consent (Marketing, Analytics, Cookies, TermsOfService) - GDPR Art. 7.

**Request Body:**
```json
{
  "userId": "user-456",
  "consentType": "Marketing",
  "granted": true,
  "privacyVersionAccepted": "1.0.0"
}
```

**Consent Types:**
- `Marketing` - Email marketing communications
- `Analytics` - Usage tracking and analytics
- `Cookies` - Cookie storage
- `TermsOfService` - Terms of Service acceptance

**Response (201 Created):**
```json
{
  "consentId": "bb0e8400-e29b-41d4-a716-446655440000",
  "consentType": "Marketing",
  "granted": true,
  "recordedAt": "2024-01-16T10:00:00Z",
  "message": "Consent for Marketing recorded: Granted"
}
```

---

#### GET `/consent/{id}`
**Authorization:** `None` (public endpoint)

Get consent log details.

**Path Parameters:**
- `id` (Guid) - Consent log ID

**Response (200 OK):**
```json
{
  "id": "bb0e8400-e29b-41d4-a716-446655440000",
  "userId": "user-456",
  "consentType": "Marketing",
  "granted": true,
  "recordedAt": "2024-01-16T10:00:00Z",
  "ipAddress": "192.168.1.100",
  "privacyVersionAccepted": "1.0.0"
}
```

---

#### GET `/consent/user/{userId}`
**Authorization:** `None` (public endpoint)

Get all consent logs for a specific user.

**Path Parameters:**
- `userId` (string) - User ID

**Response (200 OK):**
```json
[
  {
    "id": "bb0e8400-e29b-41d4-a716-446655440000",
    "userId": "user-456",
    "consentType": "Marketing",
    "granted": true,
    "recordedAt": "2024-01-16T10:00:00Z",
    "privacyVersionAccepted": "1.0.0"
  },
  {
    "id": "bb0e8400-e29b-41d4-a716-446655440001",
    "userId": "user-456",
    "consentType": "Analytics",
    "granted": false,
    "recordedAt": "2024-01-16T09:45:00Z",
    "privacyVersionAccepted": "1.0.0"
  }
]
```

---

### Compliance Status

#### GET `/compliance-status`
**Authorization:** `AdminOnly`

Get GDPR compliance status dashboard with overdue tracking.

**Response (200 OK):**
```json
{
  "totalErasureRequests": 45,
  "pendingErasures": 12,
  "overdueErasures": 2,
  "totalConsents": 1250,
  "recentConsents": 450,
  "totalAuditLogs": 5840,
  "gdprAuditLogs": 320,
  "complianceCheckDate": "2024-01-16T10:00:00Z",
  "message": "WARNING: 2 overdue erasure requests"
}
```

---

## Error Handling

### Standard Error Response Format

All errors return the following format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email and FullName are required",
  "instance": "/api/admin/users"
}
```

### Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200 OK` | Request successful |
| `201 Created` | Resource created successfully |
| `202 Accepted` | Request accepted, pending further action |
| `204 No Content` | Request successful, no content returned |
| `400 Bad Request` | Invalid request format or parameters |
| `401 Unauthorized` | Missing or invalid authentication |
| `403 Forbidden` | Authenticated but insufficient permissions |
| `404 Not Found` | Resource does not exist |
| `409 Conflict` | Resource already exists (e.g., duplicate email) |
| `500 Internal Server Error` | Server error during processing |

---

## Rate Limiting & Security

### Kill Switch Rate Limiting
- **Maximum:** 3 pending requests per admin per hour
- **Expiry:** 10 minutes (auto-expire if not confirmed)
- **Quorum:** Must be confirmed by different admin

### Financial Transaction Limits
- **MFA Required:** Transactions over €2,000
- **Self-Approval Block:** Cannot self-approve amounts > €2,000
- **Super Admin Required:** Only SuperAdmin can approve > €2,000

### GDPR SLA
- **Erasure SLA:** 30 days from request (Art. 12(3))
- **Overdue Tracking:** Compliance dashboard alerts on overdue requests

---

## Integration Examples

### Example 1: Create Admin User (SuperAdmin)

```bash
curl -X POST https://localhost:7000/api/admin/users \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newadmin@example.com",
    "fullName": "Jane Doe",
    "rank": "Admin",
    "isEnabled": true
  }'
```

### Example 2: Initiate Kill Switch (Admin)

```bash
curl -X POST https://localhost:7000/api/security/kill-switch/initiate \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "targetUserId": "user-456",
    "level": "AccountLock",
    "reason": "Account compromised"
  }'
```

### Example 3: Approve Financial Transaction (SeniorManager)

```bash
curl -X POST https://localhost:7000/api/financial/transactions/990e8400-e29b-41d4-a716-446655440000/approve \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "mfaConfirmed": true
  }'
```

---

## Next Steps

1. **Database Migrations** - Run EF Core migrations to create tables
2. **Token Generation** - Implement JWT token generation in AuthController
3. **Middleware** - Add claims transformation middleware for rank injection
4. **Tests** - Create integration tests for all endpoints
5. **Background Jobs** - Implement Quartz.NET for task expiry and cleanup
