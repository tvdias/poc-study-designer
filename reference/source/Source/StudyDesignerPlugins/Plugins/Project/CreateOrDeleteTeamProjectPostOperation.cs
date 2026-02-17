using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace Kantar.StudyDesignerLite.Plugins.Project
{
    public class CreateOrDeleteTeamProjectPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.CreateOrDeleteTeamProjectPostOperation";

        public CreateOrDeleteTeamProjectPostOperation() : base(typeof(CreateOrDeleteTeamProjectPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            ITracingService tracingService = localPluginContext.TracingService;
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.CurrentUserService;

            context.InputParameters.TryGetValue("Target", out Entity entity);

            tracingService.Trace($"{PluginName} {entity.LogicalName}");

            if (entity.LogicalName == KT_Project.EntityLogicalName)
            {
                var project = entity.ToEntity<KT_Project>();

                if (context.MessageName == "Update")
                {
                    UpdateProjectOperation(context, service, tracingService, project);
                }
                else if (context.MessageName == "Create")
                {
                    CreateProjectOperation(context, service, tracingService, project);
                }
            }
        }

        private void UpdateProjectOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService, KT_Project project)
        {
            var preEntity = GetEntityImage<KT_Project>(context.PreEntityImages, "Image");
            var postEntity = GetEntityImage<KT_Project>(context.PostEntityImages, "Image");

            var preAccessTeam = preEntity?.KTR_AccessTeam ?? false;
            var postAccessTeam = postEntity?.KTR_AccessTeam ?? false;

            var preTeamAccessRef = preEntity?.KTR_TeamAccess;
            var postTeamAccessRef = postEntity?.KTR_TeamAccess;

            if (postAccessTeam)
            {
                if (postTeamAccessRef == null)
                {
                    postTeamAccessRef = CreateAccessTeam(service, project.Id);
                    project.KTR_TeamAccess = postTeamAccessRef;
                    service.Update(project);
                }

                GrantAccessToTeam(service, project, postTeamAccessRef, tracingService);
            }
            else if (preAccessTeam && preTeamAccessRef != null)
            {
                RevokeAccess(service, project, preTeamAccessRef, tracingService);
            }
        }

        private void CreateProjectOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracingService, KT_Project project)
        {
            var postEntity = GetEntityImage<KT_Project>(context.PostEntityImages, "Image");
            bool postAccessTeam = postEntity?.KTR_AccessTeam.Value ?? false;
            var postTeamAccessRef = postEntity?.KTR_TeamAccess;

            if (postAccessTeam)
            {
                if (postTeamAccessRef == null)
                {
                    postTeamAccessRef = CreateAccessTeam(service, project.Id);
                    project.KTR_TeamAccess = postTeamAccessRef;
                    service.Update(project);
                }

                GrantAccessToTeam(service, project, postTeamAccessRef, tracingService);
            }
        }

        private static T GetEntityImage<T>(EntityImageCollection images, string key) where T : Entity
        {
            return images.TryGetValue(key, out var entity) ? entity.ToEntity<T>() : null;
        }

        private void GrantAccessToTeam(IOrganizationService service, KT_Project project, EntityReference teamRef, ITracingService tracingService)
        {
            tracingService.Trace($"Granting Access: Project {project.Id}, Team {teamRef.Id}");

            ShareEntityWithTeam(service, project.LogicalName, project.Id, teamRef, tracingService);

            // Fetch and share KT_QuestionnaireLines (lookup: KTR_Project)
            var questionnaireLines = FetchEntityRecords(service, KT_QuestionnaireLines.EntityLogicalName, project.Id, KT_QuestionnaireLines.Fields.KTR_Project);
            foreach (var line in questionnaireLines.Entities)
            {
                ShareEntityWithTeam(service, KT_QuestionnaireLines.EntityLogicalName, line.Id, teamRef, tracingService);
                // Fetch and share KTR_QuestionnaireLinesAnswerList (lookup: KTR_QuestionnaireLines)
                var answerLines = FetchEntityRecords(service, KTR_QuestionnaireLinesAnswerList.EntityLogicalName, line.Id, KTR_QuestionnaireLinesAnswerList.Fields.KTR_QuestionnaireLine);
                foreach (var answer in answerLines.Entities)
                {
                    ShareEntityWithTeam(service, KTR_QuestionnaireLinesAnswerList.EntityLogicalName, answer.Id, teamRef, tracingService);
                }
            }
            // Fetch and share KTR_SubsetDefinition (lookup: KTR_Project)
            var subsetdefinition = FetchEntityRecords(service, KTR_SubsetDefinition.EntityLogicalName, project.Id, KTR_SubsetDefinition.Fields.KTR_Project);
            foreach (var definition in subsetdefinition.Entities)
            {
                ShareEntityWithTeam(service, KTR_SubsetDefinition.EntityLogicalName, definition.Id, teamRef, tracingService);
                // Fetch and share KTR_QuestionnaireLinesSubset (lookup: KTR_SubsetDefinitionId)
                var questionsubset = FetchEntityRecords(service, KTR_QuestionnaireLineSubset.EntityLogicalName, definition.Id, KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId);
                foreach (var question in questionsubset.Entities)
                {
                    ShareEntityWithTeam(service, KTR_QuestionnaireLineSubset.EntityLogicalName, question.Id, teamRef, tracingService);
                }
                // Fetch and share KTR_SubsetEntities (lookup: KTR_SubsetDefinitionId)

                var subsetEntity = FetchEntityRecords(service, KTR_SubsetEntities.EntityLogicalName, definition.Id, KTR_SubsetEntities.Fields.KTR_SubsetDeFinTion);
                foreach (var entity in subsetEntity.Entities)
                {
                    ShareEntityWithTeam(service, KTR_SubsetEntities.EntityLogicalName, entity.Id, teamRef, tracingService);
                }
            }

            // Fetch and share KT_Study (lookup: KT_Project)
            var studyEntities = FetchEntityRecords(service, KT_Study.EntityLogicalName, project.Id, KT_Study.Fields.KT_Project);
            foreach (var study in studyEntities.Entities)
            {
                ShareEntityWithTeam(service, KT_Study.EntityLogicalName, study.Id, teamRef, tracingService);

                // Fetch and share KTR_StudyQuestionnaireLine (lookup: KTR_Study)
                var studyLines = FetchEntityRecords(service, KTR_StudyQuestionnaireLine.EntityLogicalName, study.Id, KTR_StudyQuestionnaireLine.Fields.KTR_Study);
                foreach (var studyLine in studyLines.Entities)
                {
                    ShareEntityWithTeam(service, KTR_StudyQuestionnaireLine.EntityLogicalName, studyLine.Id, teamRef, tracingService);
                }

                // Fetch and share KTR_StudySnapshotLineChangelog (lookup: KTR_Study)
                var studySnapshot = FetchEntityRecords(service, KTR_StudySnapshotLineChangelog.EntityLogicalName, study.Id, KTR_StudySnapshotLineChangelog.Fields.KTR_CurrentStudy);
                foreach (var studySnapshots in studySnapshot.Entities)
                {
                    ShareEntityWithTeam(service, KTR_StudySnapshotLineChangelog.EntityLogicalName, studySnapshots.Id, teamRef, tracingService);
                }

                // Urls share
                var studyUrls = FetchEntityRecords(service, KTR_Url.EntityLogicalName, study.Id, KTR_Url.Fields.KTR_Study);
                foreach (var studyUrl in studyUrls.Entities)
                {
                    ShareEntityWithTeam(service, KTR_Url.EntityLogicalName, studyUrl.Id, teamRef, tracingService);
                }

                // Fetch and share KTR_StudyQuestionnaireLineSnapshot(lookup: KTR_Study)
                var studyQLineSnapshot = FetchEntityRecords(service,
                    KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                    study.Id,
                    KTR_StudyQuestionnaireLineSnapshot.Fields.KTR_Study);

                foreach (var studyQLineSnapshots in studyQLineSnapshot.Entities)
                {
                    ShareEntityWithTeam(service,
                        KTR_StudyQuestionnaireLineSnapshot.EntityLogicalName,
                        studyQLineSnapshots.Id,
                        teamRef,
                        tracingService);

                    // Fetch and share Study Question Managed List Snapshot (lookup: KTR_StudyQuestionnaireLineSnapshot)
                    var managedListSnapshots = FetchEntityRecords(service,
                        KTR_StudyQuestionManagedListSnapshot.EntityLogicalName,
                        studyQLineSnapshots.Id,
                        KTR_StudyQuestionManagedListSnapshot.Fields.KTR_QuestionnaireLinesNaPsHot
                        );

                    foreach (var managedListSnapshot in managedListSnapshots.Entities)
                    {
                        ShareEntityWithTeam(service,
                            KTR_StudyQuestionManagedListSnapshot.EntityLogicalName,
                            managedListSnapshot.Id,
                            teamRef,
                            tracingService);

                        var managedListEntitySnapshots = FetchEntityRecords(service,
                            KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName,
                            managedListSnapshot.Id,
                            KTR_StudyManagedListEntitiesSnapshot.Fields.KTR_StudyQuestionManagedListSnapshot);

                        foreach (var entitySnapshot in managedListEntitySnapshots.Entities)
                        {
                            ShareEntityWithTeam(service,
                                KTR_StudyManagedListEntitiesSnapshot.EntityLogicalName,
                                entitySnapshot.Id,
                                teamRef,
                                tracingService);
                        }
                    }
                }

                // Fetch and share KTR_StudySubsetDefinitionSnapshot
                var studySubsetSnapshots = FetchEntityRecords(service,
                    KTR_StudySubsetDefinitionSnapshot.EntityLogicalName,
                    study.Id,
                    KTR_StudySubsetDefinitionSnapshot.Fields.KTR_Study);

                foreach (var studySubsetSnapshot in studySubsetSnapshots.Entities)
                {
                    ShareEntityWithTeam(service,
                        KTR_StudySubsetDefinitionSnapshot.EntityLogicalName,
                        studySubsetSnapshot.Id,
                        teamRef,
                        tracingService);
                }

                // Fetch and share KTR_StudySubsetEntitiesSnapshot
                var studySubsetEntitiesSnapshots = FetchEntityRecords(service,
                    KTR_StudySubsetEntitiesSnapshot.EntityLogicalName,
                    study.Id,
                    KTR_StudySubsetEntitiesSnapshot.Fields.KTR_Study);

                foreach (var studySubsetEntitiesSnapshot in studySubsetEntitiesSnapshots.Entities)
                {
                    ShareEntityWithTeam(service,
                        KTR_StudySubsetEntitiesSnapshot.EntityLogicalName,
                        studySubsetEntitiesSnapshot.Id,
                        teamRef,
                        tracingService);
                }

            }

            // Fetch and share KTR_ManagedList (lookup: KTR_Project)
            var managedLists = FetchEntityRecords(service, KTR_ManagedList.EntityLogicalName, project.Id, KTR_ManagedList.Fields.KTR_Project);
            foreach (var managedList in managedLists.Entities)
            {
                ShareEntityWithTeam(service, KTR_ManagedList.EntityLogicalName, managedList.Id, teamRef, tracingService);

                // Fetch and share KTR_ManagedListEntity (lookup: KTR_ManagedList)
                var managedListEntities = FetchEntityRecords(service, KTR_ManagedListEntity.EntityLogicalName, managedList.Id, KTR_ManagedListEntity.Fields.KTR_ManagedList);
                foreach (var managedListEntity in managedListEntities.Entities)
                {
                    ShareEntityWithTeam(service, KTR_ManagedListEntity.EntityLogicalName, managedListEntity.Id, teamRef, tracingService);

                    // Fetch and share related StudyManagedListEntity (lookup: KTR_ManagedListEntity)
                    var studyMLEntities = FetchEntityRecords(service,
                        KTR_StudyManagedListEntity.EntityLogicalName,
                        managedListEntity.Id,
                        KTR_StudyManagedListEntity.Fields.KTR_ManagedListEntity);

                    foreach (var studyMLEntity in studyMLEntities.Entities)
                    {
                        ShareEntityWithTeam(service, KTR_StudyManagedListEntity.EntityLogicalName, studyMLEntity.Id, teamRef, tracingService);
                    }

                    // Fetch and Share related QuestionnaireLineManagedListEntity (lookup: KTR_ManagedListEntity)
                    var qlineMLEntities = FetchEntityRecords(service,
                        KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName,
                        managedListEntity.Id,
                        KTR_QuestionnaireLinemanAgedListEntity.Fields.KTR_ManagedListEntity);

                    foreach (var qlineMLEntity in qlineMLEntities.Entities)
                    {
                        ShareEntityWithTeam(service, KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, qlineMLEntity.Id, teamRef, tracingService);
                    }
                }
            }

            // Fetch and share KTR_QuestionnaireLinesHaRedList (lookup: KTR_Project)
            var managedListQuestionnaireLines = FetchEntityRecords(service, KTR_QuestionnaireLinesHaRedList.EntityLogicalName, project.Id, KTR_QuestionnaireLinesHaRedList.Fields.KTR_ProjectId);
            foreach (var record in managedListQuestionnaireLines.Entities)
            {
                ShareEntityWithTeam(service, KTR_QuestionnaireLinesHaRedList.EntityLogicalName, record.Id, teamRef, tracingService);
            }
        }

        private void RevokeAccess(IOrganizationService service, KT_Project project, EntityReference teamRef, ITracingService tracingService)
        {
            tracingService.Trace($"Revoking Access: Project {project.Id}, Team {teamRef.Id}");
            service.Delete(Team.EntityLogicalName, teamRef.Id);
        }

        private void ShareEntityWithTeam(IOrganizationService service, string entityName, Guid entityId, EntityReference teamRef, ITracingService tracingService)
        {
            var request = new GrantAccessRequest
            {
                Target = new EntityReference(entityName, entityId),
                PrincipalAccess = new PrincipalAccess
                {
                    Principal = teamRef,
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess
                }
            };
            service.Execute(request);
            tracingService.Trace($"Shared successfully! EntityName: {entityName}");
        }

        private EntityCollection FetchEntityRecords(IOrganizationService service, string entityName, Guid lookupId, string lookupField)
        {
            var query = new QueryExpression(entityName)
            {
                ColumnSet = new ColumnSet($"{entityName}id"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression(lookupField, ConditionOperator.Equal, lookupId) }
                }
            };
            return service.RetrieveMultiple(query);
        }

        private EntityReference CreateAccessTeam(IOrganizationService service, Guid projectId)
        {
            var team = new Team()
            {
                Name = projectId.ToString(),
                TeamType = Team_TeamType.Access
            };
            return new EntityReference(Team.EntityLogicalName, service.Create(team));
        }
    }
}
