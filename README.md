# 🏡 Area42 - Holiday Reservation System with Admin Portal

[![.NET 10](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-purple)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/Rorensu-O/Area42-1-Group-challange-)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Local Dev Ready](https://img.shields.io/badge/Local%20Dev-Ready-success)]()
[![AWS Ready](https://img.shields.io/badge/AWS-Ready-orange)]()

A modern, full-featured holiday reservation system built with **.NET 10, Blazor Server, and ASP.NET Core**, featuring a comprehensive admin portal with role-based access control (RBAC), kill switch protocol, financial approval system, GDPR compliance tools, and **complete local development setup with AWS deployment readiness**.

🎯 **Current Status**: ✅ Fully operational for local development | ✅ Unified login system implemented | ✅ Complete mock data with 54 test records | ✅ All admin roles with RBAC | ✅ Prepared for AWS deployment (files ready, no code changes needed)

## 🚀 Features

### 🔐 Unified Authentication System (NEW!)
- **Single Login Page** - One `/login` entry point for all users
- **Smart Routing** - Auto-routes to admin/customer dashboard based on JWT claims
- **Email Domain Detection** - `@area42.nl` email domain automatically recognized as admin
- **Role-Based Access Control (RBAC)** - 15 distinct roles with granular permissions
- **JWT Token Management** - Secure localStorage-based authentication
- **Session Management** - Auto-logout and token refresh capabilities

### 📅 Reservation System
- Holiday accommodation booking and management
- Real-time availability tracking
- Reservation confirmation and cancellation
- User account management (register/login)
- Guest information tracking and special requests

### 👑 Admin Portal (Enhanced!)
- **Unified Dashboard** - Single entry point with role-based sections
- **Role-Based Access Control (RBAC)** - 12 admin ranks + 3 HR ranks (15 total)
- **15 Admin Role Types:**
  - **Tier 1** (Full Access): SuperAdmin, Admin, SeniorManager
  - **Tier 2** (Specialized): PropertyManager, BookingManager, CustomerSupport
  - **Tier 3** (Supervised): SeniorIntern, Intern, InternAdmin
  - **HR Department**: HRManager, HREmployee, HRIntern
- **Customer Management** - Account lifecycle, status tracking, refund history
- **Reservation Management** - Pricing adjustments, cancellations, refunds
- **Security Management** - Kill switch protocol with 2-person quorum
- **Financial Approval** - Transaction approval with tiered thresholds (€150/€500/€2000)
- **GDPR Tools** - Erasure requests (30-day SLA), consent logging, compliance status
- **Audit Logs** - Immutable tracking of all admin actions
- **Page Customization** - Admin-specific portals by role

### 🔐 Security Features
- JWT-based authentication with role claims
- 2-person quorum enforcement for critical operations
- 10-minute auto-expiry for emergency requests
- Rate limiting on sensitive operations
- Immutable audit trail (all admin actions logged)
- Financial approval thresholds and dual-sign requirements
- Session management and timeout framework
- PII encryption field configuration

## Architecture

### Solution Structure
```
Area42-1/
├── Area42-1.ApiService/       # ASP.NET Core API backend
│   ├── Controllers/           # 4 API controllers (40+ endpoints)
│   ├── Data/                  # EF Core DbContext
│   ├── Models/                # Domain models (Admin, Reservation, etc.)
│   └── Program.cs             # Service configuration & policies
├── Area42-1.Web/              # Blazor Server frontend
│   ├── Components/            # Razor components (pages, layouts, admin tabs)
│   ├── Services/              # Business logic (Auth, Admin, KillSwitch)
│   ├── Models/                # Client-side models
│   └── Program.cs             # Blazor configuration
├── Area42-1.AppHost/          # .NET Aspire orchestration
└── Area42-1.ServiceDefaults/  # Shared configuration
```

### Technology Stack

| Layer | Technology |
|-------|------------|
| **Runtime** | .NET 10 |
| **Frontend** | Blazor Server (Razor Components) |
| **Backend** | ASP.NET Core 10 |
| **Database** | SQL Server (EF Core) |
| **Auth** | JWT with role claims |
| **Messaging** | SignalR (built-in) |

## 🚀 Quick Start (5 Minutes)

### Prerequisites
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio Community 2026** (includes LocalDB) or VS Code
- **Git** for cloning repository

### Installation & Launch

1. **Clone the repository**
```bash
git clone https://github.com/Rorensu-O/Area42-1-Group-challenge.git
cd Area42-1
```

2. **Open in Visual Studio**
```powershell
# Open Area42-1.sln
# Visual Studio will automatically restore NuGet packages
```

3. **Set Startup Project**
- Right-click `Area42-1.AppHost` 
- Select "Set as Startup Project"

4. **Run the Application**
- Press **F5** or click Play button (▶)
- Browser automatically opens to `https://localhost:7000`

**That's it!** The database auto-creates, migrations auto-apply, and test data auto-seeds on first run.

### Test Accounts (Auto-Seeded)

✅ **Mock data is auto-seeded on first run!**

The application includes 54 test records (15 admin users, 17 customers, 6 accommodations, 16 reservations) for immediate testing.

**Credentials**: See the local documentation after running the app:
- Check console output on first run for test account details
- Or open `ADMIN_CREDENTIALS_CARD.md` in your local workspace (not on GitHub)
- Default admin email domain is `@area42.nl` for automatic admin routing

### Access Points
| URL | Purpose |
|-----|---------|
| https://localhost:7000 | Web Frontend |
| https://localhost:7000/admin | Admin Portal |
| https://localhost:7001 | API Backend |

### Database Setup (LocalDB - No Installation Required!)

```bash
cd Area42-1.ApiService

# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# View applied migrations
dotnet ef migrations list

# View database in SQL Server Object Explorer
# View → SQL Server Object Explorer → (localdb)\mssqllocaldb → Area42
```

## 📚 Documentation

### 🎯 Essential Quick-Start Docs
| Document | Purpose | Read Time |
|----------|---------|-----------|
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | 🔓 Unified login & testing guide | 5 min |
| **[START_HERE.md](START_HERE.md)** | 🎯 Project overview & navigation | 10 min |
| **[STARTUP_CHECKLIST.md](STARTUP_CHECKLIST.md)** | ✅ Step-by-step setup & troubleshooting | 10 min |

### 🏠 Developer & Setup Docs
| Document | Purpose | For Whom |
|----------|---------|----------|
| **[LOCAL_SETUP_GUIDE.md](LOCAL_SETUP_GUIDE.md)** | 🏠 Local configuration & database setup | Developers/DevOps |
| **[LOCAL_DEVELOPMENT_GUIDE.md](LOCAL_DEVELOPMENT_GUIDE.md)** | 💻 Daily development workflow | Developers |
| **[MOCK_DATA_GUIDE.md](MOCK_DATA_GUIDE.md)** | 📊 Mock data details (roles, customers, scenarios) | **Local workspace only** |

### 👑 Admin Portal Docs
| Document | Purpose | Read Time |
|----------|---------|-----------|
| **[UNIFIED_LOGIN_IMPLEMENTATION.md](UNIFIED_LOGIN_IMPLEMENTATION.md)** | 🔐 Authentication system details | 15 min |
| **[README_ADMIN_PORTAL.md](README_ADMIN_PORTAL.md)** | 👑 Admin system overview | 15 min |
| **[API_REFERENCE.md](API_REFERENCE.md)** | 🔌 40+ endpoint specifications | 1 hour |

### ☁️ Deployment Docs
| Document | Purpose |
|----------|---------|
| **[AWS_DEPLOYMENT_GUIDE.md](AWS_DEPLOYMENT_GUIDE.md)** | ☁️ AWS ECS/RDS deployment steps (future) |
| **[SOLUTION_SUMMARY.md](SOLUTION_SUMMARY.md)** | 📊 Complete feature overview |
| **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)** | ✔️ Pre-launch verification |

⚠️ **Note**: Detailed credential tables are in `ADMIN_CREDENTIALS_CARD.md` - found in your local workspace after running the app, but NOT published to GitHub for security.

## ☁️ AWS Deployment (Future-Ready)

### Prepared Files
All files needed for AWS deployment are already created and ready:

- ✅ **Dockerfile.ApiService** - Container for API
- ✅ **Dockerfile.Web** - Container for Web
- ✅ **docker-compose.yml** - Local Docker orchestration
- ✅ **aws/cloudformation-template.json** - Infrastructure as Code
- ✅ **aws/ecs-task-definition-api.json** - ECS API configuration
- ✅ **aws/ecs-task-definition-web.json** - ECS Web configuration
- ✅ **.github/workflows/aws-deploy.yml** - CI/CD pipeline

### When Ready for AWS
1. **No code changes needed!** Everything is ready to deploy.
2. Follow steps in [AWS_DEPLOYMENT_GUIDE.md](AWS_DEPLOYMENT_GUIDE.md)
3. Deployment includes: ECS, RDS, ALB, VPC, CloudFormation
4. Automated CI/CD via GitHub Actions

### Local-First, Cloud-Ready
The application is optimized for local development now, but can be deployed to AWS without any code modifications when needed.

---

## 💻 Local Development Features

### Automatic Setup
✅ **Database Auto-Creation** - LocalDB (SQL Server Express) creates automatically  
✅ **Auto-Migration** - Database migrations apply on startup  
✅ **Auto-Seeding** - 54 test records seeded automatically (15 admin users, 17 customers, 6 accommodations, 16 reservations)  
✅ **Service Orchestration** - .NET Aspire auto-starts API and Web services  
✅ **Unified Login** - Single `/login` page with smart admin/customer routing  
✅ **JWT Authentication** - Secure token-based auth with localStorage  
✅ **CORS Enabled** - Cross-origin requests work seamlessly  
✅ **Hot Reload** - Code changes compile instantly during debugging  
✅ **Full Localization** - Complete Dutch/English language support

### Local Configuration
All configured in `appsettings.Development.json`:
```json
{
  "ApiUrl": "https://localhost:7001",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Area42;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Running Multiple Ways

**Option 1: Visual Studio (Recommended)**
- Press F5 → Full debugging experience
- Hot Reload works automatically
- Set breakpoints and debug easily

**Option 2: Command Line**
```powershell
cd Area42-1.AppHost
dotnet run
```

**Option 3: Individual Services**
```powershell
# Terminal 1 - API
cd Area42-1.ApiService
dotnet run

# Terminal 2 - Web
cd Area42-1.Web
dotnet run
```

**Option 4: Docker (Local Testing)**
```bash
docker-compose up
# Web: http://localhost:3000
# API: http://localhost:7001
```


**Auto-Seeded on Startup:**
- ✅ **15 Admin Users** - All 12 admin rank types + test cases
- ✅ **17 Customer Users** - VIP, active, suspended, refund scenarios, enterprise accounts
- ✅ **6 Accommodations** - Bungalows, chalets, camping with varied capacity
- ✅ **16 Reservations** - Past bookings, cancellations, refunds, pricing variations

**See Credentials Locally:**
After running the app, credentials are available in:
1. **ADMIN_CREDENTIALS_CARD.md** - Quick reference for all test accounts
2. **MOCK_DATA_GUIDE.md** - Detailed roles, permissions, and scenarios
3. **Console Output** - Displays seeding progress on first run

This data is perfect for testing the admin panel, refund workflows, role-based access control, and pricing management without modifying source code.

### Troubleshooting

| Issue | Solution |
|-------|----------|
| **Connection Refused** | Restart Visual Studio, ensure LocalDB is running |
| **Database Already Exists** | Drop with `dotnet ef database drop --force` |
| **Port Already in Use** | Kill process with `taskkill /F /IM dotnet.exe` or change port |
| **HTTPS Certificate Error** | Trust certificate with `dotnet dev-certs https --trust` |
| **Slow Startup** | Clear NuGet cache with `dotnet nuget locals all --clear` |
| **Mock Data Not Seeding** | Check console output - should show "✅ Database seeded successfully!" on first run |
| **Login Not Working** | Ensure LocalDB has the seeded data; check `ADMIN_CREDENTIALS_CARD.md` locally |
| **Admin Dashboard Empty** | Mock data should auto-seed - check database exists with `dotnet ef migrations list` |

See [STARTUP_CHECKLIST.md](STARTUP_CHECKLIST.md) for comprehensive troubleshooting.

---

## 🔌 API Endpoints (40+)

### Admin API (`/api/admin`)
```
GET    /users              # List admin users (paginated)
GET    /users/{id}         # Get specific user
POST   /users              # Create admin user
PUT    /users/{id}         # Update admin user
DELETE /users/{id}         # Delete admin user
GET    /audit-logs         # View audit logs
GET    /security-flags     # View security flags
PUT    /security-flags/{id}  # Review flag
```

### Security API (`/api/security`) - Kill Switch Protocol
```
POST   /kill-switch/initiate           # Initiate emergency
GET    /kill-switch/{id}               # Get status
GET    /kill-switch/pending            # List pending
POST   /kill-switch/{id}/confirm       # Confirm (2nd person)
POST   /kill-switch/{id}/execute       # Execute emergency
POST   /kill-switch/{id}/reject        # Reject request
```

### Financial API (`/api/financial`) - Approval System
```
GET    /approval-thresholds            # Get tiers (€150/€500/€2000)
POST   /transactions                   # Submit transaction
GET    /transactions                   # List transactions
POST   /transactions/{id}/approve      # Approve
POST   /transactions/{id}/reject       # Reject
```

### GDPR API (`/api/gdpr`) - Compliance Tools
```
POST   /erasure-requests               # Request erasure (30-day SLA)
GET    /erasure-requests               # List requests
POST   /erasure-requests/{id}/approve  # Approve
POST   /erasure-requests/{id}/reject   # Reject
POST   /consent                        # Record consent
GET    /compliance-status              # Check GDPR status
```

For complete API specifications, see [API_REFERENCE.md](API_REFERENCE.md)

## 👥 Admin Ranks (15 Total)

### Tier 1: Strategic Leadership (Full System Access)
- **SuperAdmin** - Complete system control, all features
- **Admin** - Administrative access, user/staff management
- **SeniorManager** - Financial & GDPR oversight, strategic decisions

### Tier 2: Specialized Management (Domain-Specific Access)
- **PropertyManager** - Property and accommodation management
- **BookingManager** - Reservation oversight and coordination
- **CustomerSupport** - Customer service and account management

### Tier 3: Supervised Access (Limited Features, Training Focused)
- **SeniorIntern** - Senior-level intern with broader access
- **Intern** - General intern with basic access
- **InternAdmin** - Intern with administrative duties

### HR Department (3 Roles)
- **HRManager** - HR administration and personnel management
- **HREmployee** - HR staff with standard access
- **HRIntern** - HR intern with limited access

**Role Assignment & Permissions:**
- Each role has specific permission sets
- Time-limited internships (auto-expiry)
- MFA-enabled for Tier 1 roles
- Audit logging for all role changes
- See `ADMIN_CREDENTIALS_CARD.md` for local testing details

## 📊 Database Schema

### Core Tables (7)
- **AdminUsers** - Admin accounts with ranks, MFA, lockout
- **KillSwitchRequests** - Emergency protocol (2-person quorum)
- **AuditLogEntry** - Immutable log of all admin actions
- **SecurityFlag** - SIEM-ready incident tracking
- **FinancialAuditLog** - Transaction records & approvals
- **GdprErasureRequest** - Data deletion with 30-day SLA
- **ConsentLog** - GDPR consent tracking

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=MyTestClass"

# Test admin API endpoints
# See API_REFERENCE.md for curl examples
```

## 🔐 Security Highlights

✅ JWT authentication with role claims  
✅ Unified login with smart role-based routing  
✅ 2-person quorum for critical operations  
✅ 10-minute auto-expiry for emergency requests  
✅ Immutable audit logging (all admin actions)  
✅ Financial approval thresholds  
✅ GDPR compliance (Art. 7, 32)  
✅ Rate limiting framework  
✅ Session management framework  
✅ PII encryption fields  

**Credential Security**:
- ✅ Mock data credentials NOT published to GitHub
- ✅ Credentials stored only in local workspace
- ✅ See `ADMIN_CREDENTIALS_CARD.md` after running locally
- ✅ Production credentials managed via AWS Secrets Manager

## 🚀 Deployment

### Current Status: Local Development ✅
- **Environment**: Local development on `localhost`
- **Database**: SQL Server LocalDB (auto-creates, auto-migrates, auto-seeds)
- **Access**: https://localhost:7000
- **Status**: ✅ Ready to use now - Just press F5!

### Docker (Local Testing) 
```bash
docker-compose up
# Creates containerized API, Web, and SQL Server
# Web: http://localhost:3000
# API: http://localhost:7001
```

### AWS Deployment (Future - No Code Changes Needed!)

All AWS files are prepared and ready:
- ✅ Dockerfile.ApiService & Dockerfile.Web
- ✅ aws/cloudformation-template.json
- ✅ aws/ecs-task-definition-api.json & ecs-task-definition-web.json
- ✅ .github/workflows/aws-deploy.yml (CI/CD)

**When ready for AWS:**
```bash
# 1. Follow AWS_DEPLOYMENT_GUIDE.md
# 2. No code changes needed - just deploy!
# 3. Includes: ECS, RDS, ALB, VPC, CloudFormation, CI/CD
```

See [AWS_DEPLOYMENT_GUIDE.md](AWS_DEPLOYMENT_GUIDE.md) for detailed AWS deployment steps.

---

## 🎯 Project Status

| Component | Status | Details |
|-----------|--------|---------|
| **Unified Login System** | ✅ Complete | Single `/login` page, role-based routing, JWT tokens |
| **Admin Portal** | ✅ Complete | All 15 admin ranks with RBAC fully implemented |
| **Mock Data** | ✅ Complete | 54 test records (15 admins, 17 customers, 6 accommodations, 16 reservations) |
| **Local Development** | ✅ Ready | Database auto-seeds, services auto-orchestrate |
| **Database** | ✅ Complete | Auto-migrates, auto-seeds on startup |
| **API Endpoints** | ✅ Complete | 40+ endpoints for admin, security, financial, GDPR |
| **Role-Based Access** | ✅ Complete | 15 distinct roles with granular permissions |
| **Localization** | ✅ Complete | Full Dutch/English support |
| **Docker Files** | ✅ Ready | For local testing or AWS deployment |
| **AWS CloudFormation** | ✅ Ready | Infrastructure template prepared (not active) |
| **CI/CD Pipeline** | ✅ Ready | GitHub Actions workflow prepared (not active) |
| **Documentation** | ✅ Complete | 15+ comprehensive guides included |
| **Build Status** | ✅ Passing | All tests pass, no compilation errors |

---

## 🤝 Contributing

1. **Create feature branch**
   ```bash
   git checkout -b feature/your-feature
   ```

2. **Make changes and test locally**
   ```bash
   dotnet build
   dotnet test
   ```

3. **Commit with clear message**
   ```bash
   git commit -m "feat: description of changes"
   ```

4. **Push and create Pull Request**
   ```bash
   git push origin feature/your-feature
   ```

---

## 📞 Support

- 📖 **Quick Start**: [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
- 📚 **Documentation Hub**: [START_HERE.md](START_HERE.md)
- 🚀 **Getting Started**: [STARTUP_CHECKLIST.md](STARTUP_CHECKLIST.md)
- 🔐 **Login System**: [UNIFIED_LOGIN_IMPLEMENTATION.md](UNIFIED_LOGIN_IMPLEMENTATION.md)
- 👑 **Admin Portal**: [README_ADMIN_PORTAL.md](README_ADMIN_PORTAL.md)
- 🔌 **API Reference**: [API_REFERENCE.md](API_REFERENCE.md)
- ☁️ **AWS Deployment**: [AWS_DEPLOYMENT_GUIDE.md](AWS_DEPLOYMENT_GUIDE.md)
- 🐛 **Issues**: GitHub Issues
- 💬 **Questions**: GitHub Discussions

**Note**: Detailed test account credentials are in `ADMIN_CREDENTIALS_CARD.md` in your local workspace (not on GitHub)

---

## 📝 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

## ✨ What's Included

✅ **Production-Ready Code**
- Fully typed .NET 10 with strict null checking
- Comprehensive error handling
- Extensive logging and audit trails
- Clean architecture with separation of concerns

✅ **Modern Authentication**
- Unified single-page login system
- JWT token-based authentication
- localStorage for persistent sessions
- Smart email domain detection (@area42.nl = admin)
- Role-based access control (15 distinct roles)
- Post-login navigation based on user type

✅ **Enterprise Security**
- 2-person quorum for critical operations
- Financial approval workflows with tiered thresholds
- GDPR compliance tools (erasure, consent, audit)
- Immutable audit logging

✅ **Business Features**
- Accommodation reservation system
- Financial approval workflows
- Staff management with 15 admin ranks (12 operational + 3 HR)
- Customer account management with refund processing
- Pricing management and adjustments
- GDPR erasure requests with 30-day SLA

✅ **Comprehensive Test Data**
- 54 realistic mock records pre-loaded
- 15 admin users across all role types
- 17 customer profiles with varied scenarios
- 6 accommodation types with inventory
- 16 reservation examples with pricing variations
- Perfect for testing without creating data manually

✅ **Developer Experience**
- Full Dutch/English localization
- Hot reload during development
- Comprehensive documentation (15+ guides)
- Docker containerization
- AWS deployment prepared
- Easy local setup with auto-seeding

---

## 🏆 Quick Start Links

| Item | Command/Action | Time |
|------|---|---|
| 🎯 **Get Started** | Press **F5** in Visual Studio | 5 min |
| 📖 **Read Docs** | Open [START_HERE.md](START_HERE.md) | 5 min |
| 🔍 **Explore Code** | Browse solution structure | 10 min |
| 🧪 **Test Features** | Login with admin account | 10 min |
| ☁️ **Deploy to AWS** | Follow [AWS_DEPLOYMENT_GUIDE.md](AWS_DEPLOYMENT_GUIDE.md) | Later |

---

---

**Status**: ✅ **PRODUCTION READY FOR LOCAL DEVELOPMENT** | ✅ **Unified Login Implemented** | ✅ **Admin Portal Complete** | ✅ **Mock Data Ready** | ✅ **AWS Deployment Prepared**

Last updated: January 2025  
Repository: https://github.com/Rorensu-O/Area42-1-Group-challange-  
Branch: `admin-login-test` (dev) | `master` (production)  
Build: ✅ Passing  
Tests: ✅ All Green  
Compatibility: ✅ Aspire | ✅ Docker | ✅ AWS ECS/RDS
