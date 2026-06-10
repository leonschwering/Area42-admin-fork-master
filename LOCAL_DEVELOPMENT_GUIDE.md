# Local Development Setup Guide for Area42

## Quick Start

### Option 1: Using Aspire (Recommended for local development)

The solution uses .NET Aspire for local development orchestration.

1. **Start the AppHost project**:
   ```
   cd Area42-1.AppHost
   dotnet run
   ```

   This automatically starts:
   - API Service on `https://localhost:7001` (or assigned port)
   - Web Frontend on `https://localhost:7000` (or assigned port)

2. **Access the application**:
   - Web UI: https://localhost:7000
   - API: https://localhost:7001
   - Admin Dashboard: https://localhost:7000/admin

### Option 2: Running Services Separately

If Aspire doesn't work or you prefer manual startup:

**Terminal 1 - API Service:**
```bash
cd Area42-1.ApiService
dotnet run
# API runs on https://localhost:7001 and http://localhost:5000
```

**Terminal 2 - Web Frontend:**
```bash
cd Area42-1.Web
dotnet run
# Web runs on https://localhost:7000 and http://localhost:5001
```

### Option 3: Using Docker Compose

For a more realistic environment:

```bash
# Start all services (API, Web, SQL Server)
docker-compose up

# Access the application
# Web: http://localhost:3000
# API: http://localhost:7001

# Stop services
docker-compose down
```

## Test Accounts

### Admin Accounts

| Email | Password | Role |
|-------|----------|------|
| superadmin@area42.nl | SuperAdmin@123 | Super Admin |
| admin1@area42.nl | Admin@123 | Admin |
| propertymanager@area42.nl | Property@111 | Property Manager |
| bookingmanager@area42.nl | Booking@222 | Booking Manager |
| support@area42.nl | Support@333 | Customer Support |
| hrmanager@area42.nl | HRManager@444 | HR Manager |

### Customer Accounts

| Email | Password | Role |
|-------|----------|------|
| guest1@example.com | Guest@123 | Customer |
| guest2@example.com | Guest@456 | Customer |
| staff@area42.nl | Staff@789 | Staff |

## Admin Dashboard Features

- **Admin Dashboard** (Super Admin only)
- **Security Dashboard** - Audit logs, kill switches, security flags
- **Staff Management** - Employee records, attendance
- **Financial Reports** - Revenue analysis, expense tracking
- **Property Management** - Accommodation CRUD
- **Booking Management** - Reservation system
- **GDPR Tools** - Data export, anonymization

## Database

### Connection String (Local)
```
Server=(localdb)\mssqllocaldb;Database=Area42;Trusted_Connection=true;
```

### Seeded Data

On first run, the database is automatically populated with:
- 8 Admin users with different roles
- 3 Regular users (2 customers + 1 staff)
- 6 Sample accommodations

### Database Migrations

Migrations run automatically on startup. To manually migrate:

```bash
cd Area42-1.ApiService
dotnet ef database update --context Area42Context
```

## Development Ports

| Service | HTTP | HTTPS |
|---------|------|-------|
| Web Frontend | 5001 | 7000 |
| API Service | 5000 | 7001 |
| SQL Server | 1433 | - |

## Troubleshooting

### Connection Refused Error

If you see `ERR_CONNECTION_REFUSED` when accessing admin:

1. **Check if both services are running**:
   ```bash
   # Check if API is listening on port 7001
   netstat -ano | findstr :7001
   ```

2. **Verify the API URL configuration**:
   - Check `Area42-1.Web/appsettings.json` has correct `ApiUrl`
   - For local dev: `"ApiUrl": "https://localhost:7001"`

3. **Clear browser cache**:
   - Press F12, go to Application/Cookies
   - Delete all Area42 cookies
   - Refresh page

4. **Restart both services**:
   ```bash
   # Kill existing processes and restart
   ```

### Database Connection Issues

```bash
# Check localdb status
sqllocaldb info

# Create instance if missing
sqllocaldb create mssqllocaldb

# Start instance
sqllocaldb start mssqllocaldb

# View connection string
sqllocaldb info mssqllocaldb
```

### Port Already in Use

If a port is already in use:

1. Find the process:
   ```bash
   netstat -ano | findstr :7000  # Replace 7000 with your port
   ```

2. Kill the process:
   ```bash
   taskkill /PID <PID> /F
   ```

3. Or modify launch profile in `Properties/launchSettings.json`

## IDE Configuration

### Visual Studio

1. Set startup project to `Area42-1.AppHost`
2. Run project (F5)
3. Access dashboard at generated URL

### VS Code

Install extensions:
- C# (by Microsoft)
- REST Client (for testing API)
- Docker (if using Docker)

Then press F5 to start debugging.

## API Testing

### Using REST Client

Create `test.http` file:

```http
### Get accommodations
GET https://localhost:7001/api/accommodations

### Get admin stats (requires auth)
GET https://localhost:7001/api/admin/stats
Authorization: Bearer <YOUR_JWT_TOKEN>

### Create accommodation
POST https://localhost:7001/api/accommodations
Content-Type: application/json

{
  "name": "Test Bungalow",
  "description": "A test accommodation",
  "type": 0,
  "maxGuests": 4,
  "bedrooms": 2,
  "bathrooms": 1,
  "imageUrl": "https://example.com/image.jpg"
}
```

## Environment Variables

You can override settings with environment variables:

```bash
# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection = "your-connection-string"
$env:ApiUrl = "https://localhost:7001"

# Linux/Mac
export ConnectionStrings__DefaultConnection="your-connection-string"
export ApiUrl="https://localhost:7001"
```

## Performance Tips

1. **Enable Entity Framework query logging** (development only):
   ```csharp
   optionsBuilder.LogTo(Console.WriteLine);
   ```

2. **Use SQL Query Analyzer** in SQL Server Management Studio

3. **Monitor with Application Insights**:
   Add instrumentation for performance tracking

## Next Steps

- Review code at `Area42-1.Web/Components/Pages/AdminDashboard.razor`
- Check API endpoints in `Area42-1.ApiService/Controllers/`
- Study security model in `Area42-1.ApiService/Models/Admin/AdminModels.cs`
- Review database schema in `Area42-1.ApiService/Data/Area42Context.cs`
