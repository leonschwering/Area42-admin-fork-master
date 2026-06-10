namespace Area42_1.ApiService.Services;

using Area42_1.ApiService.Data.Repositories;
using Area42_1.ApiService.Models.Pricing;
using Area42_1.ApiService.Models.Reservations;
using Area42_1.ApiService.Models.Accommodations;

public class ReservationRequest
{
    public Guid AccommodationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string SpecialRequests { get; set; } = string.Empty;
}

public interface IReservationService
{
    Task<Reservation?> GetReservationAsync(Guid id);
    Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId);
    Task<IEnumerable<Reservation>> GetAccommodationReservationsAsync(Guid accommodationId);
    Task<Reservation> CreateReservationAsync(ReservationRequest request, IAccommodationRepository accommodationRepo);
    Task<Reservation> UpdateReservationAsync(Guid id, Reservation reservation);
    Task<bool> CancelReservationAsync(Guid id);
    Task<bool> IsAvailableAsync(Guid accommodationId, DateTime checkIn, DateTime checkOut);
}

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;
    private readonly IAccommodationRepository _accommodationRepository;

    public ReservationService(IReservationRepository repository, IAccommodationRepository accommodationRepository)
    {
        _repository = repository;
        _accommodationRepository = accommodationRepository;
    }

    public async Task<Reservation?> GetReservationAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Reservation>> GetAccommodationReservationsAsync(Guid accommodationId)
    {
        return await _repository.GetByAccommodationIdAsync(accommodationId);
    }

    public async Task<Reservation> CreateReservationAsync(ReservationRequest request, IAccommodationRepository accommodationRepo)
    {
        // Validation
        if (request.CheckOutDate <= request.CheckInDate)
            throw new ArgumentException("Check-out date must be after check-in date.");

        if (request.NumberOfGuests < 1)
            throw new ArgumentException("At least one guest is required.");

        // Check availability
        var isAvailable = await _repository.IsAccommodationAvailableAsync(
            request.AccommodationId, 
            request.CheckInDate, 
            request.CheckOutDate
        );

        if (!isAvailable)
            throw new InvalidOperationException("Accommodation is not available for the selected dates.");

        // Get accommodation to validate max guests
        var accommodation = await accommodationRepo.GetByIdAsync(request.AccommodationId);
        if (accommodation == null)
            throw new InvalidOperationException("Accommodation not found.");

        if (request.NumberOfGuests > accommodation.MaxGuests)
            throw new ArgumentException($"Number of guests exceeds maximum capacity of {accommodation.MaxGuests}.");

        // Calculate price
        var numberOfNights = (request.CheckOutDate - request.CheckInDate).Days;
        var pricingStrategy = PricingStrategyFactory.CreateStrategy(accommodation.Type);
        var totalPrice = pricingStrategy.CalculatePrice(numberOfNights, request.NumberOfGuests, request.CheckInDate);

        // Create reservation
        var reservation = new Reservation
        {
            AccommodationId = request.AccommodationId,
            UserId = request.UserId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NumberOfGuests = request.NumberOfGuests,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Pending,
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            SpecialRequests = request.SpecialRequests
        };

        return await _repository.CreateAsync(reservation);
    }

    public async Task<Reservation> UpdateReservationAsync(Guid id, Reservation reservation)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new InvalidOperationException("Reservation not found.");

        existing.Status = reservation.Status;
        existing.SpecialRequests = reservation.SpecialRequests;

        return await _repository.UpdateAsync(existing);
    }

    public async Task<bool> CancelReservationAsync(Guid id)
    {
        return await _repository.CancelAsync(id);
    }

    public async Task<bool> IsAvailableAsync(Guid accommodationId, DateTime checkIn, DateTime checkOut)
    {
        return await _repository.IsAccommodationAvailableAsync(accommodationId, checkIn, checkOut);
    }
}
