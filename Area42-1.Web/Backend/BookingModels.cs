using System.ComponentModel.DataAnnotations;

namespace Area42_1.Web.Backend;

public sealed record Accommodation(
    string Id,
    string Name,
    string Description,
    int MaxGuests,
    int Bedrooms,
    decimal PricePerNight,
    string CssClass);

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}

public sealed class Booking
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string AccommodationId { get; set; } = string.Empty;
    public string AccommodationName { get; set; } = string.Empty;
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int Guests { get; set; }
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class BookingRequest
{
    [Required(ErrorMessage = "Kies een verblijf.")]
    public string AccommodationId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kies een aankomstdatum.")]
    public DateOnly? CheckIn { get; set; }

    [Required(ErrorMessage = "Kies een vertrekdatum.")]
    public DateOnly? CheckOut { get; set; }

    [Range(1, 8, ErrorMessage = "Kies minimaal 1 gast.")]
    public int Guests { get; set; } = 2;

    [Required(ErrorMessage = "Vul je naam in.")]
    [StringLength(120)]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vul je e-mailadres in.")]
    [EmailAddress(ErrorMessage = "Vul een geldig e-mailadres in.")]
    public string GuestEmail { get; set; } = string.Empty;

    [StringLength(30)]
    public string GuestPhone { get; set; } = string.Empty;

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}

public sealed record BookingResult(bool Success, Booking? Booking, string? Error);

public sealed record DashboardSummary(
    int Total,
    int Upcoming,
    int Pending,
    int Cancelled,
    decimal ConfirmedRevenue);
