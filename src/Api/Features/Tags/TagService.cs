using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Tags;

public interface ITagService
{
    Task<CreateTagResponse> CreateTagAsync(
        CreateTagRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<UpdateTagResponse> UpdateTagAsync(
        Guid id,
        UpdateTagRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task DeleteTagAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<GetTagsResponse>> GetTagsAsync(
        string? query,
        CancellationToken cancellationToken = default);

    Task<GetTagByIdResponse?> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagService> _logger;

    public TagService(ApplicationDbContext context, ILogger<TagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateTagResponse> CreateTagAsync(
        CreateTagRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating tag: {TagName}", request.Name);

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Tags.Add(tag);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Duplicate tag name: {TagName}", request.Name);
                throw new InvalidOperationException($"Tag '{request.Name}' already exists.", ex);
            }
            throw;
        }

        _logger.LogInformation("Successfully created tag {TagId}: {TagName}", tag.Id, tag.Name);

        return new CreateTagResponse(tag.Id, tag.Name);
    }

    public async Task<UpdateTagResponse> UpdateTagAsync(
        Guid id,
        UpdateTagRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating tag {TagId}", id);

        var tag = await _context.Tags.FindAsync([id], cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException($"Tag {id} not found.");
        }

        tag.Name = request.Name;
        tag.ModifiedOn = DateTime.UtcNow;
        tag.ModifiedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated tag {TagId}", id);

        return new UpdateTagResponse(tag.Id, tag.Name);
    }

    public async Task DeleteTagAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting tag {TagId}", id);

        var tag = await _context.Tags.FindAsync([id], cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException($"Tag {id} not found.");
        }

        tag.IsActive = false;
        tag.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted tag {TagId}", id);
    }

    public async Task<List<GetTagsResponse>> GetTagsAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var tagsQuery = _context.Tags.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            tagsQuery = tagsQuery.Where(t => EF.Functions.ILike(t.Name, pattern));
        }

        return await tagsQuery
            .Select(t => new GetTagsResponse(t.Id, t.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<GetTagByIdResponse?> GetTagByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _context.Tags
            .Where(t => t.IsActive)
            .Where(t => t.Id == id)
            .Select(t => new GetTagByIdResponse(t.Id, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return tag;
    }
}
