namespace Area42_1.ApiService.Data;

using Microsoft.EntityFrameworkCore;
using Area42_1.ApiService.Models.Accommodations;
using Area42_1.ApiService.Models.Reservations;
using Area42_1.ApiService.Models.Users;
using Area42_1.ApiService.Models.Admin;

public class Area42Context : DbContext
{
    public Area42Context(DbContextOptions<Area42Context> options) : base(options)
    {
    }

    public DbSet<Accommodation> Accommodations { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    // Admin portal tables
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    public DbSet<KillSwitchRequest> KillSwitchRequests { get; set; } = null!;
    public DbSet<AuditLogEntry> AuditLogs { get; set; } = null!;
    public DbSet<SecurityFlag> SecurityFlags { get; set; } = null!;
    public DbSet<FinancialAuditLog> FinancialAuditLogs { get; set; } = null!;
    public DbSet<GdprErasureRequest> GdprErasureRequests { get; set; } = null!;
    public DbSet<ConsentLog> ConsentLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Accommodation configuration
        modelBuilder.Entity<Accommodation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.MaxGuests).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccommodationId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CheckInDate).IsRequired();
            entity.Property(e => e.CheckOutDate).IsRequired();
            entity.Property(e => e.NumberOfGuests).IsRequired();
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.GuestName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.GuestEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.GuestPhone).HasMaxLength(20);
            entity.Property(e => e.SpecialRequests).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => new { e.AccommodationId, e.CheckInDate, e.CheckOutDate });
            entity.HasIndex(e => e.UserId);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Admin User configuration
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Rank).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastLoginAt).IsRequired(false);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.Rank, e.IsEnabled });
        });

        // Kill Switch Request configuration
        modelBuilder.Entity<KillSwitchRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetUserId).IsRequired(false);
            entity.Property(e => e.InitiatorUserId).IsRequired();
            entity.Property(e => e.ConfirmerUserId).IsRequired(false);
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ConfirmedAt).IsRequired(false);
            entity.Property(e => e.ExecutedAt).IsRequired(false);
            entity.HasIndex(e => new { e.Status, e.ExpiresAt });
            entity.HasIndex(e => e.InitiatorUserId);
        });

        // Audit Log configuration
        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserRank).IsRequired();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.OldValues).IsRequired(false);
            entity.Property(e => e.NewValues).IsRequired(false);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Action);
        });

        // Security Flag configuration
        modelBuilder.Entity<SecurityFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TriggeredByUserId).IsRequired(false);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ReviewNote).HasMaxLength(500);
            entity.Property(e => e.TriggeredAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ResolvedAt).IsRequired(false);
            entity.HasIndex(e => new { e.Status, e.TriggeredAt });
        });

        // Financial Audit Log configuration
        modelBuilder.Entity<FinancialAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InitiatorUserId).IsRequired();
            entity.Property(e => e.ApproverUserId).IsRequired(false);
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AmountBefore).HasPrecision(18, 2);
            entity.Property(e => e.AmountAfter).HasPrecision(18, 2);
            entity.Property(e => e.MfaConfirmed).HasDefaultValue(false);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => new { e.InitiatorUserId, e.Timestamp });
        });

        // GDPR Erasure Request configuration
        modelBuilder.Entity<GdprErasureRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CompletedAt).IsRequired(false);
            entity.HasIndex(e => new { e.Status, e.DueAt });
        });

        // Consent Log configuration
        modelBuilder.Entity<ConsentLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ConsentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Granted).IsRequired();
            entity.Property(e => e.PrivacyVersionAccepted).HasMaxLength(1000);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => new { e.UserId, e.ConsentType });
        });
    }
}
