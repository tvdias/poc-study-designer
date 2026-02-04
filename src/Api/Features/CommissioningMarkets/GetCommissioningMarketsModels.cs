namespace Api.Features.CommissioningMarkets;

public record GetCommissioningMarketsResponse(Guid Id, string IsoCode, string Name, bool IsActive);
