# Area42 Security & Architecture

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        INTERNET / USERS                         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    ┌─────────────────────┐
                    │   Load Balancer     │
                    │   (Optional)        │
                    └─────────────────────┘
           ↙                        ↘
    ┌─────────────────┐      ┌─────────────────┐
    │   DOMAIN #1     │      │   DOMAIN #2     │
    │  Customer       │      │  Admin Portal   │
    │  (Public)       │      │  (Protected)    │
    └─────────────────┘      └─────────────────┘
           │                         │
           └────────────┬────────────┘
                        ↓
            ┌──────────────────────┐
            │  API Gateway / CORS  │
            └──────────────────────┘
                        ↓
        ┌───────────────────────────────┐
        │  ASP.NET Core API Service     │
        │  (Port: 7001, HTTPS)          │
        │  ┌─────────────────────────┐  │
        │  │ JWT Auth Middleware     │  │
        │  │ CORS Policy             │  │
        │  │ Authorization Guards    │  │
        │  └─────────────────────────┘  │
        │  ┌─────────────────────────┐  │
        │  │  AuthController         │  │
        │  │  - Login                │  │
        │  │  - Register             │  │
        │  └─────────────────────────┘  │
        │  ┌─────────────────────────┐  │
        │  │  AccommodationsController  │
        │  │  - GetAll               │  │
        │  │  - GetById              │  │
        │  │  - Create (Admin)       │  │
        │  │  - Update (Admin)       │  │
        │  │  - Delete (Admin)       │  │
        │  └─────────────────────────┘  │
        │  ┌─────────────────────────┐  │
        │  │  ReservationsController │  │
        │  │  - Create               │  │
        │  │  - GetById              │  │
        │  │  - GetUserReservations  │  │
        │  │  - Cancel               │  │
        │  │  - CheckAvailability    │  │
        │  └─────────────────────────┘  │
        │              ↓                │
        │  ┌─────────────────────────┐  │
        │  │  Service Layer          │  │
        │  │  - AuthService          │  │
        │  │  - AccommodationService │  │
        │  │  - ReservationService   │  │
        │  └─────────────────────────┘  │
        │              ↓                │
        │  ┌─────────────────────────┐  │
        │  │  Repository Layer       │  │
        │  │  - UserRepository       │  │
        │  │  - AccommodationRepo    │  │
        │  │  - ReservationRepo      │  │
        │  └─────────────────────────┘  │
        └───────────────────────────────┘
                        ↓
        ┌───────────────────────────────┐
        │   Entity Framework Core       │
        │   (Database Access Layer)     │
        └───────────────────────────────┘
                        ↓
        ┌───────────────────────────────┐
        │   SQL Server Database         │
        │   ┌─────────────────────────┐ │
        │   │ Users Table             │ │
        │   │ Accommodations Table    │ │
        │   │ Reservations Table      │ │
        │   └─────────────────────────┘ │
        │   ┌─────────────────────────┐ │
        │   │ Indexes:                │ │
        │   │ - Email (Unique)        │ │
        │   │ - AccommodationId       │ │
        │   │ - UserId                │ │
        │   │ - CheckIn/CheckOut      │ │
        │   └─────────────────────────┘ │
        └───────────────────────────────┘
```

## Two-Domain Separation

### Domain 1: Customer Portal (Public)
**URL**: `https://area42-customer.example.com`
**Port (Local)**: `https://localhost:7000`

**Accessible Routes**:
- `/` - Home/Browse
- `/accommodations` - View all accommodations
- `/login` - Customer login
- `/register` - Customer registration
- `/reservations` - My bookings
- `/account` - Profile management

**API Access**:
- `GET /api/accommodations` - Read only
- `GET /api/accommodations/{id}` - Read only
- `POST /api/reservations` - Create bookings
- `GET /api/reservations/user/{userId}` - Own reservations
- `POST /api/auth/login` - Authentication
- `POST /api/auth/register` - Registration

### Domain 2: Admin Panel (Protected)
**URL**: `https://area42-admin.example.com`
**Port (Local)**: `https://localhost:7000/admin`

**Accessible Routes**:
- `/admin` - Dashboard
- `/admin/accommodations` - Manage accommodations
- `/admin/reservations` - View all reservations
- `/admin/users` - User management
- `/admin/reports` - Analytics

**API Access**:
- ALL endpoints with Admin role authorization

**Access Control**:
- Requires Admin role
- IP whitelisting (optional)
- Additional MFA (optional)

## Security Model

### 1. Authentication Flow

```
User Input (Email + Password)
        ↓
    [API]
        ↓
Validate Input ← BadRequest if invalid
        ↓
Hash Password (SHA-256)
        ↓
Compare with DB ← Unauthorized if no match
        ↓
Generate JWT Token
        ↓
Return Token to Client
        ↓
Client Stores in Session/LocalStorage
        ↓
Include in Authorization Header
```

### 2. JWT Token Structure

```
Header.Payload.Signature

Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user-id-guid",
  "email": "user@example.com",
  "role": "Customer|Admin|Staff",
  "name": "User Name",
  "exp": 1234567890,
  "iat": 1234567800,
  "iss": "Area42API",
  "aud": "Area42Client"
}

Signature:
HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  secret_key
)
```

### 3. Authorization Attributes

```csharp
// Public - No auth required
[AllowAnonymous]
public async Task<IActionResult> CheckAvailability(...)

// Authenticated - Any logged-in user
[Authorize]
public async Task<IActionResult> GetUserReservations(...)

// Role-based - Admin only
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateAccommodation(...)

// Role-based - Admin or Staff
[Authorize(Roles = "Admin,Staff")]
public async Task<IActionResult> UpdateReservation(...)
```

### 4. Request Flow with Authentication

```
Client Request
        ↓
Authorization Header: "Bearer {jwt_token}"
        ↓
    [API]
        ↓
JwtBearerMiddleware extracts token
        ↓
Verify Token Signature ← Unauthorized if invalid
        ↓
Check Token Expiration ← Unauthorized if expired
        ↓
Extract Claims (sub, email, role)
        ↓
Create ClaimsPrincipal
        ↓
[Authorize] attribute checks role
        ↓
✓ Access Granted / ✗ Forbidden
```

## Password Security

### Hashing Algorithm
- **Algorithm**: SHA-256
- **Format**: Base64 encoded hex string
- **Process**:
  1. Convert password to bytes (UTF-8)
  2. Compute SHA-256 hash
  3. Convert hash bytes to Base64 string
  4. Store in database

```csharp
private static string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(password)
        );
        return Convert.ToBase64String(hashedBytes);
    }
}
```

### Security Considerations
- ⚠️ Consider upgrading to bcrypt or Argon2 for production
- Never log passwords
- Use HTTPS only for password transmission
- Implement rate limiting on login attempts
- Add password complexity requirements

## Role-Based Access Control (RBAC)

### Role Hierarchy

```
┌─────────────────────────────────────────┐
│           Guest (Unauthenticated)       │
│                                         │
│  Permissions:                          │
│  - Browse accommodations                │
│  - Check availability                   │
│  - View accommodation details           │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│        Customer (Authenticated)         │
│        Inherits: Guest                  │
│                                         │
│  Additional Permissions:                │
│  - Create reservations                  │
│  - View own reservations                │
│  - Cancel own reservations              │
│  - Update own profile                   │
│  - Contact support                      │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│         Staff (Restricted)              │
│         Inherits: Customer              │
│                                         │
│  Additional Permissions:                │
│  - View all reservations                │
│  - Manage check-ins/outs                │
│  - View guest details                   │
│  - Generate guest reports               │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│       Admin (Full Access)               │
│       Inherits: Staff                   │
│                                         │
│  Additional Permissions:                │
│  - Create/Edit/Delete accommodations    │
│  - Create/Edit staff accounts           │
│  - View system analytics                │
│  - Configure system settings            │
│  - Manage all users                     │
│  - Access audit logs                    │
│  - Generate financial reports           │
└─────────────────────────────────────────┘
```

### Permission Matrix

| Action | Guest | Customer | Staff | Admin |
|--------|-------|----------|-------|-------|
| Browse Accommodations | ✓ | ✓ | ✓ | ✓ |
| Check Availability | ✓ | ✓ | ✓ | ✓ |
| Create Reservation | ✗ | ✓ | ✓ | ✓ |
| View Own Reservations | ✗ | ✓ | ✓ | ✓ |
| View All Reservations | ✗ | ✗ | ✓ | ✓ |
| Manage Check-in/out | ✗ | ✗ | ✓ | ✓ |
| Create Accommodation | ✗ | ✗ | ✗ | ✓ |
| Edit Accommodation | ✗ | ✗ | ✗ | ✓ |
| Delete Accommodation | ✗ | ✗ | ✗ | ✓ |
| Manage Users | ✗ | ✗ | ✗ | ✓ |
| View Analytics | ✗ | ✗ | ✗ | ✓ |

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // Production: Specify exact domains
        policy.WithOrigins(
            "https://area42-customer.example.com",
            "https://area42-admin.example.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();

        // Development: Allow localhost
        if (app.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                "https://localhost:7000",
                "https://localhost:7100"
            );
        }
    });
});
```

## Data Protection

### Sensitive Data in Transit
- ✓ All communication via HTTPS/TLS 1.2+
- ✓ No sensitive data in logs
- ✓ Tokens with 24-hour expiration
- ✓ Automatic token refresh (recommended)

### Sensitive Data at Rest
- ✓ Passwords hashed with SHA-256
- ✓ Database encryption (SQL Server)
- ✓ No PII in logs
- ✓ Backup encryption

### Input Validation
```csharp
// Server-side validation on all endpoints
if (string.IsNullOrWhiteSpace(request.Email))
    return BadRequest("Email is required");

if (request.Password.Length < 8)
    return BadRequest("Password must be at least 8 characters");

if (request.CheckOutDate <= request.CheckInDate)
    return BadRequest("Check-out must be after check-in");
```

## Audit & Logging

### Events to Log
```csharp
[Audited]
- User Registration
- User Login
- User Logout
- Password Change
- Accommodation Created
- Accommodation Modified
- Reservation Created
- Reservation Cancelled
- Admin Actions
- Failed Login Attempts
- Authorization Failures
```

### Log Format
```json
{
  "Timestamp": "2024-01-15T10:30:00Z",
  "Event": "UserLogin",
  "UserId": "guid",
  "UserEmail": "user@example.com",
  "IPAddress": "192.168.1.100",
  "Result": "Success|Failure",
  "Details": "Optional details",
  "Severity": "Info|Warning|Error"
}
```

## Production Deployment Checklist

### Security
- [ ] Enable HTTPS/TLS 1.2+
- [ ] Use strong JWT secret (minimum 256-bit)
- [ ] Implement rate limiting
- [ ] Add Web Application Firewall (WAF)
- [ ] Enable SQL injection protection
- [ ] Configure CORS for specific origins
- [ ] Implement CSRF tokens
- [ ] Add API key validation
- [ ] Enable audit logging
- [ ] Implement DDoS protection

### Infrastructure
- [ ] Configure load balancer with health checks
- [ ] Set up database backups (daily minimum)
- [ ] Implement database replication
- [ ] Configure monitoring and alerts
- [ ] Set up CDN for static assets
- [ ] Configure environment variables securely
- [ ] Implement secrets management (Key Vault)
- [ ] Enable SQL Server encryption

### Operational
- [ ] Disaster recovery plan
- [ ] Incident response procedures
- [ ] Security patch management
- [ ] Regular security audits
- [ ] Penetration testing
- [ ] Employee security training
- [ ] Data retention policies
- [ ] GDPR compliance review

---

**Version**: 1.0.0  
**Last Updated**: January 2024  
**Document Owner**: Architecture Team
