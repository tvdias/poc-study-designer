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
        Guid parentStudyId,
        string userId,
        CancellationToken cancellationToken = default);
    
    Task<GetStudiesResponse> GetStudiesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
    
    Task<GetStudyDetailsResponse?> GetStudyByIdAsync(
        Guid studyId,
        CancellationToken cancellationToken = default);

    Task ValidateStudyNameUniquenessAsync(
        Guid projectId,
        string name,
        Guid masterStudyId,
        CancellationToken cancellationToken = default);

    Task<GetStudyQuestionsResponse?> GetStudyQuestionsAsync(
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
        _logger.LogInformation("Creating Study V1 for ProjectId={ProjectId}, Name={Name}", request.ProjectId, request.Name);

        // initialize study
        var studyId = Guid.CreateVersion7();
        var study = new Study
        {
            Id = studyId,
            ProjectId = request.ProjectId,
            Name = request.Name.Trim(),
            Version = 1,
            Status = StudyStatus.Draft,
            MasterStudyId = studyId,
            ParentStudyId = null,
            Category = request.Category.Trim(),
            MaconomyJobNumber = request.MaconomyJobNumber.Trim(),
            ProjectOperationsUrl = request.ProjectOperationsUrl.Trim(),
            ScripterNotes = request.ScripterNotes,
            FieldworkMarketId = request.FieldworkMarketId,
            CreatedBy = userId,
            CreatedOn = DateTime.UtcNow
        };

        // Validate project exists and is active
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == study.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project {request.ProjectId} not found");

        await ValidateStudyNameUniquenessAsync(study.ProjectId, study.Name, study.MasterStudyId, cancellationToken);

        var masterQuestions = await _context.QuestionnaireLines
            .Include(q => q.QuestionBankItem)
            .Where(q => q.ProjectId == request.ProjectId)
            .OrderBy(q => q.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (masterQuestions.Count == 0)
        {
            throw new InvalidOperationException($"Project has no questionnaire lines.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _context.Studies.Add(study);
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
                await UpdateProjectCountersAsync(request.ProjectId, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully created Study V1: StudyId={StudyId}, Name={Name}, QuestionCount={QuestionCount}",
                    study.Id, study.Name, studyQuestions.Count);

                return new CreateStudyResponse
                {
                    StudyId = study.Id,
                    Name = study.Name,
                    Version = study.Version,
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
        });
    }

    public async Task<CreateStudyVersionResponse> CreateStudyVersionAsync(
        Guid parentStudyId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new version for StudyId={StudyId}", parentStudyId);

        // Load parent study
        var parentStudy = await _context.Studies.Include(s => s.Project).FirstOrDefaultAsync(s => s.Id == parentStudyId, cancellationToken)
            ?? throw new InvalidOperationException($"Study {parentStudyId} not found");

        // Check for existing Draft version in lineage (only one Draft is allowed)
        var isExistingDraft = await _context.Studies
            .Where(s => s.MasterStudyId == parentStudy.MasterStudyId && s.Status == StudyStatus.Draft)
            .AnyAsync(cancellationToken);

        if (isExistingDraft)
        {
            throw new InvalidOperationException("Only one Draft version is allowed in this Study; finish or abandon the existing Draft first.");
        }

        // Get next version number
        var maxVersion = await _context.Studies
            .Where(s => s.MasterStudyId == parentStudy.MasterStudyId)
            .MaxAsync(s => s.Version, cancellationToken);

        var newVersionNumber = maxVersion + 1;

        // Load parent study questions with all relationships
        var parentQuestions = await _context.StudyQuestionnaireLines
            .Include(q => q.ManagedListAssignments)
            .Include(q => q.SubsetLinks)
                .ThenInclude(sl => sl.SubsetDefinition)
                    .ThenInclude(sd => sd!.Memberships)
            .Where(q => q.StudyId == parentStudyId)
            .OrderBy(q => q.SortOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var study = new Study
                {
                    Id = Guid.CreateVersion7(),
                    ProjectId = parentStudy.ProjectId,
                    Name = parentStudy.Name,
                    Version = newVersionNumber,
                    Status = StudyStatus.Draft,
                    MasterStudyId = parentStudy.MasterStudyId,
                    ParentStudyId = parentStudy.Id,
                    Category = parentStudy.Category,
                    MaconomyJobNumber = parentStudy.MaconomyJobNumber,
                    ProjectOperationsUrl = parentStudy.ProjectOperationsUrl,
                    ScripterNotes = parentStudy.ScripterNotes,
                    FieldworkMarketId = parentStudy.FieldworkMarketId,
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
                await UpdateProjectCountersAsync(parentStudy.ProjectId, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully created Study V{Version}: StudyId={StudyId}, Name={Name}, QuestionCount={QuestionCount}",
                    newVersionNumber, study.Id, study.Name, studyQuestions.Count);

                return new CreateStudyVersionResponse
                {
                    StudyId = study.Id,
                    Name = study.Name,
                    Version = study.Version,
                    Status = study.Status,
                    ParentStudyId = study.ParentStudyId.Value,
                    QuestionCount = studyQuestions.Count
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to create new version for StudyId={StudyId}", parentStudyId);
                throw;
            }
        });
    }

    public async Task<GetStudiesResponse> GetStudiesAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var studies = await _context.Studies
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedOn)
            .Include(s => s.FieldworkMarket)
            .Select(s => new StudySummary
            {
                StudyId = s.Id,
                Name = s.Name,
                Version = s.Version,
                Status = s.Status,
                CreatedOn = s.CreatedOn,
                CreatedBy = s.CreatedBy,
                QuestionCount = s.QuestionnaireLines.Count,
                Category = s.Category,
                FieldworkMarketName = s.FieldworkMarket.Name
            })
            .ToListAsync(cancellationToken);

        return new GetStudiesResponse
        {
            Studies = studies
        };
    }

    public async Task<GetStudyDetailsResponse?> GetStudyByIdAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        var study = await _context.Studies
            .Include(s => s.Project)
            .Include(s => s.ParentStudy)
            .Include(s => s.MasterStudy)
            .Include(s => s.FieldworkMarket)
            .Where(s => s.Id == studyId)
            .Select(s => new GetStudyDetailsResponse
            {
                StudyId = s.Id,
                ProjectId = s.ProjectId,
                ProjectName = s.Project.Name,
                Name = s.Name,
                Version = s.Version,
                Status = s.Status,
                MasterStudyId = s.MasterStudyId,
                ParentStudyId = s.ParentStudyId,
                CreatedOn = s.CreatedOn,
                CreatedBy = s.CreatedBy,
                ModifiedOn = s.ModifiedOn,
                ModifiedBy = s.ModifiedBy,
                QuestionCount = s.QuestionnaireLines.Count,
                Category = s.Category,
                MaconomyJobNumber = s.MaconomyJobNumber,
                ProjectOperationsUrl = s.ProjectOperationsUrl,
                ScripterNotes = s.ScripterNotes,
                FieldworkMarketId = s.FieldworkMarketId,
                FieldworkMarketName = s.FieldworkMarket.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        return study;
    }

    public async Task<GetStudyQuestionsResponse?> GetStudyQuestionsAsync(Guid studyId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Studies.AnyAsync(s => s.Id == studyId, cancellationToken);
        if (!exists) return null;

        var questions = await _context.StudyQuestionnaireLines
            .Where(q => q.StudyId == studyId)
            .OrderBy(q => q.SortOrder)
            .Select(q => new StudyQuestionnaireLineDto
            {
                Id = q.Id,
                StudyId = q.StudyId,
                QuestionBankItemId = q.QuestionBankItemId,
                SortOrder = q.SortOrder,
                VariableName = q.VariableName,
                Version = q.Version,
                QuestionText = q.QuestionText,
                QuestionTitle = q.QuestionTitle,
                QuestionType = q.QuestionType,
                Classification = q.Classification,
                QuestionRationale = q.QuestionRationale,
                ScraperNotes = q.ScraperNotes,
                CustomNotes = q.CustomNotes,
                RowSortOrder = q.RowSortOrder,
                ColumnSortOrder = q.ColumnSortOrder,
                AnswerMin = q.AnswerMin,
                AnswerMax = q.AnswerMax,
                QuestionFormatDetails = q.QuestionFormatDetails,
                IsDummy = q.IsDummy
            })
            .ToListAsync(cancellationToken);

        return new GetStudyQuestionsResponse
        {
            Questions = questions
        };
    }

    public async Task ValidateStudyNameUniquenessAsync(
        Guid projectId,
        string name,
        Guid masterStudyId,
        CancellationToken cancellationToken = default)
    {        
        // A name must be unique within a project, 
        // EXCEPT for studies in the same lineage (same MasterStudyId)
        var query = _context.Studies
            .Where(s => s.MasterStudyId != masterStudyId
                && s.ProjectId == projectId
                && s.Name.ToLower() == name.ToLower());

        var exists = await query.AnyAsync(cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"A study with the name '{name}' already exists in this project.");
        }
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
