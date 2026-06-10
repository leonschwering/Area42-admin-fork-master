namespace Area42_1.ApiService.Models.Pricing;

public interface IPricingStrategy
{
    decimal CalculatePrice(int numberOfNights, int numberOfGuests, DateTime checkInDate);
    string GetAccommodationType();
}
