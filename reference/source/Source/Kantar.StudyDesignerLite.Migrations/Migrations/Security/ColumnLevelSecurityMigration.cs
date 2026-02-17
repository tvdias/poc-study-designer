namespace Kantar.StudyDesignerLite.Migrations.Migrations.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Migrations.Enums;
using Kantar.StudyDesignerLite.Migrations.Models;
using Kantar.StudyDesignerLite.Migrations.Models.Migrations.Security;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public class ColumnLevelSecurityMigration : BaseMigration
{
    public override int ExecutionOrder => 1;
    public override string Description => "Sync field security profile assignments";
    public override MigrationType Type => MigrationType.Security;

    // Override this to specify which profiles to sync assignments for.
    // !! If Not specified, then ALL profiles will be synced.
    protected virtual List<string> GetSpecificProfileNamesToSync()
    {
        return [];
    }

    public override async Task<MigrationResult> ExecuteAsync()
    {
        try
        {
            LogStart();

            var specificProfilesToSync = GetSpecificProfileNamesToSync();

            // Step 1: Find profiles in both source and target
            var profilesSource = await GetSecurityProfilesAsync(SourceService, specificProfilesToSync);
            var profilesTarget = await GetSecurityProfilesAsync(TargetService, specificProfilesToSync);

            if (!profilesSource.Any() && !profilesTarget.Any())
            {
                Logger.LogWarning("No profiles found in either source or target.");
                return MigrationResult.Skipped("No profiles found to sync.");
            }

            if (specificProfilesToSync.Any() && specificProfilesToSync.Count() != profilesSource.Count())
            {
                Logger.LogWarning("Profiles to sync mismatch in source.");
                return MigrationResult.Skipped("Profiles to sync mismatch in source. Pls make sure they exist in source.");
            }

            var profilesProcessed = 0;
            var assignmentsCreated = 0;
            var assignmentsSkipped = 0;
            var assignmentsFailed = 0;
            var assignmentsDeleted = 0;
            
            foreach (var sourceProfile in profilesSource)
            {
                Logger.LogInformation($"Processing assignments for profile: {sourceProfile["name"]}");

                try
                {
                    // Step 2: Get current assignments from source
                    var sourceTeamAssignment = await FindTeamProfileAssignmentAsync(SourceService, sourceProfile.Id);
                    var sourceUserAssignment = await FindUserProfileAssignmentAsync(SourceService, sourceProfile.Id);

                    var targetTeamAssignment = await FindTeamProfileAssignmentAsync(TargetService, sourceProfile.Id);
                    var targetUserAssignment = await FindUserProfileAssignmentAsync(TargetService, sourceProfile.Id);

                    Logger.LogDebug($"Found {sourceTeamAssignment.Count} Team assignments in source for profile: {sourceProfile["name"]}");
                    Logger.LogDebug($"Found {sourceUserAssignment.Count} User assignments in source for profile: {sourceProfile["name"]}");

                    if (!sourceTeamAssignment.Any() && !sourceUserAssignment.Any())
                    {
                        Logger.LogInformation($"No assignments found for profile: {sourceProfile["name"]}");
                        profilesProcessed++;
                        continue;
                    }

                    // Step 2.1: Remove extra assignments from target
                    var deletedTeamResults = await RemoveExtraFromTarget(sourceTeamAssignment, targetTeamAssignment, sourceProfile.Id, true);
                    var deletedUserResults = await RemoveExtraFromTarget(sourceUserAssignment, targetUserAssignment, sourceProfile.Id, false);
                    
                    // Step 3.1: Sync assignments to target
                    var targetProfile = profilesTarget
                        .FirstOrDefault(p => p.Id == sourceProfile.Id);

                    if (targetProfile == null)
                    {
                        Logger.LogInformation($"No target profile found: {sourceProfile["name"]}");
                        profilesProcessed++;
                        continue;
                    }

                    var teamResults = await SyncTeamAssignmentsToTargetAsync(targetProfile.Id, sourceTeamAssignment, sourceProfile.Id);
                    var userResults = await SyncUserAssignmentsToTargetAsync(targetProfile.Id, sourceUserAssignment, sourceProfile.Id);

                    assignmentsCreated += teamResults.Created + userResults.Created;
                    assignmentsSkipped += teamResults.Skipped + userResults.Skipped;
                    assignmentsFailed += teamResults.Failed + userResults.Failed;
                    assignmentsDeleted += deletedTeamResults.Deleted + deletedUserResults.Deleted;

                    profilesProcessed++;
                    Logger.LogInformation($"Completed Team profile '{sourceProfile}': {teamResults.Created} created, {teamResults.Skipped} skipped, {teamResults.Failed} failed, {deletedTeamResults.Deleted} deleted");
                    Logger.LogInformation($"Completed User profile '{sourceProfile}': {userResults.Created} created, {userResults.Skipped} skipped, {userResults.Failed} failed, {deletedUserResults.Deleted} deleted");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    assignmentsFailed++;
                }
            }

            var result = MigrationResult.Successful($"Synced assignments for {profilesProcessed} field security profiles");
            result.RecordsProcessed = profilesProcessed;
            result.RecordsCreated = assignmentsCreated;
            result.RecordsSkipped = assignmentsSkipped;
            result.RecordsDeleted = assignmentsFailed;
            result.RecordsDeleted = assignmentsDeleted;

            LogEnd();

            return result;
        }
        catch (Exception ex)
        {
            return LogException(ex);
        }
    }

    protected virtual async Task<List<Entity>> GetSecurityProfilesAsync(IOrganizationServiceAsync service, List<string> specificProfileNames)
    {
        var query = new QueryExpression("fieldsecurityprofile")
        {
            ColumnSet = new ColumnSet("fieldsecurityprofileid", "name", "description")
        };

        if (specificProfileNames.Count > 0)
        {
            query.Criteria.AddCondition("name", ConditionOperator.In, specificProfileNames.ToArray());
        }

        var result = await service.RetrieveMultipleAsync(query);

        return result.Entities
            .ToList();
    }

    protected virtual async Task<AssignmentSyncResult> SyncTeamAssignmentsToTargetAsync(Guid targetProfileId, List<Entity> sourceTeamAssignments, Guid profileId)
    {
        var result = new AssignmentSyncResult();

        if (sourceTeamAssignments.Count == 0)
        {
            return result;
        }

        foreach (var teamAssignement in sourceTeamAssignments)
        {
            var teamName = GetNameFromEntity(teamAssignement, true);

            try
            {
                var targetPrincipal = await FindTeamAsync(teamName);
                if (targetPrincipal == null)
                {
                    Logger.LogWarning($"Team not found in target environment: {teamName}");
                    result.Failed++;
                    continue;
                }

                if (targetPrincipal == null)
                {
                    Logger.LogWarning($"Could not resolve principal for assignment in profile '{profileId}'");
                    result.Failed++;
                    continue;
                }

                // Check if assignment already exists in target
                var existingTeamAssignement = await FindTeamProfileAssignmentAsync(TargetService, targetProfileId, targetPrincipal.Id);
                if (existingTeamAssignement.Count > 0)
                {
                    Logger.LogDebug($"Team Assignment already exists for {teamName}");
                    result.Skipped++;
                    continue;
                }

                // Create new assignment
                await CreateTeamProfileAssignmentAsync(targetProfileId, targetPrincipal.Id);

                Logger.LogDebug($"Created Team assignment: {teamName}");
                result.Created++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to sync assignment for {teamName}");
                result.Failed++;
            }
        }

        return result;
    }

    protected virtual async Task<AssignmentSyncResult> SyncUserAssignmentsToTargetAsync(Guid targetProfileId, List<Entity> sourceUserAssignments, Guid profileId)
    {
        var result = new AssignmentSyncResult();

        if (sourceUserAssignments.Count == 0)
        {
            return result;
        }

        foreach (var userAssignment in sourceUserAssignments)
        {
            var userName = GetNameFromEntity(userAssignment, false);

            try
            {
                var targetPrincipal = await FindUserAsync(userName);
                if (targetPrincipal == null)
                {
                    Logger.LogWarning($"User not found in target environment: {userName}");
                    result.Failed++;
                    continue;
                }

                if (targetPrincipal == null)
                {
                    Logger.LogWarning($"User not found in target environment: {userName}");
                    result.Failed++;
                    continue;
                }

                if (targetPrincipal == null)
                {
                    Logger.LogWarning($"Could not resolve principal for assignment in profile '{profileId}'");
                    result.Failed++;
                    continue;
                }

                // Check if assignment already exists in target
                var existingUserAssignement = await FindUserProfileAssignmentAsync(TargetService, targetProfileId, targetPrincipal.Id);
                if (existingUserAssignement.Count > 0)
                {
                    Logger.LogDebug($"User Assignment already exists for {userName}");
                    result.Skipped++;
                    continue;
                }

                // Create new assignment
                await CreateUserProfileAssignmentAsync(targetProfileId, targetPrincipal.Id);

                Logger.LogDebug($"Created User assignment: {userName}");
                result.Created++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to sync assignment for {userName}");
                result.Failed++;
            }
        }

        return result;
    }

    protected virtual async Task<Entity?> FindUserAsync(string userName)
    {
        var query = new QueryExpression("systemuser")
        {
            ColumnSet = new ColumnSet("systemuserid", "fullname", "internalemailaddress"),
            Criteria = new FilterExpression
            {
                Conditions =
                    {
                        new ConditionExpression("fullname", ConditionOperator.Equal, userName),
                        new ConditionExpression("isdisabled", ConditionOperator.Equal, false)
                    }
            },
            TopCount = 1
        };

        var result = await TargetService.RetrieveMultipleAsync(query);
        return result.Entities.FirstOrDefault();
    }

    protected virtual async Task<Entity?> FindTeamAsync(string teamName)
    {
        var query = new QueryExpression("team")
        {
            ColumnSet = new ColumnSet("teamid", "name"),
            Criteria = new FilterExpression
            {
                Conditions = { new ConditionExpression("name", ConditionOperator.Equal, teamName) }
            },
            TopCount = 1
        };

        var result = await TargetService.RetrieveMultipleAsync(query);
        return result.Entities.FirstOrDefault();
    }

    protected virtual async Task<List<Entity>> FindTeamProfileAssignmentAsync(IOrganizationServiceAsync service, Guid profileId, Guid? teamId = null)
    {
        var query = new QueryExpression("teamprofiles")
        {
            ColumnSet = new ColumnSet("teamid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("fieldsecurityprofileid", ConditionOperator.Equal, profileId)
                }
            }
        };

        if (teamId != null)
        {
            query.Criteria.AddCondition("teamid", ConditionOperator.Equal, teamId.Value);
        }

        var teamLink = query.AddLink(
            "team",
            "teamid",
            "teamid",
            JoinOperator.Inner);
        teamLink.Columns = new ColumnSet("name");
        teamLink.EntityAlias = "team";

        var result = await service.RetrieveMultipleAsync(query);
        return result.Entities
            .ToList();
    }

    protected virtual async Task<List<Entity>> FindUserProfileAssignmentAsync(IOrganizationServiceAsync service, Guid profileId, Guid? userId = null)
    {
        var query = new QueryExpression("systemuserprofiles")
        {
            ColumnSet = new ColumnSet("systemuserid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("fieldsecurityprofileid", ConditionOperator.Equal, profileId)
                }
            }
        };

        if (userId != null)
        {
            query.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId.Value);
        }

        var userLink = query.AddLink(
            "systemuser",
            "systemuserid",
            "systemuserid",
            JoinOperator.Inner);
        userLink.Columns = new ColumnSet("fullname");
        userLink.EntityAlias = "user";

        var result = await service.RetrieveMultipleAsync(query);
        return result.Entities
            .ToList();
    }

    protected virtual async Task CreateTeamProfileAssignmentAsync(Guid profileId, Guid teamId)
    {
        var relationship = new Relationship("teamprofiles_association");
        var relatedEntities = new EntityReferenceCollection
        {
            new EntityReference("team", teamId)
        };
        await TargetService.AssociateAsync("fieldsecurityprofile", profileId, relationship, relatedEntities);
    }

    protected virtual async Task DeleteTeamProfileAssignmentsAsync(Guid profileId, IEnumerable<Guid> teamIds)
    {
        var relationship = new Relationship("teamprofiles_association");

        var relatedEntities = new EntityReferenceCollection(
           teamIds.Select(id => new EntityReference("team", id)).ToList()
        );

        await TargetService.DisassociateAsync("fieldsecurityprofile", profileId, relationship, relatedEntities);
    }

    protected virtual async Task CreateUserProfileAssignmentAsync(Guid profileId, Guid userId)
    {
        var relationship = new Relationship("systemuserprofiles_association");
        var relatedEntities = new EntityReferenceCollection
        {
            new EntityReference("systemuser", userId)
        };
        await TargetService.AssociateAsync("fieldsecurityprofile", profileId, relationship, relatedEntities);
    }

    protected virtual async Task DeleteUserProfileAssignmentsAsync(Guid profileId, IEnumerable<Guid> userIds)
    {
        var relationship = new Relationship("systemuserprofiles_association");

        var relatedEntities = new EntityReferenceCollection(
            userIds.Select(id => new EntityReference("systemuser", id)).ToList()
        );

        await TargetService.DisassociateAsync("fieldsecurityprofile", profileId, relationship, relatedEntities);
    }

    protected virtual async Task<AssignmentSyncResult> RemoveExtraFromTarget(
        List<Entity> sourceAssignment,
        List<Entity> targetAssignment,
        Guid profileId,
        bool isTeam)
    {
        var result = new AssignmentSyncResult();

        if (targetAssignment.Count() > sourceAssignment.Count())
        {
            var userIds = targetAssignment
                .Select(t => GetIdFromEntity(t, isTeam))
                .Where(id => id != Guid.Empty)
                .ToList();

            if (isTeam)
            {
                await DeleteTeamProfileAssignmentsAsync(profileId, userIds);
                result.Deleted++;
            }
            else
            {
                await DeleteUserProfileAssignmentsAsync(profileId, userIds);
                result.Deleted++;
            }
        }

        return result;
    }

    private string GetNameFromEntity(Entity entity, bool isTeam)
    {
        var attributeName = isTeam ? "team.name" : "user.fullname";
        return entity.GetAttributeValue<AliasedValue>(attributeName)?.Value as string;
    }

    private Guid GetIdFromEntity(Entity entity, bool isTeam)
    {
        var attributeName = isTeam ? "teamid" : "systemuserid";
        return Guid.TryParse(entity[attributeName].ToString(), out var id) ? id : Guid.Empty;
    }
}
