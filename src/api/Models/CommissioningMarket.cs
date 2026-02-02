namespace Api.Models;

public class CommissioningMarket : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty; // ISO country code
}
