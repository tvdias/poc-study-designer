using Api.Data;
using Api.Features.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.ManagedLists;

public interface IAutoAssociationService
{
    Task OnManagedListItemCreatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task OnManagedListItemDeactivatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task OnManagedListItemReactivatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task OnManagedListAssignedToQuestionAsync(
        Guid questionnaireLineId,
        Guid managedListId,
        string userId,
        CancellationToken cancellationToken = default);
}

public class AutoAssociationService : IAutoAssociationService
{
    private readonly ApplicationDbContext _context;
    private readonly ISubsetManagementService _subsetService;
    private readonly ILogger<AutoAssociationService> _logger;

    public AutoAssociationService(
        ApplicationDbContext context,
        ISubsetManagementService subsetService,
        ILogger<AutoAssociationService> logger)
    {
        _context = context;
        _subsetService = subsetService;
        _logger = logger;
    }

    public async Task OnManagedListItemCreatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing auto-association for new ManagedListItem {ItemId}", managedListItemId);

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

        // Only process if the project is in Draft state
        if (item.ManagedList.Project.Status != ProjectStatus.Draft)
        {
            _logger.LogInformation("Skipping auto-association for item {ItemId} - project is not in Draft status", 
                managedListItemId);
            return;
        }

        // Find all questions in Draft Studies that reference this ManagedList
        var affectedQuestions = await _context.QuestionManagedLists
            .Include(qml => qml.QuestionnaireLine)
                .ThenInclude(ql => ql.Project)
            .AsNoTracking()
            .Where(qml => qml.ManagedListId == item.ManagedListId && 
                         qml.QuestionnaireLine.Project.Status == ProjectStatus.Draft)
            .Select(qml => new { qml.QuestionnaireLineId, qml.QuestionnaireLine.ProjectId })
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {QuestionCount} questions in Draft Studies affected by new item {ItemId}", 
            affectedQuestions.Count, managedListItemId);

        if (!affectedQuestions.Any())
        {
            return;
        }

        // For each affected question, ensure there's a QuestionSubsetLink
        // If the link exists with a subset, we need to recalculate subsets
        // If the link doesn't exist or has no subset (full selection), the new item is automatically included
        foreach (var question in affectedQuestions)
        {
            var existingLink = await _context.QuestionSubsetLinks
                .FirstOrDefaultAsync(
                    qsl => qsl.QuestionnaireLineId == question.QuestionnaireLineId && 
                           qsl.ManagedListId == item.ManagedListId,
                    cancellationToken);

            if (existingLink == null)
            {
                // Create a new link with full selection (SubsetDefinitionId = null means all active items)
                var newLink = new QuestionSubsetLink
                {
                    Id = Guid.NewGuid(),
                    ProjectId = question.ProjectId,
                    QuestionnaireLineId = question.QuestionnaireLineId,
                    ManagedListId = item.ManagedListId,
                    SubsetDefinitionId = null, // Full selection
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.QuestionSubsetLinks.Add(newLink);
                _logger.LogInformation("Created full selection link for question {QuestionId}", question.QuestionnaireLineId);
            }
            else if (existingLink.SubsetDefinitionId.HasValue)
            {
                // If a subset is in use, it needs to be recalculated
                // The new item is added to the "pool" but not automatically to the subset
                // Subsets use signature-based reuse, so they remain unchanged unless user re-selects
                _logger.LogInformation("Question {QuestionId} uses subset - new item added to pool but not to subset", 
                    question.QuestionnaireLineId);
            }
            // If SubsetDefinitionId is null, it's already a full selection, no action needed
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Trigger project summary refresh for affected projects
        var affectedProjectIds = affectedQuestions.Select(q => q.ProjectId).Distinct().ToList();
        foreach (var projectId in affectedProjectIds)
        {
            await _subsetService.RefreshProjectSummaryAsync(projectId, cancellationToken);
        }
    }

    public async Task OnManagedListItemDeactivatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing deactivation for ManagedListItem {ItemId}", managedListItemId);

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
            _logger.LogInformation("Skipping deactivation for item {ItemId} - project is not in Draft status", 
                managedListItemId);
            return;
        }

        // Find all subsets in Draft Studies that include this item
        var affectedSubsets = await _context.SubsetMemberships
            .Include(sm => sm.SubsetDefinition)
                .ThenInclude(sd => sd.Project)
            .Where(sm => sm.ManagedListItemId == managedListItemId && 
                        sm.SubsetDefinition.Project.Status == ProjectStatus.Draft)
            .Select(sm => sm.SubsetDefinition)
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {SubsetCount} subsets in Draft Studies affected by deactivation of item {ItemId}", 
            affectedSubsets.Count, managedListItemId);

        // Remove the item from all affected subsets
        var membershipsToRemove = await _context.SubsetMemberships
            .Where(sm => sm.ManagedListItemId == managedListItemId)
            .Join(_context.SubsetDefinitions.Where(sd => sd.Project.Status == ProjectStatus.Draft),
                  sm => sm.SubsetDefinitionId,
                  sd => sd.Id,
                  (sm, sd) => sm)
            .ToListAsync(cancellationToken);

        if (membershipsToRemove.Any())
        {
            _context.SubsetMemberships.RemoveRange(membershipsToRemove);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Removed {Count} subset memberships for deactivated item {ItemId}", 
                membershipsToRemove.Count, managedListItemId);
        }

        // Trigger refresh for affected subsets and projects
        foreach (var subset in affectedSubsets)
        {
            await _subsetService.RefreshQuestionDisplaysAsync(subset.Id, cancellationToken);
        }

        var affectedProjectIds = affectedSubsets.Select(s => s.ProjectId).Distinct().ToList();
        foreach (var projectId in affectedProjectIds)
        {
            await _subsetService.RefreshProjectSummaryAsync(projectId, cancellationToken);
        }
    }

    public async Task OnManagedListItemReactivatedAsync(
        Guid managedListItemId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing reactivation for ManagedListItem {ItemId}", managedListItemId);

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
            _logger.LogInformation("Skipping reactivation for item {ItemId} - project is not in Draft status", 
                managedListItemId);
            return;
        }

        // Reactivation is similar to creation - the item becomes available again
        // For full selections, it's automatically included (no action needed)
        // For subsets, it's added to the pool but not automatically to existing subsets
        await OnManagedListItemCreatedAsync(managedListItemId, userId, cancellationToken);
    }

    public async Task OnManagedListAssignedToQuestionAsync(
        Guid questionnaireLineId,
        Guid managedListId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing ML assignment to question: QuestionId={QuestionId}, ManagedListId={ManagedListId}", 
            questionnaireLineId, managedListId);

        // Validate the question exists and get its project
        var question = await _context.QuestionnaireLines
            .Include(ql => ql.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(ql => ql.Id == questionnaireLineId, cancellationToken);

        if (question == null)
        {
            _logger.LogWarning("QuestionnaireLine {QuestionId} not found", questionnaireLineId);
            return;
        }

        // Only process if project is in Draft state
        if (question.Project.Status != ProjectStatus.Draft)
        {
            _logger.LogInformation("Skipping ML assignment auto-association - project is not in Draft status");
            return;
        }

        // Check if a QuestionSubsetLink already exists
        var existingLink = await _context.QuestionSubsetLinks
            .FirstOrDefaultAsync(
                qsl => qsl.QuestionnaireLineId == questionnaireLineId && 
                       qsl.ManagedListId == managedListId,
                cancellationToken);

        if (existingLink == null)
        {
            // Create a new link with full selection (all active MLEs)
            var newLink = new QuestionSubsetLink
            {
                Id = Guid.NewGuid(),
                ProjectId = question.ProjectId,
                QuestionnaireLineId = questionnaireLineId,
                ManagedListId = managedListId,
                SubsetDefinitionId = null, // Full selection
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId
            };
            _context.QuestionSubsetLinks.Add(newLink);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created full selection link for question {QuestionId} with managed list {ManagedListId}", 
                questionnaireLineId, managedListId);
        }

        // Trigger project summary refresh
        await _subsetService.RefreshProjectSummaryAsync(question.ProjectId, cancellationToken);
    }
}
