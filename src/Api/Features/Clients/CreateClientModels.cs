namespace Api.Features.Clients;

public record CreateClientRequest(string Name, string? IntegrationMetadata, string? ProductsModules);

public record CreateClientResponse(Guid Id, string Name, string? IntegrationMetadata, string? ProductsModules);
