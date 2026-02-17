import { ColumnItem } from "../models/ColumnItem";
import { DropdownItem } from "../models/DropdownItem";
import { JunctionItem } from "../models/JunctionItem";
import { JunctionToSave } from "../models/JunctionToSave";
import { RowItem } from "../models/RowItem";
import { EntityMappingsHelper } from "../utils/EntityMappingsHelper";
import { CustomApiService } from "./CustomApiService";
import { DataService } from "./DataService";

export class MatrixService {

    private _dataService: DataService;
    private _customApiService: CustomApiService;

    constructor(dataService: DataService, customApiService: CustomApiService) {
        this._dataService = dataService;
        this._customApiService = customApiService;
    }

    public async getMatrixDataSet(
        context: ComponentFramework.Context<any>
    ): Promise<JunctionItem[]>
    {
        const matrixDataSetQuery = EntityMappingsHelper.getMatrixDataSetQuery(context);

        const records = await this._dataService.getRecords(
            matrixDataSetQuery.entityName,
            matrixDataSetQuery.fetchXml
        );

        const junctionItems: JunctionItem[] = records.map(r => ({
            id: r.id ?? '',
            rowId: r.rowId ?? '',
            columnId: r.columnId ?? '',
            dropdownValueToFilter: r.dropdownValueToFilter ?? ''
        }));

        return junctionItems;
    }

    public async getDropdownItems(
        context: ComponentFramework.Context<any>
    ): Promise<DropdownItem[]>
    {
        const dropdownFilterQuery = EntityMappingsHelper.getDropdownFilterQuery(context);

        const records = await this._dataService.getRecords(
            dropdownFilterQuery.entityName,
            dropdownFilterQuery.fetchXml
        );

        const latestRecords = new Map<string, any>();

        records.forEach(r => {
            const key = r.masterid ?? r.id;
            const currentVersion = r.version ?? 0;

            if (!latestRecords.has(key) || currentVersion > (latestRecords.get(key).version ?? 0))
            {
                latestRecords.set(key, r);
            }
        });

        const dropdownItems: DropdownItem[] = Array.from(latestRecords.values()).map(r => {
            const id = r.id;
            const name = r.name;
            const version = r.version ?? '';
            const isReadOnly = EntityMappingsHelper.mapDropdownIsReadonly(context, r);

            return {
                id: id,
                name: version ? `${name} (v${version})` : name,
                isReadOnly: isReadOnly
            };
        });

        return dropdownItems;
    }

    public async getRowItems(context: ComponentFramework.Context<any>): Promise<RowItem[]>
    {
        const rowsQuery = EntityMappingsHelper.getRowItemsQuery(context);

        const records = await this._dataService.getRecords(
            rowsQuery.entityName,
            rowsQuery.fetchXml
        );

        const rowItems: RowItem[] = records.map(r => ({
            id: r.id ?? '',
            name: r.name ?? '',
            sortOrder: r.sortOrder ?? 0,
            dropdownValueToFilter: r.dropdownValueToFilter ?? ''
        }));

        return rowItems;
    }

    public async getColumnItems(context: ComponentFramework.Context<any>): Promise<ColumnItem[]>
    {
        const columnsQuery = EntityMappingsHelper.getColumnItemsQuery(context);

        const records = await this._dataService.getRecords(
            columnsQuery.entityName,
            columnsQuery.fetchXml
        );

        const columnItems: ColumnItem[] = records.map(r => ({
            id: r.id ?? '',
            name: r.name ?? '',
            disabled: false,
            dropdownValueToFilter: r.dropdownValueToFilter ?? ''
        }));

        return columnItems;
    }

    public async saveJunctionItems(
        context: ComponentFramework.Context<any>,
        junctionItemsToSave: JunctionToSave[],
        dropdownValueSelected: string
    ): Promise<void>
    {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';
        
        const junctionsQuery = EntityMappingsHelper.getJunctionItemsQuery(context, dropdownValueSelected);

        const existingJunctionRecords = await this._dataService.getRecords(
            junctionsQuery.entityName,
            junctionsQuery.fetchXml
        );

        // Delete existing junction records
        if (existingJunctionRecords.length > 0) {
            await this._dataService.deleteRecordsBatch(
                junctionEntityName,
                existingJunctionRecords.map(item => item.id));

            console.log(`Deleted ${existingJunctionRecords.length} existing records`);
        }
        
        // Create new junction records
        if (junctionItemsToSave.length > 0) {
            const recordsToCreate = EntityMappingsHelper.mapCreateJunctionRecords(
                context,
                junctionItemsToSave,
                dropdownValueSelected
            );

            await this._dataService.createRecordsBatch(
                junctionEntityName,
                recordsToCreate);

            console.log(`Created ${junctionItemsToSave.length} new records`);
        }
        
        // ************* Last steps, depending on Entity **************
        await EntityMappingsHelper.callCustomApi(context, dropdownValueSelected, this._customApiService);
    }
}