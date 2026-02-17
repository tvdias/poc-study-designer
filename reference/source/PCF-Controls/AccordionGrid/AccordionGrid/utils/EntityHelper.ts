/**
 * EntityHelper - Utility functions for entity operations
 */

export class EntityHelper {
    /**
     * Generates dynamic MDA Edit URL for editing records
     * @param context - PCF context
     * @param entityName - Name of the entity
     * @param recordId - ID of the record to edit
     * @returns Promise<string> - The generated MDA URL
     */
    static async generateEditUrl(context: any, entityName: string, recordId: string): Promise<string> {
        try {
            // Try to get the organization URL from context
            let orgUrl = '';
            let appId = '';

            if ((context as any).client?.getGlobalContext?.()) {
                const globalContext = (context as any).client.getGlobalContext();
                orgUrl = globalContext.getClientUrl();

                try {
                    const app = await globalContext.getCurrentAppProperties();
                    appId = app.appId;
                    console.log("Current App ID:", appId);
                } catch (appError) {
                    console.log("Error retrieving app properties from client context:", appError);
                    throw new Error(`Failed to retrieve OrgUrl: ${appError}`);
                }
            }
            // Try from window.Xrm if available
            else if (window && (window as any).Xrm?.Utility?.getGlobalContext) {
                const globalContext = (window as any).Xrm.Utility.getGlobalContext();
                orgUrl = globalContext.getClientUrl();

                try {
                    const app = await globalContext.getCurrentAppProperties();
                    appId = app.appId;
                    console.log("Current App ID:", appId);
                } catch (appError) {
                    console.log("Error retrieving app properties from Xrm:", appError);
                    throw new Error(`Failed to retrieve OrgUrl: ${appError}`);
                }
            }

            // Remove trailing slash if present
            orgUrl = orgUrl.replace(/\/$/, '');

            // Construct the MDA URL
            const mdaUrl = `${orgUrl}/main.aspx?appid=${appId}&pagetype=entityrecord&etn=${entityName}&id=${recordId}`;

            console.log("Organization URL:", orgUrl);
            console.log("App ID:", appId);
            console.log("Generated Edit URL:", mdaUrl);
            return mdaUrl;
        } catch (error) {
            console.error("Error generating Edit URL:", error);
            return "";
        }
    }

    /**
     * Extracts projectId from gridDataset.
     */
    static getProjectId(context: ComponentFramework.Context<any>) : string {
        const dataSet = Object.values(context.parameters.gridDataSet.records) as Array<{ getValue: (fieldName: string) => string | null }>;
        return (dataSet[0]?.getValue('ktr_project') as any)?.id?.guid || "";
    }

    /**
     * Extracts projectId from Dynamics 365 URL query params.
     * Strips curly braces if present.
     */
    static getProjectIdFromUrl(url: string = window.location.href): string
    {
        try {
            const parsed = new URL(url);
            const idParam = parsed.searchParams.get("id");
            return idParam ? idParam.replace(/[{}]/g, "") : "";
        } catch (e) {
            console.error("Invalid URL in getProjectIdFromUrl:", e);
            return "";
        }
    }
}
