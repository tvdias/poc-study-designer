namespace Api.Features.MetricGroups;

public record GetMetricGroupsResponse(Guid Id, string Name, bool IsActive);
