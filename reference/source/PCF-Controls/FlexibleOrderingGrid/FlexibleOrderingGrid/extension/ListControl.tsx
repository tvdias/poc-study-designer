import * as React from "react";
import { Link } from "@fluentui/react/lib/Link";
import { Label } from "@fluentui/react/lib/Label";
import { ScrollablePane, ScrollbarVisibility } from "@fluentui/react/lib/ScrollablePane";
import { DetailsList, IColumn, DetailsListLayoutMode, ConstrainMode, Selection, SelectionMode, IDetailsHeaderProps, IDragDropEvents } from "@fluentui/react/lib/DetailsList";
import { getTheme, mergeStyles } from "@fluentui/react/lib/Styling";
import { IRenderFunction } from "@fluentui/react/lib/Utilities";
import { Sticky, StickyPositionType } from "@fluentui/react/lib/Sticky";
import { CommandBar, ICommandBarItemProps } from "@fluentui/react/lib/CommandBar";
import { ThemeProvider } from "@fluentui/react/lib/Theme";
import { initializeIcons } from "@fluentui/react/lib/Icons";
import { Spinner } from "@fluentui/react";

// Initialize icons in case this example uses them
initializeIcons();

//#region Style Constants

const theme = getTheme();
const dragEnterClass = mergeStyles({
    backgroundColor: theme.palette.neutralLight,
});

//#endregion

//#region Interfaces
export interface IListControlProps {
    data: IListData[];
    columns: IListColumn[];
    orderColumn?: IListColumn;
    totalResultCount: number;
    allocatedWidth: number;
    enableAutoSave: boolean;
    isSaving: boolean;
    isReadOnly: boolean;
    triggerNavigate?: (id: string) => void;
    triggerPaging?: (pageCommand: string) => void;
    triggerSelection?: (selectedKeys: any[]) => void;
    triggerUpdate?: (records: any[]) => void;
}

export interface IListData {
    attribute: string;
    value: string;
}

export interface IListColumn extends IColumn {
    dataType?: string;
    isPrimary?: boolean;
}

export interface IListControlState extends React.ComponentState {}
//#endregion

//#region Utils
function stripHtml(html: string) {
    const doc = new DOMParser().parseFromString(html, "text/html");
    return doc.body.textContent || "";
}
//#endregion

export class ListControl extends React.Component<IListControlProps, IListControlState> {
    //#region Global Variables
    private _selection: Selection;
    private _totalWidth: number;
    private _dragDropEvents: IDragDropEvents;
    private _draggedIndex: number;
    private _draggedItem: IListData | undefined;
    //#endregion

    private _cmdBarFarItems: ICommandBarItemProps[];
    private _cmdBarItems: ICommandBarItemProps[];
    private _totalRecords: number;

    constructor(props: IListControlProps) {
        super(props);

        this._totalWidth = this._totalColumnWidth(props.columns);
        this._totalWidth = this._totalWidth > props.allocatedWidth ? this._totalWidth : props.allocatedWidth;
        this._dragDropEvents = this._getDragDropEvents(props.isReadOnly);
        this._totalRecords = props.totalResultCount;

        this.state = {
            _items: props.data,
            _columns: this._buildColumns(props.columns),
            _triggerNavigate: props.triggerNavigate,
            _triggerSelection: props.triggerSelection,
            _triggerPaging: props.triggerPaging,
            _triggerUpdate: props.triggerUpdate,
            _selectionCount: 0,
            _isSaving: props.isSaving,
            _unsavedChanged: false,
        };

        this._selection = new Selection({
            onSelectionChanged: () => {
                this.setState({
                    _selectionCount: this._setSelectionDetails(),
                });
            },
        });

        this._cmdBarItems = this.renderCommandBarItems(this.props.enableAutoSave);
        this._cmdBarFarItems = this.renderCommandBarFarItem(props.data.length);
    }

    public componentWillUpdate(nextProps: IListControlProps, nextState: IListControlState) {
        if(!nextProps.isReadOnly) {
            var isReadOnly = nextState._isSaving;
            this._dragDropEvents = this._getDragDropEvents(isReadOnly);
        }
    }

    public componentWillReceiveProps(newProps: IListControlProps): void {
        this.setState({
            _items: newProps.data,
            _columns: this._buildColumns(newProps.columns),
            _isSaving: newProps.isSaving,
        });
        this._totalWidth = this._totalColumnWidth(newProps.columns);
        this._cmdBarItems = this.renderCommandBarItems(this.props.enableAutoSave);
        this._cmdBarFarItems = this.renderCommandBarFarItem(newProps.data.length);
    }

    //#region Private functions
    private _onRenderDetailsHeader = (props: IDetailsHeaderProps | undefined, defaultRender?: IRenderFunction<IDetailsHeaderProps>): JSX.Element => {
        return (
            <Sticky stickyPosition={StickyPositionType.Header} isScrollSynced={true}>
                {defaultRender!({ ...props! })}
            </Sticky>
        );
    };

    private _onRenderDetailsFooter = (): JSX.Element => {
        let unsavedChangedStatusRow: any;
        
        if (this.state._unsavedChanged) {
            unsavedChangedStatusRow = <Label className="footerLabel footerUnsavedChanges">{"Unsaved changes"}</Label>;
        } else {
            unsavedChangedStatusRow = "";
        }
        return (
            <Sticky stickyPosition={StickyPositionType.Footer} isScrollSynced={true}>
                <div className={"footer"}>
                    {/* <Label className={"footerLabel"}>{`${this.state._selectionCount} selected`}</Label> */}
                    {unsavedChangedStatusRow}
                    <CommandBar className={"footerCmdBar"} farItems={this._cmdBarFarItems} items={this._cmdBarItems} />
                </div>
            </Sticky>
        );
    };


    private _setSelectionDetails(): number {
        let selectedKeys = [];
        let selections = this._selection.getSelection();
        for (let selection of selections) {
            selectedKeys.push(selection.key as string);
        }

        this.state._triggerSelection(selectedKeys);

        switch (selectedKeys.length) {
            case 0:
                return 0;
            default:
                return selectedKeys.length;
        }
    }

    private _sort = <T,>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] => {
        let key = columnKey as keyof T;
        return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
    };

    private renderCommandBarItems(isAutoSaveEnabled: boolean): ICommandBarItemProps[] {
        if (isAutoSaveEnabled) {
            return [];
        } else {
            return [
                {
                    key: "save",
                    text: "Save",
                    iconProps: { iconName: "Save" },
                    onClick: () => {
                        if (this.state._triggerUpdate) {
                            this.setState({ _isSaving: true, _unsavedChanged: false });
                            this.state._triggerUpdate(this.state._items);
                        }
                    },
                },
            ];
        }
    }

    private renderCommandBarFarItem(recordsLoaded: number): ICommandBarItemProps[] {
        return [
            {
                key: "next",
                text: recordsLoaded == this._totalRecords ? `${recordsLoaded} of ${this._totalRecords}` : `Load more (${recordsLoaded} of ${this._totalRecords})`,
                ariaLabel: "Next",
                iconProps: { iconName: "ChevronRight" },
                disabled: recordsLoaded == this._totalRecords,
                onClick: () => {
                    if (this.state._triggerPaging) {
                        this.state._triggerPaging("next");
                    }
                },
            },
        ];
    }

    private _onItemInvoked(item: any): void {
        this.state._triggerNavigate(item.key);
    }

     private _buildColumns(listData: IListColumn[]): IColumn[] {
        let iColumns: IColumn[] = [];
   
        const TARGET_COLUMN_INDEX = 1;
   
        for (let i = 0; i < listData.length; i++) {
            const column = listData[i];
            const isTargetColumn = i === TARGET_COLUMN_INDEX;

            if (!column.name?.trim()) {
                continue;
            }
           
            let iColumn: IColumn = {
                key: column.key,
                name: column.name,
                fieldName: column.fieldName,
                currentWidth: column.currentWidth,
                minWidth: column.minWidth,
                maxWidth: column.maxWidth,
                isResizable: column.isResizable,
                sortAscendingAriaLabel: column.sortAscendingAriaLabel,
                sortDescendingAriaLabel: column.sortDescendingAriaLabel,
                className: column.className,
                headerClassName: column.headerClassName,
                data: column.data,
                isSorted: column.isSorted,
                isSortedDescending: column.isSortedDescending,
            };
   
            // PRIORITY 1: Make column at index 2 (first column of subgrid) clickable
            if (isTargetColumn) {
                const originalDataType = column.dataType;
               
                iColumn.onRender = (item: any, index: number | undefined, column: IColumn | undefined) => {
   
                    // Use the column's fieldName to get the data
                    let fieldValue: any = null;
                    if (column?.fieldName) {
                        fieldValue = item[column.fieldName];
                    }
                    
                    let displayText: string;
                    if (typeof fieldValue === 'object' && fieldValue?.text) {
                        displayText = fieldValue.text;
                    } else {
                        displayText = fieldValue?.toString() || "";
                    }
                   
                    if (originalDataType === "Text") {
                        displayText = stripHtml(displayText);
                    }
                   
                    return (
                        <Link
                            key={item.key}
                            onClick={() => {
                                this.state._triggerNavigate?.(item.key);
                            }}
                        >
                            {displayText}
                        </Link>
                    );
                };
            }
           
            // PRIORITY 2: Email columns
            else if (column.dataType === "Email" && column.fieldName) {
                const fieldName = column.fieldName;
                iColumn.onRender = (item) => (
                    <Link href={`mailto:${item[fieldName]}`}>
                        {item[fieldName]}
                    </Link>
                );
            }

            // PRIORITY 3 Phone columns
            else if (column.dataType === "Phone" && column.fieldName) {
                const fieldName = column.fieldName;
                iColumn.onRender = (item) => (
                    <Link href={`skype:${item[fieldName]}?call`}>
                        {item[fieldName]}
                    </Link>
                );
            }
            // PRIORITY 4: Text columns
            else if (column.dataType === "Text" && column.fieldName) {
                const fieldName = column.fieldName;
                iColumn.onRender = (item) => {
                    const plainText = stripHtml(item[fieldName] || "");
                    return <span title={plainText}>{plainText}</span>;
                };
            }
            else {
                iColumn.onRender = (item) => <span>{item[column.fieldName || '']}</span>;
            }
            iColumns.push(iColumn);
        }
        return iColumns;
    }

    private _totalColumnWidth(listData: IListColumn[]): number {
        let totalColumnWidth: number;

        totalColumnWidth = listData.map((v) => v.maxWidth!).reduce((sum, current) => sum + current);

        // Add extra buffer
        return totalColumnWidth + 100;
    }

    private _insertBeforeItem(item: IListData): void {
        const draggedItems = this._selection.isIndexSelected(this._draggedIndex) ? (this._selection.getSelection() as IListData[]) : [this._draggedItem!];

        const insertIndex = this.state._items.indexOf(item);
        const items: IListData[] = this.state._items.filter((item: IListData) => draggedItems.indexOf(item) === -1);

        items.splice(insertIndex, 0, ...draggedItems);

        this.setState({ _items: items });

        if (this.props.enableAutoSave) {
            this.setState({ _isSaving: true });
            if (this.state._triggerUpdate) {
                this.state._triggerUpdate(items);
            }
        } else {
            this.setState({ _unsavedChanged: true });
        }
    }

    private _getDragDropEvents(isReadOnly: boolean): IDragDropEvents {
        return {
            canDrop: () => {
                return true;
            },
            canDrag: () => {
                return !isReadOnly;
            },
            onDragEnter: () => {
                // return string is the css classes that will be added to the entering element.
                return dragEnterClass;
            },
            onDragLeave: () => {
                return;
            },
            onDrop: (item?: any) => {
                if (this._draggedItem) {
                    this._insertBeforeItem(item);
                }
            },
            onDragStart: (item?: any, itemIndex?: number) => {
                this._draggedItem = item;
                this._draggedIndex = itemIndex!;
            },
            onDragEnd: () => {
                this._draggedItem = undefined;
                this._draggedIndex = -1;
            },
        };
    }
    //#endregion

    //#region Main Render Function
    public render() {

        return (
            <ThemeProvider>
                <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                    <DetailsList
                        setKey="parentcustomerid"
                        items={this.state._items}
                        columns={this.state._columns}
                        //onColumnHeaderClick={this._onColumnClick}
                        layoutMode={DetailsListLayoutMode.justified}
                        constrainMode={ConstrainMode.unconstrained}
                        onItemInvoked={this._onItemInvoked}
                        dragDropEvents={this._dragDropEvents}
                        //selection={this._selection}
                        //selectionPreservedOnEmptyClick={true}
                        selectionMode={SelectionMode.none}
                        onRenderDetailsHeader={this._onRenderDetailsHeader}
                        onRenderDetailsFooter={this._onRenderDetailsFooter}
                        ariaLabelForSelectionColumn="Toggle selection"
                        ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                        checkButtonAriaLabel="Row checkbox"
                    />
                    
                    {this.state._isSaving && (
                    <div className="savingOverlay">
                        <Spinner className="footerSave" label="Saving changes..." ariaLive="assertive" labelPosition="left"/>
                    </div>
                    )}
                </ScrollablePane>
            </ThemeProvider>
        );
    }
    //#endregion
}
