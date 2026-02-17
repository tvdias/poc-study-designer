export class UserRoleService {

    private static readonly SCRIPTER_ROLE_VALUE = 847610002;

    static async CheckIfUserHasOnlyScripterRole(userId: string): Promise<boolean> {
        try {
            // Query systemuser table for this user
            const query = `?$select=ktr_businessrole&$filter=systemuserid eq ${userId.replace(/[{}]/g, '')}`;
            const result = await Xrm.WebApi.retrieveMultipleRecords("systemuser", query);

            if (!result.entities || result.entities.length !== 1) {
                // Either no record or multiple records found — not valid
                return false;
            }

            const user = result.entities[0];

            // Safely extract the OptionSet value
            const roleValue =
                user.ktr_businessrole?.Value ??
                user.ktr_businessrole?.value ??
                user.ktr_businessrole;

            console.log("Fetched role value:", roleValue);

            return roleValue === UserRoleService.SCRIPTER_ROLE_VALUE;
        } catch (error) {
            console.error("Error checking scripter role:", error);
            return false;
        }
    }
}
