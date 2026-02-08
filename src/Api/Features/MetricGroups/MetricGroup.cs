using Api.Features.Shared;

namespace Api.Features.MetricGroups;

public class MetricGroup : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
