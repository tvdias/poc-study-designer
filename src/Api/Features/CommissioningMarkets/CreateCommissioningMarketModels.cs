namespace Api.Features.CommissioningMarkets;

public record CreateCommissioningMarketRequest(string IsoCode, string Name);

public record CreateCommissioningMarketResponse(Guid Id, string IsoCode, string Name);
