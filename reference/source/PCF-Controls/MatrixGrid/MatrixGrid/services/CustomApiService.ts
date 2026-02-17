import { UrlHelpers } from "../utils/UrlHelpers";

export class CustomApiService {

    private _webAPI: ComponentFramework.WebApi;

    constructor(webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
    }

    /**
     * Calls the ktr_detect_or_create_subset Custom API
     * @param studyId Guid of the study record
    */
    async createOrDetectSubsetAPI(studyId: string, context: ComponentFramework.Context<any>): Promise<void> {
        const actionName = "ktr_detect_or_create_subset";

        try {
            // Build the URL for the unbound Custom API
            let baseUrl = UrlHelpers.getBaseUrl(context);
            const url = `${baseUrl}/${actionName}`;

            // Prepare the request body
            const body = { studyId };

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
                console.log(`${actionName} completed successfully for StudyId: ${studyId}`);
            } else {
                console.error(`Error executing ${actionName} API:`, response.statusText);
            }
        } catch (error: any) {
            console.error(`Error executing ${actionName} API:`, error);
            throw error;
        }
    }
}