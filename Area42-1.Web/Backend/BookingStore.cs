using System.Text.Json;

namespace Area42_1.Web.Backend;

public sealed class BookingStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _dataFile;
    private readonly ILogger<BookingStore> _logger;
    private List<Booking>? _bookings;

    public BookingStore(IWebHostEnvironment environment, ILogger<BookingStore> logger)
    {
        _logger = logger;
        _dataFile = Path.Combine(environment.ContentRootPath, "App_Data", "bookings.json");
    }

    public IReadOnlyList<Accommodation> Accommodations { get; } =
    [
        new("bosvilla", "Bosvilla", "Ruime villa aan de bosrand voor het hele gezin.", 4, 2, 129m, "cabin-one"),
        new("waterlodge", "Waterlodge", "Comfortabele lodge aan het water met extra leefruimte.", 6, 3, 169m, "cabin-two"),
        new("heidehuisje", "Heidehuisje", "Knus huisje voor twee, midden in het groen.", 2, 1, 99m, "cabin-three")
    ];

    public Accommodation? GetAccommodation(string id) =>
        Accommodations.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<Booking>> GetBookingsAsync()
    {
        await EnsureLoadedAsync();
        return _bookings!
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .Select(Clone)
            .ToList();
    }

    public async Task<bool> IsAvailableAsync(
        string accommodationId,
        DateOnly checkIn,
        DateOnly checkOut)
    {
        await EnsureLoadedAsync();
        return IsAvailable(accommodationId, checkIn, checkOut);
    }

    public async Task<BookingResult> CreateAsync(BookingRequest request)
    {
        await EnsureLoadedAsync();

        var accommodation = GetAccommodation(request.AccommodationId);
        if (accommodation is null)
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
            return new(false, null, $"{accommodation.Name} is geschikt voor maximaal {accommodation.MaxGuests} gasten.");
        }

        await _gate.WaitAsync();
        try
        {
            if (!IsAvailable(accommodation.Id, checkIn, checkOut))
            {
                return new(false, null, "Dit verblijf is niet beschikbaar voor de gekozen periode.");
            }

            var nights = checkOut.DayNumber - checkIn.DayNumber;
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Reference = $"A42-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}",
                AccommodationId = accommodation.Id,
                AccommodationName = accommodation.Name,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Guests = request.Guests,
                Nights = nights,
                TotalPrice = nights * accommodation.PricePerNight,
                Status = BookingStatus.Confirmed,
                GuestName = request.GuestName.Trim(),
                GuestEmail = request.GuestEmail.Trim(),
                GuestPhone = request.GuestPhone.Trim(),
                Notes = request.Notes.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _bookings!.Add(booking);
            await SaveAsync();
            return new(true, Clone(booking), null);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> SetStatusAsync(Guid id, BookingStatus status)
    {
        await EnsureLoadedAsync();
        await _gate.WaitAsync();
        try
        {
            var booking = _bookings!.FirstOrDefault(item => item.Id == id);
            if (booking is null)
            {
                return false;
            }

            booking.Status = status;
            await SaveAsync();
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        await EnsureLoadedAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        return new(
            _bookings!.Count,
            _bookings.Count(item => item.CheckIn >= today && item.Status != BookingStatus.Cancelled),
            _bookings.Count(item => item.Status == BookingStatus.Pending),
            _bookings.Count(item => item.Status == BookingStatus.Cancelled),
            _bookings
                .Where(item => item.Status == BookingStatus.Confirmed)
                .Sum(item => item.TotalPrice));
    }

    private bool IsAvailable(string accommodationId, DateOnly checkIn, DateOnly checkOut) =>
        !_bookings!.Any(booking =>
            booking.AccommodationId == accommodationId &&
            booking.Status != BookingStatus.Cancelled &&
            booking.CheckIn < checkOut &&
            booking.CheckOut > checkIn);

    private async Task EnsureLoadedAsync()
    {
        if (_bookings is not null)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (_bookings is not null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_dataFile)!);
            if (!File.Exists(_dataFile))
            {
                _bookings = CreateDemoBookings();
                await SaveAsync();
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_dataFile);
                _bookings = JsonSerializer.Deserialize<List<Booking>>(json, JsonOptions) ?? [];
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Booking data could not be read. Starting with demo data.");
                _bookings = CreateDemoBookings();
                await SaveAsync();
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_bookings, JsonOptions);
        await File.WriteAllTextAsync(_dataFile, json);
    }

    private static List<Booking> CreateDemoBookings()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return
        [
            new()
            {
                Id = Guid.NewGuid(),
                Reference = "A42-DEMO-1042",
                AccommodationId = "bosvilla",
                AccommodationName = "Bosvilla",
                CheckIn = today.AddDays(12),
                CheckOut = today.AddDays(16),
                Guests = 4,
                Nights = 4,
                TotalPrice = 516m,
                Status = BookingStatus.Confirmed,
                GuestName = "Familie De Jong",
                GuestEmail = "demo@area42.nl",
                GuestPhone = "06 12 34 56 78",
                Notes = "Graag een kinderstoel klaarzetten.",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Reference = "A42-DEMO-2042",
                AccommodationId = "heidehuisje",
                AccommodationName = "Heidehuisje",
                CheckIn = today.AddDays(28),
                CheckOut = today.AddDays(31),
                Guests = 2,
                Nights = 3,
                TotalPrice = 297m,
                Status = BookingStatus.Pending,
                GuestName = "Sanne Bakker",
                GuestEmail = "sanne@example.nl",
                GuestPhone = "06 87 65 43 21",
                CreatedAtUtc = DateTime.UtcNow.AddHours(-5)
            }
        ];
    }

    private static Booking Clone(Booking source) => new()
    {
        Id = source.Id,
        Reference = source.Reference,
        AccommodationId = source.AccommodationId,
        AccommodationName = source.AccommodationName,
        CheckIn = source.CheckIn,
        CheckOut = source.CheckOut,
        Guests = source.Guests,
        Nights = source.Nights,
        TotalPrice = source.TotalPrice,
        Status = source.Status,
        GuestName = source.GuestName,
        GuestEmail = source.GuestEmail,
        GuestPhone = source.GuestPhone,
        Notes = source.Notes,
        CreatedAtUtc = source.CreatedAtUtc
    };
}
