using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.ApplyTemplateSimulation;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product.ApplyTemplateSimulation;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Product
{
    public class ApplyTemplateSimulationCustomAPI : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ApplyTemplateSimulationCustomAPI";

        public ApplyTemplateSimulationCustomAPI()
           : base(typeof(ApplyTemplateSimulationCustomAPI))
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
            var dataverseContext = new DataverseContext(service);

            tracingService.Trace($"{PluginName} {KT_Project.EntityLogicalName}");

            var productIdParam = context.GetInputParameter<Guid>("productId");
            var productTemplateIdParam = context.GetInputParameter<Guid>("productTemplateId");
            var configurationQuestionsParam = context.GetInputParameter<string>("configurationQuestions");

            var configQuestionsRequest = JsonHelper.Deserialize<List<ConfigurationQuestionRequest>>(configurationQuestionsParam, "configurationQuestions")
                    ?? throw new InvalidPluginExecutionException("Invalid ConfigurationQuestions.");
            var product = GetProduct(dataverseContext, productIdParam)
                ?? throw new InvalidPluginExecutionException("ProductId not found.");
            var productTemplate = GetProductTemplate(dataverseContext, productTemplateIdParam)
                ?? throw new InvalidPluginExecutionException("ProductTemplateId not found.");
            var configQuestionIds = configQuestionsRequest
                .Select(x => x.Id)
                .ToList();
            var configQuestions = GetConfigurationQuestionsByProductId(service, configQuestionIds, productIdParam);
            ValidateConfigurationQuestionsRequest(configQuestions, configQuestionsRequest);
                
            tracingService.Trace($"ConfigurationQuestions request validations executed");

            var configQuestionAnswerIds = configQuestionsRequest
                .SelectMany(x => x.Answers)
                .Select(x => x.Id)
                .ToList();
            var configQuestionAnswers = GetConfigurationAnswers(service, configQuestionAnswerIds, configQuestionIds);
            ValidateConfigurationAnswersRequest(configQuestionAnswers, configQuestionAnswerIds);
            if (configQuestions.Count > configQuestionAnswers.Count)
            {
                throw new InvalidPluginExecutionException("Configuration Answers not found.");
            }

            tracingService.Trace($"ConfigurationAnswers request validations executed");

            var result = ApplyProductTemplate(
                dataverseContext,
                service,
                tracingService,
                configQuestionIds,
                configQuestionAnswerIds,
                productTemplate);

            tracingService.Trace($"ProductTemplate Applied successfully");

            var finalResult = MapFinalResult(service, result);

            context.OutputParameters["ktr_response"] = JsonHelper.Serialize(finalResult);
        }

        private IList<TemplateLineQuestionResult> ApplyProductTemplate(
            DataverseContext dataverseContext,
            IOrganizationService service,
            ITracingService tracingService,
            IList<Guid> configQuestionIds,
            IList<Guid> configQuestionAnswerIds,
            KTR_ProductTemplate productTemplate)
        {
            var dependencyRulesSingleCoded = GetSingleCodedDependencyRules(service, configQuestionIds, configQuestionAnswerIds);
            var dependencyRulesMultiCoded = GetMultiCodedDependencyRules(service, configQuestionIds, configQuestionAnswerIds);

            tracingService.Trace($"SingleCoded DependencyRules count {dependencyRulesSingleCoded.Count().ToString()}");
            tracingService.Trace($"MultiCoded DependencyRules count {dependencyRulesMultiCoded.Count().ToString()}");

            var applyTemplateService = new ProductTemplateApplyService(dataverseContext, service, tracingService);

            var validDependencyRulesMultiCoded = applyTemplateService.FilterExactMatchMultiChoiceDependencyRules(
                dependencyRulesMultiCoded,
                configQuestionAnswerIds);

            tracingService.Trace($"FilterExactMatchMultiChoiceDependencyRules count {validDependencyRulesMultiCoded.Count.ToString()}");

            var dependencyRules = dependencyRulesSingleCoded
                .Concat(validDependencyRulesMultiCoded)
                .ToList();

            return applyTemplateService.ApplyProductTemplate(productTemplate.Id, dependencyRules);
        }

        private void ValidateConfigurationQuestionsRequest(
            IList<KTR_ConfigurationQuestion> configQuestions,
            IList<ConfigurationQuestionRequest> configQuestionsRequest)
        {
            if (configQuestions != null)
            {
                foreach (var cq in configQuestions)
                {
                    var request = configQuestionsRequest
                        .First(x => x.Id == cq.Id);

                    if (request.Answers == null || request.Answers.Count == 0)
                    {
                        throw new InvalidPluginExecutionException($"Configuration Answers not found in Configuration Question: {cq.Id}");
                    }

                    if (cq.KTR_Rule == KTR_Rule.SingleCoded && request.Answers.Count > 1)
                    {
                        throw new InvalidPluginExecutionException($"Single-coded Configuration Questions should have only one answer responded: {cq.Id}");
                    }
                }

                //Make sure each question in request exists in DB
                if (configQuestions.Count < configQuestionsRequest.Count)
                {
                    foreach (var requestQuestion in configQuestionsRequest)
                    {
                        if (!configQuestions.Any(x => x.Id == requestQuestion.Id))
                        {
                            throw new InvalidPluginExecutionException($"Question not found: {requestQuestion.Id}");

                        }
                    }
                }
            }
        }

        private void ValidateConfigurationAnswersRequest(IList<KTR_ConfigurationAnswer> configAnswers, IList<Guid> configAnswersRequest)
        {
            //Make sure each answer in request exists in DB
            if (configAnswers.Count < configAnswersRequest.Count)
            {
                foreach (var requestAnswerId in configAnswersRequest)
                {
                    if (!configAnswers.Any(x => x.Id == requestAnswerId))
                    {
                        throw new InvalidPluginExecutionException($"Answer not found: {requestAnswerId}");
                    }
                }
            }
        }

        private ApplyTemplateSimulationResponse MapFinalResult(IOrganizationService service, IList<TemplateLineQuestionResult> result)
        {
            var questionIds = result
                    .Select(x => x.QuestionId)
                    .ToList();
            var questionBanks = GetQuestionBanks(service, questionIds);
            var questionAnswers = GetQuestionAnswers(service, questionIds);
            var modules = GetModules(service, questionIds);

            return result.MapToResponse(questionBanks, questionAnswers, modules);
        }

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

        #region Queries to Dataverse - Configuration Questions
        private IList<KTR_ConfigurationQuestion> GetConfigurationQuestionsByProductId(
            IOrganizationService service,
            IList<Guid> configQuestionIds,
            Guid productId)
        {
            if (configQuestionIds == null || configQuestionIds.Count == 0)
            {
                return new List<KTR_ConfigurationQuestion>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_ConfigurationQuestion.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            // INNER JOIN ProductConfigQuestion
            var productConfigQuestionLink = query.AddLink(
                KTR_ProductConfigQuestion.EntityLogicalName,
                KTR_ConfigurationQuestion.Fields.Id,
                KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion
            );
            productConfigQuestionLink.LinkCriteria.AddCondition(KTR_ProductConfigQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProductConfigQuestion_StatusCode.Active);
            productConfigQuestionLink.LinkCriteria.AddCondition(KTR_ProductConfigQuestion.Fields.KTR_Product, ConditionOperator.Equal, productId);

            query.Criteria.AddCondition(KTR_ConfigurationQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationQuestion_StatusCode.Active);
            query.Criteria.AddCondition(KTR_ConfigurationQuestion.Fields.Id, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_ConfigurationQuestion>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Configuration Questions Answers
        private IList<KTR_ConfigurationAnswer> GetConfigurationAnswers(
            IOrganizationService service,
            IList<Guid> configQuestionAnswerIds,
            IList<Guid> configQuestionIds)
        {
            if (configQuestionAnswerIds == null || configQuestionAnswerIds.Count == 0
                || configQuestionIds == null || configQuestionIds.Count == 0)
            {
                return new List<KTR_ConfigurationAnswer>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_ConfigurationAnswer.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            // INNER JOIN ConfigurationQuestion
            var configQuestionLink = query.AddLink(
                 KTR_ConfigurationQuestion.EntityLogicalName,
                 KTR_ConfigurationAnswer.Fields.KTR_ConfigurationQuestion,
                 KTR_ConfigurationQuestion.Fields.Id
             );
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationQuestion_StatusCode.Active);
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.Id, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());

            query.Criteria.AddCondition(KTR_ConfigurationAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationAnswer_StatusCode.Active);
            query.Criteria.AddCondition(KTR_ConfigurationAnswer.Fields.Id, ConditionOperator.In, configQuestionAnswerIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_ConfigurationAnswer>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Dependency Rules
        private IList<KTR_DependencyRule> GetSingleCodedDependencyRules(
            IOrganizationService service,
            IList<Guid> configQuestionIds,
            IList<Guid> configAnswerIds)
        {
            if ((configQuestionIds == null || configQuestionIds.Count == 0)
                && (configAnswerIds == null || configAnswerIds.Count == 0))
            {
                return new List<KTR_DependencyRule>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_DependencyRule.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            // INNER JOIN ConfigurationQuestion
            var configQuestionLink = query.AddLink(
                 KTR_ConfigurationQuestion.EntityLogicalName,
                 KTR_DependencyRule.Fields.KTR_ConfigurationQuestion,
                 KTR_ConfigurationQuestion.Fields.Id
             );
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationQuestion_StatusCode.Active);
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.KTR_Rule, ConditionOperator.Equal, (int)KTR_Rule.SingleCoded);
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.Id, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());

            query.Criteria.AddCondition(KTR_DependencyRule.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);
            query.Criteria.AddCondition(KTR_DependencyRule.Fields.KTR_TriggeringAnswer, ConditionOperator.In, configAnswerIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRule>())
                .ToList();
        }

        private IList<KTR_DependencyRule> GetMultiCodedDependencyRules(
            IOrganizationService service,
            IList<Guid> configQuestionIds,
            IList<Guid> configAnswerIds)
        {
            if ((configQuestionIds == null || configQuestionIds.Count == 0)
                && (configAnswerIds == null || configAnswerIds.Count == 0))
            {
                return new List<KTR_DependencyRule>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_DependencyRule.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            // INNER JOIN ConfigurationQuestion
            var configQuestionLink = query.AddLink(
                 KTR_ConfigurationQuestion.EntityLogicalName,
                 KTR_DependencyRule.Fields.KTR_ConfigurationQuestion,
                 KTR_ConfigurationQuestion.Fields.Id
            );

            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationQuestion_StatusCode.Active);
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.KTR_Rule, ConditionOperator.Equal, (int)KTR_Rule.MultiCoded);
            configQuestionLink.LinkCriteria.AddCondition(KTR_ConfigurationQuestion.Fields.Id, ConditionOperator.In, configQuestionIds.Cast<object>().ToArray());

            // INNER JOIN DependencyRuleAnswer
            var dependencyRuleAnswerLink = query.AddLink(
                KTR_DependencyRuleAnswer.EntityLogicalName,
                KTR_DependencyRule.Fields.Id,
                KTR_DependencyRuleAnswer.Fields.KTR_DependencyRule
            );
            dependencyRuleAnswerLink.LinkCriteria.AddCondition(KTR_DependencyRuleAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRuleAnswer_StatusCode.Active);
            dependencyRuleAnswerLink.LinkCriteria.AddCondition(KTR_DependencyRuleAnswer.Fields.KTR_ConfigurationAnswer, ConditionOperator.In, configAnswerIds.Cast<object>().ToArray());

            query.Criteria.AddCondition(KTR_DependencyRule.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_DependencyRule_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_DependencyRule>())
                .ToList();
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
            };

            query.Criteria.AddCondition(KT_QuestionBank.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionBank_StatusCode.Active);
            query.Criteria.AddCondition(KT_QuestionBank.Fields.Id, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_QuestionBank>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Question Answer Bank
        private IList<KTR_QuestionAnswerList> GetQuestionAnswers(
            IOrganizationService service,
            IList<Guid> questionIds)
        {
            if (questionIds == null || questionIds.Count == 0)
            {
                return new List<KTR_QuestionAnswerList>();
            }

            var query = new QueryExpression()
            {
                EntityName = KTR_QuestionAnswerList.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_QuestionAnswerList_StatusCode.Active);
            query.Criteria.AddCondition(KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());
            
            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KTR_QuestionAnswerList>())
                .ToList();
        }
        #endregion

        #region Queries to Dataverse - Modules
        private IList<KT_Module> GetModules(
            IOrganizationService service,
            IList<Guid> questionIds)
        {
            if (questionIds == null || questionIds.Count == 0)
            {
                return new List<KT_Module>();
            }

            var query = new QueryExpression()
            {
                EntityName = KT_Module.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
            };

            // INNER JOIN ModuleQuestionBank
            var moduleQuestionBankLink = query.AddLink(
                KTR_ModuleQuestionBank.EntityLogicalName,
                KT_Module.Fields.Id,
                KTR_ModuleQuestionBank.Fields.KTR_Module
            );
            moduleQuestionBankLink.LinkCriteria.AddCondition(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank, ConditionOperator.In, questionIds.Cast<object>().ToArray());

            query.Criteria.AddCondition(KT_Module.Fields.StatusCode, ConditionOperator.Equal, (int)KT_Module_StatusCode.Active);

            var results = service.RetrieveMultiple(query);

            return results.Entities
                .Select(e => e.ToEntity<KT_Module>())
                .ToList();
        }
        #endregion
    }
}
