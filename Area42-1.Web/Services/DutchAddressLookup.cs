namespace Area42_1.Web.Services;

public class DutchAddressLookup
{
    private readonly HttpClient _httpClient;

    public DutchAddressLookup(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Lookup Dutch address by postal code and house number.
    /// Uses the open Dutch address database API.
    /// </summary>
    public async Task<DutchAddress?> LookupAsync(string postalCode, string houseNumber)
    {
        try
        {
            // Remove spaces from postal code (format: 1234AB)
            var cleanPostal = postalCode.Replace(" ", "").ToUpper();
            var cleanHouseNum = houseNumber.Trim();

            if (string.IsNullOrWhiteSpace(cleanPostal) || string.IsNullOrWhiteSpace(cleanHouseNum))
            {
                return null;
            }

            // Using pdok.nl API (Dutch national geo-information service)
            // This is a free, open API for Dutch addresses
            var url = $"https://nominatim.openstreetmap.org/search?postalcode={cleanPostal}&housenumber={cleanHouseNum}&country=Netherlands&format=json&limit=1";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var results = System.Text.Json.JsonDocument.Parse(json);
            var root = results.RootElement;

            if (root.GetArrayLength() == 0)
            {
                return null;
            }

            var first = root[0];
            var address = first.GetProperty("address");

            return new DutchAddress
            {
                PostalCode = cleanPostal,
                HouseNumber = cleanHouseNum,
                Street = address.TryGetProperty("road", out var road) ? road.GetString() ?? "" : "",
                City = address.TryGetProperty("city", out var city) ? city.GetString() ?? "" : "",
                Country = "Netherlands"
            };
        }
        catch
        {
            // If lookup fails, return null and let user enter manually
            return null;
        }
    }
}

public class DutchAddress
{
    public string PostalCode { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "Netherlands";
}
