namespace Api.Features.Clients;

public record CreateClientRequest(string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode);

public record CreateClientResponse(Guid Id, string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode, bool IsActive, DateTime CreatedOn);
