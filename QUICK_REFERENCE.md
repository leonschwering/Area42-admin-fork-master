# Area42 Quick Reference Guide

## 🚀 Quick Start Commands

### Setup (First Time)
```powershell
# Open Package Manager Console in Visual Studio
# Make sure "Area42-1.ApiService" is selected as Default Project

Add-Migration InitialCreate
Update-Database

# Press F5 to run both services
```

### Run Services

**Option 1: Visual Studio (Recommended)**
```
Right-click Solution → Properties
Multi-startup projects:
- Area42-1.ApiService (Start)
- Area42-1.Web (Start)
Press F5
```

**Option 2: Command Line**
```bash
# Terminal 1: API Service
cd Area42-1.ApiService
dotnet run
# Runs at https://localhost:7001

# Terminal 2: Web App
cd Area42-1.Web
dotnet run
# Runs at https://localhost:7000
```

---

## 📡 API Quick Reference

### Base URL
```
https://localhost:7001/api
```

### Authentication Endpoints

**Register**
```bash
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}

# Response (201 Created):
{
  "success": true,
  "token": "eyJhbGc...",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Customer"
  }
}
```

**Login**
```bash
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}

# Response (200 OK):
{
  "success": true,
  "token": "eyJhbGc...",
  "user": { ... }
}
```

### Accommodations Endpoints

**Get All**
```bash
GET /accommodations
# No auth required

# Response (200 OK):
[
  {
    "id": "guid",
    "name": "Cozy Bungalow",
    "type": "Bungalow",
    "maxGuests": 4,
    "bedrooms": 2,
    "bathrooms": 1,
    ...
  }
]
```

**Get By Type**
```bash
GET /accommodations/type/Bungalow
# Types: Bungalow, Chalet, CampingSite

# Response: Array of accommodations of that type
```

**Get By ID**
```bash
GET /accommodations/{id}

# Response (200 OK):
{
  "id": "guid",
  "name": "...",
  ...
}
```

**Create** (Admin only)
```bash
POST /accommodations
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Luxury Villa",
  "description": "Beautiful villa with pool",
  "type": 0,  // 0=Bungalow, 1=Chalet, 2=CampingSite
  "maxGuests": 6,
  "bedrooms": 3,
  "bathrooms": 2,
  "imageUrl": "https://...",
  "isActive": true
}

# Response (201 Created)
```

**Update** (Admin only)
```bash
PUT /accommodations/{id}
Authorization: Bearer {token}
Content-Type: application/json

{ /* full accommodation object */ }

# Response (200 OK)
```

**Delete** (Admin only)
```bash
DELETE /accommodations/{id}
Authorization: Bearer {token}

# Response (204 No Content)
```

### Reservations Endpoints

**Create Reservation**
```bash
POST /reservations
Authorization: Bearer {token}
Content-Type: application/json

{
  "accommodationId": "guid",
  "userId": "guid",
  "checkInDate": "2024-03-15",
  "checkOutDate": "2024-03-22",
  "numberOfGuests": 4,
  "guestName": "John Doe",
  "guestEmail": "john@example.com",
  "guestPhone": "+31612345678",
  "specialRequests": "High floor preferred"
}

# Response (201 Created):
{
  "id": "guid",
  "accommodationId": "guid",
  "userId": "guid",
  "checkInDate": "2024-03-15",
  "checkOutDate": "2024-03-22",
  "numberOfGuests": 4,
  "totalPrice": 1400.00,
  "status": "Pending",
  ...
}
```

**Get Reservation**
```bash
GET /reservations/{id}
Authorization: Bearer {token}

# Response (200 OK): Reservation object
```

**Get User's Reservations**
```bash
GET /reservations/user/{userId}
Authorization: Bearer {token}

# Response (200 OK): Array of reservations
```

**Check Availability**
```bash
GET /reservations/availability/check?accommodationId={id}&checkIn={date}&checkOut={date}

# Dates format: 2024-03-15 or ISO 8601
# No auth required

# Response (200 OK):
{
  "available": true  // or false
}
```

**Cancel Reservation**
```bash
POST /reservations/{id}/cancel
Authorization: Bearer {token}

# Response (204 No Content)
```

---

## 🗄️ Database Commands

### Using Package Manager Console

```powershell
# Show migrations
Get-Migration

# Add new migration
Add-Migration YourMigrationName -Project Area42-1.ApiService

# Update database
Update-Database

# Revert last migration
Update-Database -Migration PreviousMigrationName

# Remove last migration (not applied)
Remove-Migration

# Script migration to SQL
Script-Migration -From InitialCreate -To YourMigrationName
```

### Using Entity Framework CLI

```bash
# Install EF CLI globally
dotnet tool install --global dotnet-ef

# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop

# Show connection string
dotnet ef dbcontext info
```

---

## 🔐 Testing as Different Roles

### Create Test Users

```bash
# 1. Register Admin
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@area42.nl",
    "firstName": "Admin",
    "lastName": "User",
    "password": "Admin123!",
    "confirmPassword": "Admin123!"
  }'

# Note: This creates as Customer role by default
# To create Admin, manually update database:
# UPDATE Users SET Role = 1 WHERE Email = 'admin@area42.nl'
# (0=Customer, 1=Admin, 2=Staff)

# 2. Register Customer
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@area42.nl",
    "firstName": "John",
    "lastName": "Doe",
    "password": "Customer123!",
    "confirmPassword": "Customer123!"
  }'
```

### Get Tokens

```bash
# Admin Login
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@area42.nl", "password": "Admin123!"}'

# Customer Login
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "customer@area42.nl", "password": "Customer123!"}'
```

### Use Tokens

```bash
# Copy token from response, then use in requests:
curl -X GET https://localhost:7001/api/accommodations \
  -H "Authorization: Bearer <YOUR_TOKEN_HERE>"
```

---

## 🎨 UI Accessibility

### Customer Portal URLs
- **Home**: https://localhost:7000/
- **Accommodations**: https://localhost:7000/accommodations
- **Login**: https://localhost:7000/login
- **Register**: https://localhost:7000/register
- **My Bookings**: https://localhost:7000/reservations

### Admin Portal URLs
- **Dashboard**: https://localhost:7000/admin
- **(Others not yet implemented in UI)**

---

## 🐛 Debugging Tips

### Enable Detailed Logging
```csharp
// In Program.cs
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});
```

### Common Issues

**Issue: Port 7001 already in use**
```powershell
# Find process
netstat -ano | findstr :7001

# Kill process
taskkill /PID {PID} /F
```

**Issue: Database locked**
```powershell
# In Package Manager Console
Drop-Database -Force
Update-Database
```

**Issue: JWT token invalid**
```
✓ Include "Bearer " prefix in header
✓ Check token hasn't expired (24 hours)
```

---

## 🔐 Unified Login System

### Overview
- **Single Login Page:** `/login`
- **Single API Endpoint:** `POST /api/auth/login`
- **Admin Recognition:** Email ends with `@area42.nl`
- **Role-Based Routing:** Admin → `/admin`, Customer → `/`

### Test Accounts

**Customer (Test)**
```
Email: customer@example.com
Password: <any valid password>
Expected Result: Redirect to / (home page)
```

**Admin (Test)**
```
Email: admin@area42.nl
Password: <valid admin password>
Expected Result: Redirect to /admin (admin dashboard)
```

### Login Response Format
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGc...",
  "user": {
    "id": "user-id",
    "email": "user@example.com"
  },
  "isAdmin": false,
  "userRank": null
}
```

**For Admin:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGc...",
  "user": {
    "id": "admin-id",
    "email": "admin@area42.nl"
  },
  "isAdmin": true,
  "userRank": "SuperAdmin"
}
```

### Key Files
| File | Purpose |
|------|---------|
| `Area42-1.ApiService/Services/AuthService.cs` | Email domain routing & JWT generation |
| `Area42-1.ApiService/Data/Repositories/AdminRepository.cs` | Admin user lookups |
| `Area42-1.Web/Components/Pages/Login.razor` | Login form & post-login routing |
| `Area42-1.Web/CustomAuthStateProvider.cs` | JWT storage & claims parsing |
| `Area42-1.Web/Components/Layout/NavMenu.razor` | Auth-aware navigation |

### Troubleshooting

**Admin login goes to home instead of admin panel**
- Verify email ends with `@area42.nl`
- Check JWT token contains `isAdmin: true` claim
- Clear browser localStorage and try again

**Admin link missing in NavMenu**
- Check browser console for errors
- Verify localStorage has `is_admin` flag set
- Inspect JWT token in developer tools

**"Contact administrator" error on login**
- Check AdminUsers table: account must have `IsEnabled=true` and `IsLocked=false`
- Verify password is correct
✓ Check JWT key in appsettings.json
✓ Verify token format: Header.Payload.Signature
```

**Issue: CORS errors**
```
✓ Check ApiUrl in Web's appsettings.json
✓ Verify CORS policy in API's Program.cs
✓ Check browser console for specific origin
```

---

## 📊 Pricing Quick Reference

### Bungalow
- Base: €150/night
- Extra guest: €25/night
- Weekend: ×1.2
- Special: None

### Chalet
- Base: €200/night
- Extra guest: €35/night
- Weekend: ×1.15
- Special: -10% for 7+ nights

### Camping Site
- Base: €35/night
- Extra guest: €5/night
- Weekend: ×1.3
- Special: -15% for 30+ nights

**Example Calculation (Chalet)**:
- 7 nights, March 15-22 (includes weekend)
- 4 guests (2 base + 2 extra)
- Base: €200 × 7 = €1,400
- Surcharge: €35 × 2 × 7 = €490
- Subtotal: €1,890
- Weekend: ×1.15 = €2,173.50
- Weekly Discount: ×0.9 = €1,956.15
- **Total: €1,956.15**

---

## 📚 File Locations

| Purpose | Location |
|---------|----------|
| API Startup | `Area42-1.ApiService/Program.cs` |
| Web Startup | `Area42-1.Web/Program.cs` |
| Models | `Area42-1.ApiService/Models/` |
| Database Context | `Area42-1.ApiService/Data/Area42Context.cs` |
| Repositories | `Area42-1.ApiService/Data/Repositories/` |
| Services | `Area42-1.ApiService/Services/` |
| API Controllers | `Area42-1.ApiService/Controllers/` |
| UI Components | `Area42-1.Web/Components/Pages/` |
| Styling | `Area42-1.Web/wwwroot/css/app.css` |
| API Clients | `Area42-1.Web/Services/` |
| Config (API) | `Area42-1.ApiService/appsettings.json` |
| Config (Web) | `Area42-1.Web/appsettings.json` |

---

## 🚀 Deployment Quick Commands

### Docker Build
```bash
# Build images
docker-compose build

# Run containers
docker-compose up -d

# Stop containers
docker-compose down
```

### Azure Deployment
```bash
# Create resource group
az group create --name area42-rg --location eastus

# Deploy API
az webapp up --name area42api --resource-group area42-rg

# Deploy Web
az webapp up --name area42web --resource-group area42-rg
```

---

## 📖 Documentation Files

| Document | Purpose |
|----------|---------|
| README.md | Full project overview and API reference |
| GETTING_STARTED.md | Setup instructions and quick start |
| ARCHITECTURE_AND_SECURITY.md | System design and security model |
| IMPLEMENTATION_SUMMARY.md | What was built and next steps |
| This File | Quick reference commands |

---

**Quick Reference Version**: 1.0.0
**Last Updated**: January 2024
**Status**: Ready to Use ✅
