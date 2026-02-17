namespace Kantar.StudyDesignerLite.Plugins.ManagedListEntity
{
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

    public class ManagedlistEntityPostOperation : PluginBase
    {
        public ManagedlistEntityPostOperation()
            : base(typeof(ManagedlistEntityPostOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            var tracing = localContext.TracingService;
            var service = localContext.CurrentUserService;
            var context = localContext.PluginExecutionContext;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != KTR_ManagedListEntity.EntityLogicalName)
            {
                tracing.Trace("Skipping because entity is not MLE.");
                return;
            }

            var pre = context.PreEntityImages.Contains("PreImage") ? context.PreEntityImages["PreImage"] : null;
            var post = context.PostEntityImages.Contains("PostImage") ? context.PostEntityImages["PostImage"] : null;

            if (context.MessageName != nameof(ContextMessageEnum.Update))
            {
                tracing.Trace("Skipping because not an Update message.");
                return;
            }

            tracing.Trace("Processing activation/deactivation of MLE.");

            CascadeStateToChildrenEntities(service, tracing, pre, post);
        }

        private void CascadeStateToChildrenEntities(IOrganizationService service, ITracingService tracing, Entity pre, Entity post)
        {
            if (pre != null)
            {
                tracing.Trace($"PreImage attributes: {string.Join(",", pre.Attributes.Select(a => a.Key))}");
            }

            if (post != null)
            {
                tracing.Trace($"PostImage attributes: {string.Join(",", post.Attributes.Select(a => a.Key))}");
            }

            if (pre == null || post == null)
            {
                tracing.Trace("ERROR: Pre or Post Image is NULL. Check plugin registration.");
                return;
            }

            var preImageStateCode = pre.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value;
            var postImageStateCode = post.GetAttributeValue<OptionSetValue>(KTR_ManagedListEntity.Fields.StateCode)?.Value;

            tracing.Trace($"preImageStateCode = {preImageStateCode}, postImageStateCode = {postImageStateCode}");

            if (preImageStateCode == (int)KTR_ManagedListEntity_StateCode.Active
                && postImageStateCode == (int)KTR_ManagedListEntity_StateCode.Inactive)
            {
                // Deactivate SMLE and collect study IDs
                var affectedStudyIds = DeactivateStudyMLEntityCascade(service, tracing, pre.Id);

                // Deactivate QLMLE
                DeactivateQLMLEntityCascade(service, tracing, pre.Id);

                // Recalculate subsets for each study affected
                RecalculateSubsets(service, tracing, affectedStudyIds);
            }
        }

        private List<Guid> DeactivateStudyMLEntityCascade(
            IOrganizationService service,
            ITracingService tracing,
            Guid entityId)
        {
            var studyMlEntityRepository = new StudyManagedlistEntityRepository(service);

            var studyMlEntities = studyMlEntityRepository.GetDraftStudyMLEsByEntityId(entityId);

            studyMlEntityRepository.BulkUpdateStatus(
                studyMlEntities,
                KTR_StudyManagedListEntity_StateCode.Inactive,
                KTR_StudyManagedListEntity_StatusCode.Inactive);

            // Collect unique study IDs
            var studyIds = studyMlEntities
                .Where(e => e.KTR_Study != null)
                .Select(e => e.KTR_Study.Id)
                .Distinct()
                .ToList();

            tracing.Trace("Affected Draft Study IDs: " + string.Join(",", studyIds));

            return studyIds;
        }

        private void DeactivateQLMLEntityCascade(
           IOrganizationService service,
           ITracingService tracing,
           Guid entityId)
        {
            var qlMlEntityRepository = new QuestionnaireLineManagedListEntityRepository(service, tracing);

            var qlMlEntities = qlMlEntityRepository.GetDraftStudyQLMLEsByEntityId(entityId);

            qlMlEntityRepository.BulkUpdateStatus(
                qlMlEntities,
                KTR_QuestionnaireLinemanAgedListEntity_StateCode.Inactive,
                KTR_QuestionnaireLinemanAgedListEntity_StatusCode.Inactive);
        }

        private void RecalculateSubsets(
        IOrganizationService service,
        ITracingService tracing,
        List<Guid> studyIds)
        {

            if (studyIds == null || studyIds.Count == 0)
            {
                tracing.Trace("No study IDs to process for subset logic.");
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

                foreach (var id in studyIds)
                {
                    subsetSvc.ProcessSubsetLogic(id);
                }
            }
            catch (Exception ex)
            {
                tracing.Trace("Subset re-calculation error: " + ex.Message);
            }
        }
    }
}
