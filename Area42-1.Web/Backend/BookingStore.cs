using Microsoft.EntityFrameworkCore;

namespace Area42_1.Web.Backend;

public sealed class BookingStore(
    IDbContextFactory<BookingDbContext> contextFactory,
    ILogger<BookingStore> logger)
{
    private const int PendingStatus = 0;
    private const int ConfirmedStatus = 1;
    private const int CancelledStatus = 4;

    public IReadOnlyList<Accommodation> Accommodations { get; } =
    [
        new(
            "bosvilla",
            "Bosvilla",
            "Ruime villa aan de bosrand voor het hele gezin.",
            4,
            2,
            129m,
            "cabin-one"),
        new(
            "waterlodge",
            "Waterlodge",
            "Comfortabele lodge aan het water met extra leefruimte.",
            6,
            3,
            169m,
            "cabin-two"),
        new(
            "heidehuisje",
            "Heidehuisje",
            "Knus huisje voor twee, midden in het groen.",
            2,
            1,
            99m,
            "cabin-three")
    ];

    public Accommodation? GetAccommodation(string id) =>
        Accommodations.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<Booking>> GetBookingsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var reservations = await context.Reservations
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return reservations.Select(ToBooking).ToList();
    }

    public async Task<bool> IsAvailableAsync(
        string accommodationId,
        DateOnly checkIn,
        DateOnly checkOut)
    {
        if (!TryGetAccommodationGuid(accommodationId, out var accommodationGuid))
        {
            return false;
        }

        await using var context = await contextFactory.CreateDbContextAsync();
        var start = checkIn.ToDateTime(TimeOnly.MinValue);
        var end = checkOut.ToDateTime(TimeOnly.MinValue);
        return !await context.Reservations.AnyAsync(item =>
            item.AccommodationId == accommodationGuid &&
            item.Status != CancelledStatus &&
            item.CheckInDate < end &&
            item.CheckOutDate > start);
    }

    public async Task<BookingResult> CreateAsync(BookingRequest request)
    {
        var accommodation = GetAccommodation(request.AccommodationId);
        if (accommodation is null ||
            !TryGetAccommodationGuid(request.AccommodationId, out var accommodationGuid))
        {
            return new(false, null, "Het gekozen verblijf bestaat niet.");
        }

        if (request.CheckIn is null || request.CheckOut is null)
        {
            return new(false, null, "Kies een aankomst- en vertrekdatum.");
        }

        var checkIn = request.CheckIn.Value;
        var checkOut = request.CheckOut.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (checkIn < today)
        {
            return new(false, null, "De aankomstdatum kan niet in het verleden liggen.");
        }

        if (checkOut <= checkIn)
        {
            return new(false, null, "De vertrekdatum moet na de aankomstdatum liggen.");
        }

        if (request.Guests < 1 || request.Guests > accommodation.MaxGuests)
        {
            return new(
                false,
                null,
                $"{accommodation.Name} is geschikt voor maximaal {accommodation.MaxGuests} gasten.");
        }

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var start = checkIn.ToDateTime(TimeOnly.MinValue);
            var end = checkOut.ToDateTime(TimeOnly.MinValue);
            var isOccupied = await context.Reservations.AnyAsync(item =>
                item.AccommodationId == accommodationGuid &&
                item.Status != CancelledStatus &&
                item.CheckInDate < end &&
                item.CheckOutDate > start);

            if (isOccupied)
            {
                return new(false, null, "Dit verblijf is niet beschikbaar voor de gekozen periode.");
            }

            var now = DateTime.UtcNow;
            var nights = checkOut.DayNumber - checkIn.DayNumber;
            var reservation = new SqlReservation
            {
                Id = Guid.NewGuid(),
                AccommodationId = accommodationGuid,
                UserId = Guid.Empty,
                CheckInDate = start,
                CheckOutDate = end,
                NumberOfGuests = request.Guests,
                TotalPrice = nights * accommodation.PricePerNight,
                Status = ConfirmedStatus,
                GuestName = request.GuestName.Trim(),
                GuestEmail = request.GuestEmail.Trim(),
                GuestPhone = request.GuestPhone.Trim(),
                SpecialRequests = request.Notes.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();
            return new(true, ToBooking(reservation), null);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The reservation could not be saved to SQL Server.");
            return new(
                false,
                null,
                "De boeking kon niet worden opgeslagen. Probeer het later opnieuw.");
        }
    }

    public async Task<bool> SetStatusAsync(Guid id, BookingStatus status)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var reservation = await context.Reservations.FindAsync(id);
        if (reservation is null)
        {
            return false;
        }

        reservation.Status = ToSqlStatus(status);
        reservation.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var reservations = await context.Reservations.AsNoTracking().ToListAsync();

        return new(
            reservations.Count,
            reservations.Count(item =>
                item.CheckInDate >= today && item.Status != CancelledStatus),
            reservations.Count(item => item.Status == PendingStatus),
            reservations.Count(item => item.Status == CancelledStatus),
            reservations
                .Where(item => item.Status == ConfirmedStatus)
                .Sum(item => item.TotalPrice));
    }

    private Booking ToBooking(SqlReservation reservation)
    {
        var accommodationId = GetAccommodationId(reservation.AccommodationId);
        var accommodation = GetAccommodation(accommodationId);

        return new Booking
        {
            Id = reservation.Id,
            Reference = BuildReference(reservation),
            AccommodationId = accommodationId,
            AccommodationName = accommodation?.Name ?? "Onbekend verblijf",
            CheckIn = DateOnly.FromDateTime(reservation.CheckInDate),
            CheckOut = DateOnly.FromDateTime(reservation.CheckOutDate),
            Guests = reservation.NumberOfGuests,
            Nights = (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days,
            TotalPrice = reservation.TotalPrice,
            Status = FromSqlStatus(reservation.Status),
            GuestName = reservation.GuestName,
            GuestEmail = reservation.GuestEmail,
            GuestPhone = reservation.GuestPhone,
            Notes = reservation.SpecialRequests,
            CreatedAtUtc = DateTime.SpecifyKind(reservation.CreatedAt, DateTimeKind.Utc)
        };
    }

    private static string BuildReference(SqlReservation reservation) =>
        $"A42-{reservation.CreatedAt:yyMMdd}-{reservation.Id.ToString("N")[..4].ToUpperInvariant()}";

    private static int ToSqlStatus(BookingStatus status) => status switch
    {
        BookingStatus.Pending => PendingStatus,
        BookingStatus.Confirmed => ConfirmedStatus,
        BookingStatus.Cancelled => CancelledStatus,
        _ => PendingStatus
    };

    private static BookingStatus FromSqlStatus(int status) => status switch
    {
        ConfirmedStatus => BookingStatus.Confirmed,
        CancelledStatus => BookingStatus.Cancelled,
        _ => BookingStatus.Pending
    };

    private static bool TryGetAccommodationGuid(string id, out Guid guid)
    {
        guid = id.ToLowerInvariant() switch
        {
            "bosvilla" => Guid.Parse("42000000-0000-0000-0000-000000000001"),
            "waterlodge" => Guid.Parse("42000000-0000-0000-0000-000000000002"),
            "heidehuisje" => Guid.Parse("42000000-0000-0000-0000-000000000003"),
            _ => Guid.Empty
        };
        return guid != Guid.Empty;
    }

    private static string GetAccommodationId(Guid id) => id.ToString() switch
    {
        "42000000-0000-0000-0000-000000000001" => "bosvilla",
        "42000000-0000-0000-0000-000000000002" => "waterlodge",
        "42000000-0000-0000-0000-000000000003" => "heidehuisje",
        _ => string.Empty
    };
}
