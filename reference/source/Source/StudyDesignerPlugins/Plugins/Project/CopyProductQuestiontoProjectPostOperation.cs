using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Project
{
    public class CopyProductConfigurationQuestionToProjectPostOperation : PluginBase
    {
        public CopyProductConfigurationQuestionToProjectPostOperation()
            : base(typeof(CopyProductConfigurationQuestionToProjectPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            ITracingService tracingService = localContext.TracingService;
            IOrganizationService orgService = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            tracingService.Trace($"{nameof(CopyProductConfigurationQuestionToProjectPostOperation)} plugin execution started.");
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
            {
                CopyQuestions(orgService, tracingService, context);
            }
        }

        public void CopyQuestions(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context)
        {

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
            {
                var projectId = entity.Id;
                var productId = Guid.Empty;
                var isProductFieldPresent = false;

                tracingService.Trace($"Project ID: {projectId}");

                // Check if product field is in the Target
                if (entity.Attributes.Contains(KT_Project.Fields.KTR_Product))
                {
                    isProductFieldPresent = true;
                    if (entity[KT_Project.Fields.KTR_Product] is EntityReference productRef)
                    {
                        productId = productRef.Id; // Product present
                    }
                }

                tracingService.Trace($"Product Present: {isProductFieldPresent}, Product ID: {productId}");
                
                DeleteExistingProjectProductConfigQuestions(service, tracingService, projectId);

                // If Product field was removed, exit after deactivating existing records
                if (isProductFieldPresent && productId == Guid.Empty)
                {
                    tracingService.Trace("Product was removed, exiting.");
                    return;
                }

                // If Product was updated properly, proceed to add new questions
                if (productId != Guid.Empty)
                {
                    tracingService.Trace("Retrieving Product Configuration Questions...");

                    var productConfigQuestionQuery = GetProductConfigQuestionQuery(productId);

                    var productConfigQuestions = service.RetrieveMultiple(productConfigQuestionQuery);
                    tracingService.Trace($"Product Config Questions Found: {productConfigQuestions.Entities.Count}");

                    // Create new Project Product Config records
                    foreach (var pq in productConfigQuestions.Entities)
                    {
                        if (pq.Attributes.Contains(KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion))
                        {
                            var questionRef = (EntityReference)pq[KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion];

                            var projectProductConfig = new KTR_ProjectProductConfig();
                            projectProductConfig[KTR_ProjectProductConfig.Fields.KTR_KT_Project] = new EntityReference("kt_project", projectId);
                            projectProductConfig[KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion] = new EntityReference("ktr_configurationquestion", questionRef.Id);
                            
                            if (pq.Attributes.Contains(KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder))
                            {
                                projectProductConfig[KTR_ProjectProductConfig.Fields.KTR_DisplayOrder] = pq[KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder];
                            }

                            var projectProductConfigId = service.Create(projectProductConfig);
                            tracingService.Trace($"Created Project Product Config for Question ID: {questionRef.Id}");

                            // If Product was updated properly, proceed to add new questions

                            tracingService.Trace("Retrieving Product Configuration Questions...");

                            // call configiration Answer Query
                            var productConfigAnswerQuery = GetConfigurationAnswerQuery(questionRef.Id);

                            var productConfigAnswers = service.RetrieveMultiple(productConfigAnswerQuery);
                            tracingService.Trace($"Product Config Questions Found: {productConfigAnswers.Entities.Count}");

                            // Create new Project Product Config Question Answer records
                            if (productConfigAnswers.Entities.Count > 0)
                            {
                                foreach (var answer in productConfigAnswers.Entities)
                                {
                                    var projectproductConfigAnswerQuery = GetProjectProductConfigAnswersQuery(projectProductConfigId, answer.Id);
                                    var projectproductConfigAnswers = service.RetrieveMultiple(projectproductConfigAnswerQuery);
                                    tracingService.Trace($"Product Config Questions Found: {productConfigAnswers.Entities.Count}");

                                    if (projectproductConfigAnswers.Entities.Count == 0)
                                    {
                                        CreateProjectProductConfigQuestionsAnswer(service, tracingService, questionRef.Id, projectProductConfigId, answer.Id);
                                    }
                                }
                            }
                        }
                    }
                    tracingService.Trace("Plugin Execution Completed Successfully.");
                }
            }
        }

        private static void DeleteExistingProjectProductConfigQuestions(IOrganizationService service, ITracingService tracingService, Guid projectId)
        {
            // Retrieve existing Project Product Config records
            var existingConfigsQuery = new QueryExpression(KTR_ProjectProductConfig.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ProjectProductConfig.Fields.Id, KTR_ProjectProductConfig.Fields.StatusCode),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression(KTR_ProjectProductConfig.Fields.KTR_KT_Project , ConditionOperator.Equal, projectId),
                            new ConditionExpression(KTR_ProjectProductConfig.Fields.StateCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfig_StateCode.Active) // Only active ones
                        }
                }
            };

            var existingConfigs = service.RetrieveMultiple(existingConfigsQuery);

            if (existingConfigs.Entities != null && existingConfigs.Entities.Count > 0)
            {
                // Deactivate existing records
                foreach (var config in existingConfigs.Entities)
                {
                    service.Delete(KTR_ProjectProductConfig.EntityLogicalName, config.Id);
                    tracingService.Trace($"Deleting Config ID: {config.Id}");
                }
            }
        }

        /// <summary>
        /// Create Project Product Config Question Answer
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tracingService"></param>
        /// <param name="questionId"></param>
        /// <param name="projectProductConfigId"></param>
        /// <param name="answerId"></param>
        private static void CreateProjectProductConfigQuestionsAnswer(IOrganizationService service, ITracingService tracingService, Guid questionId, Guid projectProductConfigId, Guid answerId)
        {
            // If Answer does not exist, create new project product config question answer
            Entity newProjectProductConfigQuestionAnswer = new Entity(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName);
            newProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationQuestion] = new EntityReference(KTR_ConfigurationQuestion.EntityLogicalName, questionId);
            newProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected] = false;
            newProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion] = new EntityReference(KTR_ProjectProductConfig.EntityLogicalName, projectProductConfigId);
            newProjectProductConfigQuestionAnswer[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer] = new EntityReference(KTR_ConfigurationAnswer.EntityLogicalName, answerId);
            service.Create(newProjectProductConfigQuestionAnswer);
            tracingService.Trace($"Successfully created Project Product Config question Record.");
        }

        /// <summary>
        /// Get Project Product Config Question Answer Query
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectProductConfigId"></param>
        /// <param name="answerId"></param>
        /// <returns></returns>
        private QueryExpression GetProjectProductConfigAnswersQuery(Guid projectProductConfigId, Guid answerId)
        {
            // conditional check before we create final records
            QueryExpression projectproductConfigAnswerQuery = new QueryExpression(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestionAnswerId),
                Criteria = new FilterExpression
                {
                    Conditions =
                          {
                               new ConditionExpression(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion, ConditionOperator.Equal, projectProductConfigId),
                               new ConditionExpression(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer, ConditionOperator.Equal, answerId),
                               new ConditionExpression(KTR_ProjectProductConfigQuestionAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfigQuestionAnswer_StatusCode.Active) // Only active records
                          }
                }
            };

            return projectproductConfigAnswerQuery;
        }

        /// <summary>
        /// Get Product Config Question Query
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectProductConfigId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        private QueryExpression GetProductConfigQuestionQuery(Guid productId)
        {
            var productConfigQuestionQuery = new QueryExpression(KTR_ProductConfigQuestion.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion, KTR_ProductConfigQuestion.Fields.KTR_DisplayOrder),
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression(KTR_ProductConfigQuestion.Fields.KTR_Product, ConditionOperator.Equal, productId),
                                new ConditionExpression(KTR_ProductConfigQuestion.Fields.StateCode, ConditionOperator.Equal, (int)KTR_ProductConfigQuestion_StateCode.Active) // Only active records
                            }
                }
            };
            return productConfigQuestionQuery;
        }

        /// <summary>
        /// Get Product Config Answer Query
        /// </summary>
        /// <param name="service"></param>
        /// <param name="tracingService"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        private QueryExpression GetConfigurationAnswerQuery(Guid questionId)
        {
            QueryExpression productConfigAnswerQuery = new QueryExpression(KTR_ConfigurationAnswer.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ConfigurationAnswer.Fields.KTR_ConfigurationQuestion, KTR_ConfigurationAnswer.Fields.KTR_ConfigurationAnswerId),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression(KTR_ConfigurationAnswer.Fields.KTR_ConfigurationQuestion, ConditionOperator.Equal, questionId),
                            new ConditionExpression(KTR_ConfigurationAnswer.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ConfigurationAnswer_StatusCode.Active) // Only active records
                        }
                }
            };
            return productConfigAnswerQuery;
        }
    }
}
