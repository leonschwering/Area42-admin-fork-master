namespace Area42_1.ApiService.Models.Pricing;

using Area42_1.ApiService.Models.Accommodations;

public class PricingStrategyFactory
{
    public static IPricingStrategy CreateStrategy(AccommodationType type)
    {
        return type switch
        {
            AccommodationType.Bungalow => new BungalowPricingStrategy(),
            AccommodationType.Chalet => new ChaletPricingStrategy(),
            AccommodationType.CampingSite => new CampingSitePricingStrategy(),
            _ => throw new ArgumentException($"Unknown accommodation type: {type}")
        };
    }
}
