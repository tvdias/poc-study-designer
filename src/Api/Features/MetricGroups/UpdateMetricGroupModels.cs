namespace Api.Features.MetricGroups;

public record UpdateMetricGroupRequest(string Name);
public record UpdateMetricGroupResponse(Guid Id, string Name);
