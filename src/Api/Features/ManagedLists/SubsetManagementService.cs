using Api.Data;
using Api.Features.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.ManagedLists;

public interface ISubsetManagementService
{
    Task<SaveQuestionSelectionResponse> SaveQuestionSelectionAsync(
        SaveQuestionSelectionRequest request,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task<GetSubsetDetailsResponse?> GetSubsetDetailsAsync(
        Guid subsetDefinitionId,
        CancellationToken cancellationToken = default);
    
    Task<GetSubsetsForProjectResponse> GetSubsetsForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
    
    Task<DeleteSubsetResponse> DeleteSubsetAsync(
        Guid subsetDefinitionId,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task RefreshQuestionDisplaysAsync(
        Guid subsetDefinitionId,
        CancellationToken cancellationToken = default);
    
    Task<ProjectSubsetSummaryResponse> RefreshProjectSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
    
    Task InvalidateSubsetsForItemAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default);
}

public class SubsetManagementService : ISubsetManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubsetManagementService> _logger;

    public SubsetManagementService(
        ApplicationDbContext context,
        ILogger<SubsetManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SaveQuestionSelectionResponse> SaveQuestionSelectionAsync(
        SaveQuestionSelectionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing question selection for ProjectId={ProjectId}, QuestionnaireLineId={QuestionnaireLineId}, ManagedListId={ManagedListId}",
            request.ProjectId, request.QuestionnaireLineId, request.ManagedListId);

        // Validate project exists and is in Draft state
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project {request.ProjectId} not found");
        }

        // Check if project is in Draft status (allow edits only in Draft)
        if (project.Status != ProjectStatus.Draft)
        {
            throw new InvalidOperationException($"This project is read-only. Create a new version to edit subsets. Current status: {project.Status}");
        }

        // Validate questionnaire line exists
        var questionnaireLine = await _context.QuestionnaireLines
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionnaireLineId, cancellationToken);

        if (questionnaireLine == null)
        {
            throw new InvalidOperationException($"QuestionnaireLine {request.QuestionnaireLineId} not found");
        }

        // Validate managed list exists and belongs to the project
        var managedList = await _context.ManagedLists
            .AsNoTracking()
            .FirstOrDefaultAsync(ml => ml.Id == request.ManagedListId && ml.ProjectId == request.ProjectId, cancellationToken);

        if (managedList == null)
        {
            throw new InvalidOperationException($"ManagedList {request.ManagedListId} not found or does not belong to project {request.ProjectId}");
        }

        // Validate all selected items exist and belong to the managed list
        var selectedItemIds = request.SelectedManagedListItemIds.Distinct().ToList();
        if (selectedItemIds.Count == 0)
        {
            throw new ArgumentException("Selection cannot be empty. Select at least one entity or use the full list.");
        }

        var validItems = await _context.ManagedListItems
            .AsNoTracking()
            .Where(mli => mli.ManagedListId == request.ManagedListId && selectedItemIds.Contains(mli.Id))
            .ToListAsync(cancellationToken);

        if (validItems.Count != selectedItemIds.Count)
        {
            throw new ArgumentException("One or more selected items are invalid for this managed list");
        }

        // Get all active items for this managed list in the project context
        var allActiveItems = await _context.ManagedListItems
            .AsNoTracking()
            .Where(mli => mli.ManagedListId == request.ManagedListId && mli.IsActive)
            .Select(mli => mli.Id)
            .ToListAsync(cancellationToken);

        // Check if this is a full selection
        var isFullSelection = selectedItemIds.Count == allActiveItems.Count && 
                              selectedItemIds.All(id => allActiveItems.Contains(id));

        Guid? subsetDefinitionId = null;
        string? subsetName = null;

        if (isFullSelection)
        {
            // Full selection - clear any existing subset link
            var existingLink = await _context.QuestionSubsetLinks
                .FirstOrDefaultAsync(
                    qsl => qsl.QuestionnaireLineId == request.QuestionnaireLineId && 
                           qsl.ManagedListId == request.ManagedListId,
                    cancellationToken);

            if (existingLink != null)
            {
                existingLink.SubsetDefinitionId = null;
                existingLink.ModifiedOn = DateTime.UtcNow;
                existingLink.ModifiedBy = userId;
            }
            else
            {
                // Create link without subset
                var newLink = new QuestionSubsetLink
                {
                    Id = Guid.NewGuid(),
                    ProjectId = request.ProjectId,
                    QuestionnaireLineId = request.QuestionnaireLineId,
                    ManagedListId = request.ManagedListId,
                    SubsetDefinitionId = null,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.QuestionSubsetLinks.Add(newLink);
            }

            _logger.LogInformation("Full selection detected - cleared subset link");
        }
        else
        {
            // Partial selection - compute signature and create or reuse subset
            var signature = SubsetSignatureBuilder.BuildSignature(selectedItemIds);

            // Check if subset with this signature already exists for this project + managed list
            var existingSubset = await _context.SubsetDefinitions
                .Include(sd => sd.Memberships)
                .FirstOrDefaultAsync(
                    sd => sd.ProjectId == request.ProjectId && 
                          sd.ManagedListId == request.ManagedListId && 
                          sd.SignatureHash == signature,
                    cancellationToken);

            if (existingSubset != null)
            {
                // Reuse existing subset
                subsetDefinitionId = existingSubset.Id;
                subsetName = existingSubset.Name;
                _logger.LogInformation("Reusing existing subset: {SubsetName} (ID: {SubsetId})", subsetName, subsetDefinitionId);
            }
            else
            {
                // Create new subset
                var newSubsetName = await GenerateNextSubsetNameAsync(
                    request.ProjectId,
                    request.ManagedListId,
                    managedList.Name,
                    cancellationToken);

                var newSubset = new SubsetDefinition
                {
                    Id = Guid.NewGuid(),
                    ProjectId = request.ProjectId,
                    ManagedListId = request.ManagedListId,
                    Name = newSubsetName,
                    SignatureHash = signature,
                    Status = SubsetStatus.Active,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                };

                // Add memberships
                foreach (var itemId in selectedItemIds)
                {
                    newSubset.Memberships.Add(new SubsetMembership
                    {
                        Id = Guid.NewGuid(),
                        SubsetDefinitionId = newSubset.Id,
                        ManagedListItemId = itemId,
                        CreatedOn = DateTime.UtcNow
                    });
                }

                _context.SubsetDefinitions.Add(newSubset);

                subsetDefinitionId = newSubset.Id;
                subsetName = newSubset.Name;
                _logger.LogInformation("Created new subset: {SubsetName} (ID: {SubsetId})", subsetName, subsetDefinitionId);
            }

            // Update or create question subset link
            var existingLink = await _context.QuestionSubsetLinks
                .FirstOrDefaultAsync(
                    qsl => qsl.QuestionnaireLineId == request.QuestionnaireLineId && 
                           qsl.ManagedListId == request.ManagedListId,
                    cancellationToken);

            if (existingLink != null)
            {
                existingLink.SubsetDefinitionId = subsetDefinitionId;
                existingLink.ModifiedOn = DateTime.UtcNow;
                existingLink.ModifiedBy = userId;
            }
            else
            {
                var newLink = new QuestionSubsetLink
                {
                    Id = Guid.NewGuid(),
                    ProjectId = request.ProjectId,
                    QuestionnaireLineId = request.QuestionnaireLineId,
                    ManagedListId = request.ManagedListId,
                    SubsetDefinitionId = subsetDefinitionId,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.QuestionSubsetLinks.Add(newLink);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Trigger refresh after save (AC-SYNC-01)
        if (subsetDefinitionId.HasValue)
        {
            await RefreshQuestionDisplaysAsync(subsetDefinitionId.Value, cancellationToken);
        }
        
        // Refresh project summary (AC-SYNC-02)
        await RefreshProjectSummaryAsync(request.ProjectId, cancellationToken);

        return new SaveQuestionSelectionResponse(
            request.QuestionnaireLineId,
            request.ManagedListId,
            isFullSelection,
            subsetDefinitionId,
            subsetName
        );
    }

    public async Task<GetSubsetDetailsResponse?> GetSubsetDetailsAsync(
        Guid subsetDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var subset = await _context.SubsetDefinitions
            .Include(sd => sd.Memberships)
                .ThenInclude(m => m.ManagedListItem)
            .Include(sd => sd.ManagedList)
            .AsNoTracking()
            .FirstOrDefaultAsync(sd => sd.Id == subsetDefinitionId, cancellationToken);

        if (subset == null)
        {
            return null;
        }

        var members = subset.Memberships
            .Select(m => new SubsetMembershipDto(
                m.ManagedListItemId,
                m.ManagedListItem.Code,
                m.ManagedListItem.Label,
                m.ManagedListItem.SortOrder
            ))
            .OrderBy(m => m.SortOrder)
            .ToList();

        return new GetSubsetDetailsResponse(
            subset.Id,
            subset.ProjectId,
            subset.ManagedListId,
            subset.ManagedList.Name,
            subset.Name,
            subset.SignatureHash,
            subset.Status,
            members,
            subset.CreatedOn,
            subset.CreatedBy ?? "system"
        );
    }

    public async Task<GetSubsetsForProjectResponse> GetSubsetsForProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var subsets = await _context.SubsetDefinitions
            .Include(sd => sd.ManagedList)
            .Include(sd => sd.Memberships)
            .AsNoTracking()
            .Where(sd => sd.ProjectId == projectId)
            .OrderBy(sd => sd.ManagedListId)
            .ThenBy(sd => sd.CreatedOn)
            .ToListAsync(cancellationToken);

        var summaries = subsets
            .Select(sd => new SubsetSummaryDto(
                sd.Id,
                sd.ManagedListId,
                sd.ManagedList.Name,
                sd.Name,
                sd.Memberships.Count,
                sd.CreatedOn
            ))
            .ToList();

        return new GetSubsetsForProjectResponse(summaries);
    }

    private async Task<string> GenerateNextSubsetNameAsync(
        Guid projectId,
        Guid managedListId,
        string managedListName,
        CancellationToken cancellationToken)
    {
        // Get the maximum suffix number for this project + managed list
        var existingSubsets = await _context.SubsetDefinitions
            .AsNoTracking()
            .Where(sd => sd.ProjectId == projectId && sd.ManagedListId == managedListId)
            .Select(sd => sd.Name)
            .ToListAsync(cancellationToken);

        var maxSuffix = 0;
        var prefix = $"{managedListName}_SUB";

        foreach (var name in existingSubsets)
        {
            if (name.StartsWith(prefix))
            {
                var suffixPart = name.Substring(prefix.Length);
                if (int.TryParse(suffixPart, out var suffix) && suffix > maxSuffix)
                {
                    maxSuffix = suffix;
                }
            }
        }

        var nextSuffix = maxSuffix + 1;
        return $"{prefix}{nextSuffix}";
    }

    public async Task<DeleteSubsetResponse> DeleteSubsetAsync(
        Guid subsetDefinitionId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting subset {SubsetId}", subsetDefinitionId);

        var subset = await _context.SubsetDefinitions
            .Include(sd => sd.QuestionLinks)
            .Include(sd => sd.Memberships)
            .Include(sd => sd.Project)
            .FirstOrDefaultAsync(sd => sd.Id == subsetDefinitionId, cancellationToken);

        if (subset == null)
        {
            throw new InvalidOperationException($"Subset {subsetDefinitionId} not found");
        }

        // Validate project is in Draft state
        if (subset.Project.Status != ProjectStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot delete subset in read-only project. Current status: {subset.Project.Status}");
        }

        var questionIds = subset.QuestionLinks.Select(ql => ql.QuestionnaireLineId).ToList();
        var projectId = subset.ProjectId;

        // Clear subset links (set SubsetDefinitionId to null for full selection fallback)
        foreach (var link in subset.QuestionLinks)
        {
            link.SubsetDefinitionId = null;
            link.ModifiedOn = DateTime.UtcNow;
            link.ModifiedBy = userId;
        }

        // Remove memberships and subset definition
        _context.SubsetMemberships.RemoveRange(subset.Memberships);
        _context.SubsetDefinitions.Remove(subset);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted subset {SubsetId}, cleared {QuestionCount} question links", 
            subsetDefinitionId, questionIds.Count);

        return new DeleteSubsetResponse(subsetDefinitionId, questionIds);
    }

    public async Task RefreshQuestionDisplaysAsync(
        Guid subsetDefinitionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing question displays for subset {SubsetId}", subsetDefinitionId);

        // Find all questions using this subset
        var questionIds = await _context.QuestionSubsetLinks
            .AsNoTracking()
            .Where(qsl => qsl.SubsetDefinitionId == subsetDefinitionId)
            .Select(qsl => qsl.QuestionnaireLineId)
            .ToListAsync(cancellationToken);

        // In a real implementation, this would trigger UI refresh events (e.g., SignalR)
        // For now, we log the questions that need refreshing
        _logger.LogInformation("Identified {QuestionCount} questions for refresh: {QuestionIds}", 
            questionIds.Count, string.Join(", ", questionIds));

        // Future enhancement: Publish refresh events to SignalR hub or message queue
        // await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("RefreshQuestions", questionIds);
    }

    public async Task<ProjectSubsetSummaryResponse> RefreshProjectSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing project summary for {ProjectId}", projectId);

        // Get all subsets for the project with their details
        var subsets = await _context.SubsetDefinitions
            .Include(sd => sd.ManagedList)
            .Include(sd => sd.Memberships)
                .ThenInclude(m => m.ManagedListItem)
            .Include(sd => sd.QuestionLinks)
            .AsNoTracking()
            .Where(sd => sd.ProjectId == projectId && sd.Status == SubsetStatus.Active)
            .OrderBy(sd => sd.ManagedListId)
            .ThenBy(sd => sd.Name)
            .ToListAsync(cancellationToken);

        // Get all managed lists for the project to calculate full vs partial
        var managedListCounts = await _context.ManagedListItems
            .AsNoTracking()
            .Where(mli => mli.ManagedList.ProjectId == projectId && mli.IsActive)
            .GroupBy(mli => mli.ManagedListId)
            .Select(g => new { ManagedListId = g.Key, TotalCount = g.Count() })
            .ToDictionaryAsync(x => x.ManagedListId, x => x.TotalCount, cancellationToken);

        var summaries = subsets.Select(sd =>
        {
            var totalItemsInList = managedListCounts.GetValueOrDefault(sd.ManagedListId, 0);
            var memberCount = sd.Memberships.Count;
            var isFull = memberCount == totalItemsInList;
            var memberLabels = sd.Memberships
                .OrderBy(m => m.ManagedListItem.SortOrder)
                .Select(m => m.ManagedListItem.Label)
                .ToList();
            var questionCount = sd.QuestionLinks.Count;

            return new SubsetDetailSummaryDto(
                sd.Id,
                sd.ManagedListId,
                sd.ManagedList.Name,
                sd.Name,
                memberCount,
                totalItemsInList,
                isFull,
                memberLabels,
                questionCount,
                sd.CreatedOn
            );
        }).ToList();

        _logger.LogInformation("Generated summary for {SubsetCount} subsets in project {ProjectId}", 
            summaries.Count, projectId);

        return new ProjectSubsetSummaryResponse(projectId, summaries);
    }

    public async Task InvalidateSubsetsForItemAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating subsets for ManagedListItem {ItemId}", managedListItemId);

        // Find the managed list item
        var item = await _context.ManagedListItems
            .Include(mli => mli.ManagedList)
                .ThenInclude(ml => ml.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(mli => mli.Id == managedListItemId, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("ManagedListItem {ItemId} not found", managedListItemId);
            return;
        }

        // Only process if project is in Draft state
        if (item.ManagedList.Project.Status != ProjectStatus.Draft)
        {
            _logger.LogInformation("Skipping invalidation for item {ItemId} - project is not in Draft status", 
                managedListItemId);
            return;
        }

        // Find all subsets that include this item
        var affectedSubsets = await _context.SubsetMemberships
            .Include(sm => sm.SubsetDefinition)
                .ThenInclude(sd => sd.QuestionLinks)
            .Where(sm => sm.ManagedListItemId == managedListItemId)
            .Select(sm => sm.SubsetDefinition)
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {SubsetCount} subsets affected by item {ItemId}", 
            affectedSubsets.Count, managedListItemId);

        // For each affected subset, trigger question display refresh
        foreach (var subset in affectedSubsets)
        {
            await RefreshQuestionDisplaysAsync(subset.Id, cancellationToken);
        }

        // Refresh project summary
        if (affectedSubsets.Any())
        {
            await RefreshProjectSummaryAsync(item.ManagedList.ProjectId, cancellationToken);
        }
    }
}
