using Area42_1.ApiService.Models.Admin;
using Area42_1.ApiService.Models.Accommodations;
using Area42_1.ApiService.Models.Reservations;
using Area42_1.ApiService.Models.Users;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Area42_1.ApiService.Data;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with comprehensive mock data for admin testing and client management
    /// Includes: all admin roles, diverse clients, reservations with pricing variations
    /// </summary>
    public static void SeedDatabase(Area42Context context)
    {
        // Only seed if data doesn't exist
        if (context.AdminUsers.Any())
        {
            System.Console.WriteLine("ℹ️ Database already seeded, skipping initialization.");
            return;
        }

        System.Console.WriteLine("🌱 Seeding database with comprehensive mock data...");

        // Seed Admin Users (15 total - all role types)
        var adminUsers = GetMockAdminUsers();
        context.AdminUsers.AddRange(adminUsers);
        context.SaveChanges();
        System.Console.WriteLine($"✅ Seeded {adminUsers.Count} admin users");

        // Seed Regular Users (17 total - varied customer profiles)
        var users = GetMockUsers();
        context.Users.AddRange(users);
        context.SaveChanges();
        System.Console.WriteLine($"✅ Seeded {users.Count} customer users");

        // Seed Accommodations
        var accommodations = GetMockAccommodations();
        context.Accommodations.AddRange(accommodations);
        context.SaveChanges();
        System.Console.WriteLine($"✅ Seeded {accommodations.Count} accommodations");

        // Seed Reservations (with pricing variations, cancellations, etc.)
        var reservations = GetMockReservations(users, accommodations);
        context.Reservations.AddRange(reservations);
        context.SaveChanges();
        System.Console.WriteLine($"✅ Seeded {reservations.Count} reservations");

        System.Console.WriteLine("✅ Database seeded successfully! Ready for admin panel testing.");
    }

    /// <summary>
    /// Get mock admin users - comprehensive test data for all 12 admin roles
    /// Based on security compliance document requirements
    /// </summary>
    private static List<AdminUser> GetMockAdminUsers()
    {
        var now = DateTime.UtcNow;
        return new List<AdminUser>
        {
            // ============================================================================
            // TIER 1: Super Admin & Admin (System Control & User Management)
            // ============================================================================

            // 1.1 - SuperAdmin (System control, security settings, kill switch, audit logs)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "superadmin@area42.nl",
                FullName = "Jan de Vries",
                PasswordHash = HashPassword("SuperAdmin@123"),
                Rank = AdminRank.SuperAdmin,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = true,
                CreatedAt = now.AddMonths(-12),
                LastLoginAt = now.AddHours(-2),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // 1.2 - Admin (User management, security policy, property/booking oversight)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin1@area42.nl",
                FullName = "Maria García",
                PasswordHash = HashPassword("Admin@123"),
                Rank = AdminRank.Admin,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = true,
                CreatedAt = now.AddMonths(-10),
                LastLoginAt = now.AddHours(-4),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // 1.2 - Admin 2 (Redundancy + multi-admin scenarios)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin2@area42.nl",
                FullName = "Peter Müller",
                PasswordHash = HashPassword("Admin@456"),
                Rank = AdminRank.Admin,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-8),
                LastLoginAt = now.AddDays(-3),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // 1.3 - Senior Manager (Staff management, financial reports, operational dashboards)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "seniormanager@area42.nl",
                FullName = "Anne Lemmens",
                PasswordHash = HashPassword("Manager@789"),
                Rank = AdminRank.SeniorManager,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-6),
                LastLoginAt = now.AddHours(-8),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // ============================================================================
            // TIER 2: Managers (Specialized Functions)
            // ============================================================================

            // 2.1 - Property Manager (Property listings CRUD, availability, pricing)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "propertymanager@area42.nl",
                FullName = "Emma de Boer",
                PasswordHash = HashPassword("Property@111"),
                Rank = AdminRank.PropertyManager,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-5),
                LastLoginAt = now.AddHours(-1),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // 2.2 - Booking Manager (Reservations, cancellations, disputes)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "bookingmanager@area42.nl",
                FullName = "Thomas van den Berg",
                PasswordHash = HashPassword("Booking@222"),
                Rank = AdminRank.BookingManager,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-4),
                LastLoginAt = now.AddHours(-3),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // 2.3 - Customer Support (View booking details, respond to inquiries - read-only)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "support@area42.nl",
                FullName = "Lisa Jansen",
                PasswordHash = HashPassword("Support@333"),
                Rank = AdminRank.CustomerSupport,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-3),
                LastLoginAt = now.AddHours(-6),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // ============================================================================
            // TIER 3: Interns (Supervised Access)
            // ============================================================================

            // 3.1 - Senior Intern (Supervised read access + limited task execution)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "seniorintern@area42.nl",
                FullName = "David Vermeulen",
                PasswordHash = HashPassword("Senior@444"),
                Rank = AdminRank.SeniorIntern,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-2),
                LastLoginAt = now.AddDays(-1),
                DeactivatedAt = null,
                InternshipEndDate = now.AddMonths(4), // 4-month internship
                SessionExpiresAt = null
            },

            // 3.2 - Intern (Read-only dashboard, supervised support replies)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "intern@area42.nl",
                FullName = "Sophie Hendrickx",
                PasswordHash = HashPassword("Intern@555"),
                Rank = AdminRank.Intern,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-1),
                LastLoginAt = now.AddHours(-12),
                DeactivatedAt = null,
                InternshipEndDate = now.AddMonths(3),
                SessionExpiresAt = null
            },

            // IA - Intern Admin (Supervised admin access - all writes require 1 approval)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "internAdmin@area42.nl",
                FullName = "Marcus de Jong",
                PasswordHash = HashPassword("InternAdmin@666"),
                Rank = AdminRank.InternAdmin,
                HRRank = null,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddDays(-14),
                LastLoginAt = now.AddDays(-2),
                DeactivatedAt = null,
                InternshipEndDate = now.AddMonths(5),
                SessionExpiresAt = null
            },

            // ============================================================================
            // HR PORTAL ROLES (Employee Data Management)
            // ============================================================================

            // HR.1 - HR Manager (Full employee data CRUD - only role with BSN + salary + IBAN access)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "hrmanager@area42.nl",
                FullName = "Sophia Kramer",
                PasswordHash = HashPassword("HRManager@444"),
                Rank = null,
                HRRank = HRRank.HRManager,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = true,
                CreatedAt = now.AddMonths(-9),
                LastLoginAt = now.AddHours(-5),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // HR.2 - HR Employee (Read/write employee records - no BSN, no financial data)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "hremployee@area42.nl",
                FullName = "Nicole van Dorp",
                PasswordHash = HashPassword("HREmployee@777"),
                Rank = null,
                HRRank = HRRank.HREmployee,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-4),
                LastLoginAt = now.AddHours(-7),
                DeactivatedAt = null,
                SessionExpiresAt = null
            },

            // HR.3 - HR Intern (Read-only basic employee info - supervised, time-limited)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "hrintern@area42.nl",
                FullName = "Lucas van Acker",
                PasswordHash = HashPassword("HRIntern@888"),
                Rank = null,
                HRRank = HRRank.HRIntern,
                IsEnabled = true,
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddDays(-30),
                LastLoginAt = now.AddDays(-5),
                DeactivatedAt = null,
                InternshipEndDate = now.AddMonths(2),
                SessionExpiresAt = null
            },

            // ============================================================================
            // TEST ACCOUNTS: Disabled/Locked for testing
            // ============================================================================

            // Disabled account (for testing access denial)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "disabled@area42.nl",
                FullName = "Inactive Admin",
                PasswordHash = HashPassword("Disabled@999"),
                Rank = AdminRank.Admin,
                HRRank = null,
                IsEnabled = false,  // DISABLED
                IsLocked = false,
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-12),
                LastLoginAt = now.AddMonths(-6),
                DeactivatedAt = now.AddMonths(-6),
                SessionExpiresAt = null
            },

            // Locked account (for testing lockout scenario)
            new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "locked@area42.nl",
                FullName = "Locked Admin",
                PasswordHash = HashPassword("Locked@999"),
                Rank = AdminRank.PropertyManager,
                HRRank = null,
                IsEnabled = true,
                IsLocked = true,  // LOCKED
                LockoutUntil = now.AddHours(1),
                MfaEnabled = false,
                CreatedAt = now.AddMonths(-8),
                LastLoginAt = now.AddDays(-3),
                DeactivatedAt = null,
                SessionExpiresAt = null
            }
        };
    }

    /// <summary>
    /// Get comprehensive mock clients for testing admin panel functionality
    /// Includes varied booking history, price adjustments, refund scenarios
    /// </summary>
    private static List<User> GetMockUsers()
    {
        var now = DateTime.UtcNow;
        var random = new Random(42); // Fixed seed for reproducibility

        return new List<User>
        {
            // ============================================================================
            // TIER 1: Frequent Guests (VIP/High-Value Clients)
            // ============================================================================

            new User
            {
                Id = Guid.NewGuid(),
                Email = "vip.guest@example.com",
                FirstName = "Alexander",
                LastName = "Beaumont",
                PasswordHash = HashPassword("VIPGuest@123"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddYears(-2),
                UpdatedAt = now.AddHours(-2)
            },

            // ============================================================================
            // TIER 2: Active Guests (Regular Customers)
            // ============================================================================

            new User
            {
                Id = Guid.NewGuid(),
                Email = "john.smith@example.com",
                FirstName = "John",
                LastName = "Smith",
                PasswordHash = HashPassword("Guest@123"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddDays(-5)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "sarah.johnson@example.com",
                FirstName = "Sarah",
                LastName = "Johnson",
                PasswordHash = HashPassword("Guest@456"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-8),
                UpdatedAt = now.AddDays(-2)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "michael.chen@example.com",
                FirstName = "Michael",
                LastName = "Chen",
                PasswordHash = HashPassword("Guest@789"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-4),
                UpdatedAt = now.AddHours(-6)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "emma.wilson@example.com",
                FirstName = "Emma",
                LastName = "Wilson",
                PasswordHash = HashPassword("Guest@101"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-3),
                UpdatedAt = now.AddHours(-12)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "david.bauer@example.com",
                FirstName = "David",
                LastName = "Bauer",
                PasswordHash = HashPassword("Guest@202"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-5),
                UpdatedAt = now.AddDays(-3)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "lisa.weber@example.com",
                FirstName = "Lisa",
                LastName = "Weber",
                PasswordHash = HashPassword("Guest@303"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-7),
                UpdatedAt = now.AddHours(-8)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "marco.rossi@example.com",
                FirstName = "Marco",
                LastName = "Rossi",
                PasswordHash = HashPassword("Guest@404"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-2),
                UpdatedAt = now.AddDays(-1)
            },

            // ============================================================================
            // TIER 3: Occasional Guests (Returning but Infrequent)
            // ============================================================================

            new User
            {
                Id = Guid.NewGuid(),
                Email = "anna.mueller@example.com",
                FirstName = "Anna",
                LastName = "Müller",
                PasswordHash = HashPassword("Guest@505"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-12),
                UpdatedAt = now.AddMonths(-3)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "james.taylor@example.com",
                FirstName = "James",
                LastName = "Taylor",
                PasswordHash = HashPassword("Guest@606"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-9),
                UpdatedAt = now.AddMonths(-4)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "sophie.martin@example.com",
                FirstName = "Sophie",
                LastName = "Martin",
                PasswordHash = HashPassword("Guest@707"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-11),
                UpdatedAt = now.AddMonths(-6)
            },

            // ============================================================================
            // TIER 4: One-Time Guests / Recently Registered
            // ============================================================================

            new User
            {
                Id = Guid.NewGuid(),
                Email = "carlos.garcia@example.com",
                FirstName = "Carlos",
                LastName = "García",
                PasswordHash = HashPassword("Guest@808"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-7)
            },

            new User
            {
                Id = Guid.NewGuid(),
                Email = "julia.andersson@example.com",
                FirstName = "Julia",
                LastName = "Andersson",
                PasswordHash = HashPassword("Guest@909"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2)
            },

            // ============================================================================
            // TIER 5: Special Cases (For Testing Admin Functions)
            // ============================================================================

            // Suspended account
            new User
            {
                Id = Guid.NewGuid(),
                Email = "suspended.guest@example.com",
                FirstName = "Suspended",
                LastName = "Account",
                PasswordHash = HashPassword("Suspended@123"),
                Role = UserRole.Customer,
                IsActive = false,  // SUSPENDED
                CreatedAt = now.AddMonths(-10),
                UpdatedAt = now.AddMonths(-1)
            },

            // Frequent refund requester
            new User
            {
                Id = Guid.NewGuid(),
                Email = "refund.frequent@example.com",
                FirstName = "Robert",
                LastName = "Refund",
                PasswordHash = HashPassword("Guest@111"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-8),
                UpdatedAt = now.AddDays(-15)
            },

            // High-value client (large group bookings)
            new User
            {
                Id = Guid.NewGuid(),
                Email = "enterprise.group@example.com",
                FirstName = "Corporate",
                LastName = "Retreat",
                PasswordHash = HashPassword("Guest@222"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now.AddMonths(-4),
                UpdatedAt = now.AddDays(-20)
            },

            // Staff test account
            new User
            {
                Id = Guid.NewGuid(),
                Email = "staff@area42.nl",
                FirstName = "Robert",
                LastName = "Wilson",
                PasswordHash = HashPassword("Staff@789"),
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddDays(-1)
            }
        };
    }

    /// <summary>
    /// Get mock accommodations for testing
    /// </summary>
    private static List<Accommodation> GetMockAccommodations()
    {
        return new List<Accommodation>
        {
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Modern Family Bungalow",
                Description = "Beautiful modern bungalow in Eindhoven with spacious living areas and garden.",
                Type = AccommodationType.Bungalow,
                MaxGuests = 4,
                Bedrooms = 2,
                Bathrooms = 1,
                ImageUrl = "https://images.unsplash.com/photo-1570129477492-45201b8edff0?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Luxury Countryside Chalet",
                Description = "Premium chalet near Eindhoven with panoramic views and modern amenities.",
                Type = AccommodationType.Chalet,
                MaxGuests = 6,
                Bedrooms = 3,
                Bathrooms = 2,
                ImageUrl = "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Nature Camping Experience",
                Description = "Beautiful camping spot in nature near Eindhoven. Perfect for outdoor enthusiasts.",
                Type = AccommodationType.CampingSite,
                MaxGuests = 2,
                Bedrooms = 0,
                Bathrooms = 1,
                ImageUrl = "https://images.unsplash.com/photo-1478131143081-80f7f84ca84d?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Cozy Garden Bungalow",
                Description = "Intimate bungalow with beautiful garden in Eindhoven suburb. Great for couples.",
                Type = AccommodationType.Bungalow,
                MaxGuests = 2,
                Bedrooms = 1,
                Bathrooms = 1,
                ImageUrl = "https://images.unsplash.com/photo-1568605114967-8130f3a36994?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Alpine Mountain Chalet",
                Description = "Exclusive mountain chalet near Eindhoven with premium facilities and stunning views.",
                Type = AccommodationType.Chalet,
                MaxGuests = 8,
                Bedrooms = 4,
                Bathrooms = 3,
                ImageUrl = "https://images.unsplash.com/photo-1578037014386-3c5f1f7c7325?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Forest Camping Adventure",
                Description = "Budget camping in scenic forest area near Eindhoven. Nature lovers' paradise.",
                Type = AccommodationType.CampingSite,
                MaxGuests = 4,
                Bedrooms = 0,
                Bathrooms = 1,
                ImageUrl = "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=400&h=250&fit=crop",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Get mock reservations with varied scenarios for admin testing:
    /// - Past/current/future bookings
    /// - Different guest counts and prices
    /// - Various statuses (pending, confirmed, checked-out, cancelled)
    /// - Refund/price adjustment scenarios
    /// </summary>
    private static List<Reservation> GetMockReservations(List<User> users, List<Accommodation> accommodations)
    {
        var now = DateTime.UtcNow;
        var reservations = new List<Reservation>();
        var random = new Random(123); // Fixed seed for reproducibility

        // Helper: base prices per accommodation type
        var basePrices = new Dictionary<AccommodationType, decimal>
        {
            { AccommodationType.Bungalow, 85m },
            { AccommodationType.Chalet, 150m },
            { AccommodationType.CampingSite, 40m }
        };

        // ============================================================================
        // SCENARIO 1: Recent Past Reservations (Already checked out - completed)
        // ============================================================================

        // User 1: Successfully completed stay
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[0].Id,  // Modern Family Bungalow
            UserId = users[1].Id,  // John Smith
            CheckInDate = now.AddDays(-45),
            CheckOutDate = now.AddDays(-39),
            NumberOfGuests = 4,
            TotalPrice = basePrices[AccommodationType.Bungalow] * 6 * 1.1m,  // 10% seasonal premium
            Status = ReservationStatus.CheckedOut,
            GuestName = "John Smith",
            GuestEmail = "john.smith@example.com",
            GuestPhone = "+31612345678",
            SpecialRequests = "High floor preferred, early breakfast available",
            CreatedAt = now.AddDays(-50),
            UpdatedAt = now.AddDays(-39)
        });

        // User 2: Long stay (past)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[1].Id,  // Luxury Countryside Chalet
            UserId = users[2].Id,  // Sarah Johnson
            CheckInDate = now.AddDays(-90),
            CheckOutDate = now.AddDays(-75),
            NumberOfGuests = 6,
            TotalPrice = basePrices[AccommodationType.Chalet] * 15 * 0.95m,  // 5% group discount
            Status = ReservationStatus.CheckedOut,
            GuestName = "Sarah Johnson",
            GuestEmail = "sarah.johnson@example.com",
            GuestPhone = "+31687654321",
            SpecialRequests = "Birthday celebration setup",
            CreatedAt = now.AddDays(-95),
            UpdatedAt = now.AddDays(-75)
        });

        // ============================================================================
        // SCENARIO 2: Current/Upcoming Reservations (Active bookings)
        // ============================================================================

        // User 3: Currently checked in
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[2].Id,  // Nature Camping Experience
            UserId = users[3].Id,  // Michael Chen
            CheckInDate = now.AddDays(-3),
            CheckOutDate = now.AddDays(4),
            NumberOfGuests = 2,
            TotalPrice = basePrices[AccommodationType.CampingSite] * 7,
            Status = ReservationStatus.CheckedIn,
            GuestName = "Michael Chen",
            GuestEmail = "michael.chen@example.com",
            GuestPhone = "+31698765432",
            SpecialRequests = "Hiking guides available",
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now.AddDays(-3)
        });

        // User 4: Upcoming reservation (confirmed)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[3].Id,  // Cozy Garden Bungalow
            UserId = users[4].Id,  // Emma Wilson
            CheckInDate = now.AddDays(14),
            CheckOutDate = now.AddDays(21),
            NumberOfGuests = 2,
            TotalPrice = basePrices[AccommodationType.Bungalow] * 7 * 1.05m,  // Slight premium
            Status = ReservationStatus.Confirmed,
            GuestName = "Emma Wilson",
            GuestEmail = "emma.wilson@example.com",
            GuestPhone = "+31611223344",
            SpecialRequests = "Romantic getaway - wine service appreciated",
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5)
        });

        // User 5: Next month booking (pending confirmation)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[4].Id,  // Alpine Mountain Chalet
            UserId = users[5].Id,  // David Bauer
            CheckInDate = now.AddMonths(1),
            CheckOutDate = now.AddMonths(1).AddDays(5),
            NumberOfGuests = 8,
            TotalPrice = basePrices[AccommodationType.Chalet] * 5 * 0.9m,  // Group discount
            Status = ReservationStatus.Pending,
            GuestName = "David Bauer",
            GuestEmail = "david.bauer@example.com",
            GuestPhone = "+31633445566",
            SpecialRequests = "Family reunion - need large group meal planning",
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2)
        });

        // ============================================================================
        // SCENARIO 3: Cancelled Reservations (For refund testing)
        // ============================================================================

        // User 6: Recently cancelled (full refund)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[0].Id,  // Modern Family Bungalow
            UserId = users[6].Id,  // Lisa Weber
            CheckInDate = now.AddDays(30),
            CheckOutDate = now.AddDays(37),
            NumberOfGuests = 4,
            TotalPrice = basePrices[AccommodationType.Bungalow] * 7,
            Status = ReservationStatus.Cancelled,
            GuestName = "Lisa Weber",
            GuestEmail = "lisa.weber@example.com",
            GuestPhone = "+31655667788",
            SpecialRequests = "CANCELLED: Customer requested refund due to work conflict",
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now.AddDays(-5)
        });

        // User 8: Old cancelled booking (partial refund)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[5].Id,  // Forest Camping Adventure
            UserId = users[8].Id,  // Marco Rossi
            CheckInDate = now.AddDays(-30),
            CheckOutDate = now.AddDays(-25),
            NumberOfGuests = 4,
            TotalPrice = basePrices[AccommodationType.CampingSite] * 5 * 0.5m,  // 50% refunded
            Status = ReservationStatus.Cancelled,
            GuestName = "Marco Rossi",
            GuestEmail = "marco.rossi@example.com",
            GuestPhone = "+31677889900",
            SpecialRequests = "CANCELLED & PARTIALLY REFUNDED: Illness prevented attendance",
            CreatedAt = now.AddMonths(-1),
            UpdatedAt = now.AddDays(-30)
        });

        // ============================================================================
        // SCENARIO 4: VIP/Frequent Guest Bookings
        // ============================================================================

        // User 0 (VIP): Multiple bookings showing loyalty
        for (int i = 0; i < 3; i++)
        {
            var checkIn = now.AddMonths(-(6 - i * 2));
            reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                AccommodationId = accommodations[i % accommodations.Count].Id,
                UserId = users[0].Id,  // VIP Guest
                CheckInDate = checkIn,
                CheckOutDate = checkIn.AddDays(5),
                NumberOfGuests = 2,
                TotalPrice = 500m + (100m * i),  // VIP premium pricing
                Status = i == 0 ? ReservationStatus.CheckedOut : 
                         i == 1 ? ReservationStatus.CheckedOut : 
                         ReservationStatus.Confirmed,
                GuestName = "Alexander Beaumont",
                GuestEmail = "vip.guest@example.com",
                GuestPhone = "+31699001122",
                SpecialRequests = $"VIP Booking #{i + 1} - Concierge service included",
                CreatedAt = checkIn.AddDays(-20),
                UpdatedAt = checkIn
            });
        }

        // ============================================================================
        // SCENARIO 5: Special Cases for Admin Testing
        // ============================================================================

        // Suspended user's past booking (for account investigation)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[2].Id,
            UserId = users[14].Id,  // Suspended account
            CheckInDate = now.AddDays(-120),
            CheckOutDate = now.AddDays(-114),
            NumberOfGuests = 2,
            TotalPrice = 350m,
            Status = ReservationStatus.CheckedOut,
            GuestName = "Suspended Account",
            GuestEmail = "suspended.guest@example.com",
            GuestPhone = "+31611112222",
            SpecialRequests = "Account suspended - review this booking for compliance",
            CreatedAt = now.AddMonths(-4),
            UpdatedAt = now.AddMonths(-4)
        });

        // Frequent refund requester's bookings
        for (int i = 0; i < 2; i++)
        {
            reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                AccommodationId = accommodations[i].Id,
                UserId = users[15].Id,  // Refund frequent requester
                CheckInDate = now.AddDays(-(60 + i * 20)),
                CheckOutDate = now.AddDays(-(55 + i * 20)),
                NumberOfGuests = 2,
                TotalPrice = 300m,
                Status = i == 0 ? ReservationStatus.CheckedOut : ReservationStatus.Cancelled,
                GuestName = "Robert Refund",
                GuestEmail = "refund.frequent@example.com",
                GuestPhone = "+31622223333",
                SpecialRequests = i == 1 ? "CANCELLED: Refund requested - check pattern" : "Previous booking",
                CreatedAt = now.AddDays(-(65 + i * 20)),
                UpdatedAt = now.AddDays(-(60 + i * 20))
            });
        }

        // Enterprise group booking (high-value, multiple guests)
        reservations.Add(new Reservation
        {
            Id = Guid.NewGuid(),
            AccommodationId = accommodations[4].Id,  // Alpine Chalet - largest
            UserId = users[16].Id,  // Corporate Retreat
            CheckInDate = now.AddMonths(3),
            CheckOutDate = now.AddMonths(3).AddDays(3),
            NumberOfGuests = 25,  // Large group
            TotalPrice = 2500m,  // Significant amount
            Status = ReservationStatus.Confirmed,
            GuestName = "Corporate Retreat Group",
            GuestEmail = "enterprise.group@example.com",
            GuestPhone = "+31644445555",
            SpecialRequests = "Team building event - needs conference room, catering arrangements",
            CreatedAt = now.AddMonths(1),
            UpdatedAt = now.AddMonths(1)
        });

        return reservations;
    }

    /// <summary>
    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
