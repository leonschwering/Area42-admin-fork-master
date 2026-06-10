namespace Area42_1.ApiService.Models.Pricing;

public class BungalowPricingStrategy : IPricingStrategy
{
    private const decimal BasePricePerNight = 150m;
    private const decimal GuestSurcharge = 25m;
    private const decimal WeekendMultiplier = 1.2m;

    public decimal CalculatePrice(int numberOfNights, int numberOfGuests, DateTime checkInDate)
    {
        var basePrice = BasePricePerNight * numberOfNights;
        var guestPrice = (numberOfGuests - 2) * GuestSurcharge * numberOfNights;

        var totalPrice = basePrice + guestPrice;

        // Weekend multiplier
        if (IsWeekend(checkInDate))
        {
            totalPrice *= WeekendMultiplier;
        }

        return totalPrice;
    }

    public string GetAccommodationType() => "Bungalow";

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
