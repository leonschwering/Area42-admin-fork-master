namespace Area42_1.ApiService.Data.Repositories;

using Area42_1.ApiService.Models.Reservations;
using Microsoft.EntityFrameworkCore;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Reservation>> GetByAccommodationIdAsync(Guid accommodationId);
    Task<bool> IsAccommodationAvailableAsync(Guid accommodationId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null);
    Task<Reservation> CreateAsync(Reservation reservation);
    Task<Reservation> UpdateAsync(Reservation reservation);
    Task<bool> CancelAsync(Guid id);
}

public class ReservationRepository : IReservationRepository
{
    private readonly Area42Context _context;

    public ReservationRepository(Area42Context context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _context.Reservations.FindAsync(id);
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Reservations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByAccommodationIdAsync(Guid accommodationId)
    {
        return await _context.Reservations
            .Where(r => r.AccommodationId == accommodationId)
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<bool> IsAccommodationAvailableAsync(Guid accommodationId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.AccommodationId == accommodationId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.CheckInDate < checkOut &&
                       r.CheckOutDate > checkIn);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        reservation.Id = Guid.NewGuid();
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> UpdateAsync(Reservation reservation)
    {
        reservation.UpdatedAt = DateTime.UtcNow;
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task<bool> CancelAsync(Guid id)
    {
        var reservation = await GetByIdAsync(id);
        if (reservation == null || reservation.Status == ReservationStatus.Cancelled)
        {
            return false;
        }

        reservation.Status = ReservationStatus.Cancelled;
        await UpdateAsync(reservation);
        return true;
    }
}
