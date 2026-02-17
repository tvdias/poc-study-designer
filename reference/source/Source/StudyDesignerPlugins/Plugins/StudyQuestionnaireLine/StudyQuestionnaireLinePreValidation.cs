namespace Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLine
{
    using System;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Pre-validation plugin for Study Questionnaire Line entity that validates if the associated 
    /// study is in draft status before allowing create or update operations.
    /// </summary>
    public class StudyQuestionnaireLinePreValidation : PluginBase
    {
        public static readonly string CreateStepID = "b98594ed-9d68-f011-bec2-6045bd9637eb";

        public StudyQuestionnaireLinePreValidation()
            : base(typeof(StudyQuestionnaireLinePreValidation))
        {
        }

        /// <summary>
        /// Executes the plugin logic to validate that the associated study is in draft status before allowing create or update operations on the Study Questionnaire Line.
        /// </summary>
        /// <param name="localPluginContext">The local plugin context containing execution context and services.</param>
        /// <exception cref="ArgumentNullException">Thrown when localPluginContext is null.</exception>
        /// <exception cref="InvalidPluginExecutionException">Thrown when the study is not in draft status.</exception>
        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            var tracingService = localPluginContext.TracingService;

            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.CurrentUserService;

            if (context.MessageName != nameof(ContextMessageEnum.Create) && context.MessageName != nameof(ContextMessageEnum.Update))
            {
                return;
            }

            Entity target = context.InputParameters["Target"] as Entity;
            if (target == null)
            {
                return;
            }

            // Get the study reference from the StudyQuestionnaireLine entity
            EntityReference studyRef = null;
            if (target.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_Study))
            {
                studyRef = target[KTR_StudyQuestionnaireLine.Fields.KTR_Study] as EntityReference;
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Update) && context.PreEntityImages.Contains("PreImage"))
            {
                var preImage = context.PreEntityImages["PreImage"];
                if (preImage.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_Study))
                {
                    studyRef = preImage[KTR_StudyQuestionnaireLine.Fields.KTR_Study] as EntityReference;
                }
            }

            if (studyRef == null)
            {
                return;
            }

            // Retrieve the study to check its statuscode
            var study = service.Retrieve(KT_Study.EntityLogicalName, studyRef.Id, new ColumnSet(KT_Study.Fields.StatusCode));
            if (study == null || !study.Attributes.Contains(KT_Study.Fields.StatusCode))
            {
                return;
            }

            var statusCode = ((OptionSetValue)study[KT_Study.Fields.StatusCode]).Value;
            if (statusCode != (int)KT_Study_StatusCode.Draft) // Use enum for Draft status
            {
                throw new InvalidPluginExecutionException("Cannot create or update the record since the study is not in draft status.");
            }
        }
    }
}
