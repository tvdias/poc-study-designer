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
                m.ManagedListItem.Value,
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
}
