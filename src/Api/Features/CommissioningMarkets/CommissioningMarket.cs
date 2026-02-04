using Api.Features.Shared;

namespace Api.Features.CommissioningMarkets;

public class CommissioningMarket : AuditableEntity
{
    public Guid Id { get; set; }
    public required string IsoCode { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
