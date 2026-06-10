namespace Area42_1.ApiService.Services;

using Area42_1.ApiService.Data.Repositories;
using Area42_1.ApiService.Models.Accommodations;
using Area42_1.ApiService.Models.Pricing;

public interface IAccommodationService
{
    Task<Accommodation?> GetAccommodationAsync(Guid id);
    Task<IEnumerable<Accommodation>> GetAllAccommodationsAsync();
    Task<IEnumerable<Accommodation>> GetAccommodationsByTypeAsync(AccommodationType type);
    Task<Accommodation> CreateAccommodationAsync(Accommodation accommodation);
    Task<Accommodation> UpdateAccommodationAsync(Accommodation accommodation);
    Task<bool> DeleteAccommodationAsync(Guid id);
}

public class AccommodationService : IAccommodationService
{
    private readonly IAccommodationRepository _repository;

    public AccommodationService(IAccommodationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Accommodation?> GetAccommodationAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Accommodation>> GetAllAccommodationsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Accommodation>> GetAccommodationsByTypeAsync(AccommodationType type)
    {
        return await _repository.GetByTypeAsync(type);
    }

    public async Task<Accommodation> CreateAccommodationAsync(Accommodation accommodation)
    {
        if (string.IsNullOrWhiteSpace(accommodation.Name))
            throw new ArgumentException("Accommodation name is required.");

        return await _repository.CreateAsync(accommodation);
    }

    public async Task<Accommodation> UpdateAccommodationAsync(Accommodation accommodation)
    {
        var existing = await _repository.GetByIdAsync(accommodation.Id);
        if (existing == null)
            throw new InvalidOperationException("Accommodation not found.");

        return await _repository.UpdateAsync(accommodation);
    }

    public async Task<bool> DeleteAccommodationAsync(Guid id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
