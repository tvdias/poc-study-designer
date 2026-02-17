namespace DigTx.Designer.FunctionApp.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Exceptions;
using DigTx.Designer.FunctionApp.Models;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;

/// <summary>
/// Project Service Creation Validation implementation.
/// </summary>
public partial class ProjectService : IProjectService
{
    private const string ErrorCreateCustomQuestion = "Error Creating Custom Question";
    private const string ErrorCreateAnswersCustomQuestion = "Error Creating Answers in Custom Question";
    private readonly HashSet<QuestionType> _openQuestionsList =
        [
            QuestionType.SmallTextInput,
            QuestionType.DisplayScreen,
            QuestionType.LargeTextInput,
            QuestionType.Logic,
            QuestionType.NumericInput,
            QuestionType.TextInputMatrix,
        ];

    private static void ValidateSequencialQuestionDisplayOrder(List<QuestionCreationRequest> questions)
    {
        var expectedOrder = Enumerable.Range(1, questions.Count).ToList();

        var actualOrder = questions
            .Select(q => q.DisplayOrder)
            .OrderBy(o => o)
            .ToList();

        if (!expectedOrder.SequenceEqual(actualOrder))
        {
            throw new InvalidRequestException("DisplayOrder in Questions must be sequential starting from 1 without gaps or duplicates.");
        }
    }

    private async Task<IList<KT_QuestionBank>> ValidateProjectCreationRequestAsync(ProjectCreationRequest request)
    {
        await Task.WhenAll(
            ValidateClientAsync(request.ClientId),
            ValidateComissionMarketAsync(request.CommissioningMarketId),
            ValidateProductAsync(request.ProductId, request.ProductTemplateId));

        return await ValidateQuestionsAsync(request.Questions);
    }

    private async Task ValidateProductAsync(Guid productId, Guid productTemplateId)
    {
        if (productId != Guid.Empty)
        {
            var product = await _uow.ProductRepository.GetByIdAsync(productId);

            if (product is null)
            {
                _logger.LogWarning("Product with ID {Id} is not active.", productId);
                throw new NotFoundException($"Product with ID {productId} is not found.");
            }

            if (product is not null && product.StatusCode != KTR_Product_StatusCode.Active)
            {
                _logger.LogWarning("Product with ID {Id} is not active.", productId);
                throw new InvalidRequestException($"Product with ID {productId} is not active.");
            }
        }

        if (productTemplateId != Guid.Empty)
        {
            if (productId == Guid.Empty)
            {
                _logger.LogWarning("ProductId is required when ProducTemplateId is requested");
                throw new InvalidRequestException($"ProductId is required when ProducTemplateId is requested");
            }

            var productTemplate = await _uow.ProductTemplateRepository.GetByIdAsync(productTemplateId);

            if (productTemplate is null)
            {
                _logger.LogWarning("Product Template with ID {Id} is not active.", productTemplateId);
                throw new NotFoundException($"Product Template with ID {productTemplateId} is not found.");
            }

            if (productTemplate is not null && productTemplate.StatusCode != KTR_ProductTemplate_StatusCode.Active)
            {
                _logger.LogWarning("Product Template with ID {Id} is not active.", productTemplateId);
                throw new InvalidRequestException($"Product Template with ID {productTemplateId} is not active.");
            }
        }
    }

    private async Task ValidateClientAsync(Guid id)
    {
        var account = await _uow.ClientRepository.GetByIdAsync(id);

        if (account is null)
        {
            _logger.LogWarning("Account Client with ID {Id} not found.", id);
            throw new NotFoundException($"Client with ID {id} does not exist.");
        }

        if (account.StatusCode != Account_StatusCode.Active)
        {
            _logger.LogWarning("Account Client with ID {Id} is not active.", id);
            throw new InvalidRequestException($"Client with ID {id} is not active.");
        }
    }

    private async Task ValidateComissionMarketAsync(Guid id)
    {
        var commissionMarket = await _uow.CommissioningMarketRepository.GetByIdAsync(id);

        if (commissionMarket is null)
        {
            _logger.LogWarning("Commissioning Market with ID {Id} not found.", id);
            throw new NotFoundException($"Commissioning Market with ID {id} does not exist.");
        }

        if (commissionMarket is not null && commissionMarket.StatusCode != KT_CommissioningMarket_StatusCode.Active)
        {
            _logger.LogWarning("Commission Market with ID {Id} is not active.", id);
            throw new InvalidRequestException($"Commissioning Market with ID {id} is not active.");
        }
    }

    private async Task<IList<KT_QuestionBank>> ValidateQuestionsAsync(List<QuestionCreationRequest> questionCreationRequests)
    {
        var existingQuestions = questionCreationRequests.Where(q => q.Origin == OriginType.QuestionBank).ToList();

        var newQuestions = questionCreationRequests.Where(q => q.Origin == OriginType.New).ToList();

        var questions = await _uow.QuestionBankRepository.GetByIdsAsync([.. existingQuestions.Select(q => q.Id!.Value)]);

        if (existingQuestions.Count != questions.Count)
        {
            throw new NotFoundException("Questions not found in Question Bank.");
        }

        await ValidateQuestionsInModuleAsync([.. questionCreationRequests]);

        ValidateSequencialQuestionDisplayOrder(questionCreationRequests);

        ValidateNewQuestions(newQuestions);

        return questions;
    }

    private void ValidateNewQuestions(List<QuestionCreationRequest> questionCreationRequests)
    {
        foreach (var question in questionCreationRequests)
        {
            if (question.StandardOrCustom is null or StandardOrCustomType.Standard)
            {
                throw new InvalidRequestException("Only Custom Questions can be created.");
            }

            if (string.IsNullOrWhiteSpace(question.VariableName))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - VariableName is required.");
            }

            if (string.IsNullOrWhiteSpace(question.Title))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - Title is required.");
            }

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - Text is required.");
            }

            if (string.IsNullOrWhiteSpace(question.ScripterNotes))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - ScripterNotes is required.");
            }

            if (string.IsNullOrWhiteSpace(question.QuestionRationale))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - QuestionRationale is required.");
            }

            if (question.QuestionType is null)
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - QuestionType is required.");
            }

            if (!_openQuestionsList.Contains(question.QuestionType!.Value)
                && (question.Answers == null || question.Answers.Count == 0))
            {
                throw new InvalidRequestException($"{ErrorCreateCustomQuestion} - Answers are required for some QuestionTypes.");
            }

            ValidateAnswersRequest(question.Answers);
        }
    }

    private void ValidateAnswersRequest(List<AnswerCreationRequest> answersRequest)
    {
        if (answersRequest == null || answersRequest.Count == 0)
        {
            return;
        }

        if (answersRequest.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidRequestException($"{ErrorCreateAnswersCustomQuestion} - Answer Name is required.");
        }

        if (answersRequest.Any(x => string.IsNullOrWhiteSpace(x.Text)))
        {
            throw new InvalidRequestException($"{ErrorCreateAnswersCustomQuestion} - Answer Text is required.");
        }
    }

    private async Task ValidateQuestionsInModuleAsync(List<QuestionCreationRequest> questionCreationRequests)
    {
        var questionsWithModules = questionCreationRequests
                .Where(x => x.Module != null);

        var flattenedModules = await GetModuleWithQuestionAsync(questionsWithModules);

        foreach (var question in questionsWithModules)
        {
            var matchingModules = flattenedModules
                .Where(m => m!.Id == question.Module!.Id);

            var modulesWithAttribute = matchingModules
                        .Where(machingModule => machingModule.Id == question.Module!.Id)?
                        .Where(machingModule => machingModule.Attributes.TryGetValue($"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}", out _))
                        .ToList();

            if (modulesWithAttribute == null || modulesWithAttribute.Count == 0)
            {
                throw new InvalidRequestException($"Module with ID {question.Module!.Id} not found for Question {question.Id}.");
            }

            var moduleExistes = modulesWithAttribute
                .Select(machingModule => machingModule.Attributes[$"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}"])
                .OfType<AliasedValue>()
                .Select(av => av.Value)
                .OfType<EntityReference>()
                .Any(entityRef => entityRef.Id == question.Id);

            if (!moduleExistes)
            {
                throw new InvalidRequestException($"Module with ID {question.Module!.Id} not found for Question {question.Id}.");
            }
        }

        return;
    }

    private async Task<List<KT_Module>> GetModuleWithQuestionAsync(IEnumerable<QuestionCreationRequest> questionCreationRequests)
    {
        var moduleIds = questionCreationRequests
            .Select(x => x.Module!.Id)
            .Distinct()
            .ToList();

        var modules = await Task.WhenAll(moduleIds.Select(async moduleId =>
        {
            var module = await _uow.ModuleRepository.GetWithQuestionAsync(moduleId);

            if (module == null || module.Count == 0)
            {
                throw new NotFoundException($"Module {moduleId} not found.");
            }

            return module;
        }));

        return [.. modules.SelectMany(m => m)];
    }
}
