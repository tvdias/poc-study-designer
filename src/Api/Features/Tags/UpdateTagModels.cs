namespace Api.Features.Tags;

public record UpdateTagRequest(string Name, bool IsActive);
public record UpdateTagResponse(Guid Id, string Name, bool IsActive);
