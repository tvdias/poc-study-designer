namespace Api.Features.FieldworkMarkets;

public record CreateFieldworkMarketRequest(string IsoCode, string Name);

public record CreateFieldworkMarketResponse(Guid Id, string IsoCode, string Name);
