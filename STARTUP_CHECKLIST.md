# 🚀 Area42 Local Startup Checklist

## Before First Run

### Prerequisites Check
- [ ] .NET 10 SDK installed: `dotnet --version`
- [ ] SQL Server LocalDB installed: `sqllocaldb info`
- [ ] Git configured: `git config --global user.name`
- [ ] Visual Studio Community 2026 (or VS Code) installed
- [ ] Node.js (optional, if using npm): `npm --version`

### Repository Setup
- [ ] Repository cloned to: `C:\Users\LAURE\source\repos\Area42-1`
- [ ] Branch is master: `git branch` (shows `* master`)
- [ ] Remote is configured: `git remote -v`
- [ ] No uncommitted changes: `git status`

## Starting the Application (Choose One Method)

### Method 1: Visual Studio (Recommended for Development)

#### Initial Setup
1. [ ] Open `Area42-1.sln` in Visual Studio
2. [ ] Wait for NuGet packages to restore (bottom of screen)
3. [ ] Right-click `Area42-1.AppHost` → Set as Startup Project
4. [ ] Solution should build without errors

#### Running
1. [ ] Press **F5** or click Play button (▶)
2. [ ] Wait for console output to show startup messages
3. [ ] Browser should open automatically to `https://localhost:7000`
4. [ ] Check that both API and Web services started

#### What to Look For
```
Starting Area42 API Service...
Area42 Reservation API is running on https://localhost:7001
Starting Area42 Web Service...
Area42 Web Frontend is running on https://localhost:7000
Applying migrations...
Database seeding completed successfully!
```

### Method 2: PowerShell Command Line

```powershell
# Navigate to AppHost
cd C:\Users\LAURE\source\repos\Area42-1\Area42-1.AppHost

# Run the application
dotnet run

# In new terminal, access the web app
start https://localhost:7000
```

### Method 3: Individual Service Startup

```powershell
# Terminal 1 - API Service
cd C:\Users\LAURE\source\repos\Area42-1\Area42-1.ApiService
dotnet run

# Terminal 2 - Web Frontend
cd C:\Users\LAURE\source\repos\Area42-1\Area42-1.Web
dotnet run

# Then open browser
start https://localhost:7000
```

## First Run Setup (Auto-Executed)

When you run the application for the first time, these happen automatically:

✅ Database created (if not exists)
✅ Migrations applied
✅ Test data seeded (8 admin users, 3 customers, 6 accommodations)
✅ Logging configured
✅ CORS enabled

**No manual setup required!** Just run and use.

## Verification Checklist

### Web Application
- [ ] Navigate to https://localhost:7000
- [ ] Home page loads without errors
- [ ] Images display correctly
- [ ] Language selector visible (top-right)
- [ ] Navigation menu shows all links

### Admin Portal
- [ ] Go to https://localhost:7000/admin
- [ ] Login with: superadmin@area42.nl / SuperAdmin@123
- [ ] Dashboard loads
- [ ] All menu items visible
- [ ] Can navigate between sections

### API Service
- [ ] Open https://localhost:7001 in new tab
- [ ] Should see: "Area42 Reservation API is running."
- [ ] Open DevTools (F12) → Network tab
- [ ] Make a request to API (e.g., GET /api/accommodations)
- [ ] Response shows accommodation data

### Database
- [ ] Open SQL Server Object Explorer (View → SQL Server Object Explorer)
- [ ] Expand (localdb)\mssqllocaldb
- [ ] Look for Area42 database
- [ ] Expand → Tables
- [ ] Verify tables exist: AdminUsers, Users, Accommodations, etc.

### Localization
- [ ] Switch language to Dutch (top-right)
- [ ] Entire page updates to Dutch
- [ ] Switch to English
- [ ] Entire page updates to English

## Common First-Run Issues & Fixes

### Issue: "Connection Refused"
```powershell
# Check if services are running
netstat -ano | findstr :7000

# If not showing, restart Visual Studio
# Close VS completely and reopen
```

### Issue: "HTTPS Certificate Error"
```powershell
# Trust development certificate
dotnet dev-certs https --trust

# If still issues, remove and regenerate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Issue: Database Already Exists Error
```powershell
# If you want fresh database, drop it
cd Area42-1.ApiService
dotnet ef database drop --force --context Area42Context

# Then rebuild
dotnet ef database update
```

### Issue: Port Already in Use
```powershell
# Find process on port 7000
netstat -ano | findstr :7000

# Kill it
taskkill /PID <PID> /F

# Or change port in Area42-1.AppHost\Properties\launchSettings.json
```

### Issue: NuGet Restore Fails
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild solution
dotnet build
```

## Post-Startup Checks

### Developer Console (F12)
- [ ] No red errors in console
- [ ] No 404 errors for images
- [ ] CORS warnings (if any) are acceptable
- [ ] Network tab shows successful API calls

### Database Connections
- [ ] Connection pooling active
- [ ] No connection timeout warnings
- [ ] Queries complete in < 100ms

### Performance
- [ ] Home page loads in < 2 seconds
- [ ] Admin dashboard loads in < 3 seconds
- [ ] Form submission responsive
- [ ] No noticeable lag or freezing

## Test Scenarios

### Scenario 1: Customer Browsing
1. [ ] View home page
2. [ ] Click "Browse Accommodations"
3. [ ] See accommodation list
4. [ ] View accommodation details
5. [ ] Check "Attractions" page

### Scenario 2: Admin Access
1. [ ] Logout (if logged in)
2. [ ] Go to /admin
3. [ ] Login with superadmin account
4. [ ] Access different dashboard sections
5. [ ] Verify data displays correctly

### Scenario 3: Language Switching
1. [ ] Start on English page
2. [ ] Switch to Dutch
3. [ ] Verify all text is Dutch
4. [ ] Switch back to English
5. [ ] Verify all text is English

## Daily Startup Routine

Every time you start development:

```powershell
# 1. Navigate to project
cd C:\Users\LAURE\source\repos\Area42-1

# 2. Pull latest changes
git pull origin master

# 3. Restore packages (if dependencies changed)
dotnet restore

# 4. Run tests (if any)
dotnet test

# 5. Start application
cd Area42-1.AppHost
dotnet run
```

## Stopping the Application

### Clean Shutdown
- Press **Ctrl + C** in the terminal/console
- Or click Stop button (⬛) in Visual Studio
- Wait for services to shutdown gracefully

### Force Shutdown
```powershell
# If needed to forcefully kill processes
taskkill /F /IM dotnet.exe

# Then delete any lock files
Remove-Item -Path ".vs" -Recurse -Force
```

## Development Tips

### 1. Keep Services Running
- Don't stop services between code changes
- Use Hot Reload (F5 or Ctrl + Alt + F5)
- Services auto-compile changes

### 2. Monitor Logs
- Keep Developer Console open (F12)
- Watch VS Output window
- Check Application Insights (if configured)

### 3. Test Database
- Make test queries in SSMS
- Verify seed data was created
- Check for migration issues

### 4. Git Workflow
- Commit frequently during development
- Use meaningful commit messages
- Pull before starting work
- Push before ending work

### 5. Browser Cache
- Clear cache when styles change (Ctrl + Shift + Delete)
- Use Hard Refresh (Ctrl + Shift + R)
- Keep DevTools open to prevent caching

## When Ready for AWS Deployment

1. [ ] All local testing complete
2. [ ] Code committed to git
3. [ ] Review AWS_DEPLOYMENT_GUIDE.md
4. [ ] Set up AWS credentials
5. [ ] Follow AWS deployment steps

**Note:** Local setup does NOT interfere with AWS deployment. All AWS files are separate and ready to use.

## Support & Troubleshooting

| Issue | Solution |
|-------|----------|
| Slow startup | Clear NuGet cache, restart VS |
| Database errors | Drop and recreate database |
| Port conflicts | Kill process or change port |
| HTTPS errors | Trust dev certificate |
| Image not loading | Clear cache, hard refresh |
| Language not switching | Clear cookies, hard refresh |

## Helpful Commands

```powershell
# General
dotnet --version                    # Check .NET version
dotnet new --list                   # List project templates

# Database
dotnet ef database update           # Apply migrations
dotnet ef migrations list           # View migrations
dotnet ef database drop --force     # Reset database

# Build & Run
dotnet build                        # Build solution
dotnet run                          # Run project
dotnet test                         # Run tests

# Cleaning
dotnet clean                        # Remove build artifacts
dotnet nuget locals all --clear     # Clear NuGet cache
```

---

**Status**: ✅ Ready for Local Development  
**Estimated Time**: 5-10 minutes first run  
**Required Storage**: ~1 GB  
**Last Updated**: 2024
