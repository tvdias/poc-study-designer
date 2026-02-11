namespace Api.Features.MetricGroups;

public record CreateMetricGroupRequest(string Name);
public record CreateMetricGroupResponse(Guid Id, string Name);
