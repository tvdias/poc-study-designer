import type { ModuleEntity } from "../models/ModuleEntity";
import type { RowEntity } from "../models/RowEntity";
import { sanitizeGuid } from "../utils/StringHelper";

export class ModuleDataService {

    private _webAPI: ComponentFramework.WebApi;

    private STATUS_ACTIVE = 1;

    constructor(webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
    }


    /**
     * Retrieve all active kt_module records, excluding any already in RowEntity[] based on middleLabelText
     */
    async getActiveModules(): Promise<ModuleEntity[]> {
        try {
            const query = `?$select=kt_moduleid,kt_name,kt_modulelabel,kt_moduledescription,statuscode&$filter=statuscode eq ${this.STATUS_ACTIVE}`;
            const result = await this._webAPI.retrieveMultipleRecords("kt_module", query);

            return (result.entities || []).map(module => ({
                id: module.kt_moduleid,
                moduleName: module.kt_name,
                moduleLabel: module.kt_modulelabel,
                moduleDescription: module.kt_moduledescription,
                statusCode: module.statuscode
            }));
        } catch (error) {
            console.error("getActiveModules - failed to fetch active modules:", error);
            return [];
        }
    }

    async getModulesByIds(moduleIds: string[]): Promise<ModuleEntity[]> {
        if (!moduleIds.length) return [];

        // Convert GUIDs to lowercase + remove duplicates
        const uniqueIds = [...new Set(moduleIds.map(id => sanitizeGuid(id)))];

        // Build OData filter
        const filter = uniqueIds.map(id => `kt_moduleid eq ${id}`).join(" or ");
        const query = `?$select=kt_moduleid,kt_name,kt_modulelabel,kt_moduledescription,statuscode&$filter=${filter}`;

        try {
            const result = await this._webAPI.retrieveMultipleRecords("kt_module", query);
            return (result.entities || []).map(module => ({
                id: module.kt_moduleid,
                moduleName: module.kt_name,
                moduleLabel: module.kt_modulelabel,
                moduleDescription: module.kt_moduledescription,
                statusCode: module.statuscode
            }));
        } catch (error) {
            console.error("getModulesByIds - failed to fetch modules:", error);
            return [];
        }
    }
}