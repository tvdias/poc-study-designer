using System;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLineSnapshot
{
    public class StudyQuestionnaireLineSnapshotPreValidation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLineSnapshotPreValidation";
        public StudyQuestionnaireLineSnapshotPreValidation()
            : base(typeof(StudyQuestionnaireLineSnapshotPreValidation))
        {
        }

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

            // Get the study reference from the snapshot entity
            EntityReference studyRef = null;
            if (target.Contains(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study))
            {
                studyRef = target[KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study] as EntityReference;
            }
            else if (context.MessageName == nameof(ContextMessageEnum.Update) && context.PreEntityImages.Contains("PreImage"))
            {
                var preImage = context.PreEntityImages["PreImage"];
                if (preImage.Contains(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study))
                {
                    studyRef = preImage[KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study] as EntityReference;
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
