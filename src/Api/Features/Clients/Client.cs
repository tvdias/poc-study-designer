using Api.Features.Shared;

namespace Api.Features.Clients;

public class Client : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? IntegrationMetadata { get; set; }
    public string? ProductsModules { get; set; }
    public bool IsActive { get; set; } = true;
}
