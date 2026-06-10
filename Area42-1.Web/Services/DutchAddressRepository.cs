namespace Area42_1.Web.Services;

public class DutchAddressRepository
{
    private static readonly List<Models.DutchPostalCode> DutchAddresses = new()
    {
        // Amsterdam
        new() { PostalCode = "1012AB", Street = "Warmoesstraat", City = "Amsterdam", MinHouseNumber = 1, MaxHouseNumber = 150, Province = "North Holland" },
        new() { PostalCode = "1012JX", Street = "Zeedijk", City = "Amsterdam", MinHouseNumber = 1, MaxHouseNumber = 95, Province = "North Holland" },
        new() { PostalCode = "1013AC", Street = "Kalverstraat", City = "Amsterdam", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "North Holland" },
        new() { PostalCode = "1015CW", Street = "Geldersekade", City = "Amsterdam", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "North Holland" },
        new() { PostalCode = "1016AH", Street = "Herengracht", City = "Amsterdam", MinHouseNumber = 1, MaxHouseNumber = 600, Province = "North Holland" },

        // Rotterdam
        new() { PostalCode = "3011BA", Street = "Meent", City = "Rotterdam", MinHouseNumber = 1, MaxHouseNumber = 250, Province = "South Holland" },
        new() { PostalCode = "3011DB", Street = "Hoogstraat", City = "Rotterdam", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "South Holland" },
        new() { PostalCode = "3012CL", Street = "Blaaktuin", City = "Rotterdam", MinHouseNumber = 1, MaxHouseNumber = 150, Province = "South Holland" },
        new() { PostalCode = "3021AB", Street = "Westersingel", City = "Rotterdam", MinHouseNumber = 1, MaxHouseNumber = 300, Province = "South Holland" },

        // The Hague
        new() { PostalCode = "2511AA", Street = "Spui", City = "The Hague", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "South Holland" },
        new() { PostalCode = "2511AC", Street = "Plein", City = "The Hague", MinHouseNumber = 1, MaxHouseNumber = 150, Province = "South Holland" },
        new() { PostalCode = "2512AA", Street = "Groenmarkt", City = "The Hague", MinHouseNumber = 1, MaxHouseNumber = 80, Province = "South Holland" },
        new() { PostalCode = "2514AJ", Street = "Kneuterdijk", City = "The Hague", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "South Holland" },

        // Utrecht
        new() { PostalCode = "3511AS", Street = "Choorstraat", City = "Utrecht", MinHouseNumber = 1, MaxHouseNumber = 120, Province = "Utrecht" },
        new() { PostalCode = "3511AW", Street = "Domplein", City = "Utrecht", MinHouseNumber = 1, MaxHouseNumber = 50, Province = "Utrecht" },
        new() { PostalCode = "3512AB", Street = "Mariaplaats", City = "Utrecht", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "Utrecht" },
        new() { PostalCode = "3513CD", Street = "Wittevrouwenstraat", City = "Utrecht", MinHouseNumber = 1, MaxHouseNumber = 180, Province = "Utrecht" },

        // Eindhoven
        new() { PostalCode = "5611AB", Street = "Wilhelminaplein", City = "Eindhoven", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "North Brabant" },
        new() { PostalCode = "5611BH", Street = "Heuvelplein", City = "Eindhoven", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "North Brabant" },
        new() { PostalCode = "5611DA", Street = "Catharina van Rij-Bouwmanstraat", City = "Eindhoven", MinHouseNumber = 1, MaxHouseNumber = 50, Province = "North Brabant" },
        new() { PostalCode = "5612AA", Street = "Piazza", City = "Eindhoven", MinHouseNumber = 1, MaxHouseNumber = 80, Province = "North Brabant" },
        new() { PostalCode = "5613AJ", Street = "Emmasingel", City = "Eindhoven", MinHouseNumber = 1, MaxHouseNumber = 300, Province = "North Brabant" },

        // Groningen
        new() { PostalCode = "9711AB", Street = "Grote Markt", City = "Groningen", MinHouseNumber = 1, MaxHouseNumber = 150, Province = "Groningen" },
        new() { PostalCode = "9711AG", Street = "Poelestraat", City = "Groningen", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "Groningen" },
        new() { PostalCode = "9711BA", Street = "A-Straat", City = "Groningen", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "Groningen" },
        new() { PostalCode = "9712AA", Street = "Gedempte Zuiderdiep", City = "Groningen", MinHouseNumber = 1, MaxHouseNumber = 250, Province = "Groningen" },

        // Zwolle
        new() { PostalCode = "8011AA", Street = "Sassenstraat", City = "Zwolle", MinHouseNumber = 1, MaxHouseNumber = 80, Province = "Overijssel" },
        new() { PostalCode = "8011AF", Street = "Melkmarkt", City = "Zwolle", MinHouseNumber = 1, MaxHouseNumber = 100, Province = "Overijssel" },
        new() { PostalCode = "8012LL", Street = "Thorbeckerplein", City = "Zwolle", MinHouseNumber = 1, MaxHouseNumber = 120, Province = "Overijssel" },

        // Arnhem
        new() { PostalCode = "6811AB", Street = "Rijnkade", City = "Arnhem", MinHouseNumber = 1, MaxHouseNumber = 200, Province = "Gelderland" },
        new() { PostalCode = "6811AC", Street = "Modekwartier", City = "Arnhem", MinHouseNumber = 1, MaxHouseNumber = 150, Province = "Gelderland" },
        new() { PostalCode = "6812AE", Street = "Willemsplein", City = "Arnhem", MinHouseNumber = 1, MaxHouseNumber = 80, Province = "Gelderland" },
    };

    public async Task<Models.DutchPostalCode?> LookupByPostalCodeAndHouseNumberAsync(string postalCode, string houseNumber)
    {
        try
        {
            // Clean input
            var cleanPostal = postalCode.Replace(" ", "").ToUpper();
            if (!int.TryParse(houseNumber.Trim(), out var houseNum))
            {
                return null;
            }

            // Search in-memory database
            var result = DutchAddresses.FirstOrDefault(a =>
                a.PostalCode.Equals(cleanPostal, StringComparison.OrdinalIgnoreCase) &&
                houseNum >= a.MinHouseNumber &&
                houseNum <= a.MaxHouseNumber);

            return await Task.FromResult(result);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Models.DutchPostalCode>> SearchByStreetAndCityAsync(string street, string city)
    {
        try
        {
            var results = DutchAddresses
                .Where(a =>
                    a.Street.Contains(street, StringComparison.OrdinalIgnoreCase) &&
                    a.City.Contains(city, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return await Task.FromResult(results);
        }
        catch
        {
            return new List<Models.DutchPostalCode>();
        }
    }

    public async Task<List<string>> GetAllCitiesAsync()
    {
        var cities = DutchAddresses
            .Select(a => a.City)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        return await Task.FromResult(cities);
    }
}
