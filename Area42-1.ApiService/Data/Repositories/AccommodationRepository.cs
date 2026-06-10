namespace Area42_1.ApiService.Data.Repositories;

using Area42_1.ApiService.Models.Accommodations;
using Microsoft.EntityFrameworkCore;

public interface IAccommodationRepository
{
    Task<Accommodation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Accommodation>> GetAllAsync();
    Task<IEnumerable<Accommodation>> GetByTypeAsync(AccommodationType type);
    Task<Accommodation> CreateAsync(Accommodation accommodation);
    Task<Accommodation> UpdateAsync(Accommodation accommodation);
    Task DeleteAsync(Guid id);
}

public class AccommodationRepository : IAccommodationRepository
{
    private readonly Area42Context _context;

    public AccommodationRepository(Area42Context context)
    {
        _context = context;
    }

    public async Task<Accommodation?> GetByIdAsync(Guid id)
    {
        return await _context.Accommodations.FindAsync(id);
    }

    public async Task<IEnumerable<Accommodation>> GetAllAsync()
    {
        return await _context.Accommodations.Where(a => a.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Accommodation>> GetByTypeAsync(AccommodationType type)
    {
        return await _context.Accommodations
            .Where(a => a.Type == type && a.IsActive)
            .ToListAsync();
    }

    public async Task<Accommodation> CreateAsync(Accommodation accommodation)
    {
        accommodation.Id = Guid.NewGuid();
        accommodation.CreatedAt = DateTime.UtcNow;
        accommodation.UpdatedAt = DateTime.UtcNow;

        _context.Accommodations.Add(accommodation);
        await _context.SaveChangesAsync();

        return accommodation;
    }

    public async Task<Accommodation> UpdateAsync(Accommodation accommodation)
    {
        accommodation.UpdatedAt = DateTime.UtcNow;
        _context.Accommodations.Update(accommodation);
        await _context.SaveChangesAsync();

        return accommodation;
    }

    public async Task DeleteAsync(Guid id)
    {
        var accommodation = await GetByIdAsync(id);
        if (accommodation != null)
        {
            accommodation.IsActive = false;
            await UpdateAsync(accommodation);
        }
    }
}
