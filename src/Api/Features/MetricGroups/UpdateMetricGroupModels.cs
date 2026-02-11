namespace Api.Features.MetricGroups;

public record UpdateMetricGroupRequest(string Name, bool IsActive);
public record UpdateMetricGroupResponse(Guid Id, string Name, bool IsActive);
