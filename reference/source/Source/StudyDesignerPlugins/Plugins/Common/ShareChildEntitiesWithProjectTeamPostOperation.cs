using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Common
{
    public class ShareChildEntitiesWithProjectTeamPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.ShareChildEntitiesWithProjectTeamPostOperation";

        public ShareChildEntitiesWithProjectTeamPostOperation() : base(typeof(ShareChildEntitiesWithProjectTeamPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.CurrentUserService;

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity entity))
            {
                return;
            }

            tracingService.Trace($"Executing {PluginName} for {entity.LogicalName}");

            var projectId = GetProjectIdFromEntity(entity, service, tracingService);
            if (!projectId.HasValue)
            {
                tracingService.Trace("No related KT_Project found. Skipping...");
                return;
            }

            var project = GetProject(service, projectId.Value);
            tracingService.Trace($"Project fetched: ", project.Id);

            ShareWithOwnerIfExists(project, service, entity, tracingService);
            ShareWithTeamIfApplicable(project, service, entity, tracingService);
        }

        private void ShareWithOwnerIfExists(KT_Project project, IOrganizationService service, Entity entity, ITracingService tracingService)
        {
            if (project.OwnerId == null)
            {
                return;
            }

            tracingService.Trace($"Sharing entity {entity.Id} with project owner {project.OwnerId.Id}");
            ShareEntityWithPrincipal(service, entity.LogicalName, entity.Id, project.OwnerId, tracingService);
        }

        private void ShareWithTeamIfApplicable(KT_Project project, IOrganizationService service, Entity entity, ITracingService tracingService)
        {
            if (!project.KTR_AccessTeam.GetValueOrDefault() || project.KTR_TeamAccess == null)
            {
                tracingService.Trace("Project does not have an access team enabled.");
                return;
            }

            tracingService.Trace($"Sharing entity {entity.Id} with project team {project.KTR_TeamAccess.Id}");
            ShareEntityWithPrincipal(service, entity.LogicalName, entity.Id, project.KTR_TeamAccess, tracingService);
        }

        private KT_Project GetProject(IOrganizationService service, Guid projectId)
        {
            return service.Retrieve(
                KT_Project.EntityLogicalName,
                projectId,
                new ColumnSet(
                    KT_Project.Fields.KTR_AccessTeam,
                    KT_Project.Fields.KTR_TeamAccess,
                    KT_Project.Fields.OwnerId)
            ).ToEntity<KT_Project>();
        }

        private Guid? GetProjectIdFromEntity(Entity entity, IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                if (entity.LogicalName == KT_QuestionnaireLines.EntityLogicalName && entity.Contains(KT_QuestionnaireLines.Fields.KTR_Project))
                {
                    return entity.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_Project)?.Id;
                }

                if (entity.LogicalName == KT_Study.EntityLogicalName && entity.Contains(KT_Study.Fields.KT_Project))
                {
                    return entity.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project)?.Id;
                }

                if (entity.LogicalName == KTR_StudyQuestionnaireLine.EntityLogicalName && entity.Contains(KTR_StudyQuestionnaireLine.Fields.KTR_Study))
                {
                    var studyRef = entity.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLine.Fields.KTR_Study);
                    return GetProjectIdFromStudy(service, studyRef);
                }

                if (entity.LogicalName == KTR_StudySnapshotLineChangelog.EntityLogicalName && entity.Contains(KTR_StudySnapshotLineChangelog.Fields.KTR_CurrentStudy))
                {
                    var studyRef = entity.GetAttributeValue<EntityReference>(KTR_StudySnapshotLineChangelog.Fields.KTR_CurrentStudy);
                    return GetProjectIdFromStudy(service, studyRef);
                }

                if (entity.LogicalName == KTR_QuestionnaireLinesAnswerList.EntityLogicalName && entity.Contains(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine))
                {
                    var questionnaireLineRef = entity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
                    return GetProjectIdFromQuestionnaireLine(service, questionnaireLineRef);
                }

                if (entity.LogicalName == KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName && entity.Contains(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine))
                {
                    var questionnaireLineRef = entity.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine);
                    return GetProjectIdFromQuestionnaireLine(service, questionnaireLineRef);
                }

                if (entity.LogicalName == KTR_StudyQuestionAnswerListSnapshot.EntityLogicalName && entity.Contains(KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot))
                {
                    var questionnaireLineSnapshotRef = entity.GetAttributeValue<EntityReference>(KTR_StudyQuestionAnswerListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);
                    return GetProjectIdFromQuestionnaireLineSnapshot(service, questionnaireLineSnapshotRef);
                }

                if (entity.LogicalName == KTR_Url.EntityLogicalName && entity.Contains(KTR_Url.Fields.KTR_Study))
                {
                    var studyRef = entity.GetAttributeValue<EntityReference>(KTR_Url.Fields.KTR_Study);
                    return GetProjectIdFromStudy(service, studyRef);
                }

                if (entity.LogicalName == KTR_ManagedList.EntityLogicalName && entity.Contains(KTR_ManagedList.Fields.KTR_Project))
                {
                    return entity.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project)?.Id;
                }

                if (entity.LogicalName == KTR_QuestionnaireLinesHaRedList.EntityLogicalName && entity.Contains(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId))
                {
                    return entity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId)?.Id;
                }

                if (entity.LogicalName == KTR_StudyQuestionManagedListSnapshot.EntityLogicalName && entity.Contains(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot))
                {
                    var questionnaireLineSnapshotRef = entity.GetAttributeValue<EntityReference>(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);
                    return GetProjectIdFromQuestionnaireLineSnapshot(service, questionnaireLineSnapshotRef);
                }

                if (entity.LogicalName == KTR_ManagedListEntity.EntityLogicalName && entity.Contains(KTR_ManagedListEntity.Fields.KTR_ManagedList))
                {
                    var managedListRef = entity.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
                    return GetProjectIdFromManagedList(service, managedListRef);
                }

                if (entity.LogicalName == KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName && entity.Contains(KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot))
                {
                    var smleSnapshotRef = entity.ToEntityReference();
                    return GetProjectIdFromStudyManagedListEntitySnapshot(service, smleSnapshotRef);
                }

                if (entity.LogicalName == KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName && entity.Contains(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity))
                {
                    var managedListEntityRef = entity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity);
                    return GetProjectIdFromManagedListEntity(service, managedListEntityRef);
                }

                if (entity.LogicalName == KTR_StudyManagedListEntity.EntityLogicalName && entity.Contains(KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity))
                {
                    var managedListEntityRef = entity.GetAttributeValue<EntityReference>(KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity);
                    return GetProjectIdFromManagedListEntity(service, managedListEntityRef);
                }
                if (entity.LogicalName == KTR_SubsetDefinition.EntityLogicalName && entity.Contains(KTR_SubsetDefinition.Fields.KTR_Project))
                {
                    return entity.GetAttributeValue<EntityReference>(KTR_SubsetDefinition.Fields.KTR_Project)?.Id;
                }
                if (entity.LogicalName == KTR_QuestionnaireLineSubset.EntityLogicalName && entity.Contains(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId))
                {
                    var subsetDefinitionRef = entity.GetAttributeValue<EntityReference>(KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId);
                    return GetProjectIdFromSubsetDefinition(service, subsetDefinitionRef);
                }
                if (entity.LogicalName == KTR_SubsetEntities.EntityLogicalName && entity.Contains(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion))
                {
                    var subsetDefinitionRef = entity.GetAttributeValue<EntityReference>(KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion);
                    return GetProjectIdFromSubsetDefinition(service, subsetDefinitionRef);
                }

                if (entity.LogicalName == KTR_StudySubsetDefinitionSnapshot.EntityLogicalName && entity.Contains(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot))
                {
                    var questionnaireLineSnapshotRef = entity.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);
                    return GetProjectIdFromQuestionnaireLineSnapshot(service, questionnaireLineSnapshotRef);
                }

                if (entity.LogicalName == KTR_StudySubsetEntitiesSnapshot.EntityLogicalName && entity.Contains(KTR_StudySubsetEntitiesSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot))
                {
                    var questionnaireLineSnapshotRef = entity.GetAttributeValue<EntityReference>(KTR_StudySubsetDefinitionSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);
                    return GetProjectIdFromQuestionnaireLineSnapshot(service, questionnaireLineSnapshotRef);
                }

            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error retrieving KT_Project: {ex.Message}");
            }

            return null;
        }

        private void ShareEntityWithPrincipal(IOrganizationService service, string entityName, Guid entityId, EntityReference principalRef, ITracingService tracingService)
        {
            try
            {
                var accessRights = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess;
                var grantAccessRequest = new GrantAccessRequest
                {
                    Target = new EntityReference(entityName, entityId),
                    PrincipalAccess = new PrincipalAccess
                    {
                        Principal = principalRef,
                        AccessMask = accessRights
                    }
                };

                service.Execute(grantAccessRequest);
                tracingService.Trace($"Successfully shared {entityName} ({entityId}) with {principalRef.LogicalName} {principalRef.Id}");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error sharing entity: {ex.Message}");
                throw new InvalidPluginExecutionException("Error sharing entity", ex);
            }
        }

        private Guid? GetProjectIdFromQuestionnaireLine(IOrganizationService service, EntityReference questionnaireLineRef)
        {
            if (questionnaireLineRef == null)
            {
                return null;
            }

            var questionnaireLine = service.Retrieve(KT_QuestionnaireLines.EntityLogicalName, questionnaireLineRef.Id, new ColumnSet(KT_QuestionnaireLines.Fields.KTR_Project));
            return questionnaireLine.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_Project)?.Id;

        }

        private Guid? GetProjectIdFromQuestionnaireLineSnapshot(IOrganizationService service, EntityReference questionnaireLineSnapshotRef)
        {
            if (questionnaireLineSnapshotRef == null)
            {
                return null;
            }

            var questionnaireLineSnapshot = service.Retrieve(KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName, questionnaireLineSnapshotRef.Id, new ColumnSet(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine));
            var questionnaireLineRef = questionnaireLineSnapshot.GetAttributeValue<EntityReference>(KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_QuestionnaireLine);
            return GetProjectIdFromQuestionnaireLine(service, questionnaireLineRef);
        }

        private Guid? GetProjectIdFromStudy(IOrganizationService service, EntityReference studyRef)
        {
            if (studyRef == null)
            {
                return null;
            }

            var study = service.Retrieve(KT_Study.EntityLogicalName, studyRef.Id, new ColumnSet(KT_Study.Fields.KT_Project));
            return study.GetAttributeValue<EntityReference>(KT_Study.Fields.KT_Project)?.Id;
        }

        private Guid? GetProjectIdFromManagedList(IOrganizationService service, EntityReference managedListRef)
        {
            if (managedListRef == null)
            {
                return null;
            }

            var managedList = service.Retrieve(KTR_ManagedList.EntityLogicalName, managedListRef.Id, new ColumnSet(KTR_ManagedList.Fields.KTR_Project));
            return managedList.GetAttributeValue<EntityReference>(KTR_ManagedList.Fields.KTR_Project)?.Id;
        }

        private Guid? GetProjectIdFromStudyManagedListEntitySnapshot(IOrganizationService service, EntityReference studyManagedListEntitySnapshotRef)
        {
            if (studyManagedListEntitySnapshotRef == null)
            {
                return null;
            }

            // Retrieve the StudyManagedListEntitySnapshot
            var smleSnapshot = service.Retrieve(
                KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName,
                studyManagedListEntitySnapshotRef.Id,
                new ColumnSet(KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot)
            );

            var sqmlSnapshotRef = smleSnapshot.GetAttributeValue<EntityReference>(KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot);
            if (sqmlSnapshotRef == null)
            {
                return null;
            }

            // Retrieve the StudyQuestionManagedListSnapshot
            var sqmlSnapshot = service.Retrieve(
                KTR_StudyQuestionManagedListSnapshot.EntityLogicalName,
                sqmlSnapshotRef.Id,
                new ColumnSet(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot)
            );

            var questionnaireLineSnapshotRef = sqmlSnapshot.GetAttributeValue<EntityReference>(KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot);
            return GetProjectIdFromQuestionnaireLineSnapshot(service, questionnaireLineSnapshotRef);
        }
        private Guid? GetProjectIdFromSubsetDefinition(IOrganizationService service, EntityReference subsetDefinitionRef)
        {
            if (subsetDefinitionRef == null)
            {
                return null;
            }

            var subsetDefinition = service.Retrieve(KTR_SubsetDefinition.EntityLogicalName, subsetDefinitionRef.Id, new ColumnSet(KTR_SubsetDefinition.Fields.KTR_Project));
            return subsetDefinition.GetAttributeValue<EntityReference>(KTR_SubsetDefinition.Fields.KTR_Project)?.Id;

        }
        private Guid? GetProjectIdFromManagedListEntity(IOrganizationService service, EntityReference managedListEntityRef)
        {
            if (managedListEntityRef == null)
            {
                return null;
            }

            // Retrieve ManagedListEntity to get ManagedList
            var managedListEntity = service.Retrieve(
                KTR_ManagedListEntity.EntityLogicalName,
                managedListEntityRef.Id,
                new ColumnSet(KTR_ManagedListEntity.Fields.KTR_ManagedList)
            );

            var managedListRef = managedListEntity.GetAttributeValue<EntityReference>(KTR_ManagedListEntity.Fields.KTR_ManagedList);
            if (managedListRef == null)
            {
                return null;
            }

            return GetProjectIdFromManagedList(service, managedListRef);
        }
    }
}
