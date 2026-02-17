import { RowEntity } from "../models/RowEntity";

export class DataService {

    private _webAPI: ComponentFramework.WebApi;

    private STATE_INACTIVE = 1;
    private STATUS_INACTIVE = 2;
    private STATE_ACTIVE = 0;
    private STATUS_ACTIVE = 1;

    constructor(webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
    }

    async inactivateRecord(
        entityName: string,
        id: string
    ) {
        try {
            await this._webAPI.updateRecord(entityName, id, {
                statecode: this.STATE_INACTIVE,
                statuscode: this.STATUS_INACTIVE,
            });

            console.log(`Record ${id} inactivated successfully`);
            return { success: true };
        } catch (error) {
            console.error("Error inactivating record: ", error);
            return { success: false };
        }
    }

    async reactivateRecord(
        entityName: string,
        id: string
    ) {
        try {
            await this._webAPI.updateRecord(entityName, id, {
                statecode: this.STATE_ACTIVE,
                statuscode: this.STATUS_ACTIVE,
            });

            console.log(`Record ${id} reactivated successfully`);
            return { success: true };
        } catch (error) {
            console.error("Error reactivating record: ", error);
            return { success: false };
        }
    }

    async saveOrder(
        entityName: string,
        orderField: string,
        rows: RowEntity[]): Promise<void>
    {
        try {
            await Promise.all(
                rows.map((row) =>
                    this._webAPI.updateRecord(entityName, row.id, {
                        [orderField]: row.sortOrder,
                    })
                )
            );
        } catch (err) {
            console.error("Error saving order:", err);
        }
    }
}