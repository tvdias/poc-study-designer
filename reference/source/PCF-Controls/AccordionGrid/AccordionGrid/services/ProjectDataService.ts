import { UrlHelper } from "../utils/UrlHelper";

export class ProjectDataService {

    private _webAPI: ComponentFramework.WebApi;
    private _context: ComponentFramework.Context<any>;

    constructor(context: ComponentFramework.Context<any>, webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
        this._context = context;
    }

    /**
     * Calls the ktr_reorder_project_questionnaire_unbound Custom API
     * @param projectId Guid of the project to reorder
     */
    async reorderProjectQuestionnaire(projectId: string): Promise<void> 
    {
        try {
            const actionName = "ktr_reorder_project_questionnaire_unbound";

            // Build the URL for the unbound Custom API
            let baseUrl = UrlHelper.getBaseUrl(this._context);
            const url = `${baseUrl}/${actionName}`;

            // Prepare the request body
            const body = {
                projectId: projectId
            };

            // Execute the HTTP POST request
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Accept": "application/json",
                    "Content-Type": "application/json; charset=utf-8",
                    "OData-MaxVersion": "4.0",
                    "OData-Version": "4.0"
                },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                console.log(`Reorder completed successfully for ProjectId: ${projectId}`);
            } else {
                console.error("Error executing reorder API:", response.statusText);
            }
        } catch (error: any) {
            console.error("Error executing reorder API:", error);
            throw error;
        }
    }
}