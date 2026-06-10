namespace Area42_1.ApiService.Models.Pricing;

public class ChaletPricingStrategy : IPricingStrategy
{
    private const decimal BasePricePerNight = 200m;
    private const decimal GuestSurcharge = 35m;
    private const decimal WeekendMultiplier = 1.15m;
    private const decimal WeeklyDiscount = 0.9m; // 10% discount for 7+ nights

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

        // Weekly discount
        if (numberOfNights >= 7)
        {
            totalPrice *= WeeklyDiscount;
        }

        return totalPrice;
    }

    public string GetAccommodationType() => "Chalet";

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
