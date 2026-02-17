using Kantar.StudyDesignerLite.Plugins.Project;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.ProjectProductConfigQuestionAnswer
{
    public class ProjectProductConfigQuestionAnswerPostOperation : PluginBase
    {
        public ProjectProductConfigQuestionAnswerPostOperation()
           : base(typeof(ProjectProductConfigQuestionAnswerPostOperation))
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
                ActivateDeactivateTargetEntityOnDisplayRule(orgService, context);
            }
        }

        /// <summary>
        /// Main function Of Activation and Deactivation on Display Rule
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="service"></param>
        /// <param name="tracingService"></param>
        /// <param name="context"></param>
        public void ActivateDeactivateTargetEntityOnDisplayRule(IOrganizationService service, IPluginExecutionContext context)
        {
            // Ensure Target entity exists
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity && context.Depth == 1)  //We don't want to keep looping 
            {
                var preEntity = GetEntityImage<KTR_ProjectProductConfigQuestionAnswer>(context.PreEntityImages, "Image");
                var postEntity = GetEntityImage<KTR_ProjectProductConfigQuestionAnswer>(context.PostEntityImages, "Image");

                // Get product and question references
                EntityReference postProjectProductConfigQuestion = postEntity?.KTR_ProjectProductConfigQuestion;

                //If Statement  Check if preentity and postentity is selected changed

                if ((!preEntity.KTR_IsSelected.HasValue && postEntity.KTR_IsSelected.HasValue) || (preEntity.KTR_IsSelected.HasValue && postEntity.KTR_IsSelected.HasValue && preEntity.KTR_IsSelected.Value != postEntity.KTR_IsSelected.Value)
                    && postEntity.KTR_ProjectProductConfigQuestion.Id != Guid.Empty)
                {
                    //Get Product and projectID from Project record
                    // Set Condition Values
                    var projectProductConfigQuestionId = postEntity.KTR_ProjectProductConfigQuestion.Id; //     

                    var projectquery = GetProjectQuery(projectProductConfigQuestionId);
                    var projectresults = service.RetrieveMultiple(projectquery);

                    if (projectresults.Entities.Count != 0)
                    {
                        KT_Project project = (KT_Project)projectresults.Entities[0];
                        var query_ktr_productconfigquestion = Guid.Empty;
                        //Lookup the records I need to either hide or unhide if the is selected value changed
                        //Get and Loop through display rules for question changed

                        // Set Condition Values

                        var query_ktr_ruleconfiganswer = postEntity.KTR_ConfigurationAnswer.Id;   // "6ea48605-1e19-f011-998a-7c1e5275a51f";   //Post Entity  config answer
                        var query_ktr_ruleconfigquestion = postEntity.KTR_ConfigurationQuestion.Id;

                        
                        // Following Query expression will give us ProductConfig question Id
                        var productConfigquestionQuery = GetProductConfigQuestionQuery(postEntity.KTR_ConfigurationQuestion.Id, project.KTR_Product.Id);

                        var productConfigquestion = service.RetrieveMultiple(productConfigquestionQuery);
                        

                        if (productConfigquestion != null && productConfigquestion.Entities.Count > 0)
                        {
                            query_ktr_productconfigquestion = new Guid(productConfigquestion.Entities[0][KTR_ProductConfigQuestion.Fields.KTR_ProductConfigQuestionId].ToString());
                        }

                        // Instantiate QueryExpression query
                        var displayRulequery = GetProductConfigQuestionDisplayRuleQuery(query_ktr_ruleconfiganswer, query_ktr_ruleconfigquestion, query_ktr_productconfigquestion);
                        var displayRuleResults = service.RetrieveMultiple(displayRulequery);

                        //Call Apply display logic rule based on above collection
                        ApplyDisplayRules(service, displayRuleResults, project.Id, (bool)postEntity.KTR_IsSelected);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="displayRuleResults"></param>
        /// <param name="projectId"></param>
        /// <param name="isSelected"></param>
        private void ApplyDisplayRules(IOrganizationService service, EntityCollection displayRuleResults, Guid projectId, bool isSelected)
        {
            if (displayRuleResults.Entities.Count != 0)
            {
                //For each display rule, check the type is config question or config answer, find and apply logic for each type
                foreach (KTR_ProductConfigQuestionDisplayRule rule in displayRuleResults.Entities)
                {
                    var query_ktr_kt_project = projectId;
                    Guid query_ktr_configurationquestion;

                    if (rule.KTR_ImpactedConfigQuestion.Id != Guid.Empty)
                    {
                        query_ktr_configurationquestion = rule.KTR_ImpactedConfigQuestion.Id;
                    }
                    else
                    {
                        //Throw Error, missing impacted config question 
                        return;
                    }

                    if (rule.KTR_Type == KTR_DisplayRuleType.ConfigurationQuestion)
                    {
                        // Set Condition Values
                        // Instantiate QueryExpression query
                        var projectProductConfigquery = GetProjectProductConfigQuery( query_ktr_kt_project, query_ktr_configurationquestion);

                        var impactedquestions = service.RetrieveMultiple(projectProductConfigquery);

                        if (impactedquestions.Entities.Count != 0)
                        {
                            //apply hide/display logic based on rule and is selected value   When Is Selected = false, reverse the rule
                            foreach (KTR_ProjectProductConfig question in impactedquestions.Entities)
                            {
                                switch (isSelected)
                                {

                                    case true:

                                        if (rule.KTR_DisplaySettings == KTR_DisplayRuleSetting.Hide)
                                        {
                                            question.StateCode = KTR_ProjectProductConfig_StateCode.Inactive;
                                            UnselectRelatedAnswers(question.Id, service);//Unselect the associated answers
                                        }
                                        else
                                        {
                                            question.StateCode = KTR_ProjectProductConfig_StateCode.Active;
                                        }
                                        break;

                                    case false:
                                        question.StateCode = KTR_ProjectProductConfig_StateCode.Active;
                                        break;
                                }
                                service.Update(question);
                            }
                        }
                    }

                    // Conditional block for Configuration Answer
                    if (rule.KTR_Type == KTR_DisplayRuleType.ConfigurationAnswer && rule.KTR_ImpactedConfigAnswer.Id != Guid.Empty)
                    {
                        // Set Condition Values
                        var query_ktr_configurationanswer = rule.KTR_ImpactedConfigAnswer.Id;

                        var projectProductConfigAnswerquery = GetProjectProductConfigAnswerQuery(query_ktr_configurationanswer);

                        var impactedquestions = service.RetrieveMultiple(projectProductConfigAnswerquery);

                        if (impactedquestions.Entities.Count != 0)
                        {
                            foreach (KTR_ProjectProductConfigQuestionAnswer answer in impactedquestions.Entities)
                            {
                                switch (isSelected)
                                {
                                    case true:
                                        if (rule.KTR_DisplaySettings == KTR_DisplayRuleSetting.Hide)
                                        {
                                            answer.KTR_IsSelected = false;   //Uncheck the item if it is going to inactive
                                            answer.StateCode = KTR_ProjectProductConfigQuestionAnswer_StateCode.Inactive;
                                        }
                                        else
                                        {
                                            answer.StateCode = KTR_ProjectProductConfigQuestionAnswer_StateCode.Active;
                                        }
                                        break;

                                    case false:

                                        answer.StateCode = KTR_ProjectProductConfigQuestionAnswer_StateCode.Active;
                                        break;
                                }
                                service.Update(answer);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// To mark isSelect as False for the associated Answers  
        /// </summary>
        /// <param name="service"></param>
        /// <param name="questionId"></param>
        /// 
        private void UnselectRelatedAnswers(Guid questionId, IOrganizationService service)
        {
            // Step 1: Retrieve all answers related to the given question
            var query = new QueryExpression(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_ProjectProductConfigQuestionAnswer.Fields.Id, KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected),
                Criteria =
                 {
                     Conditions =
                     {
                         new ConditionExpression(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion, ConditionOperator.Equal, questionId)
                     }
                 }
            };

            var answerRecords = service.RetrieveMultiple(query);

            if (answerRecords.Entities.Count == 0)
            {
               return;
            }

            foreach (var answer in answerRecords.Entities)
            {
                if (answer.GetAttributeValue<bool>(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected))
                {
                    answer[KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_IsSelected] = false;
                    service.Update(answer);
                }                
          
            }

        }

        

        /// <summary>
        /// Query expression will give us ProductConfig question Id
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configurationQuestionId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        private QueryExpression GetProductConfigQuestionQuery(Guid configurationQuestionId, Guid productId)
        {
            var productConfigquestionQuery = new QueryExpression(KTR_ProductConfigQuestion.EntityLogicalName);
            productConfigquestionQuery.Distinct = true;
            productConfigquestionQuery.TopCount = 1;

            // Add columns to query.ColumnSet
            productConfigquestionQuery.ColumnSet.AddColumns(KTR_ProductConfigQuestion.Fields.KTR_ProductConfigQuestionId);

            // Add conditions to query.Criteria
            productConfigquestionQuery.Criteria.AddCondition(KTR_ProductConfigQuestion.Fields.KTR_ConfigurationQuestion, ConditionOperator.Equal, configurationQuestionId);
            productConfigquestionQuery.Criteria.AddCondition(KTR_ProductConfigQuestion.Fields.KTR_Product, ConditionOperator.Equal, productId);
            return productConfigquestionQuery;
        }

        /// <summary>
        /// Query expression will give us ProductConfig question Id
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query_ktr_ruleconfiganswer"></param>
        /// <param name="query_ktr_ruleconfigquestion"></param>
        /// <param name="query_ktr_productconfigquestion"></param>
        /// <returns></returns>
        private QueryExpression GetProductConfigQuestionDisplayRuleQuery(Guid query_ktr_ruleconfiganswer, Guid query_ktr_ruleconfigquestion, Guid query_ktr_productconfigquestion)
        {
            // Instantiate QueryExpression query
            var displayRulequery = new QueryExpression(KTR_ProductConfigQuestionDisplayRule.EntityLogicalName);
            displayRulequery.Distinct = true;
            // Add all columns to query.ColumnSet
            displayRulequery.ColumnSet.AllColumns = true;
            // Add conditions to query.Criteria
            displayRulequery.Criteria.AddCondition(KTR_ProductConfigQuestionDisplayRule.Fields.KTR_RuleConfigAnswer, ConditionOperator.Equal, query_ktr_ruleconfiganswer);
            displayRulequery.Criteria.AddCondition(KTR_ProductConfigQuestionDisplayRule.Fields.KTR_RuleConfigQuestion, ConditionOperator.Equal, query_ktr_ruleconfigquestion);
            displayRulequery.Criteria.AddCondition(KTR_ProductConfigQuestionDisplayRule.Fields.KTR_ProductConfigQuestion, ConditionOperator.Equal, query_ktr_productconfigquestion);

            return displayRulequery;
        }

        /// <summary>
        /// Query expression will give us ProductConfig question Id
        /// </summary>
        /// <param name="service"></param>
        /// <param name="query_ktr_kt_project"></param>
        /// <param name="query_ktr_configurationquestion"></param>
        /// <returns></returns>
        private QueryExpression GetProjectProductConfigQuery( Guid query_ktr_kt_project, Guid query_ktr_configurationquestion)
        {
            var projectProductConfigQuery = new QueryExpression(KTR_ProjectProductConfig.EntityLogicalName);
            projectProductConfigQuery.Distinct = true;

            // Add columns to query.ColumnSet
            projectProductConfigQuery.ColumnSet.AddColumn(KTR_ProjectProductConfig.Fields.KTR_ProjectProductConfigId);

            // Add conditions to query.Criteria
            projectProductConfigQuery.Criteria.AddCondition("ktr_kt_project", ConditionOperator.Equal, query_ktr_kt_project);
            projectProductConfigQuery.Criteria.AddCondition("ktr_configurationquestion", ConditionOperator.Equal, query_ktr_configurationquestion);
            return projectProductConfigQuery;
        }

        /// <summary>
        /// Query expression will give us ProductConfig question Id
        /// </summary>
        /// <param name="service"></param>
        /// <param name="KTR_ProjectProductConfigQuestionAnswerId"></param>
        /// <param name="query_ktr_kt_project"></param>
        /// <param name="query_ktr_configurationquestion"></param>
        /// <returns></returns>
        private QueryExpression GetProjectProductConfigAnswerQuery(Guid query_ktr_configurationanswer)
        {
            var projectProductConfigQuestionAnswerQuery = new QueryExpression(KTR_ProjectProductConfigQuestionAnswer.EntityLogicalName);
            projectProductConfigQuestionAnswerQuery.Distinct = true;

            // Add columns to query.ColumnSet
            projectProductConfigQuestionAnswerQuery.ColumnSet.AddColumn(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestionAnswerId);

            // Add conditions to query.Criteria
            projectProductConfigQuestionAnswerQuery.Criteria.AddCondition(KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ConfigurationAnswer, ConditionOperator.Equal, query_ktr_configurationanswer);

            // Add link-entity query_ktr_projectproductconfig
            var impactedquestion_query_ktr_projectproductconfig = projectProductConfigQuestionAnswerQuery.AddLink(KTR_ProjectProductConfig.EntityLogicalName, KTR_ProjectProductConfigQuestionAnswer.Fields.KTR_ProjectProductConfigQuestion, KTR_ProjectProductConfig.Fields.KTR_ProjectProductConfigId);
            
            return projectProductConfigQuestionAnswerQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="projectProductConfigQuestionId"></param>
        /// <returns></returns>
        private QueryExpression GetProjectQuery(Guid projectProductConfigQuestionId)
        {
            // Instantiate QueryExpression query
            var projectquery = new QueryExpression(KT_Project.EntityLogicalName);
            projectquery.Distinct = true;
            projectquery.TopCount = 1;

            // Add columns to query.ColumnSet
            projectquery.ColumnSet.AddColumns(KT_Project.Fields.KTR_Product, KT_Project.Fields.KT_ProjectId);

            // Add conditions to query.Criteria
            projectquery.Criteria.AddCondition(KT_Project.Fields.KTR_Product, ConditionOperator.NotNull);

            // Add link-entity query_ktr_projectproductconfig
            var query_ktr_projectproductconfig = projectquery.AddLink(KTR_ProjectProductConfig.EntityLogicalName, KT_Project.Fields.KT_ProjectId, KTR_ProjectProductConfig.Fields.KTR_KT_Project);
            // Add conditions to query_ktr_projectproductconfig.LinkCriteria
            query_ktr_projectproductconfig.LinkCriteria.AddCondition(KTR_ProjectProductConfig.Fields.KTR_ProjectProductConfigId, ConditionOperator.Equal, projectProductConfigQuestionId);
            query_ktr_projectproductconfig.Columns.AddColumns(KTR_ProjectProductConfig.Fields.KTR_Name);

            return projectquery;
        }

        private static T GetEntityImage<T>(EntityImageCollection images, string key) where T : Entity
        {
            return images.TryGetValue(key, out var entity) ? entity.ToEntity<T>() : null;
        }

    }
}
