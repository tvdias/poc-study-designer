using Api.Features.Shared;

namespace Api.Features.FieldworkMarkets;

public class FieldworkMarket : AuditableEntity
{
    public Guid Id { get; set; }
    public required string IsoCode { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
