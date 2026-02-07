namespace Api.Features.Clients;

public record UpdateClientRequest(string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode, bool IsActive);

public record UpdateClientResponse(Guid Id, string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode, bool IsActive, DateTime CreatedOn);
