namespace Kantar.StudyDesignerLite.Plugins.StudyQuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.StudyManagedlistEntity;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class StudyQuestionnaireLineQLMLESyncPostOperation : PluginBase
    {
        private static readonly string s_pluginName = typeof(StudyQuestionnaireLineQLMLESyncPostOperation).FullName;

        public StudyQuestionnaireLineQLMLESyncPostOperation()
            : base(typeof(StudyQuestionnaireLineQLMLESyncPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            tracing.Trace($"{s_pluginName} started. Message: {context.MessageName}");

            if (context.Depth > 1)
            {
                tracing.Trace("Depth > 1 – exiting.");
                return;
            }

            if (context.MessageName != "Update")
            {
                tracing.Trace("Not Update – exiting.");
                return;
            }

            if (!context.PreEntityImages.Contains("PreImage"))
            {
                tracing.Trace("PreImage missing – exiting.");
                return;
            }

            var preImage = context.PreEntityImages["PreImage"];

            if (preImage.LogicalName != KTR_StudyQuestionnaireLine.EntityLogicalName)
            {
                tracing.Trace("Wrong entity – exiting.");
                return;
            }

            var preState =
                preImage.GetAttributeValue<OptionSetValue>(
                    KTR_StudyQuestionnaireLine.Fields.StateCode)?.Value;

            var postState =
                context.InputParameters.Contains("Target")
                    ? ((Entity)context.InputParameters["Target"])
                        .GetAttributeValue<OptionSetValue>(
                            KTR_StudyQuestionnaireLine.Fields.StateCode)?.Value
                    : null;

            bool isDeactivate =
                preState == (int)KTR_StudyQuestionnaireLine_StateCode.Active &&
                postState == (int)KTR_StudyQuestionnaireLine_StateCode.Inactive;

            if (!isDeactivate)
            {
                tracing.Trace("Not deactivate – exiting.");
                return;
            }

            tracing.Trace($"Processing deactivation for S-QL {preImage.Id}");

            var qlmleRepo = new QuestionnaireLineManagedListEntityRepository(service, tracing);
            var studyMleRepo = new StudyManagedlistEntityRepository(service);

            // 1️ Deactivate QLMLEs + resolve Study
            var result = DeactivateQLMLEs(
                preImage.Id,
                service,
                qlmleRepo,
                tracing);

            if (result == null)
            {
                tracing.Trace("No result returned – exiting.");
                return;
            }

            // 2️ Deactivate Study MLEs (THIS STUDY ONLY - if no other QLMLE)
            DeactivateStudyManagedListEntitiesIfNeeded(
                result.StudyId,
                qlmleRepo,
                studyMleRepo,
                tracing);

            // 3️ Recalculate subsets (THIS STUDY ONLY)
            RecalculateSubsets(service, tracing, result.StudyId);
        }

        private QlmleDeactivationResult DeactivateQLMLEs(
            Guid studyQuestionnaireLineId,
            IOrganizationService service,
            QuestionnaireLineManagedListEntityRepository qlmleRepo,
            ITracingService tracing)
        {
            var sQl = service.Retrieve(
                KTR_StudyQuestionnaireLine.EntityLogicalName,
                studyQuestionnaireLineId,
                new ColumnSet(
                    KTR_StudyQuestionnaireLine.Fields.KTR_Study,
                    KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine));

            var studyRef =
                sQl.GetAttributeValue<EntityReference>(
                    KTR_StudyQuestionnaireLine.Fields.KTR_Study);

            var questionnaireLineRef =
                sQl.GetAttributeValue<EntityReference>(
                    KTR_StudyQuestionnaireLine.Fields.KTR_QuestionnaireLine);

            if (studyRef == null || questionnaireLineRef == null)
            {
                tracing.Trace("Study or QuestionnaireLine missing.");
                return null;
            }

            var qlmles =
                qlmleRepo.GetActiveByStudyAndQuestionnaireLine(
                    studyRef.Id,
                    questionnaireLineRef.Id);

            if (!qlmles.Any())
            {
                tracing.Trace("No active QLMLEs found.");
                return new QlmleDeactivationResult
                {
                    StudyId = studyRef.Id
                };
            }

            qlmleRepo.BulkUpdateStatus(
                qlmles,
                KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive);

            tracing.Trace($"Deactivated {qlmles.Count} QLMLEs.");

            return new QlmleDeactivationResult
            {
                StudyId = studyRef.Id
            };
        }

        private void DeactivateStudyManagedListEntitiesIfNeeded(
            Guid studyId,
            QuestionnaireLineManagedListEntityRepository qlmleRepo,
            StudyManagedlistEntityRepository studyMleRepo,
            ITracingService tracing)
        {
            var studyMles = studyMleRepo.GetByStudyId(studyId);

            var toDeactivate = new List<KTR_StudyManagedListEntity>();

            foreach (var studyMle in studyMles)
            {
                bool hasActiveQlmle =
                    qlmleRepo.HasActiveQLMLEs(
                        studyId,
                        studyMle.KTR_ManagedListEntity.Id);

                if (!hasActiveQlmle)
                {
                    toDeactivate.Add(studyMle);
                    tracing.Trace($"StudyMLE {studyMle.Id} queued.");
                }
            }

            if (toDeactivate.Any())
            {
                studyMleRepo.BulkUpdateStatus(
                    toDeactivate,
                    KTR_StudyManagedListEntity_StateCode.Inactive,
                    KTR_StudyManagedListEntity_StatusCode.Inactive);

                tracing.Trace($"Deactivated {toDeactivate.Count} StudyMLEs.");
            }
        }

        private void RecalculateSubsets(
            IOrganizationService service,
            ITracingService tracing,
            Guid studyId)
        {
            try
            {
                var subsetSvc = new SubsetDefinitionService(
                    tracing,
                    new SubsetRepository(service),
                    new QuestionnaireLineManagedListEntityRepository(service, tracing),
                    new StudyRepository(service),
                    new ManagedListEntityRepository(service));

                subsetSvc.ProcessSubsetLogic(studyId);
                tracing.Trace($"Subset recalculation completed for Study {studyId}");
            }
            catch (Exception ex)
            {
                tracing.Trace($"Subset recalculation failed: {ex}");
            }
        }

        private class QlmleDeactivationResult
        {
            public Guid StudyId { get; set; }
        }
    }
}
