namespace Api.Features.Clients;

public record GetClientsResponse(List<ClientSummary> Clients);

public record ClientSummary(
    Guid Id,
    string AccountName,
    string? CompanyNumber,
    string? CustomerNumber,
    string? CompanyCode,
    DateTime CreatedOn,
    string CreatedBy);
