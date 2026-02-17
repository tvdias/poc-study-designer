import { IInputs, IOutputs } from "./generated/ManifestTypes";
import DataSetInterfaces = ComponentFramework.PropertyHelper.DataSetApi;
import * as ReactDOM from "react-dom";
import { GridContainer } from "./components/GridContainer";
import * as React from "react";
import { RowEntity } from "./models/RowEntity";
import { DataService } from "./services/DataService";
import { QuestionType } from "./types/QuestionType";
import { stripHtml } from "./utils/StringHelper";

type DataSet = ComponentFramework.PropertyTypes.DataSet;

export class AccordionGrid implements ComponentFramework.StandardControl<IInputs, IOutputs> {
    private container: HTMLDivElement;
    private context: ComponentFramework.Context<IInputs>;
    private dataService: DataService;
    private notifyOutputChanged: () => void;
    private _isDebugMode: boolean;

    /**
     * Used to initialize the control instance. Controls can kick off remote server calls and other initialization actions here.
     * Data-set values are not initialized here, use updateView.
     * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to property names defined in the manifest, as well as utility functions.
     * @param notifyOutputChanged A callback method to alert the framework that the control has new outputs ready to be retrieved asynchronously.
     * @param state A piece of data that persists in one session for a single user. Can be set at any point in a controls life cycle by calling 'setControlState' in the Mode interface.
     * @param container If a control is marked control-type='standard', it will receive an empty div element within which it can render its content.
     */
    public init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary,
        container: HTMLDivElement
    ): void {
        this.container = container;
        this.context = context;
        this.notifyOutputChanged = notifyOutputChanged;
        this.dataService = new DataService(context.webAPI);

        const dataSet = context.parameters.gridDataSet;
        let dataItems: RowEntity[] = this.getGridItems(dataSet);

        this._isDebugMode = this.isDebugMode();
        this.HideOutOfTheBoxGridHeader();
        console.log("DATAITEMS ---> ", dataItems);
        // Render the React component
        this.renderReactComponent(dataItems);
    }

    /**
     * Called when any value in the property bag has changed. This includes field values, data-sets, global values such as container height and width, offline status, control metadata values such as label, visible, etc.
     * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to names defined in the manifest, as well as utility functions
     */
    public updateView(context: ComponentFramework.Context<IInputs>): void {
        const dataSet = context.parameters.gridDataSet;
        let dataItems: RowEntity[] = this.getGridItems(dataSet);

        this.renderReactComponent(dataItems);
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

    // Get the items from the dataset
    // TO DO: Hard-coded columns should be passed as parameters to PCF
    private getGridItems = (ds: DataSet) => {
        let dataSet = ds;

        var resultSet = dataSet.sortedRecordIds
            .map(function (key) {
                var record = dataSet.records[key];

                var newRecord: RowEntity = {
                    id: record.getRecordId(),
                    name: stripHtml(record.getFormattedValue('kt_questiontext2') ?? ""),
                    sortOrder: Number(record.getValue('kt_questionsortorder')) ?? 0,
                    firstLabelId: Number(record.getValue('kt_questiontype')) ?? 0,
                    firstLabelText: record.getFormattedValue('kt_questiontype'),
                    middleLabelText: record.getFormattedValue('ktr_module'),
                    lastLabelText: record.getFormattedValue('kt_questionvariablename'),
                    statusCode: Number(record.getValue('statuscode')) ?? 1,
                    projectId: (record.getValue('ktr_project') as any)?.id?.guid ?? "",
                    questionTitle: record.getFormattedValue('kt_questiontitle') ?? "",
                    questionFormatDetail: record.getFormattedValue('ktr_questionformatdetails') ?? "",
                    answerMin: Number(record.getValue('ktr_answermin')),
                    answerMax: Number(record.getValue('ktr_answermax')),
                    isDummy: record.getFormattedValue('ktr_isdummyquestion'),
                    answerList: record.getFormattedValue('ktr_answerlist'),
                    scripterNotes: record.getFormattedValue('ktr_scripternotes'),
                    rowSortOrder: record.getFormattedValue('ktr_rowsortorder'),
                    columnSortOrder: record.getFormattedValue('ktr_columnsortorder'),
                    standardOrCustomId: Number(record.getValue('kt_standardorcustom')),
                    standardOrCustomText: record.getFormattedValue('kt_standardorcustom') ?? "",
                    questionVersion: record.getFormattedValue('ktr_questionversion') ?? "",
                    questionRationale: record.getFormattedValue('ktr_questionrationale') ?? ""
                };

                return newRecord;
            });

        return resultSet;
    };

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

    /**
    * Render the React component
    */
    private renderReactComponent(dataItems: RowEntity[]): void {
        const dummyItems = [
            { id: '1', name: 'Item A', sortOrder: 1, statusCode: 1, firstLabelText: "SingleChoice", firstLabelId: 0, middleLabelText: 'Module A', lastLabelText: 'Intro', projectId: '', questionTitle: "QT1", questionFormatDetail: "QFT1", answerMin: 0, answerMax: 90, isDummy: "True", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "Normal", standardOrCustomId: 1, standardOrCustomText: "Custom", questionVersion: '1', questionRationale: "Test" },
            { id: '2', name: 'Item B', sortOrder: 2, statusCode: 2, firstLabelText: "Standard", firstLabelId: 1, middleLabelText: '', lastLabelText: 'Work_Sector', projectId: '', questionTitle: "QT2", questionFormatDetail: "QFT2", answerMin: 2, answerMax: 10, isDummy: "False", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "", standardOrCustomId: 0, standardOrCustomText: "Standard"  },
            { id: '3', name: 'Item C', sortOrder: 3, statusCode: 1, firstLabelText: "NumericInput", firstLabelId: 1, middleLabelText: '', lastLabelText: 'brand_More', projectId: '', questionTitle: "QT3", questionFormatDetail: "QFT3", answerMin: 0, answerMax: 0, isDummy: "True", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "Normal", standardOrCustomId: 1, standardOrCustomText: "Custom" },
            { id: '4', name: 'Item D', sortOrder: 4, statusCode: 1, firstLabelText: "SingleChoice", firstLabelId: 0, middleLabelText: 'Module B', lastLabelText: 'Intro', projectId: '', questionTitle: "QT1", questionFormatDetail: "QFT1", answerMin: 0, answerMax: 90, isDummy: "True", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "Normal", standardOrCustomId: 1, standardOrCustomText: "Custom" },
            { id: '5', name: 'Item E', sortOrder: 5, statusCode: 1, firstLabelText: "SingleChoice", firstLabelId: 0, middleLabelText: 'Module B', lastLabelText: 'Intro', projectId: '', questionTitle: "QT1", questionFormatDetail: "QFT1", answerMin: 0, answerMax: 90, isDummy: "True", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "Normal", standardOrCustomId: 1, standardOrCustomText: "Custom" },
            { id: '6', name: 'Item F', sortOrder: 6, statusCode: 2, firstLabelText: "SingleChoice", firstLabelId: 0, middleLabelText: 'Module B', lastLabelText: 'Intro', projectId: '', questionTitle: "QT1", questionFormatDetail: "QFT1", answerMin: 0, answerMax: 90, isDummy: "True", answerList: "AnswerList", scripterNotes: "Scripter Notes", rowSortOrder: "Normal", columnSortOrder: "Normal", standardOrCustomId: 1, standardOrCustomText: "Custom" },
        ];

        const props = {
            context: this.context,
            onNotifyOutputChanged: this.notifyOutputChanged,
            dataItems: this._isDebugMode ? dummyItems : dataItems,
            dataService: this.dataService,
            isReadOnly: this.context.mode.isControlDisabled
        };

        ReactDOM.render(
            React.createElement(GridContainer, props),
            this.container
        );
    }

    private isDebugMode() {
        const isDebug = this.context.parameters.gridDataSet.columns.length == 1;

        if (isDebug) {
            console.log("Debug mode (local harness)");
        } else {
            console.log("Published mode (in Power Apps)");
        }
        return isDebug;
    }
}
