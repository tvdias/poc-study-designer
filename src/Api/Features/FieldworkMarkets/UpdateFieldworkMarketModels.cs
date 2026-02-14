namespace Api.Features.FieldworkMarkets;

public record UpdateFieldworkMarketRequest(string IsoCode, string Name);
public record UpdateFieldworkMarketResponse(Guid Id, string IsoCode, string Name);
