namespace Api.Features.Clients;

public record UpdateClientRequest(string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode);

public record UpdateClientResponse(Guid Id, string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode, DateTime CreatedOn);
