using Api.Features.Shared;

namespace Api.Features.Tags;

public class Tag : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
