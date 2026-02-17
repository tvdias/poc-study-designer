namespace DigTx.Designer.FunctionApp.Services;

using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Mappers;
using DigTx.Designer.FunctionApp.Models;
using DigTx.Designer.FunctionApp.Models.Responses;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;

/// <summary>
/// Project Service Creation implementation.
/// </summary>
public partial class ProjectService : IProjectService
{
    public async Task<ProjectCreationResponse> CreateAsync(ProjectCreationRequest request)
    {
        var questionBanks = await ValidateProjectCreationRequestAsync(request);

        _logger.LogTrace($"Validate Parameters from Request succeeded.");

        var projectToCreate = request.MapToEntity();

        await CreateProjectAsync(request, projectToCreate, questionBanks);

        _logger.LogTrace($"Project {request.ProjectName} created sucefully.");

        var response = await _environmentVariableValueService.GetProjectUrlAsync(projectToCreate.Id);

        return response.MapToResponse();
    }

    private async Task<KT_Project> CreateProjectAsync(
        ProjectCreationRequest request,
        KT_Project projectToCreate,
        IList<KT_QuestionBank> questionBanks)
    {
        await _uow.ProjectRepository.CreateAsync(projectToCreate);

        await CreateQuestions(request.Questions, projectToCreate, questionBanks);

        return projectToCreate;
    }

    private async Task CreateQuestions(
        List<QuestionCreationRequest> questionCreationRequests,
        KT_Project projectToCreate,
        IList<KT_QuestionBank> questionBanks)
    {
        var existingQuestionsCreationRequest = questionCreationRequests
            .Where(q => q.Origin == OriginType.QuestionBank)
            .ToList();

        var newQuestionCreationRequests = questionCreationRequests
            .Where(q => q.Origin == OriginType.New)
            .ToList();

        var existingQuestionnaireLinesToInsert = questionBanks
            .Select(q => q.MapToEntity(existingQuestionsCreationRequest, projectToCreate.Id))
            .ToList();

        var newQuestionnaireLinesToInsert = newQuestionCreationRequests
            .Select(q => q.MapToEntity(projectToCreate.Id))
            .ToList();

        var questionnaireLinesToInsert = existingQuestionnaireLinesToInsert
            .Concat(newQuestionnaireLinesToInsert)
            .ToList();

        await _uow.QuestionnaireLinesRepository
            .CreateRecordsInParallel(questionnaireLinesToInsert);

        await CreateAnswersAsync(
            questionBanks,
            existingQuestionnaireLinesToInsert,
            newQuestionnaireLinesToInsert,
            newQuestionCreationRequests);
    }

    private async Task CreateAnswersAsync(
        IList<KT_QuestionBank> questionBanks,
        List<KT_QuestionnaireLines> existingQuestionnaireLinesToInsert,
        List<KT_QuestionnaireLines> newQuestionnaireLinesToInsert,
        IList<QuestionCreationRequest> newQuestionCreationRequests)
    {
        var questionsIds = questionBanks.Select(q => q.Id).ToList();

        var questionAnswerList = await _uow.QuestionAnswerListRepository
            .GetByQuestionIdsAsync(questionsIds);

        var questionnaireLinesAnswerListToInsert = new List<KTR_QuestionnaireLinesAnswerList>();

        foreach (var answer in questionAnswerList)
        {
            var questionnaireLine = existingQuestionnaireLinesToInsert
                .FirstOrDefault(x => x.KTR_QuestionBank.Id == answer.KTR_KT_QuestionBank.Id);

            var existingAnswer = answer
                .MapToEntity(questionnaireLine.KT_QuestionnaireLinesId.GetValueOrDefault());

            questionnaireLinesAnswerListToInsert.Add(existingAnswer);
        }

        foreach (var questionRequest in newQuestionCreationRequests)
        {
            var questionnaireLine = newQuestionnaireLinesToInsert
                .FirstOrDefault(x => x.KT_QuestionVariableName == questionRequest.VariableName);

            var newAnswers = questionRequest.Answers?
                .Select(x => x.MapToEntity(questionnaireLine.KT_QuestionnaireLinesId.GetValueOrDefault())) ?? [];

            questionnaireLinesAnswerListToInsert.AddRange(newAnswers);
        }

        await _uow.QuestionnaireLinesAnswerListRepository
            .CreateRecordsInParallel(questionnaireLinesAnswerListToInsert);
    }
}
