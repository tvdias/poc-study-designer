using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Kantar.StudyDesignerLite.Plugins.ProductConfigQuestion
{
    public class DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation : PluginBase
    {
        public DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation()
            : base(typeof(DeleteConfigQuestionfromProjectifDeletedfromProductPostOperation))
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

            tracingService.Trace("Post-operation delete plugin execution started.");

            // Call DeleteQuestions without checking InputParameters, since we rely on Pre-Image
            DeleteQuestions(orgService, tracingService, context);
        }

        public void DeleteQuestions(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context)
        {
            // Ensure Pre-Image exists
            if (!context.PreEntityImages.Contains("PreImage"))
            {
                tracingService.Trace("Pre-Image is missing, exiting plugin.");
                return;
            }

            Entity preImage = context.PreEntityImages["PreImage"];
            tracingService.Trace($"Pre-Image Retrieved: {preImage.LogicalName}");

            // Ensure required attributes exist
            if (!preImage.Contains(KTR_ProductConfigQuestion.Fields.KTR_Product) || !preImage.Contains(KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion))
            {
                tracingService.Trace("Required fields (ktr_product, ktr_configurationquestion) are missing in Pre-Image, exiting.");
                return;
            }

            Guid productId = ((EntityReference)preImage[KTR_ProductConfigQuestion.Fields.KTR_Product]).Id;
            Guid questionId = ((EntityReference)preImage[KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion]).Id;

            tracingService.Trace($"Product ID: {productId}, Deleted Question ID: {questionId}");

            // Find all projects using this product
            QueryExpression projectQuery = new QueryExpression(KT_Project.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KT_Project.Fields.KT_ProjectId),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KT_Project.Fields.KTR_Product, ConditionOperator.Equal, productId),
                        new ConditionExpression(KT_Project.Fields.StateCode, ConditionOperator.Equal, 0) // Only active projects
                    }
                }
            };

            EntityCollection projects = service.RetrieveMultiple(projectQuery);
            tracingService.Trace($"Projects Found using this product: {projects.Entities.Count}");

            foreach (var project in projects.Entities)
            {
                Guid projectId = project.Id;

                // Find and delete the project product config entry for the removed question
                QueryExpression existingConfigQuery = new QueryExpression(KTR_ProjectProductConfig.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(KTR_ProjectProductConfig.Fields.KTR_ProjectProductConfigId),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(KTR_ProjectProductConfig.Fields.KTR_KT_Project, ConditionOperator.Equal, projectId),
                                new ConditionExpression(KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion, ConditionOperator.Equal, questionId),
                                new ConditionExpression(KTR_ProjectProductConfig.Fields.StateCode, ConditionOperator.Equal, (int)KTR_ProjectProductConfig_StateCode.Active) // Active records only
                        }
                    }
                };

                EntityCollection existingConfigs = service.RetrieveMultiple(existingConfigQuery);
                tracingService.Trace($"Configs to Delete: {existingConfigs.Entities.Count}");

                foreach (var config in existingConfigs.Entities)
                {
                    service.Delete(KTR_ProjectProductConfig.EntityLogicalName, config.Id);
                    tracingService.Trace($"Deleted Project Product Config ID: {config.Id}");
                }
            }

            tracingService.Trace("Plugin Execution Completed Successfully.");
        }
    }
}
