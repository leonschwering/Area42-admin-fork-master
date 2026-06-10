namespace Area42_1.ApiService.Models.Pricing;

public class CampingSitePricingStrategy : IPricingStrategy
{
    private const decimal BasePricePerNight = 35m;
    private const decimal GuestSurcharge = 5m;
    private const decimal WeekendMultiplier = 1.3m;
    private const decimal MonthlyDiscount = 0.85m; // 15% discount for 30+ nights

    public decimal CalculatePrice(int numberOfNights, int numberOfGuests, DateTime checkInDate)
    {
        var basePrice = BasePricePerNight * numberOfNights;
        var guestPrice = (numberOfGuests - 1) * GuestSurcharge * numberOfNights;

        var totalPrice = basePrice + guestPrice;

        // Weekend multiplier
        if (IsWeekend(checkInDate))
        {
            totalPrice *= WeekendMultiplier;
        }

        // Monthly discount
        if (numberOfNights >= 30)
        {
            totalPrice *= MonthlyDiscount;
        }

        return totalPrice;
    }

    public string GetAccommodationType() => "Camping Site";

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
