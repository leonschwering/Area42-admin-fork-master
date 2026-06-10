# 🔐 Admin Credentials Card - Quick Reference

## 🌟 Primary Test Accounts

### SuperAdmin (Full System Access)
```
Email: superadmin@area42.nl
Password: SuperAdmin@123
Name: Jan de Vries
MFA: Enabled ✓
```

### Admin (Broad Management)
```
Email: admin1@area42.nl
Password: Admin@123
Name: Maria García
```

### Admin 2 (Redundancy/Backup)
```
Email: admin2@area42.nl
Password: Admin@456
Name: Peter Müller
```

---

## 📋 Department Leads

### Booking Manager (Reservation Control)
```
Email: bookingmanager@area42.nl
Password: Booking@222
Name: Thomas van den Berg
Focus: Reservations, cancellations, disputes
```

### Property Manager (Listing & Pricing)
```
Email: propertymanager@area42.nl
Password: Property@111
Name: Emma de Boer
Focus: Property CRUD, availability, pricing
```

### Customer Support (Read-Only)
```
Email: support@area42.nl
Password: Support@333
Name: Lisa Jansen
Focus: View bookings (READ-ONLY), respond to inquiries
```

### Senior Manager (Financial/Operations)
```
Email: seniormanager@area42.nl
Password: Manager@789
Name: Anne Lemmens
Focus: Staff, financial reports, dashboards
```

---

## 👨‍💼 HR Portal Access

### HR Manager (Full Employee Data)
```
Email: hrmanager@area42.nl
Password: HRManager@444
Name: Sophia Kramer
Access: BSN, salary, IBAN (⚠️ SENSITIVE)
MFA: Enabled ✓
```

### HR Employee (Limited Access)
```
Email: hremployee@area42.nl
Password: HREmployee@777
Name: Nicole van Dorp
Access: Employee records (NO BSN/salary)
```

### HR Intern (Read-Only, Time-Limited)
```
Email: hrintern@area42.nl
Password: HRIntern@888
Name: Lucas van Acker
Access: Basic info only, expires +2 months
```

---

## 🎓 Intern Programs

### Senior Intern (Supervised with Tasks)
```
Email: seniorintern@area42.nl
Password: Senior@444
Name: David Vermeulen
Access: Read + limited execution, needs approval
Expires: +4 months
```

### Junior Intern (Read-Only Dashboard)
```
Email: intern@area42.nl
Password: Intern@555
Name: Sophie Hendrickx
Access: Dashboard only (READ-ONLY)
Expires: +3 months
```

### Intern Admin (Supervised Admin, Needs 1-Click Approval)
```
Email: internAdmin@area42.nl
Password: InternAdmin@666
Name: Marcus de Jong
Access: Admin panel (all writes require approval)
Expires: +5 months
```

---

## ⚠️ Test/Access Control Accounts

### Disabled Account (Testing Denial)
```
Email: disabled@area42.nl
Password: Disabled@999
Status: DISABLED ❌
Purpose: Verify access denial
```

### Locked Account (Testing Lockout)
```
Email: locked@area42.nl
Password: Locked@999
Status: LOCKED 🔒 (1 hour timeout)
Purpose: Verify lockout mechanics
```

---

## 👥 Customer Test Accounts

### VIP Guest (High-Value Client)
```
Email: vip.guest@example.com
Password: VIPGuest@123
Name: Alexander Beaumont
Member: 2 years, 3 bookings
Purpose: Test VIP features & loyalty
```

### Sample Customers
```
John Smith          john.smith@example.com      Guest@123
Sarah Johnson       sarah.johnson@example.com   Guest@456
Michael Chen        michael.chen@example.com    Guest@789
Emma Wilson         emma.wilson@example.com     Guest@101
David Bauer         david.bauer@example.com     Guest@202
```

### Special Test Cases
```
Refund Requester    refund.frequent@example.com     Guest@111
Suspended Account   suspended.guest@example.com     Suspended@123
Enterprise Group    enterprise.group@example.com    Guest@222
```

---

## 📊 Admin Hierarchy at a Glance

```
┌─────────────────────────────────────────┐
│         SUPERADMIN (1)                  │
│    Full system control, kill switch     │
│         superadmin@area42.nl            │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┬──────────────┐
       │                │              │
┌──────▼────┐  ┌───────▼──┐   ┌──────▼────────┐
│  ADMIN(2) │  │  SENIOR  │   │   HR ROLES(3) │
│ Full mgmt │  │ MANAGER  │   │ Employee data │
└──────┬────┘  │ Staff &  │   │  (sensitive)  │
       │       │ Financial│   └───────────────┘
       │       └─────────┘
       │
   ┌───┴──────────────────────────────┐
   │                                  │
┌──▼────┐ ┌──────────┐ ┌────────────┐│
│BOOKING│ │PROPERTY  │ │ CUSTOMER   ││
│MANAGER│ │MANAGER   │ │ SUPPORT    ││
│Reserve│ │Listings, │ │ Read-only  ││
│Cancel │ │Pricing   │ │ support    ││
└───────┘ └──────────┘ └────────────┘│
                                     │
   ┌────────────────────────────────┐│
   │      INTERN PROGRAMS (3)       ││
   │ Senior | Junior | Admin        ││
   │ (Tiers of supervised access)   ││
   └────────────────────────────────┘│
```

---

## 🔍 How to Test Each Role

### SuperAdmin
1. Login with `superadmin@area42.nl`
2. Should see ALL admin panels and settings
3. Can access kill switch, audit logs, user management
4. Can modify any other admin's settings

### Property Manager
1. Login with `propertymanager@area42.nl`
2. Focus on property listings and pricing
3. Try to access user management (should be denied)
4. Modify accommodation pricing (should succeed)

### Customer Support
1. Login with `support@area42.nl`
2. Can view customer bookings (READ-ONLY)
3. Try to modify booking status (should fail)
4. Can respond to customer inquiries
5. Cannot access admin settings or financial data

### HR Manager
1. Login with `hrmanager@area42.nl`
2. Should see employee BSN fields
3. Should see salary/IBAN data
4. Other admins should NOT see these fields

### Intern Admin
1. Login with `internAdmin@area42.nl`
2. Can perform admin actions (UI should show "Pending Approval")
3. Actions appear as "pending" until SuperAdmin/Admin approves
4. Cannot approve own actions

### Disabled Account
1. Try login with `disabled@area42.nl`
2. Should receive "Account disabled" error
3. Cannot proceed to admin panel

---

## 📝 Common Testing Flows

### Test 1: Permission Verification
- Login as Customer Support
- Try to access user management → Denied ✓
- Try to access financial reports → Denied ✓
- Can view bookings → Allowed ✓

### Test 2: Role-Based Data Access
- Login as HR Employee
- View employee records → Allowed ✓
- Try to see salary field → Denied ✓
- Try to see BSN field → Denied ✓

### Test 3: Refund Scenario
- Login as Booking Manager
- Search for "Robert Refund" customer
- View his booking history (2 bookings: 1 completed, 1 cancelled)
- Process partial refund on cancelled booking
- Verify refund pattern flag set

### Test 4: VIP Customer Management
- Login as Admin
- Search for "Alexander Beaumont" (VIP)
- View 3 premium bookings
- Note pricing variations
- Test loyalty program features

---

## 🚀 Get Started

1. **Start the application:**
   ```
   dotnet run (or F5 in Visual Studio)
   ```

2. **Login to admin panel:**
   - Navigate to `/login`
   - Enter admin credentials above
   - Should redirect to `/admin` dashboard

3. **Explore mock data:**
   - View all 15 admin users in User Management
   - See 17 customers with booking history
   - Test reservation management with 16 sample bookings

4. **Test permissions:**
   - Logout and login with different roles
   - Verify role-based feature access
   - Test read-only enforcement

---

## ⏰ Account Expiration

| Account | Expiration | Action |
|---------|-----------|--------|
| Senior Intern | +4 months | Auto-disable on date |
| Junior Intern | +3 months | Auto-disable on date |
| Intern Admin | +5 months | Auto-disable on date |
| HR Intern | +2 months | Auto-disable on date |

---

**Updated:** January 16, 2025  
**Total Accounts:** 15 admins + 17 customers  
**Status:** ✅ Ready for testing
