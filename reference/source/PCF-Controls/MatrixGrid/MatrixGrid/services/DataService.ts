export class DataService {

    private _webAPI: ComponentFramework.WebApi;

    constructor(webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
    }

    public async getRecords(
        entityName: string,
        fetchXml: string
    ): Promise<any[]> {
        
        try {
            const response = await this._webAPI.retrieveMultipleRecords(entityName, `?fetchXml=${encodeURIComponent(fetchXml)}`);
            return response.entities;
        } catch (error) {
            console.error(`Error fetching records for entity ${entityName}:`, error);
            return [];
        }
    }

    public async createRecordsBatch(entityName: string, records: Record<string, any>[]): Promise<void> {
        try {
            const createPromises = records.map(r => 
                this._webAPI.createRecord(entityName, r));

            await Promise.all(createPromises);
            
            console.log(`Created ${records.length} ${entityName} records`);
        } catch (error) {
            console.error(`Error batch creating ${entityName}:`, error);
            throw error;
        }
    }

    public async deleteRecordsBatch(entityName: string, ids: string[]): Promise<void> {
        try {
            const deletePromises = ids.map(id =>
                this._webAPI.deleteRecord(entityName, id));

            await Promise.all(deletePromises);

            console.log(`Deleted ${ids.length} ${entityName} records`);
        } catch (error) {
            console.error(`Error batch deleting ${entityName}:`, error);
            throw error;
        }
    }
}