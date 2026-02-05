namespace Api.Features.Clients;

public record GetClientsResponse(Guid Id, string Name, string? IntegrationMetadata, string? ProductsModules, bool IsActive);
