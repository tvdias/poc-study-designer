namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    using System;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pre-validation plugin for Questionnaire Line entity that ensures question variable names are unique within a project.
    /// </summary>
    public class QuestionnaireLinePreValidation : PluginBase
    {
        public static readonly string UpdateStepID = "1f9240be-580b-f011-bae2-6045bd9d7ae1";

        public QuestionnaireLinePreValidation()
            : base(typeof(QuestionnaireLinePreValidation))
        {
        }

        /// <summary>
        /// Executes the plugin logic to validate that the question variable name is unique within the project.
        /// Validates on both Create and Update messages.
        /// </summary>
        /// <param name="localContext">The local plugin context containing execution details.</param>
        /// <exception cref="InvalidPluginExecutionException">Thrown when localContext is null or when a duplicate question variable name is found.</exception>
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }
            ITracingService tracingService = localContext.TracingService;

            string errorMessage = string.Empty;

            IOrganizationService currentUserService = localContext.CurrentUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            Entity entity = (Entity)context.InputParameters["Target"];

            tracingService.Trace(entity.LogicalName);

            if (entity.LogicalName == KT_QuestionnaireLines.EntityLogicalName)
            {
                var line = (context.InputParameters["Target"] as Entity).ToEntity<KT_QuestionnaireLines>();

                var preline = new KT_QuestionnaireLines();

                if (context.MessageName == nameof(ContextMessageEnum.Update))
                {
                    preline = context.PreEntityImages["Image"].ToEntity<KT_QuestionnaireLines>();
                }

                if ((context.MessageName == nameof(ContextMessageEnum.Create)
                        && line.Contains(KT_QuestionnaireLines.Fields.KTR_Project)
                        && line.KTR_Project.Id != Guid.Empty
                        && line.Contains(KT_QuestionnaireLines.Fields.KT_QuestionVariableName)
                        && line.KT_QuestionVariableName != null) ||
                    (context.MessageName == nameof(ContextMessageEnum.Update)
                        && preline.Contains(KT_QuestionnaireLines.Fields.KTR_Project)
                        && preline.KTR_Project.Id != Guid.Empty
                        && preline.Contains(KT_QuestionnaireLines.Fields.KT_QuestionVariableName)
                        && line.Contains(KT_QuestionnaireLines.Fields.KT_QuestionVariableName)
                        && preline.KT_QuestionVariableName != line.KT_QuestionVariableName))
                {

                    var query_kt_questionvariablename = line.KT_QuestionVariableName;

                    var query = new QueryExpression(KT_QuestionnaireLines.EntityLogicalName);
                    query.TopCount = 1;

                    query.ColumnSet.AddColumn(KT_QuestionnaireLines.Fields.KT_QuestionnaireLinesId);

                    query.Criteria.AddCondition(KT_QuestionnaireLines.Fields.KT_QuestionVariableName, ConditionOperator.Equal, query_kt_questionvariablename);

                    switch (context.MessageName)
                    {
                        case nameof(ContextMessageEnum.Create):
                            query.Criteria.AddCondition(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, line.KTR_Project.Id);
                            break;
                        case nameof(ContextMessageEnum.Update):
                            query.Criteria.AddCondition(KT_QuestionnaireLines.Fields.KT_QuestionnaireLinesId, ConditionOperator.NotEqual, preline.Id);
                            query.Criteria.AddCondition(KT_QuestionnaireLines.Fields.KTR_Project, ConditionOperator.Equal, preline.KTR_Project.Id);
                            break;
                    }

                    query.Criteria.AddCondition(KT_QuestionnaireLines.Fields.StatusCode, ConditionOperator.Equal, (int)KT_QuestionnaireLines_StatusCode.Active);

                    var results = currentUserService.RetrieveMultiple(query);

                    if (results.Entities.Count > 0)
                    {
                        errorMessage = $"A question already exists on the project with the same variable name: {line.KT_QuestionVariableName}";
                        tracingService.Trace($"Error Message String: {errorMessage}");
                        throw new InvalidPluginExecutionException(errorMessage);
                    }
                }
            }
        }
    }
}
