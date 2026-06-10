# Getting Started with Area42 Reservation System

## Quick Start Guide

### Step 1: Prerequisites Installation
- Install [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Install [Visual Studio 2026](https://visualstudio.microsoft.com/) (Community Edition is fine)
- Have SQL Server running (LocalDB is included with Visual Studio)

### Step 2: Clone & Open Project
```bash
cd C:\Users\LAURE\source\repos\Area42-1\
```

Open `Area42-1.sln` in Visual Studio.

### Step 3: Database Setup

Open **Package Manager Console** in Visual Studio:
```powershell
# Set Default Project to Area42-1.ApiService
Add-Migration InitialCreate
Update-Database
```

This creates the database with all necessary tables.

### Step 4: Run Both Services

**Option A: Multi-Startup (Recommended)**
1. Right-click Solution → Properties
2. Select "Multiple startup projects"
3. Set both `Area42-1.ApiService` and `Area42-1.Web` to "Start"
4. Press F5

**Option B: Run Separately**

Terminal 1 - API Service:
```bash
cd Area42-1.ApiService
dotnet run
# API runs at https://localhost:7001
```

Terminal 2 - Web Application:
```bash
cd Area42-1.Web
dotnet run
# Web runs at https://localhost:7000
```

### Step 5: Access the Application
- **Web**: https://localhost:7000
- **API Docs**: https://localhost:7001/openapi/v1.json

## Create Test Data

### Via API using Postman or curl:

**1. Create Admin User (Login)**
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@area42.nl",
    "firstName": "Admin",
    "lastName": "User",
    "password": "Admin123!",
    "confirmPassword": "Admin123!"
  }'
```

**2. Create Customer User (Login)**
```bash
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

**3. Login to Get Token**
```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@area42.nl",
    "password": "Customer123!"
  }'
```

Response will include a JWT token. Copy it.

**4. Create Accommodation (with token)**
```bash
curl -X POST https://localhost:7001/api/accommodations \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Cozy Mountain Bungalow",
    "description": "Beautiful bungalow with mountain views",
    "type": 0,
    "maxGuests": 4,
    "bedrooms": 2,
    "bathrooms": 1,
    "imageUrl": "https://via.placeholder.com/300x200?text=Bungalow",
    "isActive": true
  }'
```

Note: `type` values: 0=Bungalow, 1=Chalet, 2=CampingSite

**5. Create Reservation**
```bash
curl -X POST https://localhost:7001/api/reservations \
  -H "Authorization: Bearer CUSTOMER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "accommodationId": "ACCOMMODATION_ID_FROM_STEP_4",
    "userId": "CUSTOMER_USER_ID",
    "checkInDate": "2024-03-15",
    "checkOutDate": "2024-03-22",
    "numberOfGuests": 3,
    "guestName": "John Doe",
    "guestEmail": "john@example.com",
    "guestPhone": "+31612345678",
    "specialRequests": "High floor preferred"
  }'
```

## Architecture Overview

### Two-Domain Setup

#### Customer Portal (`/`)
- Browse accommodations
- Make reservations
- View booking history
- Manage account

#### Admin Panel (`/admin`)
- Manage accommodations
- View all reservations
- User management
- System analytics

### Technology Stack
| Layer | Technology |
|-------|-----------|
| Frontend | Blazor Server (.NET 10) |
| Backend API | ASP.NET Core Web API (.NET 10) |
| Database | SQL Server / LocalDB |
| ORM | Entity Framework Core 10.0 |
| Authentication | JWT Bearer Tokens |
| Styling | Custom CSS with dark/light theme |

## Key Design Decisions

### 1. Strategy Pattern for Pricing
Each accommodation type implements `IPricingStrategy`:
- Different base prices
- Custom surcharges
- Seasonal adjustments
- Easy to extend with new types

### 2. Repository Pattern
Abstract data access:
- Unit testable services
- Easy to swap database implementations
- Cleaner separation of concerns

### 3. JWT Authentication
- Stateless authentication
- Secure token-based API calls
- Easy to scale horizontally
- Works across domains

### 4. Role-Based Access Control
Three roles with distinct permissions:
- **Customer**: Read accommodations, manage own bookings
- **Admin**: Full system access
- **Staff**: View and manage guest check-ins

## Database Schema Quick Reference

### Users Table
```sql
- Id (GUID, PK)
- Email (unique)
- FirstName
- LastName
- PasswordHash
- Role (Customer, Admin, Staff)
- IsActive
- CreatedAt, UpdatedAt
```

### Accommodations Table
```sql
- Id (GUID, PK)
- Name
- Description
- Type (0=Bungalow, 1=Chalet, 2=CampingSite)
- MaxGuests
- Bedrooms
- Bathrooms
- ImageUrl
- IsActive
- CreatedAt, UpdatedAt
```

### Reservations Table
```sql
- Id (GUID, PK)
- AccommodationId (FK)
- UserId (FK)
- CheckInDate
- CheckOutDate
- NumberOfGuests
- TotalPrice
- Status (Pending, Confirmed, CheckedIn, CheckedOut, Cancelled)
- GuestName
- GuestEmail
- GuestPhone
- SpecialRequests
- CreatedAt, UpdatedAt
```

**Indexes:**
- (AccommodationId, CheckInDate, CheckOutDate) - Availability checks
- UserId - User reservations lookup

## Configuration Files

### Area42-1.ApiService/appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Area42;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "Area42ReservationSystemSecretKeyMustBeAtLeast32Characters1234567890",
    "Issuer": "Area42API",
    "Audience": "Area42Client",
    "ExpirationMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Area42-1.Web/appsettings.json
```json
{
  "ApiUrl": "https://localhost:7001",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## UI/UX Guidelines

### Color System
```css
Primary Blue:    #003580  /* Links, CTAs, primary actions */
Accent Green:    #00c0a3  /* Success, highlights, confirmations */
Secondary Blue:  #0066cc  /* Hover states, secondary actions */
Text Light:      #333333  /* Light mode text */
Text Dark:       #e0e0e0  /* Dark mode text */
Border Light:    #e0e0e0  /* Light mode borders *)
Border Dark:     #404040  /* Dark mode borders */
```

### Typography
- **Headings**: 600 font-weight, system fonts
- **Body**: 400 font-weight, 16px base size
- **Code**: Monospace for technical content

### Responsive Breakpoints
- Mobile: < 480px
- Tablet: 480px - 768px
- Desktop: > 768px

## Troubleshooting

### Port Already in Use
```bash
# Find process on port 7001
netstat -ano | findstr :7001

# Kill process
taskkill /PID {PID} /F
```

### Database Lock Issues
```powershell
# In Package Manager Console
[System.Reflection.Assembly]::LoadFrom("C:\Program Files\dotnet\...").GetName().Version

# Reset database
Drop-Database -Force
Update-Database
```

### JWT Token Errors
Ensure:
1. Token is passed in Authorization header: `Authorization: Bearer {token}`
2. Token hasn't expired (24-hour default expiration)
3. API secret key matches in appsettings.json

### CORS Issues
Check that `ApiUrl` in Web's appsettings.json matches actual API URL.

## Performance Tips

1. **Database Queries**
   - Availability checks use indexed columns
   - Consider caching accommodation lists (Redis)

2. **API Optimization**
   - Implemented async/await throughout
   - Entity Framework queries are tracked/untracked appropriately

3. **Frontend**
   - CSS is minified in production
   - Lazy loading for images
   - Pagination ready for large result sets

## Development Workflow

1. Create a feature branch
2. Make changes to models/services
3. Create EF migration if schema changed: `Add-Migration YourMigrationName`
4. Test locally
5. Run build to verify: `dotnet build`
6. Commit and push

## Next Steps

1. ✅ Set up database
2. ✅ Create test data
3. ✅ Test API endpoints
4. ✅ Explore admin dashboard
5. 📋 Add payment integration
6. 📋 Implement email notifications
7. 📋 Add guest reviews
8. 📋 Deploy to Azure

---

For detailed API documentation, see [README.md](README.md)
