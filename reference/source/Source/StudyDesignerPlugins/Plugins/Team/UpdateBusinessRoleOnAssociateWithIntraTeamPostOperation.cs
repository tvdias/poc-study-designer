using System;
using System.Linq;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Constants;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kantar.StudyDesignerLite.Plugins.User
{
    public class UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation : PluginBase
    {
        private const string PluginName = "Kantar.StudyDesignerLite.Plugins.UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation";

        public UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation() : base(typeof(UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation))
        {
        }

        protected override void ExecuteCdsPlugin(ILocalPluginContext localPluginContext)
        {
            ITracingService tracingService = localPluginContext.TracingService;
            
            IPluginExecutionContext context = localPluginContext.PluginExecutionContext;
            IOrganizationService service = localPluginContext.SystemUserService;

            // Ensure the context is for the Associate message
            if (context.MessageName != nameof(ContextMessageEnum.Associate))
            {
                return;
            }
            // Ensure the relationship is for teammembership_association
            var relationship = (Relationship)context.InputParameters["Relationship"];

            if (relationship.SchemaName != "teammembership_association")
            {
                return;
            }

            if (context.InputParameters.Contains("Target"))
            {
                // Get the target entity reference (Team)
                EntityReference target = (EntityReference)context.InputParameters["Target"];
                // Get the related entities (SystemUser)
                EntityReferenceCollection relatedEntities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

                tracingService.Trace($"Executing {PluginName} for target: {target.LogicalName} with ID: {target.Id}");
                tracingService.Trace($"Related entities count: {relatedEntities.Count}");

                if (target.LogicalName == Team.EntityLogicalName)
                {
                    var team = service.Retrieve(Team.EntityLogicalName, target.Id, new ColumnSet(Team.Fields.Name)).ToEntity<Team>();

                    var systemUserEntity = relatedEntities.FirstOrDefault(x => x.LogicalName == SystemUser.EntityLogicalName);

                    if (relatedEntities.Count > 0 && systemUserEntity.LogicalName == SystemUser.EntityLogicalName && team != null)
                    {
                        var teamName = team.Name;
                        tracingService.Trace($"Team Name: {teamName}");

                        string businessRoleLabel = null;
                        var systemUserId = relatedEntities[0].Id;
                        tracingService.Trace($"System User ID: {systemUserId}");
                        // Business Role logic will be executed only if the user is associated with the team.
                        if (systemUserId != Guid.Empty)
                        {
                            // get current value of Buisss Role for User.
                            var systemUser = service.Retrieve(SystemUser.EntityLogicalName, systemUserId, new ColumnSet(SystemUser.Fields.KTR_BusinessRole)).ToEntity<SystemUser>();
                            // Get the label for the user's business role.
                            if (systemUser.Contains(SystemUser.Fields.KTR_BusinessRole))
                            {
                                businessRoleLabel = systemUser.FormattedValues[SystemUser.Fields.KTR_BusinessRole];
                            }
                            // Checking If the user is already associated with the team and has the same business role, else if there is any change then will update the business role.
                            if ((teamName == BusinessRoleConstants.SDLite_ClientService && businessRoleLabel != BusinessRoleConstants.KantarCsUser) ||
                                (teamName == BusinessRoleConstants.SDLite_Librarian && businessRoleLabel != BusinessRoleConstants.KantarLibrarian) ||
                                (teamName == BusinessRoleConstants.SDLite_Scripter && businessRoleLabel != BusinessRoleConstants.KantarScripter) ||
                                businessRoleLabel == null)
                            {
                                UpdateBusinessRole(service, team.Id, systemUserId);
                                tracingService.Trace("Business Role is updated based on Security Team.");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the business role for the user when they are associated with a team.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="systemUserId"></param>
        private void UpdateBusinessRole(IOrganizationService service, Guid teamId, Guid systemUserId)
        {
            // Logic to update the business role based on the team reference
            var updateUser = new SystemUser()
            {
                Id = systemUserId
            };
            // Get business role from Business Role mapping entity based on the team
            var businessRole = GetBusinessRoleBasedOnTeams(service, teamId);
            if (businessRole != null)
            {
                updateUser.KTR_BusinessRole = businessRole;
            }
            else
            {
                updateUser.KTR_BusinessRole = KTR_KantarBusinessRole.Other; // Default to Other if no mapping found
            }
            service.Update(updateUser);
        }

        /// <summary>
        /// Create a query expression to retrieve the team membership entity
        /// </summary>
        /// <param name="accessTeamId"></param>
        /// <returns></returns>
        private KTR_KantarBusinessRole? GetBusinessRoleBasedOnTeams(IOrganizationService service, Guid teamId)
        {
            var query = new QueryExpression(KTR_BusinessRoleMapping.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(KTR_BusinessRoleMapping.Fields.KTR_KAnTarBusinessRole),
                Criteria =
                {
                    Conditions =
                {
                    new ConditionExpression(KTR_BusinessRoleMapping.Fields.KTR_Team, ConditionOperator.Equal, teamId)
                }
                },
                TopCount = 1
            };
            var results = service.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                var mapping = results.Entities[0].ToEntity<KTR_BusinessRoleMapping>();
                return mapping.KTR_KAnTarBusinessRole;
            }

            return null;
        }
    }
}
