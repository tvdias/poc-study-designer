using Api.Data;
using Api.Features.Projects;
using Api.Features.QuestionnaireLines;
using Api.Features.ManagedLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Studies;

public interface IStudyService
{
    Task<CreateStudyResponse> CreateStudyV1Async(
        CreateStudyRequest request,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task<CreateStudyVersionResponse> CreateStudyVersionAsync(
        CreateStudyVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task<GetStudiesResponse> GetStudiesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
    
    Task<GetStudyDetailsResponse?> GetStudyByIdAsync(
        Guid studyId,
        CancellationToken cancellationToken = default);
}

public class StudyService : IStudyService
{
    private readonly ApplicationDbContext _context;
    private readonly ISubsetManagementService _subsetService;
    private readonly ILogger<StudyService> _logger;

    public StudyService(
        ApplicationDbContext context,
        ISubsetManagementService subsetService,
        ILogger<StudyService> logger)
    {
        _context = context;
        _subsetService = subsetService;
        _logger = logger;
    }

    public async Task<CreateStudyResponse> CreateStudyV1Async(
        CreateStudyRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating Study V1 for ProjectId={ProjectId}, Name={Name}",
            request.ProjectId, request.Name);

        // Validate project exists and is active
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project {request.ProjectId} not found");
        }

        // Get all active questionnaire lines (Project Master Questionnaire)
        var masterQuestions = await _context.QuestionnaireLines
            .Include(q => q.QuestionBankItem)
            .Where(q => q.ProjectId == request.ProjectId)
            .OrderBy(q => q.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (masterQuestions.Count == 0)
        {
            throw new InvalidOperationException(
                $"Project {request.ProjectId} has no questionnaire lines to copy");
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create Study V1
            var study = new Study
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                VersionNumber = 1,
                Status = StudyStatus.Draft,
                MasterStudyId = null, // V1 is its own master
                ParentStudyId = null,
                VersionComment = request.Comment,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            _context.Studies.Add(study);
            await _context.SaveChangesAsync(cancellationToken);

            // Set MasterStudyId to itself for V1
            study.MasterStudyId = study.Id;
            await _context.SaveChangesAsync(cancellationToken);

            // Copy questionnaire lines
            var studyQuestions = await CopyQuestionnaireLinesAsync(
                study.Id,
                masterQuestions,
                userId,
                cancellationToken);

            // Copy managed list assignments
            await CopyManagedListAssignmentsAsync(
                study.Id,
                studyQuestions,
                masterQuestions,
                userId,
                cancellationToken);

            // Copy subsets with full selection (all active MLEs)
            await CopySubsetsForV1Async(
                study.Id,
                studyQuestions,
                request.ProjectId,
                userId,
                cancellationToken);

            // Update project counters
            await UpdateProjectCountersAsync(
                request.ProjectId,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created Study V1: StudyId={StudyId}, Name={Name}, QuestionCount={QuestionCount}",
                study.Id, study.Name, studyQuestions.Count);

            return new CreateStudyResponse
            {
                StudyId = study.Id,
                Name = study.Name,
                VersionNumber = study.VersionNumber,
                Status = study.Status,
                QuestionCount = studyQuestions.Count
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create Study V1 for ProjectId={ProjectId}", request.ProjectId);
            throw;
        }
    }

    public async Task<CreateStudyVersionResponse> CreateStudyVersionAsync(
        CreateStudyVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating new version for StudyId={StudyId}",
            request.ParentStudyId);

        // Load parent study with lineage
        var parentStudy = await _context.Studies
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == request.ParentStudyId, cancellationToken);

        if (parentStudy == null)
        {
            throw new InvalidOperationException($"Parent Study {request.ParentStudyId} not found");
        }

        // Determine master study ID (root of lineage)
        var masterStudyId = parentStudy.MasterStudyId ?? parentStudy.Id;

        // Check for existing Draft version in lineage (only one Draft allowed)
        var existingDraft = await _context.Studies
            .Where(s => s.MasterStudyId == masterStudyId && s.Status == StudyStatus.Draft)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingDraft != null)
        {
            throw new InvalidOperationException(
                "Only one Draft version is allowed in this Study; finish or abandon the existing Draft first.");
        }

        // Get next version number
        var maxVersion = await _context.Studies
            .Where(s => s.MasterStudyId == masterStudyId)
            .MaxAsync(s => s.VersionNumber, cancellationToken);

        var newVersionNumber = maxVersion + 1;

        // Load parent study questions with all relationships
        var parentQuestions = await _context.StudyQuestionnaireLines
            .Include(q => q.ManagedListAssignments)
            .Include(q => q.SubsetLinks)
                .ThenInclude(sl => sl.SubsetDefinition)
                    .ThenInclude(sd => sd!.Memberships)
            .Where(q => q.StudyId == request.ParentStudyId)
            .OrderBy(q => q.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create new Study version
            var study = new Study
            {
                Id = Guid.NewGuid(),
                ProjectId = parentStudy.ProjectId,
                Name = request.Name ?? parentStudy.Name,
                Description = request.Description ?? parentStudy.Description,
                VersionNumber = newVersionNumber,
                Status = StudyStatus.Draft,
                MasterStudyId = masterStudyId,
                ParentStudyId = parentStudy.Id,
                VersionComment = request.Comment,
                VersionReason = request.Reason,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            _context.Studies.Add(study);
            await _context.SaveChangesAsync(cancellationToken);

            // Copy questions from parent
            var studyQuestions = await CopyStudyQuestionnairesAsync(
                study.Id,
                parentQuestions,
                userId,
                cancellationToken);

            // Copy managed list assignments from parent
            await CopyStudyManagedListAssignmentsAsync(
                study.Id,
                studyQuestions,
                parentQuestions,
                userId,
                cancellationToken);

            // Copy/reuse subsets from parent
            await CopySubsetsFromParentAsync(
                study.Id,
                studyQuestions,
                parentQuestions,
                userId,
                cancellationToken);

            // Update project LastStudyModifiedOn
            await UpdateProjectCountersAsync(
                parentStudy.ProjectId,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created Study V{Version}: StudyId={StudyId}, Name={Name}, QuestionCount={QuestionCount}",
                newVersionNumber, study.Id, study.Name, studyQuestions.Count);

            return new CreateStudyVersionResponse
            {
                StudyId = study.Id,
                Name = study.Name,
                VersionNumber = study.VersionNumber,
                Status = study.Status,
                ParentStudyId = parentStudy.Id,
                QuestionCount = studyQuestions.Count
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create new version for StudyId={StudyId}", request.ParentStudyId);
            throw;
        }
    }

    public async Task<GetStudiesResponse> GetStudiesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var studies = await _context.Studies
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedOn)
            .Select(s => new StudySummary
            {
                StudyId = s.Id,
                Name = s.Name,
                VersionNumber = s.VersionNumber,
                Status = s.Status,
                CreatedOn = s.CreatedOn,
                CreatedBy = s.CreatedBy,
                QuestionCount = s.QuestionnaireLines.Count
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new GetStudiesResponse
        {
            Studies = studies
        };
    }

    public async Task<GetStudyDetailsResponse?> GetStudyByIdAsync(
        Guid studyId,
        CancellationToken cancellationToken = default)
    {
        var study = await _context.Studies
            .Include(s => s.Project)
            .Include(s => s.ParentStudy)
            .Include(s => s.MasterStudy)
            .Where(s => s.Id == studyId)
            .Select(s => new GetStudyDetailsResponse
            {
                StudyId = s.Id,
                ProjectId = s.ProjectId,
                ProjectName = s.Project.Name,
                Name = s.Name,
                Description = s.Description,
                VersionNumber = s.VersionNumber,
                Status = s.Status,
                MasterStudyId = s.MasterStudyId,
                ParentStudyId = s.ParentStudyId,
                VersionComment = s.VersionComment,
                CreatedOn = s.CreatedOn,
                CreatedBy = s.CreatedBy,
                ModifiedOn = s.ModifiedOn,
                ModifiedBy = s.ModifiedBy,
                QuestionCount = s.QuestionnaireLines.Count
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return study;
    }

    // Private helper methods
    private async Task<List<StudyQuestionnaireLine>> CopyQuestionnaireLinesAsync(
        Guid studyId,
        List<QuestionnaireLine> sourceQuestions,
        string userId,
        CancellationToken cancellationToken)
    {
        var studyQuestions = new List<StudyQuestionnaireLine>();

        foreach (var sourceQuestion in sourceQuestions)
        {
            var studyQuestion = new StudyQuestionnaireLine
            {
                Id = Guid.NewGuid(),
                StudyId = studyId,
                QuestionBankItemId = sourceQuestion.QuestionBankItemId,
                SortOrder = sourceQuestion.SortOrder,
                IsActive = true, // All questions active in V1
                VariableName = sourceQuestion.VariableName,
                Version = sourceQuestion.Version,
                QuestionText = sourceQuestion.QuestionText,
                QuestionTitle = sourceQuestion.QuestionTitle,
                QuestionType = sourceQuestion.QuestionType,
                Classification = sourceQuestion.Classification,
                QuestionRationale = sourceQuestion.QuestionRationale,
                ScraperNotes = sourceQuestion.ScraperNotes,
                CustomNotes = sourceQuestion.CustomNotes,
                RowSortOrder = sourceQuestion.RowSortOrder,
                ColumnSortOrder = sourceQuestion.ColumnSortOrder,
                AnswerMin = sourceQuestion.AnswerMin,
                AnswerMax = sourceQuestion.AnswerMax,
                QuestionFormatDetails = sourceQuestion.QuestionFormatDetails,
                IsDummy = sourceQuestion.IsDummy,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            studyQuestions.Add(studyQuestion);
        }

        _context.StudyQuestionnaireLines.AddRange(studyQuestions);
        await _context.SaveChangesAsync(cancellationToken);

        return studyQuestions;
    }

    private async Task CopyManagedListAssignmentsAsync(
        Guid studyId,
        List<StudyQuestionnaireLine> studyQuestions,
        List<QuestionnaireLine> masterQuestions,
        string userId,
        CancellationToken cancellationToken)
    {
        // Get all managed list assignments from project questionnaire
        var projectQuestionIds = masterQuestions.Select(q => q.Id).ToList();
        var managedListAssignments = await _context.QuestionManagedLists
            .Where(qml => projectQuestionIds.Contains(qml.QuestionnaireLineId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var assignments = new List<StudyManagedListAssignment>();

        foreach (var masterQuestion in masterQuestions)
        {
            var studyQuestion = studyQuestions.First(sq => sq.SortOrder == masterQuestion.SortOrder);
            var questionAssignments = managedListAssignments
                .Where(qml => qml.QuestionnaireLineId == masterQuestion.Id)
                .ToList();

            foreach (var qml in questionAssignments)
            {
                var assignment = new StudyManagedListAssignment
                {
                    Id = Guid.NewGuid(),
                    StudyId = studyId,
                    StudyQuestionnaireLineId = studyQuestion.Id,
                    ManagedListId = qml.ManagedListId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };

                assignments.Add(assignment);
            }
        }

        _context.StudyManagedListAssignments.AddRange(assignments);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task CopySubsetsForV1Async(
        Guid studyId,
        List<StudyQuestionnaireLine> studyQuestions,
        Guid projectId,
        string userId,
        CancellationToken cancellationToken)
    {
        // Get all subset links from project questionnaire
        var projectSubsetLinks = await _context.QuestionSubsetLinks
            .Include(qsl => qsl.SubsetDefinition)
            .Where(qsl => qsl.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var studySubsetLinks = new List<StudyQuestionSubsetLink>();

        foreach (var projectLink in projectSubsetLinks)
        {
            // Find corresponding study question by matching with original questionnaire line
            var projectQuestion = await _context.QuestionnaireLines
                .Where(q => q.Id == projectLink.QuestionnaireLineId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (projectQuestion == null) continue;

            var studyQuestion = studyQuestions.FirstOrDefault(sq => sq.SortOrder == projectQuestion.SortOrder);
            if (studyQuestion == null) continue;

            // For V1, copy subset reference if it exists
            // SubsetDefinitionId == null means full selection (all active MLEs)
            var studyLink = new StudyQuestionSubsetLink
            {
                Id = Guid.NewGuid(),
                StudyId = studyId,
                StudyQuestionnaireLineId = studyQuestion.Id,
                ManagedListId = projectLink.ManagedListId,
                SubsetDefinitionId = projectLink.SubsetDefinitionId, // Reuse subset if defined
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            studySubsetLinks.Add(studyLink);
        }

        _context.StudyQuestionSubsetLinks.AddRange(studySubsetLinks);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<StudyQuestionnaireLine>> CopyStudyQuestionnairesAsync(
        Guid studyId,
        List<StudyQuestionnaireLine> parentQuestions,
        string userId,
        CancellationToken cancellationToken)
    {
        var studyQuestions = new List<StudyQuestionnaireLine>();

        foreach (var parentQuestion in parentQuestions)
        {
            var studyQuestion = new StudyQuestionnaireLine
            {
                Id = Guid.NewGuid(),
                StudyId = studyId,
                QuestionBankItemId = parentQuestion.QuestionBankItemId,
                SortOrder = parentQuestion.SortOrder,
                IsActive = parentQuestion.IsActive, // Preserve active/inactive from parent
                VariableName = parentQuestion.VariableName,
                Version = parentQuestion.Version,
                QuestionText = parentQuestion.QuestionText,
                QuestionTitle = parentQuestion.QuestionTitle,
                QuestionType = parentQuestion.QuestionType,
                Classification = parentQuestion.Classification,
                QuestionRationale = parentQuestion.QuestionRationale,
                ScraperNotes = parentQuestion.ScraperNotes,
                CustomNotes = parentQuestion.CustomNotes,
                RowSortOrder = parentQuestion.RowSortOrder,
                ColumnSortOrder = parentQuestion.ColumnSortOrder,
                AnswerMin = parentQuestion.AnswerMin,
                AnswerMax = parentQuestion.AnswerMax,
                QuestionFormatDetails = parentQuestion.QuestionFormatDetails,
                IsDummy = parentQuestion.IsDummy,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            studyQuestions.Add(studyQuestion);
        }

        _context.StudyQuestionnaireLines.AddRange(studyQuestions);
        await _context.SaveChangesAsync(cancellationToken);

        return studyQuestions;
    }

    private async Task CopyStudyManagedListAssignmentsAsync(
        Guid studyId,
        List<StudyQuestionnaireLine> studyQuestions,
        List<StudyQuestionnaireLine> parentQuestions,
        string userId,
        CancellationToken cancellationToken)
    {
        var assignments = new List<StudyManagedListAssignment>();

        foreach (var parentQuestion in parentQuestions)
        {
            var studyQuestion = studyQuestions.First(sq => sq.SortOrder == parentQuestion.SortOrder);

            foreach (var parentAssignment in parentQuestion.ManagedListAssignments)
            {
                var assignment = new StudyManagedListAssignment
                {
                    Id = Guid.NewGuid(),
                    StudyId = studyId,
                    StudyQuestionnaireLineId = studyQuestion.Id,
                    ManagedListId = parentAssignment.ManagedListId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };

                assignments.Add(assignment);
            }
        }

        _context.StudyManagedListAssignments.AddRange(assignments);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task CopySubsetsFromParentAsync(
        Guid studyId,
        List<StudyQuestionnaireLine> studyQuestions,
        List<StudyQuestionnaireLine> parentQuestions,
        string userId,
        CancellationToken cancellationToken)
    {
        var studySubsetLinks = new List<StudyQuestionSubsetLink>();

        foreach (var parentQuestion in parentQuestions)
        {
            var studyQuestion = studyQuestions.First(sq => sq.SortOrder == parentQuestion.SortOrder);

            foreach (var parentSubsetLink in parentQuestion.SubsetLinks)
            {
                // Reuse subset definition from parent
                // SubsetDefinitionId == null means full selection
                var studyLink = new StudyQuestionSubsetLink
                {
                    Id = Guid.NewGuid(),
                    StudyId = studyId,
                    StudyQuestionnaireLineId = studyQuestion.Id,
                    ManagedListId = parentSubsetLink.ManagedListId,
                    SubsetDefinitionId = parentSubsetLink.SubsetDefinitionId,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };

                studySubsetLinks.Add(studyLink);
            }
        }

        _context.StudyQuestionSubsetLinks.AddRange(studySubsetLinks);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateProjectCountersAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FindAsync([projectId], cancellationToken);
        if (project == null) return;

        var studyCount = await _context.Studies
            .Where(s => s.ProjectId == projectId)
            .CountAsync(cancellationToken);

        project.HasStudies = studyCount > 0;
        project.StudyCount = studyCount;
        project.LastStudyModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
