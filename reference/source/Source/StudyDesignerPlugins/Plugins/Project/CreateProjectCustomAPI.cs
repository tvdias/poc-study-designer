using Kantar.StudyDesignerLite.PluginsAuxiliar.Builders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Converters;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.CreateProject;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLine;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLineAnswer;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.General;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Project
{
    public class CreateProjectCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.CreateProjectCustomAPI";
        private const string ErrorCreateCustomQuestion = "Error Creating Custom Question";
        private const string ErrorCreateAnswersCustomQuestion = "Error Creating Answers in Custom Question";
        private readonly List<KT_QuestionType> _openQuestionsList = new List<KT_QuestionType>
        {
            KT_QuestionType.SmallTextInput,
            KT_QuestionType.DisplayScreen,
            KT_QuestionType.LargeTextInput,
            KT_QuestionType.Logic,
            KT_QuestionType.NumericInput,
            KT_QuestionType.TextInputMatrix,
        };

        public CreateProjectCustomAPI()
           : base(typeof(CreateProjectCustomAPI))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;

            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.CurrentUserService;

            var projectRequest = GetInputParameters(context);

            using (var dataverseContext = new DataverseContext(service))
            {
                var questionBanks = ValidateRequestParameters(
                    dataverseContext,
                    service,
                    projectRequest);
                tracingService?.Trace($"Validate Parameters from Request succeeded.");

                var projectToCreate = projectRequest.MapToEntity();
                var project = CreateProject(dataverseContext, projectToCreate)
                    ?? throw new InvalidPluginExecutionException("Error creating Project.");
                tracingService?.Trace($"Project was created {project.Id}");

                CreateNewAndExistingQuestions(
                    service,
                    dataverseContext,
                    questionBanks,
                    projectRequest,
                    project);
                tracingService?.Trace($"QuestionnaireLines and QuestionnaireLineAnswers were created.");

                var envService = new EnvironmentVariablesService(service);
                var url = new UrlBuilder(envService)
                    .WithOrgUrl()
                    .WithAppId()
                    .WithEntity(KT_Project.EntityLogicalName)
                    .WithId(project.Id)
                    .BuildEntityUrl();

                context.OutputParameters["projectUrl"] = url;
            }
        }

        private CreateProjectRequest GetInputParameters(IPluginExecutionContext context)
        {
            var clientIdParam = context.GetInputParameter<Guid>("clientId");
            var commissioningMarketIdParam = context.GetInputParameter<Guid>("commissioningMarketId");
            var descriptionParam = context.GetInputParameter<string>("description");

            // Always use Guid.Empty if not present
            Guid productIdParam = context.InputParameters.Contains("productId") && context.InputParameters["productId"] != null
                ? context.GetInputParameter<Guid>("productId")
                : Guid.Empty;
            Guid productTemplateIdParam = context.InputParameters.Contains("productTemplateId") && context.InputParameters["productTemplateId"] != null
                ? context.GetInputParameter<Guid>("productTemplateId")
                : Guid.Empty;

            var projectNameParam = context.GetInputParameter<string>("projectName");
            var questionsParam = context.GetInputParameter<string>("questions");

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new QuestionRequestConverter() }
            };
            var questionsRequest = JsonHelper.Deserialize<List<QuestionRequest>>(questionsParam, "questions", settings)
                 ?? throw new InvalidPluginExecutionException("Invalid Questions Request.");

            return CreateProjectEntityMappers.MapToRequest(
                        clientIdParam,
                        commissioningMarketIdParam,
                        descriptionParam,
                        productIdParam,
                        productTemplateIdParam,
                        projectNameParam,
                        questionsRequest);
        }

        private IList<KT_QuestionBank> ValidateRequestParameters(
            DataverseContext dataverseContext,
            IOrganizationService service,
            CreateProjectRequest projectRequest)
        {
            var client = GetClient(dataverseContext, projectRequest.ClientId)
                        ?? throw new InvalidPluginExecutionException("ClientId not found.");
            var comissioningMarket = GetCommissioningMarket(dataverseContext, projectRequest.CommissioningMarketId)
                ?? throw new InvalidPluginExecutionException("CommissioningMarketId not found.");

            // Only validate if present
            if (projectRequest.ProductId != Guid.Empty)
            {
                var product = GetProduct(dataverseContext, projectRequest.ProductId)
                ?? throw new InvalidPluginExecutionException("ProductId not found.");
            }

            if (projectRequest.ProductTemplateId != Guid.Empty)
            {
                var productTemplate = GetProductTemplate(dataverseContext, projectRequest.ProductTemplateId)
                    ?? throw new InvalidPluginExecutionException("ProductTemplateId not found.");
            }

            ValidateSequentialDisplayOrder(projectRequest.Questions);

            var existingQuestions = projectRequest.GetExistingQuestions();
            var newQuestions = projectRequest.GetNewQuestions();

            var questionBanks = ValidateQuestionsRequest(service, existingQuestions, newQuestions);

            return questionBanks;
        }

        private IList<KT_QuestionBank> ValidateQuestionsRequest(
            IOrganizationService service,
            IList<ExistingQuestionRequest> existingQuestions,
            IList<NewQuestionRequest> newQuestions)
        {
            var questionIds = existingQuestions
                .Select(x => x.Id)
                .ToList();
            var questions = GetQuestionBanks(service, questionIds);

            if (existingQuestions != null && existingQuestions.Count > 0
                && (questions == null || questions.Count == 0))
            {
                throw new InvalidPluginExecutionException("Questions not found in Question Bank.");
            }

            ValidateModuleInExistingQuestions(service, existingQuestions);

            ValidateNewQuestions(newQuestions);

            return questions;
        }

        private void ValidateSequentialDisplayOrder(IList<QuestionRequest> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                throw new InvalidPluginExecutionException("Questions are required.");
            }

            var expectedOrder = Enumerable.Range(1, questions.Count).ToList();
            var actualOrder = questions
                .Select(q => q.DisplayOrder)
                .OrderBy(o => o)
                .ToList();

            if (!expectedOrder.SequenceEqual(actualOrder))
            {
                throw new InvalidPluginExecutionException("DisplayOrder in Questions must be sequential starting from 1 without gaps or duplicates.");
            }
        }

        private void ValidateModuleInExistingQuestions(
            IOrganizationService service,
            IList<ExistingQuestionRequest> existingQuestionRequests)
        {
            if (existingQuestionRequests == null || existingQuestionRequests.Count() == 0)
            {
                return;
            }

            var questionsWithModules = existingQuestionRequests
                .Where(x => x.Module != null);

            foreach (var question in questionsWithModules)
            {
                var module = GetModuleWithQuestion(service, question.Module.Id, question.Id)
                    ?? throw new InvalidPluginExecutionException($"Module not found in Question: {question.Id}.");
            }
        }

        private void ValidateNewQuestions(IList<NewQuestionRequest> questionsCreateRequest)
        {
            if (questionsCreateRequest != null && questionsCreateRequest.Count > 0)
            {
                if (questionsCreateRequest.Any(
                    x => x.StandardOrCustom.GetEnum<KT_QuestionBank_KT_StandardOrCustom>() == null
                    || x.StandardOrCustom.GetEnum<KT_QuestionBank_KT_StandardOrCustom>() == KT_QuestionBank_KT_StandardOrCustom.Standard))
                {
                    throw new InvalidPluginExecutionException("Only Custom Questions can be created.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.VariableName)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - VariableName is required.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.Title)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - Title is required.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.Text)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - Text is required.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.ScripterNotes)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - ScripterNotes is required.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.QuestionRationale)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - QuestionRationale is required.");
                }

                if (questionsCreateRequest.Any(x => string.IsNullOrWhiteSpace(x.QuestionType)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - QuestionType is required.");
                }

                if (questionsCreateRequest.Any(x => x.QuestionType.GetEnum<KT_QuestionType>() == null))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - QuestionType is invalid.");
                }

                if (questionsCreateRequest.Any(x => !_openQuestionsList
                        .Contains(x.QuestionType
                        .GetEnum<KT_QuestionType>()
                        .GetValueOrDefault())
                    && (x.Answers == null || x.Answers.Count == 0)))
                {
                    throw new InvalidPluginExecutionException($"{ErrorCreateCustomQuestion} - Answers are required for some QuestionTypes.");
                }

                var answersRequest = questionsCreateRequest
                    .Where(x => x.Answers != null && x.Answers.Count > 0)
                    .SelectMany(x => x.Answers)
                    .ToList();
                ValidateAnswersRequest(answersRequest);
            }
        }

        private void ValidateAnswersRequest(IList<AnswerRequest> answersRequest)
        {
            if (answersRequest == null || answersRequest.Count == 0)
            {
                return;
            }

            if (answersRequest.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                throw new InvalidPluginExecutionException($"{ErrorCreateAnswersCustomQuestion} - Answer Name is required.");
            }

            if (answersRequest.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            {
                throw new InvalidPluginExecutionException($"{ErrorCreateAnswersCustomQuestion} - Answer Text is required.");
            }

            if (answersRequest.Any(x =>
                    !string.IsNullOrWhiteSpace(x.Location) &&
                    x.Location.GetEnum<KTR_AnswerType>() == null))
            {
                throw new InvalidPluginExecutionException($"{ErrorCreateAnswersCustomQuestion} - Answer Location is invalid.");
            }
        }

        private void CreateNewAndExistingQuestions(
            IOrganizationService service,
            DataverseContext dataverseContext,
            IList<KT_QuestionBank> questionBanks,
            CreateProjectRequest projectRequest,
            KT_Project project)
        {
            var existingQuestions = projectRequest.GetExistingQuestions();
            var newQuestions = projectRequest.GetNewQuestions();

            var questionBanksMapped = questionBanks
                .Select(x => x.MapToEntity(existingQuestions, project.Id))
                .ToList();
            var newQuestionsMapped = newQuestions
                .Select(x => x.MapToEntity(project.Id))
                .ToList();
            var finalQuestionsToCreate = newQuestionsMapped
                .Concat(questionBanksMapped)
                .ToList();
            CreateQuestionnaireLines(dataverseContext, finalQuestionsToCreate);

            CreateNewAndExistingAnswers(
                dataverseContext,
                service,
                existingQuestions,
                newQuestions,
                questionBanksMapped,
                newQuestionsMapped);
        }

        private void CreateNewAndExistingAnswers(
            DataverseContext dataverseContext,
            IOrganizationService service,
            IList<ExistingQuestionRequest> existingQuestions,
            IList<NewQuestionRequest> newQuestions,
            IList<KT_QuestionnaireLines> questionBanksMapped,
            IList<KT_QuestionnaireLines> newQuestionsMapped)
        {
            var answerBanksMapped = new List<KTR_QuestionnaireLinesAnswerList>();
            if (existingQuestions != null && existingQuestions.Count > 0)
            {
                var existingQuestionIds = existingQuestions
                    .Select(x => x.Id)
                    .ToList();
                var answerBanks = GetQuestionBankAnswers(service, existingQuestionIds);
                foreach (var answer in answerBanks)
                {
                    var questionnaireLine = questionBanksMapped
                        .FirstOrDefault(x => x.KTR_QuestionBank.Id == answer.KTR_KT_QuestionBank.Id);
                    var a = answer
                        .MapToEntity(questionnaireLine.KT_QuestionnaireLinesId.GetValueOrDefault());
                    answerBanksMapped.Add(a);
                }
            }

            var answersToCreate = new List<KTR_QuestionnaireLinesAnswerList>();
            foreach (var questionRequest in newQuestions)
            {
                var questionnaireLine = newQuestionsMapped
                    .FirstOrDefault(x => x.KT_QuestionVariableName == questionRequest.VariableName);
                var answers = questionRequest.Answers;
                var answersMapped = answers?
                    .Select(x => x.MapToEntity(questionnaireLine.KT_QuestionnaireLinesId.GetValueOrDefault()));
                if (answersMapped != null && answersMapped.Count() > 0)
                {
                    answersToCreate.AddRange(answersMapped);
                }
            }
            var finalAnswersToCreate = answersToCreate
                .Concat(answerBanksMapped)
                .ToList();
            CreateQuestionnaireLinesAnswers(dataverseContext, finalAnswersToCreate);
        }

        #region Queries to Dataverse - Account
        private Account GetClient(DataverseContext dataverseContext, Guid clientId)
        {
            return dataverseContext
                    .CreateQuery<Account>()
                    .Where(x => x.StatusCode == Account_StatusCode.Active)
                    .FirstOrDefault(p => p.Id == clientId);
        }
        #endregion 

        #region Queries to Dataverse - Comissioning Market
        private KT_CommissioningMarket GetCommissioningMarket(DataverseContext dataverseContext, Guid comissioningMarketId)
        {
            return dataverseContext
                    .CreateQuery<KT_CommissioningMarket>()
                    .Where(x => x.StatusCode == KT_CommissioningMarket_StatusCode.Active)
                    .FirstOrDefault(p => p.Id == comissioningMarketId);
        }
        #endregion

        #region Queries to Dataverse - Product
        private KTR_Product GetProduct(DataverseContext dataverseContext, Guid productId)
        {
            return dataverseContext
                    .CreateQuery<KTR_Product>()
                    .Where(x => x.StatusCode == KTR_Product_StatusCode.Active)
                    .FirstOrDefault(p => p.Id == productId);
        }
        #endregion

        #region Queries to Dataverse - Product Template
        private KTR_ProductTemplate GetProductTemplate(DataverseContext dataverseContext, Guid productTemplateId)
        {
            return dataverseContext
                    .CreateQuery<KTR_ProductTemplate>()
                    .Where(x => x.StatusCode == KTR_ProductTemplate_StatusCode.Active)
                    .FirstOrDefault(p => p.Id == productTemplateId);
        }
        #endregion

        #region Queries to Dataverse - Question Bank
        private IList<KT_QuestionBank> GetQuestionBanks(
            IOrganizationService service,
            IList<Guid> questionIds)
        {
            if (questionIds == null || questionIds.Count == 0)
            {
                return new List<KT_QuestionBank>();
            }

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionBank.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Distinct = true,
            };

            query.Criteria.AddCondition(KT_QuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionBank_StatusCode.Active);
            query.Criteria.AddCondition(KT_QuestionBank.Fields.Id, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionBank>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Answer Question Bank
        private IList<KTR_QuestionAnswerList> GetQuestionBankAnswers(
            IOrganizationService service,
            IList<Guid> questionIds)
        {
            if (questionIds == null || questionIds.Count() == 0)
            {
                return new List<KTR_QuestionAnswerList>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Distinct = true,
            };

            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionAnswerList_StatusCode.Active);
            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionAnswerList>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Module
        private KT_Module GetModuleWithQuestion(IOrganizationService service, Guid moduleId, Guid questionId)
        {
            var query = new QueryExpression()
            {
                EntityName = KT_Module.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition(KT_Module.Fields.StatusCode, ConditionOperator.Equal, (int)KT_Module_StatusCode.Active);
            query.Criteria.AddCondition(KT_Module.Fields.Id, ConditionOperator.Equal, moduleId);

            // INNER JOIN ModuleQuestionBank
            var moduleQuestionBankLink = query.AddLink(
                KTR_ModuleQuestionBank.EntityLogicalName,
                KT_Module.Fields.Id,
                KTR_ModuleQuestionBank.Fields.KTR_Module
            );
            moduleQuestionBankLink.LinkCriteria.AddCondition(KTR_ModuleQuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ModuleQuestionBank_StatusCode.Active);
            moduleQuestionBankLink.LinkCriteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank, ConditionOperator.Equal, questionId);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_Module>())
                .FirstOrDefault();
        }
        #endregion 

        #region Queries to Dataverse - QuestionnaireLines
        private void CreateQuestionnaireLines(
            DataverseContext dataverseContext,
            IList<KT_QuestionnaireLines> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            foreach (var question in questions)
            {
                dataverseContext.AddObject(question);
            }

            dataverseContext.SaveChanges();
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLines Answers
        private void CreateQuestionnaireLinesAnswers(
            DataverseContext dataverseContext,
            IList<KTR_QuestionnaireLinesAnswerList> answers)
        {
            if (answers == null || answers.Count == 0)
            {
                return;
            }

            foreach (var answer in answers)
            {
                dataverseContext.AddObject(answer);
            }

            dataverseContext.SaveChanges();
        }
        #endregion

        #region Queries to Dataverse - Project
        private KT_Project CreateProject(
            DataverseContext dataverseContext,
            KT_Project project)
        {
            if (project == null)
            {
                return null;
            }

            dataverseContext.AddObject(project);
            dataverseContext.SaveChanges();

            return project;
        }
        #endregion
    }
}
