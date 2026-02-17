using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.ProductConfigQuestion
{
    //Plugin to update configuration questions related to project, if new configuration question is added to its related product.
    public class AddNewProductConfigQuestiontoProjectPostOperation : PluginBase
    {
        public AddNewProductConfigQuestiontoProjectPostOperation()
            : base(typeof(AddNewProductConfigQuestiontoProjectPostOperation))
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

            tracingService.Trace("Post operation plugin execution started.");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
            {
                CopyQuestions(targetEntity, orgService, tracingService, context);
            }
        }

        public void CopyQuestions(Entity targetEntity, IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context)
        {

            // Ensure Target entity exists
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
            {
                if (!entity.Contains(KTR_ProductConfigQuestion.Fields.KTR_Product) || !entity.Contains(KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion))
                {
                    tracingService.Trace("Missing required fields, exiting.");
                    return;
                }

                // Get product and question references
                Guid productId = ((EntityReference)entity[KTR_ProductConfigQuestion.Fields.KTR_Product]).Id;
                Guid questionId = ((EntityReference)entity[KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion]).Id;

                tracingService.Trace($"Product ID: {productId}, New Question ID: {questionId}");

                // Find all projects linked to this product
                QueryExpression projectQuery = new QueryExpression(KT_Project.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(KT_Project.Fields.KT_ProjectId),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(KT_Project.Fields.KTR_Product, ConditionOperator.Equal, productId),
                            new ConditionExpression(KT_Project.Fields.StateCode, ConditionOperator.Equal, 0) // Active projects
                        }
                    }
                };

                EntityCollection projects = service.RetrieveMultiple(projectQuery);
                tracingService.Trace($"Projects Found using this product: {projects.Entities.Count}");

                foreach (var project in projects.Entities)
                {
                    Guid projectId = project.Id;

                    // Check if the question already exists in Project Product Config
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

                    if (existingConfigs.Entities.Count == 0)
                    {
                        // If question does not exist, create new project product config
                        Entity newProjectConfig = new Entity(KTR_ProjectProductConfig.EntityLogicalName);
                        newProjectConfig[KTR_ProjectProductConfig.Fields.KTR_KT_Project] = new EntityReference("kt_project", projectId);
                        newProjectConfig[KTR_ProjectProductConfig.Fields.KTR_ConfigurationQuestion] = new EntityReference("ktr_configurationquestion", questionId);

                        service.Create(newProjectConfig);
                        tracingService.Trace($"Added new question {questionId} to Project {projectId}");
                    }
                    else
                    {
                        tracingService.Trace($"Question {questionId} already exists for Project {projectId}, skipping.");
                    }
                }
            }

            tracingService.Trace("Plugin Execution Completed Successfully.");
        }
    }
}
