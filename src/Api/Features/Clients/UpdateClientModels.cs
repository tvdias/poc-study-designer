namespace Api.Features.Clients;

public record UpdateClientRequest(string Name, string? IntegrationMetadata, string? ProductsModules, bool IsActive);

public record UpdateClientResponse(Guid Id, string Name, string? IntegrationMetadata, string? ProductsModules, bool IsActive);
