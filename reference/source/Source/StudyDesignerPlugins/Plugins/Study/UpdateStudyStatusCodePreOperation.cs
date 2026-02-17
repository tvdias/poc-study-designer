namespace Kantar.StudyDesignerLite.Plugins.Study
{

    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.Subset;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public class UpdateStudyStatusCodePreOperation : PluginBase
    {
        private const string MessageRestrictStudyStatusUpdate = "No Scripter exists in Project’s Access Team, the Study cannot be updated as Ready for Scripting.";

        public UpdateStudyStatusCodePreOperation() : base(typeof(UpdateStudyStatusCodePreOperation)) { }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidOperationException(nameof(localContext));
            }

            ITracingService tracingService = localContext.TracingService;
            IOrganizationService currentUserService = localContext.SystemUserService;
            IPluginExecutionContext context = localContext.PluginExecutionContext;

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target && target.LogicalName == KT_Study.EntityLogicalName)
            {
                var study = target.ToEntity<KT_Study>();
                if (study.StatusCode == KT_Study_StatusCode.ReadyForScripting)
                {
                    RecalculateSubsets(currentUserService, tracingService, new List<Guid> { study.Id });

                    var scripterExistForStudy = IsScripterExistForProject(currentUserService, study.Id);

                    if (!scripterExistForStudy)
                    {
                        throw new InvalidPluginExecutionException(MessageRestrictStudyStatusUpdate);
                    }
                }
            }
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

        /// <summary>
        /// Check if the Scripter exists in the Project's Access Team
        /// </summary>
        /// <param name="service"></param>
        /// <param name="studyId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidPluginExecutionException"></exception>
        private bool IsScripterExistForProject(IOrganizationService service, Guid studyId)
        {
            // Call Query expression method by passing tag id as a parameter
            var projectTeamIdQuery = GetTeamProjectForStudy(studyId);
            var resultsProject = service.RetrieveMultiple(projectTeamIdQuery);

            if (resultsProject != null && resultsProject.Entities.Count > 0 && resultsProject.Entities[0].Contains(KT_Project.Fields.Id))
            {
                var projectId = resultsProject.Entities[0].Id;
                if (projectId != Guid.Empty)
                {
                    // call Team Membership query method by passing project id as a parameter
                    var teamMembershipQuery = GetTeamMembershipBasedOnAccesTeam(projectId);
                    var resultsTeamMembership = service.RetrieveMultiple(teamMembershipQuery);

                    if (resultsTeamMembership != null && resultsTeamMembership.Entities.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Create a query expression to retrieve the Project entity
        /// </summary>
        /// <param name="studyId"></param>
        /// <returns></returns>
        private QueryExpression GetTeamProjectForStudy(Guid studyId)
        {
            // Create a query expression to retrieve the tag_questionbank entity
            QueryExpression query = new QueryExpression(KT_Project.EntityLogicalName);
            query.ColumnSet.AddColumn(KT_Project.Fields.Id);

            query.Distinct = true;
            query.TopCount = 1;

            // Add a link to the project entity and add a condition to filter by the study id
            var queryProject = query.AddLink(KT_Study.EntityLogicalName, KT_Project.Fields.KT_ProjectId, KT_Study.Fields.KT_Project, JoinOperator.Inner);
            queryProject.LinkCriteria.AddCondition(KT_Study.Fields.KT_StudyId, ConditionOperator.Equal, studyId);

            return query;
        }

        /// <summary>
        /// Create a query expression to retrieve the team membership entity
        /// </summary>
        /// <param name="accessTeamId"></param>
        /// <returns></returns>
        private QueryExpression GetTeamMembershipBasedOnAccesTeam(Guid accessTeamId)
        {
            // Create a query expression to retrieve the tag_questionbank entity
            QueryExpression queryTeam = new QueryExpression(Team.EntityLogicalName);

            queryTeam.Distinct = true;

            // Add the link to the Project Team Membership entity
            var queryProjectTeamMembership = queryTeam.AddLink(TeamMembership.EntityLogicalName, Team.Fields.TeamId, TeamMembership.Fields.TeamId, JoinOperator.Inner);

            // Add the link to the Systemuser entity with business Role condition
            var queryUser = queryProjectTeamMembership.AddLink(SystemUser.EntityLogicalName, TeamMembership.Fields.SystemUserId, SystemUser.Fields.SystemUserId, JoinOperator.Inner);
            queryUser.LinkCriteria.AddCondition(SystemUser.Fields.KTR_BusinessRole, ConditionOperator.Equal, (int)SystemUser_businessrole.KantarScripter);

            // Add conditions to Team record
            queryTeam.Criteria.AddCondition(Team.Fields.Name, ConditionOperator.Equal, accessTeamId.ToString());
            return queryTeam;
        }
    }
}
