using System;
using System.Collections.Generic;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.StudyManagedlistEntity;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.QuestionnaireLine
{
    public class QuestionnaireLineQLMLESyncPreValidation : PluginBase
    {
        private const string PluginName =
            "Kantar.StudyDesignerLite.Plugins.QuestionnaireLine.QuestionnaireLineQLMLESyncPreValidation";

        public QuestionnaireLineQLMLESyncPreValidation()
            : base(typeof(QuestionnaireLineQLMLESyncPreValidation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            tracing.Trace($"{PluginName} started. Message: {context.MessageName}");

            // Prevent recursion
            if (context.Depth > 1)
            {
                tracing.Trace("Depth > 1, exiting to avoid recursion.");
                return;
            }

            // Only handle Delete
            if (context.MessageName != "Delete")
            {
                tracing.Trace("Not Delete – exiting.");
                return;
            }

            // PreImage is REQUIRED for Delete
            if (!context.PreEntityImages.Contains("PreImage"))
            {
                tracing.Trace("PreImage missing – exiting.");
                return;
            }

            var preImage = context.PreEntityImages["PreImage"];

            if (preImage.LogicalName != KT_QuestionnaireLines.EntityLogicalName)
            {
                tracing.Trace("PreImage is not QuestionnaireLine – exiting.");
                return;
            }

            var qlId = preImage.Id;
            tracing.Trace($"Processing DELETE for QuestionnaireLine {qlId}");

            var qlmleRepo = new QuestionnaireLineManagedListEntityRepository(service, tracing);
            var studyMleRepo = new StudyManagedlistEntityRepository(service);

            // 1. Fetch and deactivate QLMLEs using FetchXML (works in pre-delete)
            var deactivatedQLMLEs = FetchAndDeactivateQLMLEs(qlId, qlmleRepo, tracing);

            if (!deactivatedQLMLEs.Any())
            {
                tracing.Trace("No QLMLEs were deactivated – exiting.");
                return;
            }

            // 2. Collect affected studies
            var affectedStudyIds = deactivatedQLMLEs
                .Select(x => x.KTR_StudyId?.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // 3. Recalculate subsets
            RecalculateSubsets(service, tracing, affectedStudyIds);

            // 4. Deactivate Study MLEs if needed
            DeactivateStudyManagedListEntitiesIfNeeded(deactivatedQLMLEs, qlmleRepo, studyMleRepo, tracing);
        }

        private List<KTR_QuestionnaireLinemanAgedListEntity> FetchAndDeactivateQLMLEs(
            Guid questionnaireLineId,
            QuestionnaireLineManagedListEntityRepository qlmleRepo,
            ITracingService tracing)
        {
            tracing.Trace($"Fetching QLMLEs for QuestionnaireLine {questionnaireLineId} using pre-delete FetchXML");

            // Use FetchXML to ensure the link to QuestionnaireLine works pre-delete
            var qlmles = qlmleRepo.GetActiveQLMLEsForPreDelete(questionnaireLineId, tracing);

            if (!qlmles.Any())
            {
                tracing.Trace("No QLMLEs found.");
                return new List<KTR_QuestionnaireLinemanAgedListEntity>();
            }

            tracing.Trace($"Found {qlmles.Count} QLMLEs. Deactivating...");

            // Bulk update status
            qlmleRepo.BulkUpdateStatus(
                qlmles,
                KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive
            );

            tracing.Trace("QLMLE deactivation complete.");
            return qlmles;
        }

        private void DeactivateStudyManagedListEntitiesIfNeeded(
            List<KTR_QuestionnaireLinemanAgedListEntity> deactivatedQLMLEs,
            QuestionnaireLineManagedListEntityRepository qlmleRepo,
            StudyManagedlistEntityRepository studyMleRepo,
            ITracingService tracing)
        {
            var studyMleGroups = deactivatedQLMLEs
                .Where(x => x.KTR_StudyId != null && x.KTR_ManagedListEntity != null)
                .GroupBy(x => new
                {
                    StudyId = x.KTR_StudyId.Id,
                    MleId = x.KTR_ManagedListEntity.Id
                });

            var studyMlesToDeactivate = new List<KTR_StudyManagedListEntity>();

            foreach (var group in studyMleGroups)
            {
                if (!qlmleRepo.HasActiveQLMLEs(group.Key.StudyId, group.Key.MleId))
                {
                    var studyMle =
                        studyMleRepo.GetByStudyAndMLE(group.Key.StudyId, group.Key.MleId);

                    if (studyMle != null)
                    {
                        studyMlesToDeactivate.Add(studyMle);
                        tracing.Trace($"StudyMLE {studyMle.Id} queued for deactivation.");
                    }
                }
            }

            if (studyMlesToDeactivate.Any())
            {
                studyMleRepo.BulkUpdateStatus(
                    studyMlesToDeactivate,
                    KTR_StudyManagedListEntity_StateCode.Inactive,
                    KTR_StudyManagedListEntity_StatusCode.Inactive
                );

                tracing.Trace($"Deactivated {studyMlesToDeactivate.Count} StudyManagedListEntity records.");
            }
        }

        private void RecalculateSubsets(
            IOrganizationService service,
            ITracingService tracing,
            List<Guid> studyIds)
        {
            if (!studyIds.Any())
            {
                tracing.Trace("No studies to recalculate subsets for.");
                return;
            }

            try
            {
                var subsetSvc = new SubsetDefinitionService(
                    tracing,
                    new SubsetRepository(service),
                    new QuestionnaireLineManagedListEntityRepository(service, tracing),
                    new StudyRepository(service),
                    new ManagedListEntityRepository(service));

                foreach (var studyId in studyIds)
                {
                    subsetSvc.ProcessSubsetLogic(studyId);
                }
            }
            catch (Exception ex)
            {
                tracing.Trace($"Subset recalculation failed: {ex}");
            }
        }
    }
}
