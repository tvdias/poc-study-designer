using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace Kantar.StudyDesignerLite.Plugins.Project
{
    /// <summary>
    /// NOTE: Do not use/update this!
    /// This plugin was replacecd by this one: ProjectTemplateApplyCustomAPI
    /// </summary>

    public class ProjectTemplateApplyPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ProjectTemplateApplyPostOperation";

        public ProjectTemplateApplyPostOperation()
           : base(typeof(ProjectTemplateApplyPostOperation))
        {
        }

        
        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {/*
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            ITracingService tracingService = localPluginContext.TracingService;

            try
            {
                IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
                IOrganizationService service = localPluginContext.CurrentUserService;

                EntityReference entityRef = (EntityReference)context.InputParameters["Target"];

                Entity entity = service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(true));

                tracingService.Trace($"{PluginName} {entity.LogicalName}");

                if (entity.LogicalName == KT_Project.EntityLogicalName)
                {
                    var project = entity.ToEntity<KT_Project>();

                    var projectConfiguration = GetProjectConfiguration(service, project.Id);

                    var productTemplateLine = GetProductTemplateLine(service, project.KTR_ProductTemplate.Id);

                    var questionsFromTemplate = GetTemplateQuestions(service, productTemplateLine);

                    var depencyRules = GetDependencyRules(service, projectConfiguration);

                    var questions = ApplyRules(service, depencyRules, questionsFromTemplate);

                    var questionBanks = GetQuestionBanks(service, questions);

                    var questionsAdded = InsertQuestions(service, questionBanks, questions, project.Id);

                    context.OutputParameters["response"] = JsonConvert.SerializeObject(questionsAdded);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    tracingService?.Trace($"{ex.Message}");
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                else
                {
                    tracingService?.Trace($"An expected error occurred executing Plugin {PluginName}: {0}", ex.ToString());
                    throw new InvalidPluginExecutionException($"An expected error occurred executing Plugin {PluginName}.", ex);
                }
            }
            */
        }

        /*

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

            var results = service.RetrieveMultiple(query);

            return results
                .Entities
                .Select(e => e.ToEntity<KTR_ProjectProductConfig>())
                .ToList();

        }

        /// <summary>
        /// Get Product Template Line with Module and Question Bank Name reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="productTemplateId"></param>
        /// <returns></returns>
        private List<KTR_ProductTemplateLine> GetProductTemplateLine(IOrganizationService service, Guid productTemplateId)
        {
            var query = new QueryExpression()
            {
                EntityName = KTR_ProductTemplateLine.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_ProductTemplateLine.Fields.Id,
                    KTR_ProductTemplateLine.Fields.StatusCode,
                    KTR_ProductTemplateLine.Fields.KTR_KT_QuestionBank,
                    KTR_ProductTemplateLine.Fields.KTR_KT_Module,
                    KTR_ProductTemplateLine.Fields.KTR_DisplayOrder),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ProductTemplateLine.Fields.KTR_ProductTemplate, ConditionOperator.Equal, productTemplateId),
                        new ConditionExpression(KTR_ProductTemplateLine.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProductTemplateLine_StatusCode.Active),
                    }
                },
            };

            var results = service.RetrieveMultiple(query);

            var productTemplateLines = results.Entities.Select(e => e.ToEntity<KTR_ProductTemplateLine>()).ToList();

            return productTemplateLines;
        }

        /// <summary>
        /// Get Question Bank using product template lines
        /// </summary>
        /// <param name="service"></param>
        /// <param name="productTemplateLines"></param>
        /// <returns></returns>
        private OrderedDictionary GetTemplateQuestions(IOrganizationService service, List<KTR_ProductTemplateLine> productTemplateLines)
        {
            var orderedQuestionsContextDict = new OrderedDictionary();

            foreach (var line in productTemplateLines.OrderBy(x => x.KTR_DisplayOrder))
            {
                if (line.KTR_KT_QuestionBank != null)
                {
                    if (!orderedQuestionsContextDict.Contains(line.KTR_KT_QuestionBank.Id))
                    {
                        orderedQuestionsContextDict.Add(line.KTR_KT_QuestionBank.Id, new TemplateQuestionContext(line.KTR_KT_QuestionBank, null, null));
                    }
                }
                else
                {
                    var moduleQuestions = GetQuestionContextByModule(service, line.KTR_KT_Module);
                    foreach (var context in moduleQuestions)
                    {
                        if (!orderedQuestionsContextDict.Contains(context.Question.Id))
                        {
                            orderedQuestionsContextDict.Add(context.Question.Id, context);
                        }
                    }
                }
            }

            return orderedQuestionsContextDict;
        }

        /// <summary>
        /// Get Dependency Rules with Configuration Question and Configuration Answer reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectProductConfigs"></param>
        /// <returns></returns>
        private List<KTR_DependencyRule> GetDependencyRules(IOrganizationService service, List<KTR_ProjectProductConfig> projectProductConfigs)
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
                EntityName = KTR_DependencyRule.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    KTR_DependencyRule.Fields.KTR_DependencyRuleId,
                    KTR_DependencyRule.Fields.KTR_ConfigurationQuestion,
                    KTR_DependencyRule.Fields.KTR_TriggeringAnswer,
                    KTR_DependencyRule.Fields.KTR_Classification,
                    KTR_DependencyRule.Fields.KTR_Type,
                    KTR_DependencyRule.Fields.KTR_ContentType,
                    KTR_DependencyRule.Fields.KTR_KT_QuestionBank,
                    KTR_DependencyRule.Fields.KTR_KT_Module,
                    KTR_DependencyRule.Fields.KTR_Tag),
            };

            query.Criteria.AddCondition(KTR_DependencyRule.Fields.KTR_ConfigurationQuestion, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());
            query.Criteria.AddCondition(KTR_DependencyRule.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRule>())
                .ToList();
        }

        private OrderedDictionary ApplyRules(
            IOrganizationService service,
            List<KTR_DependencyRule> rules,
            OrderedDictionary questionsFromTemplate)
        {
            if (rules == null || rules.Count == 0)
            {
                return questionsFromTemplate;
            }

            var orderededRules = rules
                .OrderBy(x => x.KTR_Classification != KTR_DependencyRule_KTR_Classification.Primary)
                .ThenBy(x => x.CreatedOn)
                .ToList();

            foreach (var rule in orderededRules)
            {
                if (rule.KTR_Type == KTR_DependencyRule_KTR_Type.Exclude)
                {
                    switch (rule.KTR_ContentType)
                    {
                        case KTR_DependencyRule_KTR_ContentType.Question:
                            ExcludeQuestions(questionsFromTemplate, new List<Guid> { rule.KTR_KT_QuestionBank.Id });
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Module:
                            var keysToRemove = new List<Guid>();
                            foreach (DictionaryEntry entry in questionsFromTemplate)
                            {
                                var questionContext = (TemplateQuestionContext)entry.Value;

                                if (questionContext.ModuleQuestionBank != null
                                    && questionContext.ModuleQuestionBank.Id == rule.KTR_KT_Module.Id)
                                {
                                    keysToRemove.Add((Guid)entry.Key);
                                }
                            }

                            if (keysToRemove != null && keysToRemove.Count > 0)
                            {
                                ExcludeQuestions(questionsFromTemplate, keysToRemove);
                            }
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Tag:
                            var tag = rule.KTR_Tag;

                            var questionBankIds = GetAllQuestionBanksByTagId(service, tag.Id, questionsFromTemplate.Keys.Cast<Guid>().ToList());

                            ExcludeQuestions(questionsFromTemplate, questionBankIds);
                            break;
                    }
                }
                else if (rule.KTR_Type == KTR_DependencyRule_KTR_Type.Include)
                {
                    switch (rule.KTR_ContentType)
                    {
                        case KTR_DependencyRule_KTR_ContentType.Question:
                            var questionEntityReference = new EntityReference(KT_QuestionBank.EntityLogicalName, rule.KTR_KT_QuestionBank.Id);

                            IncludeQuestions(questionsFromTemplate, new List<Guid> { questionEntityReference.Id }, new TemplateQuestionContext(questionEntityReference, null, null));
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Module:
                            var moduleQuestions = GetQuestionContextByModule(service, rule.KTR_KT_Module);

                            foreach (var context in moduleQuestions)
                            {
                                IncludeQuestions(questionsFromTemplate, new List<Guid> { context.Question.Id }, context);
                            }
                            break;
                        case KTR_DependencyRule_KTR_ContentType.Tag:
                            var tag = rule.KTR_Tag;

                            var questionBankIds = GetAllQuestionBanksByTagId(service, tag.Id);

                            IncludeQuestions(questionsFromTemplate, questionBankIds, new TemplateQuestionContext(null, null, tag));
                            break;
                    }
                }
            }

            return questionsFromTemplate;
        }

        private List<Guid> GetAllQuestionBanksByTagId(IOrganizationService service, Guid tagId, List<Guid> questionBankIds = null)
        {
            var query = new QueryExpression(KT_QuestionBank.EntityLogicalName);

            query.ColumnSet.AddColumn(KT_QuestionBank.Fields.KT_QuestionBankId);

            if (questionBankIds != null && questionBankIds.Count > 0)
            {
                query.Criteria.AddCondition(KT_QuestionBank.Fields.KT_QuestionBankId, ConditionOperator.In, questionBankIds.Cast<object>().ToArray());
            }

            query.Criteria.AddCondition(KT_QuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionBank_StatusCode.Active);

            var linkTag = query.AddLink(
                KTR_Tag_KT_QuestionBank.EntityLogicalName,
                KT_QuestionBank.Fields.KT_QuestionBankId,
                KTR_Tag_KT_QuestionBank.Fields.KT_QuestionBankId);

            linkTag.LinkCriteria.AddCondition(KTR_Tag_KT_QuestionBank.Fields.KTR_TagId, ConditionOperator.Equal, tagId);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.GetAttributeValue<Guid>(KT_QuestionBank.Fields.KT_QuestionBankId))
                .ToList();
        }

        /// <summary>
        /// Get Question Bank using entity reference
        /// </summary>
        /// <param name="service"></param>
        /// <param name="questionsContext"></param>
        /// <returns></returns>
        private List<KT_QuestionBank> GetQuestionBanks(IOrganizationService service, OrderedDictionary questionsContext)
        {
            if (questionsContext.Count == 0)
            {
                return new List<KT_QuestionBank>();
            }

            var keysArray = new object[questionsContext.Count];
            questionsContext.Keys.CopyTo(keysArray, 0);
            var query = new QueryExpression()
            {
                Distinct = false,
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
                    KT_QuestionBank.Fields.KT_ScriptOrNotes),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_QuestionBank.Fields.Id, ConditionOperator.In, keysArray)
                    }
                }
            };

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionBank>())
                .ToList();
        }

        private List<TemplateCustomActionResponse> InsertQuestions(
            IOrganizationService service,
            List<KT_QuestionBank> questionBanks,
            OrderedDictionary orderedQuestionsContextDict,
            Guid projectId)
        {
            var index = 0;

            var requestCollection = new OrganizationRequestCollection();

            var responseCollection = new List<TemplateCustomActionResponse>();

            foreach (DictionaryEntry entry in orderedQuestionsContextDict)
            {
                var questionId = Guid.Parse(entry.Key.ToString());

                var questionBank = questionBanks.First(x => x.Id == questionId);

                responseCollection.Add(new TemplateCustomActionResponse
                {
                    QuestionId = questionBank.Id,
                    QuestionName = questionBank.KT_Name
                });

                var questionContext = (TemplateQuestionContext)entry.Value;

                var questionLine = new Entity(KT_QuestionnaireLines.EntityLogicalName);

                questionLine[KT_QuestionnaireLines.Fields.KTR_Project] = new EntityReference(KT_Project.EntityLogicalName, projectId);
                questionLine[KT_QuestionnaireLines.Fields.KTR_Module] = questionContext.ModuleQuestionBank;

                questionLine[KT_QuestionnaireLines.Fields.KT_QuestionSortOrder] = ++index;
                questionLine[KT_QuestionnaireLines.Fields.StateCode] = (int)questionBank.StateCode;
                questionLine[KT_QuestionnaireLines.Fields.KT_StandardOrCustom] = new OptionSetValue((int)questionBank.KT_StandardOrCustom);
                questionLine[KT_QuestionnaireLines.Fields.KT_QuestionVariableName] = questionBank.KT_Name;
                questionLine[KT_QuestionnaireLines.Fields.KT_QuestionTitle] = questionBank.KT_QuestionTitle;
                questionLine[KT_QuestionnaireLines.Fields.KT_QuestionType] = new OptionSetValue((int)questionBank.KT_QuestionType);
                questionLine[KT_QuestionnaireLines.Fields.KTR_QuestionVersion] = questionBank.KT_QuestionVersion;
                questionLine[KT_QuestionnaireLines.Fields.KT_QuestionText2] = questionBank.KT_DefaultQuestionText;
                questionLine[KT_QuestionnaireLines.Fields.KTR_AnswerList] = questionBank.KTR_AnswerList;
                questionLine[KT_QuestionnaireLines.Fields.KTR_QuestionRationale] = questionBank.KT_QuestionRationale;
                questionLine[KT_QuestionnaireLines.Fields.KT_ScriptOrNotes] = questionBank.KT_ScriptOrNotes;

                requestCollection.Add(new CreateRequest { Target = questionLine });
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
                throw new InvalidPluginExecutionException($"Error while inserting questions: {response.Responses.First().Fault.Message}");
            }

            return responseCollection;
        }

        private List<TemplateQuestionContext> GetQuestionContextByModule(IOrganizationService service, EntityReference module)
        {
            if (module == null)
            {
                return new List<TemplateQuestionContext>();
            }

            var query = new QueryExpression(KTR_ModuleQuestionBank.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank),
            };

            query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_Module, ConditionOperator.Equal, module.Id);
            query.Criteria.AddCondition(KTR_ModuleQuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ModuleQuestionBank_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => new TemplateQuestionContext(e.ToEntity<KTR_ModuleQuestionBank>().KTR_QuestionBank, module, null))
                .ToList();
        }

        #region Auxiliar

        private void ExcludeQuestions(OrderedDictionary questionsFromTemplate, List<Guid> questionIds)
        {
            foreach (var q in questionIds)
            {
                questionsFromTemplate.Remove(q);
            }
        }

        private void IncludeQuestions(OrderedDictionary questionsFromTemplate, List<Guid> questionIds, TemplateQuestionContext context)
        {
            foreach (var q in questionIds)
            {
                if (!questionsFromTemplate.Contains(q))
                {
                    questionsFromTemplate.Add(q, context);
                }
            }
        }

        #endregion
         */
    }
}
