namespace Api.Features.Tags;

public record CreateTagRequest(string Name);

public record CreateTagResponse(Guid Id, string Name);
