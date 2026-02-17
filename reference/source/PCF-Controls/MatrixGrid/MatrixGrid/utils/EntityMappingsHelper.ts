import { JunctionItem } from "../models/JunctionItem";
import DataSetInterfaces = ComponentFramework.PropertyHelper.DataSetApi;
import { StudyQueryConstants } from "../services/constants/StudyQueryConstants";
import { EntityNamesConstants } from "./constants/EntityNamesConstants";
import { ManagedlistEntityQueryConstants } from "../services/constants/ManagedlistEntityQueryConstants";
import { QuestionnairelinesQueryConstants } from "../services/constants/QuestionnairelinesQueryConstants";
import { QuestionnairelineManagedlistEntityQueryConstants } from "../services/constants/QuestionnairelineManagedlistEntityQueryConstants";
import { JunctionToSave } from "../models/JunctionToSave";
import { CustomApiService } from "../services/CustomApiService";

export class EntityMappingsHelper {

    static mapJunctionItem = (
        junctionEntityName: string,
        record: DataSetInterfaces.EntityRecord
    ): JunctionItem => {

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity:
            default:
                return {
                    id: record.getRecordId(),
                    rowId: (record.getValue('ktr_managedlistentity') as ComponentFramework.EntityReference)?.id?.guid ?? "",
                    columnId: (record.getValue('ktr_questionnaireline') as ComponentFramework.EntityReference)?.id?.guid ?? "",
                    dropdownValueToFilter: (record.getValue('ktr_studyid') as ComponentFramework.EntityReference)?.id?.guid ?? "",
                };
        }
    };

    static getRowsLabel = (context: ComponentFramework.Context<any>): string => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity:
                return "Managed List Entities";
            default:
                return "Rows";
        }
    }

    static getColumnsLabel = (context: ComponentFramework.Context<any>): string => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity:
                return "Questions";
            default:
                return "Columns";
        }
    }

    static getMatrixDataSetQuery = (context: ComponentFramework.Context<any>): { entityName: string, fetchXml: string } => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const qlsMlsEntitiesXml = QuestionnairelineManagedlistEntityQueryConstants.GET_QUESTIONNAIRELINEMANAGEDLISTENTITIES_BY_MANAGEDLIST
                    .replace("{MANAGED_LIST_ID}", managedListId);

                return { entityName: EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity, fetchXml: qlsMlsEntitiesXml };
            }
            default:
                return { entityName: '', fetchXml: '' };
        }
    }

    static getDropdownFilterQuery = (context: ComponentFramework.Context<any>): { entityName: string, fetchXml: string } => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const studyByManagedListFetchXml = StudyQueryConstants.GET_STUDIES_BY_MANAGED_LIST
                    .replace("{MANAGED_LIST_ID}", managedListId);

                return { entityName: EntityNamesConstants.ktr_Study, fetchXml: studyByManagedListFetchXml };
            }
            default:
                return { entityName: '', fetchXml: '' };
        }
    }

    static getRowItemsQuery = (context: ComponentFramework.Context<any>): { entityName: string, fetchXml: string } => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const fetchXml = ManagedlistEntityQueryConstants.GET_MANAGEDLISTENTITIES_BY_STUDY_MANAGEDLIST
                    .replace("{MANAGED_LIST_ID}", managedListId);

                return { entityName: EntityNamesConstants.ktr_StudyManagedlistEntity, fetchXml: fetchXml };
            }
            default:
                return { entityName: '', fetchXml: '' };
        }
    }

    static getColumnItemsQuery = (context: ComponentFramework.Context<any>): { entityName: string, fetchXml: string } => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const fetchXml = QuestionnairelinesQueryConstants.GET_QUESTIONNAIRELINES_BY_STUDY_MANAGEDLIST
                    .replace("{MANAGED_LIST_ID}", managedListId);

                return { entityName: EntityNamesConstants.ktr_StudyQuestionnaireline, fetchXml: fetchXml };
            }
            default:
                return { entityName: '', fetchXml: '' };
        }
    }

    static getJunctionItemsQuery = (context: ComponentFramework.Context<any>, dropdownValueSelected: string): { entityName: string, fetchXml: string } => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const fetchXml = QuestionnairelineManagedlistEntityQueryConstants.GET_QUESTIONNAIRELINEMANAGEDLISTENTITIES_BY_STUDY_MANAGEDLIST
                    .replace("{MANAGED_LIST_ID}", managedListId)
                    .replace("{STUDY_ID}", dropdownValueSelected);

                return { entityName: EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity, fetchXml: fetchXml };
            }
            default:
                return { entityName: '', fetchXml: '' };
        }
    }

    static mapCreateJunctionRecords = (
        context: ComponentFramework.Context<any>,
        junctionsItemsToSave: JunctionToSave[],
        dropdownValueSelected: string
    ): any[] => {
        const junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {
                const managedListId = this.getEntityIdFromUrl('ktr_managedlist');

                const recordsToCreate = junctionsItemsToSave.map(item => ({
                    "ktr_name": `${item.columnName} - ${item.rowName}`,
                    "ktr_ManagedListEntity@odata.bind": `/ktr_managedlistentities(${item.rowId})`,
                    "ktr_QuestionnaireLine@odata.bind": `/kt_questionnairelineses(${item.columnId})`,
                    "ktr_StudyId@odata.bind": `/kt_studies(${dropdownValueSelected})`,
                    "ktr_ManagedList@odata.bind": `/ktr_managedlists(${managedListId})`
                }));

                return recordsToCreate;
            }
            default:
                return [];
        }
    }

    static mapDropdownIsReadonly = (
        context: ComponentFramework.Context<any>,
        dropdownRecordFromDb: any,
    ): boolean => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {

                return dropdownRecordFromDb.statuscode !== 1; // 1 = 'Draft'
            }
            default:
                return false;
        }
    }

    static callCustomApi = async (
        context: ComponentFramework.Context<any>,
        dropdownValueSelected: string,
        customApiService: CustomApiService
    ): Promise<void> => {
        let junctionEntityName = context.parameters.junctionEntityName.raw || '';

        switch (junctionEntityName) {
            case EntityNamesConstants.ktr_QuestionnairelineManagedlistEntity: {

                await customApiService.createOrDetectSubsetAPI(dropdownValueSelected, context);
                console.log("Successfully executed 'createOrDetectSubsetAPI' Custom API");

                break;
            }
            default: {
                console.warn(`No custom API defined for junction entity: ${junctionEntityName}`);
                break;
            }
        }
    }

    /**
     * Extracts the record ID of a given entity from the current page URL or Xrm.Page context.
     * @param entityName Logical name of the entity (e.g., 'ktr_managedlist')
     * @returns The GUID of the entity record, or empty string if not found
     */
    private static getEntityIdFromUrl(entityName: string): string {
        try {
            // Try URL first
            const urlParams = new URLSearchParams(window.location.search);
            const etn = urlParams.get('etn'); // entity logical name
            const id = urlParams.get('id');   // record GUID

            if (etn === entityName && id) {
                return id;
            }

            console.warn(`Entity ID for '${entityName}' not found in URL or page context`);
            return '';
        } catch (err) {
            console.error(`Error fetching ID for entity '${entityName}':`, err);
            return '';
        }
    }
}