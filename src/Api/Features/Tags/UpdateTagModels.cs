namespace Api.Features.Tags;

public record UpdateTagRequest(string Name);
public record UpdateTagResponse(Guid Id, string Name);
