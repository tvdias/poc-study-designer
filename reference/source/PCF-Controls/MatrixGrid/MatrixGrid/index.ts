import { IInputs, IOutputs } from "./generated/ManifestTypes";
import DataSetInterfaces = ComponentFramework.PropertyHelper.DataSetApi;
import { JunctionItem } from "./models/JunctionItem";
import * as React from "react";
import { MatrixContainer } from "./components/MatrixContainer";
import { RowItem } from "./models/RowItem";
import { ColumnItem } from "./models/ColumnItem";
import { MatrixContainerProps } from "./models/props/MatrixContainerProps";
import * as ReactDOM from "react-dom";
import { EntityMappingsHelper } from "./utils/EntityMappingsHelper";
import { MatrixService } from "./services/MatrixService";
import { DropdownItem } from "./models/DropdownItem";
import { DataService } from "./services/DataService";
import { CustomApiService } from "./services/CustomApiService";

export class MatrixGrid implements ComponentFramework.StandardControl<IInputs, IOutputs> {
    private container: HTMLDivElement;
    private context: ComponentFramework.Context<IInputs>;
    private notifyOutputChanged: () => void;
    private _isDebugMode: boolean;
    private _matrixService: MatrixService;

    /*
    Dear developer,
        Welcome to the MatrixGrid control!

        This control is designed to be totally Generic and Entity agnostic.

        You just simply need to provide the params:
            * matrixDataSet - drop this PCF to a subGrid. e.g: 'QuestionnaireLine-ManagedListEntity'
                Note: PCF's gridDataSet has a max of 250 records. So we will ignore this and will fetch the data via fetchXML. 
            * junctionEntityName - e.g: 'ktr_questionnairelinemanagedlistentity'
            * rowsEntityName  - e.g: 'ktr_studymanagedlistentity'
            * columnsEntityName - e.g: 'ktr_studyquestionnaireline'
        
        If you need to extend for another Entities, please check the EntityMappingsHelper class.

        Enjoy!
    */

    /**
     * Empty constructor.
     */
    constructor() {
        // Empty
    }

    /**
     * Used to initialize the control instance. Controls can kick off remote server calls and other initialization actions here.
     * Data-set values are not initialized here, use updateView.
     * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to property names defined in the manifest, as well as utility functions.
     * @param notifyOutputChanged A callback method to alert the framework that the control has new outputs ready to be retrieved asynchronously.
     * @param state A piece of data that persists in one session for a single user. Can be set at any point in a controls life cycle by calling 'setControlState' in the Mode interface.
     * @param container If a control is marked control-type='standard', it will receive an empty div element within which it can render its content.
     */
    public async init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary,
        container: HTMLDivElement
    ): Promise<void> {
        this.container = container;
        this.context = context;
        this.notifyOutputChanged = notifyOutputChanged;
        this._isDebugMode = this.isDebugMode();
        this.HideOutOfTheBoxGridHeader();
        
        const dataService = new DataService(this.context.webAPI);
        const customApiService = new CustomApiService(this.context.webAPI);
        this._matrixService = new MatrixService(dataService, customApiService);

        const junctionItems: JunctionItem[] = this._isDebugMode ? [] : await this.getMatrixDataSet();
        console.log('JUNCTION ITEMS ---> ', junctionItems);

        const dropdownItems = this._isDebugMode ? [] : await this.getDropdownItems();
        const rowsItems = this._isDebugMode ? [] : await this.getRowsItems();
        const columnItems = this._isDebugMode ? [] : await this.getColumnItems();

        console.log('DROPDOWN ITEMS ---> ', dropdownItems);
        console.log('ROWS ITEMS ---> ', rowsItems);
        console.log('COLUMNS ITEMS ---> ', columnItems);

        // Render the React component
        this.renderReactComponent(rowsItems, columnItems, junctionItems, dropdownItems);
    }


    /**
     * Called when any value in the property bag has changed. This includes field values, data-sets, global values such as container height and width, offline status, control metadata values such as label, visible, etc.
     * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to names defined in the manifest, as well as utility functions
     */
    public async updateView(context: ComponentFramework.Context<IInputs>): Promise<void> {
        const junctionItems: JunctionItem[] = this._isDebugMode ? [] : await this.getMatrixDataSet();

        const dropdownItems = this._isDebugMode ? [] : await this.getDropdownItems();
        const rowsItems = this._isDebugMode ? [] : await this.getRowsItems();
        const columnItems = this._isDebugMode ? [] : await this.getColumnItems();

        this.renderReactComponent(rowsItems, columnItems, junctionItems, dropdownItems);
    }

    /**
     * It is called by the framework prior to a control receiving new data.
     * @returns an object based on nomenclature defined in manifest, expecting object[s] for property marked as "bound" or "output"
     */
    public getOutputs(): IOutputs {
        return {};
    }

    /**
     * Called when the control is to be removed from the DOM tree. Controls should use this call for cleanup.
     * i.e. cancelling any pending remote calls, removing listeners, etc.
     */
    public destroy(): void {
        // Add code to cleanup control if necessary
    }

    /**
    * Render the React component
    */
    private renderReactComponent(rowsItems: RowItem[], columnItems: ColumnItem[], junctionItems: JunctionItem[], dropdownItems: DropdownItem[]): void {
        const dummyJunctionItems : JunctionItem[] = [
            { id: '123', rowId: '1', columnId: '1', dropdownValueToFilter: '001' },
            { id: '211', rowId: '2', columnId: '2', dropdownValueToFilter: '002' },
            { id: '332', rowId: '3', columnId: '3', dropdownValueToFilter: '003' },
            { id: '433', rowId: '4', columnId: '4', dropdownValueToFilter: '001' },
            { id: '565', rowId: '5', columnId: '5', dropdownValueToFilter: '002' },
            { id: '643', rowId: '6', columnId: '6', dropdownValueToFilter: '003' },
        ];
        const dummyRowItems: RowItem[] = [
            { id: '1', name: 'Entity 1', sortOrder: 1, dropdownValueToFilter: '001' },
            { id: '2', name: 'Entity 2', sortOrder: 2, dropdownValueToFilter: '001' },
            { id: '3', name: 'Entity 3', sortOrder: 3, dropdownValueToFilter: '001' },
            
            { id: '1', name: 'Entity 1', sortOrder: 4, dropdownValueToFilter: '002' },
            { id: '2', name: 'Entity 2', sortOrder: 5, dropdownValueToFilter: '002' },
            { id: '3', name: 'Entity 3', sortOrder: 6, dropdownValueToFilter: '002' },

            { id: '1', name: 'Entity 1', sortOrder: 4, dropdownValueToFilter: '003' },
            { id: '2', name: 'Entity 2', sortOrder: 5, dropdownValueToFilter: '003' },
            { id: '3', name: 'Entity 3', sortOrder: 6, dropdownValueToFilter: '003' },
        ];
        const dummyColumnItems: ColumnItem[] = [
            { id: '1', name: 'Question 1', disabled: false, dropdownValueToFilter: '001' },
            { id: '2', name: 'Question 2', disabled: false, dropdownValueToFilter: '001' },
            { id: '3', name: 'Question 3', disabled: false, dropdownValueToFilter: '001' },
            
            { id: '1', name: 'Question 1', disabled: false, dropdownValueToFilter: '002' },
            { id: '2', name: 'Question 2', disabled: false, dropdownValueToFilter: '002' },
            { id: '3', name: 'Question 3', disabled: false, dropdownValueToFilter: '002' },
            
            { id: '1', name: 'Question 1', disabled: false, dropdownValueToFilter: '003' },
            { id: '2', name: 'Question 2', disabled: false, dropdownValueToFilter: '003' },
            { id: '3', name: 'Question 3', disabled: false, dropdownValueToFilter: '003' },
        ];
        const dummyDropdownItems: DropdownItem[] = [
            { id: '001', name: 'Study 1', isReadOnly: false },
            { id: '002', name: 'Study 2', isReadOnly: true },
            { id: '003', name: 'Study 3', isReadOnly: false },
        ];

        const rowsLabel = EntityMappingsHelper.getRowsLabel(this.context);
        const columnsLabel = EntityMappingsHelper.getColumnsLabel(this.context);

        const props: MatrixContainerProps = {
            context: this.context,
            rowsItems: this._isDebugMode ? dummyRowItems : rowsItems,
            columnItems: this._isDebugMode ? dummyColumnItems : columnItems,
            junctionItems: this._isDebugMode ? dummyJunctionItems : junctionItems,
            rowsLabel: rowsLabel,
            columnsLabel: columnsLabel,
            dropdownItems: this._isDebugMode ? dummyDropdownItems : dropdownItems,
            matrixService: this._matrixService,
            isReadOnly: this.context.mode.isControlDisabled
        };

        console.log('Rendering MatrixContainer with props: ', props);
        ReactDOM.render(
            React.createElement(MatrixContainer, props),
            this.container
        );
    }

    private getMatrixDataSet = async (): Promise<JunctionItem[]> => {
        return await this._matrixService.getMatrixDataSet(this.context);
    };

    private getDropdownItems = async (): Promise<DropdownItem[]> => {
        return await this._matrixService.getDropdownItems(this.context);
    };

    private getRowsItems = async (): Promise<RowItem[]> => {
        return await this._matrixService.getRowItems(this.context);
    };

    private getColumnItems = async (): Promise<ColumnItem[]> => {
        return await this._matrixService.getColumnItems(this.context);
    };

    private isDebugMode(): boolean {
        const rawValue = this.context.parameters.rowsEntityName?.raw ?? "";
        const isDebug = !rawValue.startsWith("kt");

        if (isDebug) {
            console.log("Debug mode (local harness)");
        } else {
            console.log("Published mode (in Power Apps)");
        }
        return isDebug;
    }

    private HideOutOfTheBoxGridHeader() {
        if (!this._isDebugMode) {
            this.container
                ?.parentElement
                ?.parentElement
                ?.parentElement
                ?.parentElement
                ?.parentElement
                ?.parentElement
                ?.firstElementChild
                ?.remove();
        }
    }
}
