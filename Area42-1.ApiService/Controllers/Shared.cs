namespace Area42_1.ApiService.Controllers;

/// <summary>
/// Shared DTOs for API controllers
/// </summary>

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
