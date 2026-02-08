namespace Api.Features.MetricGroups;

public record GetMetricGroupByIdResponse(Guid Id, string Name, bool IsActive);
