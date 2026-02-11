namespace Api.Features.Clients;

public record GetClientsResponse(Guid Id, string AccountName, string? CompanyNumber, string? CustomerNumber, string? CompanyCode, DateTime CreatedOn);
