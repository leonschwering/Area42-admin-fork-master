namespace Area42_1.Web.Models;

public class DutchPostalCode
{
    public int Id { get; set; }
    public string PostalCode { get; set; } = string.Empty; // Format: 1234AB
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int MinHouseNumber { get; set; }
    public int MaxHouseNumber { get; set; }
    public string Province { get; set; } = string.Empty;
}
