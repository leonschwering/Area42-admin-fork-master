using Microsoft.EntityFrameworkCore;

namespace Area42_1.Web.Backend;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options)
    : DbContext(options)
{
    public DbSet<SqlReservation> Reservations => Set<SqlReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SqlReservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TotalPrice).HasPrecision(18, 2);
            entity.Property(item => item.GuestName).HasMaxLength(200);
            entity.Property(item => item.GuestEmail).HasMaxLength(200);
            entity.Property(item => item.GuestPhone).HasMaxLength(20);
            entity.Property(item => item.SpecialRequests).HasMaxLength(1000);
        });
    }
}

public sealed class SqlReservation
{
    public Guid Id { get; set; }
    public Guid AccommodationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string SpecialRequests { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
