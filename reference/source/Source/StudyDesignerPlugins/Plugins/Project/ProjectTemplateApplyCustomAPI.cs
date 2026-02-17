using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Services.Description;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLine;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.QuestionnaireLineAnswer;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Kantar.StudyDesignerLite.Plugins.Project
{
    public class ProjectTemplateApplyCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ProjectTemplateApplyCustomAPI";

        public ProjectTemplateApplyCustomAPI()
           : base(typeof(ProjectTemplateApplyCustomAPI))
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

            tracingService.Trace($"Starting {PluginName} {KT_Project.EntityLogicalName}");

            if (context.InputParameters.Contains("ktr_project_id") && context.InputParameters["ktr_project_id"] is string myValue)
            {
                var entity = service.Retrieve(KT_Project.EntityLogicalName, Guid.Parse(myValue), new ColumnSet(true));

                if (entity.LogicalName == KT_Project.EntityLogicalName)
                {
                    var project = entity.ToEntity<KT_Project>();

                    InactivateQuestions(service, tracingService, project.Id);

                    var projectConfigurations = GetProjectConfiguration(service, project.Id);
                    var projectProductConfigIds = projectConfigurations
                        .Select(c => c.Id)
                        .Distinct()
                        .ToList();
                    var projectConfigurationAnswers = GetProjectProductConfigAnswers(service, projectProductConfigIds);

                    var dependencyRulesSingleCoded = GetSingleCodedDependencyRules(service, projectConfigurations, projectConfigurationAnswers);
                    var dependencyRulesMultiCoded = GetMultiCodedDependencyRules(
                        service,
                        projectConfigurations);

                    tracingService.Trace($"Applying Template");
                    using (var dataverseContext = new DataverseContext(service))
                    {
                        var applyTemplateService = new ProductTemplateApplyService(dataverseContext, service, tracingService);

                        var configQuestionsAnswerIds = projectConfigurationAnswers
                            .Select(x => x.KTR_ConfigurationAnswer.Id)
                            .ToList();

                        var validDependencyRulesMultiCoded = applyTemplateService.FilterExactMatchMultiChoiceDependencyRules(
                            dependencyRulesMultiCoded,
                            configQuestionsAnswerIds);

                        var dependencyRules = dependencyRulesSingleCoded
                            .Concat(validDependencyRulesMultiCoded)
                            .ToList();

                        var result = applyTemplateService.ApplyProductTemplate(project.KTR_ProductTemplate.Id, dependencyRules);
                        tracingService.Trace($"Successfully Applied Template");

                        var questionIds = result
                            .Select(x => x.QuestionId);
                        var questionBanks = GetQuestionBanks(service, questionIds);
                        var answerList = GetQuestionAnswers(service, questionBanks);
                        var questionsAdded = InsertQuestions(service, questionBanks, result, project.Id, answerList, tracingService);
                        tracingService.Trace($"Successfully Inserted Questions and Answers in the Project Questionnaire.");

                        context.OutputParameters["ktr_response"] = JsonConvert.SerializeObject(questionsAdded);
                    }

                }
            }
        }

        /// <summary>
        /// Get project Product configuration with Configuration Question and Configuration Answer reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectId">Project</param>
        /// <returns> List of Project Product Configuration</returns>
        private List<KTR_ProjectProductConfig> GetProjectConfiguration(IOrganizationService service, Guid projectId)
        {
            var query = new QueryExpression(KTR_ProjectProductConfig.EntityLogicalName)
            {
                Distinct = false,
                ColumnSet = new ColumnSet(
                    KTR_ProjectProductConfig.Fields.Id,
                    KTR_ProjectProductConfig.Fields.KTR_Name,
                    KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion),
            };
            query.Criteria.AddCondition(KTR_ProjectProductConfig.Fields.KTR_KT_Project, ConditionOperator.Equal, projectId);
            query.Criteria.AddCondition(KTR_ProjectProductConfig.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfig_StatusCode.Active);

            var configQuestionLink = query.AddLink(
                KTR_ConfigurationQuestion.EntityLogicalName,
                KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion,
                KTR_ConfigurationQuestion.Fields.Id,
                JoinOperator.Inner);

            var results = service.RetrieveMultiple(query);

            return results
                .Entities
                .Select(e => e.ToEntity<KTR_ProjectProductConfig>())
                .ToList();

        }

        /// <summary>
        /// Get Dependency Rules with Configuration Question and Configuration Answer reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectProductConfigs"></param>
        /// <returns></returns>
        private List<KTR_DependencyRule> GetSingleCodedDependencyRules(
            IOrganizationService service,
            List<KTR_ProjectProductConfig> projectProductConfigs,
            List<KTR_ProjectProductConfigQuestionAnswer> selectedAnswers)
        {
            if (projectProductConfigs == null || projectProductConfigs.Count == 0)
            {
                return new List<KTR_DependencyRule>();
            }

            if (selectedAnswers == null || selectedAnswers.Count == 0)
            {
                return new List<KTR_DependencyRule>();
            }

            var configQuestionIds = projectProductConfigs
                .Select(c => c.KTR_ConfigurationQuestion.Id)
                .Distinct()
                .ToArray();

            var selectedAnswerIds = selectedAnswers
                .Select(x => x.Id)
                .Distinct()
                .ToArray();

            if (configQuestionIds.Length == 0)
            {
                return new List<KTR_DependencyRule>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_DependencyRule.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Distinct = true,
            };

            query.Criteria.AddCondition(KTR_DependencyRule.Fields.KTR_ConfigurationQuestion, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());
            query.Criteria.AddCondition(KTR_DependencyRule.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);

            // INNER JOIN ConfigurationQuestion
            var configQuestionLink = query.AddLink(
                KTR_ConfigurationQuestion.EntityLogicalName,
                KTR_DependencyRule.Fields.KTR_ConfigurationQuestion,
                KTR_ConfigurationQuestion.Fields.Id,
                JoinOperator.Inner
            );
            configQuestionLink.LinkCriteria
               .AddCondition(KTR_ConfigurationQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationQuestion_StatusCode.Active);
            configQuestionLink.LinkCriteria
              .AddCondition(KTR_ConfigurationQuestion.Fields.KTR_Rule, ConditionOperator.Equal, (int)KTR_Rule.SingleCoded);

            // INNER JOIN ProjectConfigQuestionAnswer
            var projectConfigQuestionAnswerLink = query.AddLink(
                KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName,
                KTR_DependencyRule.Fields.KTR_TriggeringAnswer,
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer,
                JoinOperator.Inner
            );
            projectConfigQuestionAnswerLink.LinkCriteria
                .AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfigQuestionAnswer_StatusCode.Active);
            projectConfigQuestionAnswerLink.LinkCriteria
                .AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.Id, ConditionOperator.In, selectedAnswerIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRule>())
                .ToList();
        }

        private List<KTR_DependencyRule> GetMultiCodedDependencyRules(
            IOrganizationService service,
            List<KTR_ProjectProductConfig> projectProductConfigs)
        {
            if (projectProductConfigs == null || projectProductConfigs.Count == 0)
            {
                return new List<KTR_DependencyRule>();
            }

            var configQuestionIds = projectProductConfigs
                .Select(c => c.KTR_ConfigurationQuestion.Id)
                .Distinct()
                .ToArray();

            if (configQuestionIds.Length == 0)
            {
                return new List<KTR_DependencyRule>();
            }

            var query = new QueryExpression()
            {
                Distinct = true,
                EntityName = KTR_DependencyRule.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition(KTR_DependencyRule.Fields.KTR_ConfigurationQuestion, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());
            query.Criteria.AddCondition(KTR_DependencyRule.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);

            // INNER JOIN DependencyRuleAnswer
            var dependencyRuleAnswersLink = query.AddLink(
                KTR_DependencyRuleAnswer.EntityLogicalName,
                KTR_DependencyRule.Fields.KTR_DependencyRuleId,
                KTR_DependencyRuleAnswer.Fields.KTR_DependencyRule,
                JoinOperator.Inner
            );
            dependencyRuleAnswersLink.LinkCriteria
                .AddCondition(KTR_DependencyRuleAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRuleAnswer_StatusCode.Active);

            // INNER JOIN ProjectConfigQuestionAnswer
            var projectConfigQuestionAnswerLink = dependencyRuleAnswersLink.AddLink(
                KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName,
                KTR_DependencyRuleAnswer.Fields.KTR_ConfigurationAnswer,
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer,
                JoinOperator.Inner
            );

            projectConfigQuestionAnswerLink.LinkCriteria
                .AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfigQuestionAnswer_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            var dependencyRules = results.Entities
                    .Select(e => e.ToEntity<KTR_DependencyRule>())
                    .ToList();
            var dependencyRulesAnswers = GetDependencyRuleAnswers(
                service,
                dependencyRules);

            return dependencyRules;
        }

        private List<KTR_DependencyRuleAnswer> GetDependencyRuleAnswers(IOrganizationService service, List<KTR_DependencyRule> dependencyRules)
        {
            if (dependencyRules == null || dependencyRules.Count == 0)
            {
                return new List<KTR_DependencyRuleAnswer>();
            }

            var dependencyRulesIds = dependencyRules
                .Select(d => d.Id)
                .Distinct()
                .ToArray();

            var query = new QueryExpression()
            {
                EntityName = KTR_DependencyRuleAnswer.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_DependencyRuleAnswer.Fields.KTR_ConfigurationAnswer,
                    KTR_DependencyRuleAnswer.Fields.KTR_ConfigurationQuestion,
                    KTR_DependencyRuleAnswer.Fields.KTR_DependencyRule,
                    KTR_DependencyRuleAnswer.Fields.KTR_DependencyRuleAnswerId),
            };

            query.Criteria.AddCondition(KTR_DependencyRuleAnswer.Fields.KTR_DependencyRule, ConditionOperator.In, dependencyRulesIds.Cast<object>().ToArray());
            query.Criteria.AddCondition(KTR_DependencyRuleAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRuleAnswer>())
                .ToList();
        }

        private List<KTR_ProjectProductConfigQuestionAnswer> GetProjectProductConfigAnswers(
            IOrganizationService service,
            List<Guid> projectProductConfigIds)
        {
            if (projectProductConfigIds == null || projectProductConfigIds.Count == 0)
            {
                return new List<KTR_ProjectProductConfigQuestionAnswer>();
            }

            var query = new QueryExpression(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName);

            query.ColumnSet.AddColumns(
                KTR_ProjectProductConfigQuestionAnswer.Fields.Id,
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion,
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer,
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationQuestion);

            query.Criteria.AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfigQuestionAnswer_StatusCode.Active);
            query.Criteria.AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected, ConditionOperator.Equal, true);
            query.Criteria.AddCondition(
                KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion,
                ConditionOperator.In,
                projectProductConfigIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_ProjectProductConfigQuestionAnswer>())
                .ToList();
        }

        /// <summary>
        /// Get Question Bank using entity reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="questionsContext"></param>
        /// <returns></returns>
        private IList<KT_QuestionBank> GetQuestionBanks(
            IOrganizationService service,
            IEnumerable<Guid> questionIds)
        {
            if (questionIds == null || questionIds.Count() == 0)
            {
                return new List<KT_QuestionBank>();
            }

            var query = new QueryExpression()
            {
                EntityName = KT_QuestionBank.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KT_QuestionBank.Fields.StateCode,
                    KT_QuestionBank.Fields.KT_StandardOrCustom,
                    KT_QuestionBank.Fields.KT_Name,
                    KT_QuestionBank.Fields.KT_QuestionTitle,
                    KT_QuestionBank.Fields.KT_QuestionType,
                    KT_QuestionBank.Fields.KT_QuestionVersion,
                    KT_QuestionBank.Fields.KT_DefaultQuestionText,
                    KT_QuestionBank.Fields.KTR_AnswerList,
                    KT_QuestionBank.Fields.KT_QuestionRationale,
                    KT_QuestionBank.Fields.KTR_ScripterNotes,
                    KT_QuestionBank.Fields.KTR_RowSortOrder,
                    KT_QuestionBank.Fields.KTR_ColumnSortOrder,
                    KT_QuestionBank.Fields.KTR_AnswerMin,
                    KT_QuestionBank.Fields.KTR_AnswerMax,
                    KT_QuestionBank.Fields.KTR_QuestionFormatDetails,
                    KT_QuestionBank.Fields.KTR_CustomNotes,
                    KT_QuestionBank.Fields.KT_IsDummyQuestion),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionBank.Fields.Id, ConditionOperator.In, questionIds.Cast<object>().ToArray())
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionBank>())
                .ToList();
        }

        /// <summary>
        /// Get Question Bank using entity reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="questionsContext"></param>
        /// <returns></returns>
        private List<KTR_QuestionAnswerList> GetQuestionAnswers(
           IOrganizationService service,
           IList<KT_QuestionBank> questionBanks)
        {
            var answerLists = new List<KTR_QuestionAnswerList>();

            if (questionBanks == null || questionBanks.Count == 0)
            {
                return answerLists;
            }

            var questionBankIds = questionBanks.Select(qb => qb.Id).ToList();

            var query = new QueryExpression(KTR_QuestionAnswerList.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                   Conditions =
                   {
                      new ConditionExpression(KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank, ConditionOperator.In, questionBankIds),
                      new ConditionExpression(KTR_QuestionAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionAnswerList_StatusCode.Active) // Optional: Only active records
                   }
                }
            };

            var result = service.RetrieveMultiple(query);

            foreach (var entity in result.Entities)
            {
                answerLists.Add(entity.ToEntity<KTR_QuestionAnswerList>());
            }

            return answerLists;
        }

        private List<TemplateCustomActionResponse> InsertQuestions(
            IOrganizationService service,
            IList<KT_QuestionBank> questionBanks,
            IList<TemplateLineQuestionResult> templateLinesResult,
            Guid projectId, List<KTR_QuestionAnswerList> answerLists,
            ITracingService tracingService
            )
        {
            var index = 0;

            var requestCollection = new OrganizationRequestCollection();
            var responseCollection = new List<TemplateCustomActionResponse>();

            var answersLineRequest = new OrganizationRequestCollection();

            foreach (var templateLine in templateLinesResult)
            {
                var questionId = templateLine.QuestionId;

                var questionBank = questionBanks.First(x => x.Id == questionId);

                responseCollection.Add(new TemplateCustomActionResponse
                {
                    QuestionId = questionBank.Id,
                    QuestionName = questionBank.KT_Name
                });

                var questionLine = QuestionnaireLineMapper.MapToEntity(templateLine, questionBank, projectId, ++index);

                requestCollection.Add(new CreateRequest { Target = questionLine });

                // Filter answer list records linked to this question bank
                var relatedAnswers = answerLists
                    .Where(a => a.KTR_KT_QuestionBank != null && a.KTR_KT_QuestionBank.Id == questionBank.Id)
                    .ToList();

                var requestAnswersCollection = new OrganizationRequestCollection();

                // Create KT_QuestionnaireLineAnswerList records
                foreach (var answer in relatedAnswers)
                {
                    var answerLine = QuestionnaireLineAnswerMapper.MapToEntity(answer, questionLine.KT_QuestionnaireLinesId.Value);

                    tracingService.Trace($"Adding answer for insertion. QuestionnaireLineId - {questionLine.KT_QuestionnaireLinesId.Value}. Answer Code - {answerLine.KTR_AnswerCode}");

                    answersLineRequest.Add(new CreateRequest { Target = answerLine });
                }
            }

            var executeMultiple = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                }
            };

            var response = (ExecuteMultipleResponse)service.Execute(executeMultiple);

            if (response.IsFaulted)
            {
                throw new InvalidPluginExecutionException($"Error while inserting questions: {response.Responses.First(r => r.Fault != null).Fault.Message}");
            }

            var executeMultipleAnswers = new ExecuteMultipleRequest
            {
                Requests = answersLineRequest,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = false,
                    ReturnResponses = true,
                }
            };

            var answersResponse = (ExecuteMultipleResponse)service.Execute(executeMultipleAnswers);

            if (answersResponse.IsFaulted)
            {
                throw new InvalidPluginExecutionException($"Error while inserting answers: {answersResponse.Responses.First(r => r.Fault != null).Fault.Message}");
            }

            return responseCollection;
        }

        private void InactivateQuestions(
            IOrganizationService service,
            ITracingService tracingService,
            Guid projectId)
        {
            var questionnaireLines = GetQuestionnaireLines(service, projectId);

            foreach (var ql in questionnaireLines)
            {
                ql.StateCode = KT_QuestionnaireLines_StateCode.Inactive;
                ql.StatusCode = KT_QuestionnaireLines_StatusCode.Inactive;
            }

            UpdateQuestionnaireLines(service, questionnaireLines);
            tracingService.Trace($"QuestionnaireLines inactivated.");

            InactivateAnswers(service, tracingService, questionnaireLines);
        }

        private void InactivateAnswers(
            IOrganizationService service,
            ITracingService tracingService,
            IList<KT_QuestionnaireLines> questionnaireLines)
        {
            var questionnaireLineIds = questionnaireLines
               .Select(x => x.Id);
            var questionnaireLineAnswers = GetQuestionnaireLinesAnswerList(service, questionnaireLineIds);

            foreach (var qla in questionnaireLineAnswers)
            {
                qla.StateCode = KTR_QuestionnaireLinesAnswerList_StateCode.Inactive;
                qla.StatusCode = KTR_QuestionnaireLinesAnswerList_StatusCode.Inactive;
            }

            UpdateQuestionnaireLineAnswers(service, questionnaireLineAnswers);
            tracingService.Trace($"QuestionnaireLines Answers inactivated.");
        }

        #region Queries to Dataverse - QuestionnaireLines
        public IList<KT_QuestionnaireLines> GetQuestionnaireLines(
            IOrganizationService service,
            Guid projectId)
        {
            var query = new QueryExpression()
            {
                EntityName = KT_QuestionnaireLines.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KT_QuestionnaireLines.Fields.Id),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, projectId),
                        new ConditionExpression(KT_QuestionnaireLines.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionnaireLines_StatusCode.Active)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionnaireLines>())
                .ToList();
        }

        private void UpdateQuestionnaireLines(
            IOrganizationService service,
            IList<KT_QuestionnaireLines> questionnaireLines)
        {
            if (questionnaireLines == null || questionnaireLines.Count() == 0)
            {
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var ql in questionnaireLines)
            {
                var updateRequest = new UpdateRequest
                {
                    Target = ql
                };

                requestCollection.Add(updateRequest);
            }

            // Batch request
            var executeMultipleRequest = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            service.Execute(executeMultipleRequest);
        }
        #endregion

        #region Queries to Dataverse - QuestionnaireLineAnswers
        public IList<KTR_QuestionnaireLinesAnswerList> GetQuestionnaireLinesAnswerList(
            IOrganizationService service,
            IEnumerable<Guid> questionnaireLineIds)
        {
            if (questionnaireLineIds == null || questionnaireLineIds.Count() == 0)
            {
                return new List<KTR_QuestionnaireLinesAnswerList>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionnaireLinesAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_QuestionnaireLinesAnswerList.Fields.Id),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine, ConditionOperator.In, questionnaireLineIds.Cast<object>().ToArray()),
                         new ConditionExpression(KTR_QuestionnaireLinesAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionnaireLinesAnswerList_StatusCode.Active)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionnaireLinesAnswerList>())
                .ToList();
        }

        private void UpdateQuestionnaireLineAnswers(
            IOrganizationService service,
            IList<KTR_QuestionnaireLinesAnswerList> questionnaireLineAnswers)
        {
            if (questionnaireLineAnswers == null || questionnaireLineAnswers.Count() == 0)
            {
                return;
            }

            var requestCollection = new OrganizationRequestCollection();

            foreach (var ql in questionnaireLineAnswers)
            {
                var updateRequest = new UpdateRequest
                {
                    Target = ql
                };

                requestCollection.Add(updateRequest);
            }

            // Batch request
            var executeMultipleRequest = new ExecuteMultipleRequest
            {
                Requests = requestCollection,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            service.Execute(executeMultipleRequest);
        }
        #endregion
    }
}
