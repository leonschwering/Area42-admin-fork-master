namespace Area42_1.Web.Services;

public class AccommodationApiClient
{
    private readonly HttpClient _httpClient;

    public AccommodationApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Area42API");
    }

    public async Task<List<AccommodationDto>?> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<AccommodationDto>>("/api/accommodations");
        }
        catch
        {
            return null;
        }
    }

    public async Task<AccommodationDto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AccommodationDto>($"/api/accommodations/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AccommodationDto>?> GetByTypeAsync(string type)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<AccommodationDto>>($"/api/accommodations/type/{type}");
        }
        catch
        {
            return null;
        }
    }
}

public class AccommodationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
