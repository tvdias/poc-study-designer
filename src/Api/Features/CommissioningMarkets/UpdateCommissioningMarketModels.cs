namespace Api.Features.CommissioningMarkets;

public record UpdateCommissioningMarketRequest(string IsoCode, string Name, bool IsActive);
public record UpdateCommissioningMarketResponse(Guid Id, string IsoCode, string Name, bool IsActive);
