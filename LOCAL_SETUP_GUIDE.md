# Area42 - Local Development Configuration

## Current Setup (Local Only)

The application is configured to run locally with all services on `localhost`. AWS deployment files are prepared for future use but not active.

### Running Locally

#### Option 1: .NET Aspire (Recommended - Auto Orchestration)
```powershell
cd Area42-1.AppHost
dotnet run
```
**What happens:**
- API automatically starts on `https://localhost:7001` (or assigned port)
- Web frontend automatically starts on `https://localhost:7000` (or assigned port)
- Database uses LocalDB (auto-created on first run)
- Services auto-discover each other

**Access:**
- Web UI: https://localhost:7000
- Admin: https://localhost:7000/admin
- API: https://localhost:7001

#### Option 2: Manual Service Startup

**Terminal 1 - API Service:**
```powershell
cd Area42-1.ApiService
dotnet run
# Runs on https://localhost:7001 and http://localhost:5000
```

**Terminal 2 - Web Frontend:**
```powershell
cd Area42-1.Web
dotnet run
# Runs on https://localhost:7000 and http://localhost:5001
```

#### Option 3: Docker Compose (Optional - Local Testing)
```powershell
docker-compose up
# Web: http://localhost:3000
# API: http://localhost:7001
# SQL Server: localhost:1433
```

### Database Configuration

**Current**: LocalDB (SQL Server Express LocalDB)
```
Connection String: Server=(localdb)\mssqllocaldb;Database=Area42;Trusted_Connection=true;
```

**Features:**
- ✅ Automatic database creation on first run
- ✅ Automatic migrations applied on startup
- ✅ Automatic seeding with test data
- ✅ No external SQL Server needed

**To verify database:**
```powershell
# Check if LocalDB is running
sqllocaldb info

# View Area42 database
sqlcmd -S (localdb)\mssqllocaldb -d Area42
```

### Configuration Files

#### appsettings.json (Production/Default)
```json
{
  "ApiUrl": "https://localhost:7001",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### appsettings.Development.json (Development)
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

### Service Configuration

**API Service (Area42-1.ApiService)**
- Runs on: `https://localhost:7001`
- Database: LocalDB
- CORS: Enabled for `localhost:7000` and `localhost:7001`
- Auth: JWT with local key generation

**Web Service (Area42-1.Web)**
- Runs on: `https://localhost:7000`
- API URL: Configured in appsettings
- Session: Distributed memory cache
- Auth: Custom state provider

### Test Data (Auto-Seeded)

**Admin Users:**
- superadmin@area42.nl / SuperAdmin@123
- admin1@area42.nl / Admin@123
- propertymanager@area42.nl / Property@111
- bookingmanager@area42.nl / Booking@222
- support@area42.nl / Support@333
- hrmanager@area42.nl / HRManager@444

**Customer Users:**
- guest1@example.com / Guest@123
- guest2@example.com / Guest@456

**Accommodations:**
- 6 sample accommodations (bungalows, chalets, camping)
- Auto-loaded from DatabaseSeeder.cs

### Troubleshooting Local Setup

#### "Connection Refused" Error
```powershell
# Check if services are running
netstat -ano | findstr :7000
netstat -ano | findstr :7001

# Restart both services
# Kill existing processes if needed:
taskkill /PID <PID> /F
```

#### Database Issues
```powershell
# Check LocalDB status
sqllocaldb info

# Start LocalDB if not running
sqllocaldb start mssqllocaldb

# Reset database
dotnet ef database drop --force --context Area42Context
dotnet ef database update --context Area42Context
```

#### Port Already in Use
```powershell
# Find process using port
netstat -ano | findstr :7000

# Kill the process
taskkill /PID <PID> /F

# Or change ports in launchSettings.json
```

### Development Features

✅ **Hot Reload** - Changes compile instantly during debugging  
✅ **Entity Framework Migrations** - Auto-applied on startup  
✅ **CORS** - Enabled for localhost development  
✅ **Logging** - Console and file logging available  
✅ **Error Pages** - Detailed error information in development mode  

### Browser Debugging

Press **F12** to open Developer Tools:
- **Console**: Check for JavaScript errors
- **Network**: Monitor API calls
- **Application**: View session/cookies
- **Sources**: Debug TypeScript (if used)

### Visual Studio Configuration

1. **Set Startup Project**
   - Right-click `Area42-1.AppHost`
   - Select "Set as Startup Project"
   - Press **F5** to run

2. **Launch Settings** (Properties/launchSettings.json)
   ```json
   "profiles": {
     "https": {
       "commandName": "Project",
       "dotnetRunMessages": true,
       "launchBrowser": true,
       "applicationUrl": "https://localhost:7000;http://localhost:5000"
     }
   }
   ```

3. **Breakpoints**
   - Set in C# code (left margin)
   - Set in Razor components (@code blocks)
   - Debug ASP.NET Core with F5

### Performance Tips for Local Development

1. **Use Release Build for Testing**
   ```powershell
   dotnet run --configuration Release
   ```

2. **Monitor Local Database**
   - Open SQL Server Object Explorer in Visual Studio
   - Check query execution times
   - Monitor connection pool

3. **Clear Browser Cache**
   - Ctrl + Shift + Delete
   - Clear cookies and cache
   - Hard refresh: Ctrl + Shift + R

### Git Workflow (Local Development)

```powershell
# Pull latest changes
git pull origin master

# Create feature branch
git checkout -b feature/your-feature

# Make changes, test locally

# Commit changes
git add .
git commit -m "Description of changes"

# Push to GitHub
git push origin feature/your-feature

# Create Pull Request on GitHub
```

### Environment Variables (Optional)

You can override settings with environment variables:

```powershell
# Windows PowerShell
$env:ApiUrl = "https://localhost:7001"
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\mssqllocaldb;Database=Area42;..."

# Or in .env file (if using .env loader)
# ApiUrl=https://localhost:7001
```

### Next Steps for AWS Deployment (Future)

When ready for AWS:
1. Review `AWS_DEPLOYMENT_GUIDE.md`
2. Configure AWS CLI credentials
3. Build Docker images
4. Push to ECR
5. Deploy CloudFormation stack

**Note:** All AWS deployment files are already configured and ready. Just enable when needed.

---

**Status**: ✅ Local Development Ready  
**Configuration**: ✅ Complete  
**Test Data**: ✅ Auto-Seeded  
**AWS Ready**: ✅ (For Future Use)
