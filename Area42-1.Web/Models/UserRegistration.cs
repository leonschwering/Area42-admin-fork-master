namespace Area42_1.Web.Models;

public class UserRegistration
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Tussenvoegsel { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string? HouseLetter { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "Netherlands";
    public char Gender { get; set; } // 'M' or 'F'
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    public string FullName => string.IsNullOrWhiteSpace(Tussenvoegsel)
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {Tussenvoegsel} {LastName}";

    public string FullAddress => string.IsNullOrWhiteSpace(HouseLetter)
        ? $"{Street} {HouseNumber}, {PostalCode} {City}"
        : $"{Street} {HouseNumber}{HouseLetter}, {PostalCode} {City}";
}
