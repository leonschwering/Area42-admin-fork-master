namespace Area42_1.Web.Services;

using System.Text.Json;

public class ReservationApiClient
{
    private readonly HttpClient _httpClient;

    public ReservationApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Area42API");
    }

    public async Task<ReservationDto?> CreateAsync(CreateReservationRequest request, string token)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/reservations");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            requestMessage.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ReservationDto>(content);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ReservationDto>?> GetUserReservationsAsync(Guid userId, string token)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/reservations/user/{userId}");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ReservationDto>>(content);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CheckAvailabilityAsync(Guid accommodationId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/reservations/availability/check?accommodationId={accommodationId}&checkIn={checkIn:O}&checkOut={checkOut:O}"
            );

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AvailabilityResponse>(content);
                return result?.Available ?? false;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CancelAsync(Guid reservationId, string token)
    {
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/reservations/{reservationId}/cancel");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class CreateReservationRequest
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

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid AccommodationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string SpecialRequests { get; set; } = string.Empty;
}

public class AvailabilityResponse
{
    public bool Available { get; set; }
}
