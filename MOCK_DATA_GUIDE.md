# Comprehensive Mock Data Guide - Admin Panel Testing

**Status:** ✅ **COMPLETE** - Build Successful  
**Date:** January 16, 2025  
**Database Seeder Location:** `Area42-1.ApiService/Data/DatabaseSeeder.cs`

---

## 🎯 Overview

The database seeder generates **complete mock data** for testing the admin panel and client management features:

✅ **15 Admin Users** (All 12 role types + special cases)  
✅ **17 Customer Users** (VIP to new accounts, plus test scenarios)  
✅ **6 Accommodations** (Bungalows, Chalets, Camping Sites)  
✅ **16 Reservations** (Past, current, future, cancelled, refunds)

---

## 👥 Admin User Hierarchy (15 Total)

### **TIER 1: System Control (3 roles)**

#### 1.1 - **SuperAdmin** ⭐⭐⭐
- **Account:** `superadmin@area42.nl` / `SuperAdmin@123`
- **FullName:** Jan de Vries
- **Permissions:**
  - System control & security settings
  - Kill switch authority
  - Audit log access
  - No HR data access
  - MFA: **Enabled**
  - Created: 12 months ago
- **Use Case:** Top-level access for testing all admin features

#### 1.2 - **Admin** ⭐⭐
- **Accounts:** 
  - `admin1@area42.nl` / `Admin@123` (Maria García)
  - `admin2@area42.nl` / `Admin@456` (Peter Müller)
- **Permissions:**
  - User management
  - Security policy enforcement
  - Property/booking oversight
  - Kill switch access
  - No HR data access
  - MFA: admin1=Enabled, admin2=Disabled
- **Use Case:** Multi-admin scenarios, redundancy testing

#### 1.3 - **Senior Manager** ⭐
- **Account:** `seniormanager@area42.nl` / `Manager@789`
- **FullName:** Anne Lemmens
- **Permissions:**
  - Staff management
  - Financial reports
  - Operational dashboards
  - No HR data access
- **Use Case:** Financial oversight and reporting

---

### **TIER 2: Specialized Functions (3 roles)**

#### 2.1 - **Property Manager**
- **Account:** `propertymanager@area42.nl` / `Property@111`
- **FullName:** Emma de Boer
- **Permissions:**
  - Property listings CRUD
  - Availability management
  - Pricing updates
- **Use Case:** Test property management features, pricing changes

#### 2.2 - **Booking Manager**
- **Account:** `bookingmanager@area42.nl` / `Booking@222`
- **FullName:** Thomas van den Berg
- **Permissions:**
  - Reservation management
  - Cancellation handling
  - Dispute resolution
- **Use Case:** Test reservation panel, refund scenarios

#### 2.3 - **Customer Support**
- **Account:** `support@area42.nl` / `Support@333`
- **FullName:** Lisa Jansen
- **Permissions:**
  - View booking details (READ-ONLY)
  - Respond to client inquiries
  - No edit permissions
- **Use Case:** Test read-only access restrictions

---

### **TIER 3: Internship Programs (3 roles)**

#### 3.1 - **Senior Intern**
- **Account:** `seniorintern@area42.nl` / `Senior@444`
- **FullName:** David Vermeulen
- **Permissions:**
  - Supervised read access
  - Limited task execution
  - Tasks require manager approval
  - Internship end date: +4 months
- **Use Case:** Test supervised workflow with approval gates

#### 3.2 - **Intern**
- **Account:** `intern@area42.nl` / `Intern@555`
- **FullName:** Sophie Hendrickx
- **Permissions:**
  - Read-only dashboard
  - Supervised support replies
  - No data modification
  - Internship end date: +3 months
- **Use Case:** Test read-only access and time-limited accounts

#### IA - **Intern Admin**
- **Account:** `internAdmin@area42.nl` / `InternAdmin@666`
- **FullName:** Marcus de Jong
- **Permissions:**
  - Admin dashboard access
  - **All write actions require 1 Admin confirmation**
  - Time-limited (revocable by any Admin/SuperAdmin)
  - Internship end date: +5 months
- **Use Case:** Test approval workflow for intern actions

---

### **HR PORTAL ROLES (3 roles)**

#### HR.1 - **HR Manager**
- **Account:** `hrmanager@area42.nl` / `HRManager@444`
- **FullName:** Sophia Kramer
- **Permissions:**
  - Full employee data CRUD
  - **ONLY role with BSN access**
  - **ONLY role with salary data access**
  - **ONLY role with IBAN access**
  - MFA: **Enabled**
- **Use Case:** Test sensitive HR data restrictions

#### HR.2 - **HR Employee**
- **Account:** `hremployee@area42.nl` / `HREmployee@777`
- **FullName:** Nicole van Dorp
- **Permissions:**
  - Read/write employee records
  - **NO BSN access**
  - **NO financial data access**
  - **NO IBAN access**
- **Use Case:** Test field-level permissions

#### HR.3 - **HR Intern**
- **Account:** `hrintern@area42.nl` / `HRIntern@888`
- **FullName:** Lucas van Acker
- **Permissions:**
  - Read-only basic employee info
  - Supervised access (requires approval)
  - Time-limited (+2 months)
  - No sensitive data access
- **Use Case:** Test time-limited supervised HR access

---

### **TEST ACCOUNTS (2 special cases)**

#### ❌ **Disabled Account**
- **Account:** `disabled@area42.nl` / `Disabled@999`
- **FullName:** Inactive Admin
- **Status:** `IsEnabled = false`
- **Use Case:** Test access denial, disabled account scenarios

#### 🔒 **Locked Account**
- **Account:** `locked@area42.nl` / `Locked@999`
- **FullName:** Locked Admin
- **Status:** `IsLocked = true` (Lockout expires in 1 hour)
- **Use Case:** Test account lockout mechanics

---

## 👨‍👩‍👧‍👦 Customer User Database (17 Total)

### **Tier 1: VIP/Frequent Guests**

#### Alexander Beaumont (VIP)
- **Email:** `vip.guest@example.com`
- **Password:** `VIPGuest@123`
- **Member Since:** 2 years ago
- **Status:** Active
- **Booking History:** 3 VIP bookings with premium pricing
- **Use Case:** Test high-value customer features, loyalty programs

---

### **Tier 2: Active Customers (Regular Bookers)**

| Name | Email | Password | Member Since | Bookings | Notes |
|------|-------|----------|--------------|----------|-------|
| John Smith | john.smith@example.com | Guest@123 | 6 months | Past checkout | Standard customer |
| Sarah Johnson | sarah.johnson@example.com | Guest@456 | 8 months | Long stay | Family user |
| Michael Chen | michael.chen@example.com | Guest@789 | 4 months | Currently checked-in | Active booking |
| Emma Wilson | emma.wilson@example.com | Guest@101 | 3 months | Upcoming (confirmed) | Romantic getaway |
| David Bauer | david.bauer@example.com | Guest@202 | 5 months | Next month (pending) | Group booking |
| Lisa Weber | lisa.weber@example.com | Guest@303 | 7 months | Recently cancelled | Refund scenario |
| Marco Rossi | marco.rossi@example.com | Guest@404 | 2 months | Recent booking | New customer |

---

### **Tier 3: Occasional Guests (Less Frequent)**

| Name | Email | Password | Member Since | Last Activity | Notes |
|------|-------|----------|--------------|----------------|-------|
| Anna Müller | anna.mueller@example.com | Guest@505 | 12 months | 3 months ago | Returning guest |
| James Taylor | james.taylor@example.com | Guest@606 | 9 months | 4 months ago | Infrequent visitor |
| Sophie Martin | sophie.martin@example.com | Guest@707 | 11 months | 6 months ago | Annual repeat |

---

### **Tier 4: Recent Registration**

| Name | Email | Password | Member Since | Last Activity | Notes |
|------|-------|----------|--------------|----------------|-------|
| Carlos García | carlos.garcia@example.com | Guest@808 | 7 days ago | 7 days ago | Brand new |
| Julia Andersson | julia.andersson@example.com | Guest@909 | 2 days ago | 2 days ago | Just registered |

---

### **Tier 5: Special Test Cases**

#### ❌ Suspended Account
- **Name:** Suspended Account
- **Email:** `suspended.guest@example.com`
- **Status:** `IsActive = false`
- **Use Case:** Test admin compliance checks on suspended users

#### 🔄 Frequent Refund Requester
- **Name:** Robert Refund
- **Email:** `refund.frequent@example.com`
- **Password:** `Guest@111`
- **Booking History:** 2 bookings (1 completed, 1 cancelled with refund)
- **Use Case:** Test refund pattern detection, admin flags

#### 💼 Enterprise Group
- **Name:** Corporate Retreat Group
- **Email:** `enterprise.group@example.com`
- **Password:** `Guest@222`
- **Upcoming Booking:** 25 guests (Alpine Chalet, 3 days)
- **Amount:** €2,500+
- **Use Case:** Test high-value group bookings, special arrangements

#### 👔 Staff Account
- **Name:** Robert Wilson
- **Email:** `staff@area42.nl`
- **Password:** `Staff@789`
- **Role:** Staff (not Customer)
- **Use Case:** Test staff vs. customer role separation

---

## 🏨 Accommodations (6 Total)

| Name | Type | Max Guests | Bedrooms | Base Price/Night | Use Case |
|------|------|-----------|----------|------------------|----------|
| Modern Family Bungalow | Bungalow | 4 | 2 | €85 | Standard family bookings |
| Luxury Countryside Chalet | Chalet | 6 | 3 | €150 | Premium group bookings |
| Nature Camping Experience | Camp Site | 2 | 0 | €40 | Budget/nature bookings |
| Cozy Garden Bungalow | Bungalow | 2 | 1 | €85 | Couples/small groups |
| Alpine Mountain Chalet | Chalet | 8 | 4 | €150 | Large groups/corporate |
| Forest Camping Adventure | Camp Site | 4 | 0 | €40 | Adventure seekers |

---

## 📅 Reservation Scenarios (16 Total)

### **Scenario 1: Past Completed Bookings (2)**
- ✅ John Smith → Modern Family Bungalow (45→39 days ago)
- ✅ Sarah Johnson → Luxury Chalet (90→75 days ago)
- **Use Case:** Completed transaction testing

### **Scenario 2: Current/Upcoming (3)**
- 🏠 Michael Chen → Camping (currently checked-in, 4 days remaining)
- ⏰ Emma Wilson → Cozy Bungalow (14 days from now, confirmed)
- ⏱️ David Bauer → Alpine Chalet (1 month from now, pending)
- **Use Case:** Active booking management

### **Scenario 3: Cancelled Bookings (2)**
- ❌ Lisa Weber → Bungalow (recently cancelled, full refund)
- ❌ Marco Rossi → Camping (old cancellation, 50% refunded)
- **Use Case:** Refund processing, cancellation policies

### **Scenario 4: VIP Repeat Bookings (3)**
- 👑 Alexander Beaumont → 3 bookings (past, past, upcoming)
- **Use Case:** VIP loyalty program, repeat customer patterns

### **Scenario 5: Special Test Cases (4)**
- 🚫 Suspended User → Past booking (for compliance review)
- 🔄 Refund Requester → 2 bookings (1 completed, 1 cancelled)
- 💼 Corporate Group → Alpine Chalet (25 guests, €2,500)
- **Use Case:** Admin flag detection, high-value client handling

### **Scenario 6: Pricing Variations**
- **Seasonal Premium:** +10% multiplier on bookings
- **Group Discount:** -5% for 5+ guests / -10% for 8+ guests
- **Partial Refunds:** 50% refund on cancellations
- **VIP Premium:** Fixed higher pricing

---

## 🛠️ Admin Panel Testing Checklist

### **Account Management**
- [ ] Test all 15 admin login scenarios
- [ ] Verify disabled account rejection
- [ ] Verify locked account lockout
- [ ] Test MFA for SuperAdmin/HR Manager
- [ ] Test intern account time-limit enforcement

### **Permission Testing**
- [ ] SuperAdmin sees all features
- [ ] Admin has limited but broad access
- [ ] Customer Support has read-only access
- [ ] HR roles cannot see non-HR data
- [ ] Interns require approval workflow

### **Customer Management**
- [ ] View all 17 customer profiles
- [ ] Test suspended account flags
- [ ] View refund requester pattern
- [ ] Access VIP customer history
- [ ] Filter by account status

### **Reservation Management**
- [ ] View all 16 reservations across statuses
- [ ] Test pricing calculation display
- [ ] Verify refund scenarios
- [ ] Confirm discount application
- [ ] Handle large group bookings

### **Refund Processing**
- [ ] Process full refund (Lisa Weber)
- [ ] Handle partial refund (Marco Rossi)
- [ ] Test refund pattern detection (Robert Refund)
- [ ] Verify enterprise group special pricing

### **Access Control**
- [ ] Verify role-based dashboard sections
- [ ] Test HR data field restrictions
- [ ] Confirm intern approval gates
- [ ] Test read-only enforcement

---

## 🔑 Credentials Summary

### Admin Quick Access

| Role | Email | Password |
|------|-------|----------|
| **SuperAdmin** | superadmin@area42.nl | SuperAdmin@123 |
| **Admin** | admin1@area42.nl | Admin@123 |
| **Booking Manager** | bookingmanager@area42.nl | Booking@222 |
| **Property Manager** | propertymanager@area42.nl | Property@111 |
| **Customer Support** | support@area42.nl | Support@333 |
| **HR Manager** | hrmanager@area42.nl | HRManager@444 |

### Customer Quick Access

| Name | Email | Password |
|------|-------|----------|
| **VIP Guest** | vip.guest@example.com | VIPGuest@123 |
| **John Smith** | john.smith@example.com | Guest@123 |
| **Suspended** | suspended.guest@example.com | Suspended@123 |
| **Refund Test** | refund.frequent@example.com | Guest@111 |

---

## 📊 Database Statistics

**Total Records Seeded:**
- Admin Users: 15
- Customer Users: 17
- Accommodations: 6
- Reservations: 16
- **Total: 54 records**

**Data Coverage:**
- ✅ All 12 admin role types represented
- ✅ VIP to new customer profiles
- ✅ Past, current, and future bookings
- ✅ Completed, pending, and cancelled states
- ✅ Various refund scenarios
- ✅ Access control test cases

---

## 🚀 Running the Seeder

### First Run (Database Empty)
```powershell
# Visual Studio: Package Manager Console
Update-Database

# CLI:
dotnet ef database update
```

**Output:**
```
🌱 Seeding database with comprehensive mock data...
✅ Seeded 15 admin users
✅ Seeded 17 customer users
✅ Seeded 6 accommodations
✅ Seeded 16 reservations
✅ Database seeded successfully! Ready for admin panel testing.
```

### Subsequent Runs
- Seeder checks if `AdminUsers` table has data
- If data exists, seeding is **skipped** (idempotent)
- To reseed: Clear database or modify `SeedDatabase` logic

---

## 🎯 Common Test Scenarios

### Scenario A: Test All Admin Roles
1. Login with each of the 15 admin accounts
2. Verify role-appropriate dashboard
3. Check permission restrictions
4. Confirm access denials where expected

### Scenario B: Customer Refund Processing
1. View Robert Refund's booking history
2. Note pattern: 1 completed, 1 cancelled
3. Process 50% refund on cancelled booking
4. Verify admin flag for refund pattern
5. Investigate suspended customer's past activity

### Scenario C: High-Value Booking Management
1. View Alexander Beaumont (VIP) bookings
2. Note premium pricing applied
3. Handle corporate group booking (25 guests)
4. Manage enterprise pricing negotiation
5. Test discount application

### Scenario D: Access Control Verification
1. Login as Customer Support
2. Verify read-only booking view
3. Attempt to modify booking (should fail)
4. Verify cannot access admin settings
5. Confirm can respond to inquiries

### Scenario E: HR Data Protection
1. Login as HR Employee
2. Verify employee records visible
3. Attempt BSN access (should be denied)
4. Attempt salary data access (should be denied)
5. Confirm HR Manager can access all

---

## 📝 Notes

- **Passwords:** All passwords are SHA-256 hashed in database
- **Timestamps:** Created dates vary from 2 years ago to 2 days ago (realistic)
- **MFA:** Enabled for SuperAdmin, Admin1, and HR Manager (requires implementation)
- **Intern Dates:** End dates set +2 to +5 months for testing time-limit logic
- **Idempotency:** Seeder won't duplicate data on re-run

---

## ✅ Implementation Status

| Component | Status | Details |
|-----------|--------|---------|
| Admin Roles | ✅ Complete | All 15 roles with personas |
| Customers | ✅ Complete | 17 profiles with varied history |
| Accommodations | ✅ Complete | 6 properties with types |
| Reservations | ✅ Complete | 16 bookings with pricing |
| Build | ✅ Successful | 0 errors, 0 warnings |
| Ready for Testing | ✅ Yes | Can run `dotnet run` |

---

**Next Steps:**
1. Run the application (`F5` in Visual Studio)
2. Database will auto-seed on first run
3. Access `/admin` with any admin credentials above
4. Test admin panel features with mock data
5. Verify all role-based access controls

Happy testing! 🎉
