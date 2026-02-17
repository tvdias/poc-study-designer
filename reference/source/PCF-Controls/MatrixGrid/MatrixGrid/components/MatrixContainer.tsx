import * as React from "react";
import {
  Button,
  FluentProvider,
  makeStyles,
  MessageBar,
  tokens,
  webLightTheme
} from '@fluentui/react-components';
import { MatrixContainerProps } from "../models/props/MatrixContainerProps";
import { UrlHelpers } from "../utils/UrlHelpers";
import { MatrixUtils } from "../utils/MatrixUtils";
import { MatrixTable } from "./MatrixTable";
import { SearchFiltersHelpers } from "../utils/SearchFilterHelpers";
import { Header } from "./Header";
import { Footer } from "./Footer";

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    height: 'fit-content',
    maxHeight: 'none',
    padding: '16px',
    gap: '16px',
    overflow: 'visible'
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '200px'
  },
  errorContainer: {
    marginBottom: '16px'
  },
  readonlyBanner: {
    marginBottom: '16px'
  },
  diagnosticsContainer: {
    marginBottom: '16px'
  },
  performanceStats: {
    fontSize: '12px',
    color: tokens.colorNeutralForeground2,
    textAlign: 'center',
    padding: '8px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRadius: tokens.borderRadiusSmall
  },
  debugActions: {
    display: 'flex',
    gap: '8px',
    justifyContent: 'center',
    marginBottom: '16px',
    padding: '8px',
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: tokens.borderRadiusSmall,
    border: `1px solid ${tokens.colorNeutralStroke2}`
  },
  dismissButton: {
    minWidth: '24px',
    width: '24px',
    height: '24px',
    padding: '0',
    fontSize: '14px',
    lineHeight: '1'
  },
  messageBarContent: {
    width: '100%'
  },
  bulkSelectionSummary: {
    padding: '8px 16px',
    backgroundColor: tokens.colorNeutralBackground3,
    borderRadius: tokens.borderRadiusSmall,
    marginBottom: '8px',
    fontSize: tokens.fontSizeBase200,
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center'
  },
  bulkSelectionActions: {
    display: 'flex',
    gap: '8px'
  }
});

export const MatrixContainer: React.FC<MatrixContainerProps> = ({
    context,
    rowsItems,
    columnItems,
    junctionItems,
    rowsLabel,
    columnsLabel,
    dropdownItems,
    matrixService,
    isReadOnly
}) => {
    console.log('==> context.parameters', context.parameters);
    const styles = useStyles();

    const [searchRows, setSearchRows] = React.useState("");
    const [searchColumns, setSearchColumns] = React.useState("");
    const [selectedDropdownValue, setSelectedDropdownValue] = React.useState(dropdownItems[0].id);
    const [matrixIsReadOnly, setMatrixIsReadOnly] = React.useState(isReadOnly || dropdownItems[0].isReadOnly);
    
    const filteredRows = SearchFiltersHelpers.filterRowItems(rowsItems, searchRows, selectedDropdownValue);
    const filteredColumns = SearchFiltersHelpers.filterColumnItems(columnItems, searchColumns, selectedDropdownValue);
    
    const [cellStates, setCellStates] = React.useState(() =>
      MatrixUtils.buildCellStates(rowsItems, columnItems, junctionItems, selectedDropdownValue)
    );
    const [pendingCellChanges, setPendingCellChanges] = React.useState(new Set<string>());
    const [isSaving, setIsSaving] = React.useState(false);
    const [successMessage, setSuccessMessage] = React.useState<string | null>(null);

    console.log("-> filteredRows: ", filteredRows);
    console.log("-> filteredColumns: ", filteredColumns);
    console.log("-> junctionItems: ", junctionItems);
    
    React.useEffect(() => {
      console.log("Dataset changed -> recomputing cellStates");

      const newStates = MatrixUtils.buildCellStates(
        rowsItems,
        columnItems,
        junctionItems,
        selectedDropdownValue
      );

      setCellStates(newStates);
      setPendingCellChanges(new Set());
    }, [rowsItems, columnItems, junctionItems]);

    const loadedRowsCount = filteredRows.length;
    const loadedColumnsCount = filteredColumns.length;

    const getCellInteractable = React.useCallback((columnId: string): boolean => {
        const column = columnItems
            .find(c => c.id === columnId);

        if (!column) return false; // Column not found

        // Only allowed columns are interactable
        return !column.disabled;
    }, [columnItems]);

    const handleRowClick = React.useCallback((rowId: string): void => {
        UrlHelpers.navigateToEntityForm(rowId, context, UrlHelpers.getMainEntityName(context.parameters.rowsEntityName.raw || ''));
    }, [context]);

    const handleColumnClick = React.useCallback((columnId: string): void => {
        UrlHelpers.navigateToEntityForm(columnId, context, UrlHelpers.getMainEntityName(context.parameters.columnsEntityName.raw || ''));
    }, [context]);

    const handleCellToggle = React.useCallback((rowId: string, columnId: string) => {
      const cellKey = MatrixUtils.generateCellKey(rowId, columnId);

      // update CellStates
      setCellStates(prevStates => {
          const newStates = new Map(prevStates);
          const prevCell = newStates.get(cellKey);

          if (prevCell) {
              newStates.set(cellKey, {
                  ...prevCell,
                  isChecked: !prevCell.isChecked,
                  isModified: true,
                  hasConflict: false,
                  isInteractable: getCellInteractable(columnId)
              });
          }

          console.log("-> Updated cellStates: ", newStates);
          return newStates;
      });

      // update PendingCellChanges
      setPendingCellChanges(prev => {
        const newPendingChanges = new Set(prev);

        if (newPendingChanges.has(cellKey)) {
            // If the cell was already pending, remove it (toggle off)
            newPendingChanges.delete(cellKey);
        } else {
            // Otherwise, add it
            newPendingChanges.add(cellKey);
        }

        return newPendingChanges;
      });
    }, [getCellInteractable]);

  
    const handleRowToggleAll = React.useCallback((rowId: string): void => {
      const rowCells = filteredColumns
        .map(column => {
          const cellKey = MatrixUtils.generateCellKey(rowId, column.id);
          return {
            column,
            cellKey,
            cellState: cellStates.get(cellKey)
          };
        })
        .filter(item => !!item.cellState)
        .filter(item => getCellInteractable(item.column.id));

      if (rowCells.length === 0) {
        return;
      }

 
  const shouldCheck = rowCells.some(item => !item.cellState?.isChecked);
      const cellsToToggle = rowCells.filter(item => item.cellState?.isChecked !== shouldCheck);

      if (cellsToToggle.length === 0) {
        return;
      }

      
      setCellStates(prevStates => {
        const newStates = new Map(prevStates);

        cellsToToggle.forEach(({ cellKey, column }) => {
          const prevCell = newStates.get(cellKey);
          if (prevCell) {
            newStates.set(cellKey, {
              ...prevCell,
              isChecked: shouldCheck,
              isModified: true,
              hasConflict: false,
              isInteractable: getCellInteractable(column.id)
            });
          }
        });

        console.log("-> Updated cellStates (row toggle): ", newStates);
        return newStates;
      });

      
      setPendingCellChanges(prev => {
        const newPendingChanges = new Set(prev);

        cellsToToggle.forEach(({ cellKey }) => {
          if (newPendingChanges.has(cellKey)) {
            newPendingChanges.delete(cellKey);
          } else {
            newPendingChanges.add(cellKey);
          }
        });

        return newPendingChanges;
      });
    }, [cellStates, filteredColumns, getCellInteractable]);

    function handleRowFilterChange(text: string) {
        setSearchRows(text);
    }

    function handleColumnFilterChange(text: string) {
        setSearchColumns(text);
    }

    function handleDropdownChange(dropdownValue: string) {
        const newCellStates = MatrixUtils.buildCellStates(
          rowsItems,
          columnItems,
          junctionItems,
          dropdownValue
        );

        handleCancel();
        setSelectedDropdownValue(dropdownValue);
        setCellStates(newCellStates);
        setPendingCellChanges(new Set());

        console.log("---> isReadonly", isReadOnly);
        if(!isReadOnly) {
          var makeMatrixReadOnly = dropdownItems.find(di => di.id === dropdownValue)?.isReadOnly;
          console.log("Setting matrix read-only state to: ", makeMatrixReadOnly);
          setMatrixIsReadOnly(makeMatrixReadOnly || false);
        }
    }

    async function handleSave(): Promise<void> {
      setSuccessMessage(null);
      setIsSaving(true);

      try {
        
         // Step 1: get sets of valid rowIds and columnIds for the selected dropdown
        const validRowIds = new Set(
          rowsItems
            .filter(r => r.dropdownValueToFilter === selectedDropdownValue)
            .map(r => r.id)
        );

        const validColumnIds = new Set(
          columnItems
            .filter(c => c.dropdownValueToFilter === selectedDropdownValue)
            .map(c => c.id)
        );

        // Step 2: build junctionItemsToSave from cellStates
        const junctionItemsToSave = Array.from(cellStates.entries())
          .filter(([cellKey, cell]) =>
            cell.isChecked &&
            validRowIds.has(cell.rowId) &&
            validColumnIds.has(cell.columnId)
          )
          .map(([cellKey, cell]) => ({
            rowId: cell.rowId,
            columnId: cell.columnId,
            rowName: cell.rowName,
            columnName: cell.columnName
          }));

        console.log("-> Junction items to save: ", junctionItemsToSave);

        await matrixService.saveJunctionItems(
          context,
          junctionItemsToSave,
          selectedDropdownValue);

        const changeCount = pendingCellChanges.size;
        setSuccessMessage(`Successfully saved ${changeCount} changes`);

        setPendingCellChanges(new Set());

        refreshMatrixData();

        // Auto-dismiss success message after 3 seconds
        setTimeout(() => setSuccessMessage(null), 3000);
        
      } catch(error) {
        console.error("Error saving changes: ", error);
      } finally {
        setIsSaving(false);
      }
    }

    function handleCancel(): void {
      console.log("CANCELING CHANGES");

      // update CellStates
      setCellStates(prevStates => {
          const newStates = new Map(prevStates);

          pendingCellChanges.forEach(cellKey => {
            const cell = newStates.get(cellKey);

            if (cell) {
              newStates.set(cellKey, {
                ...cell,
                isChecked: !cell.isChecked,
                isModified: false,
                hasConflict: false
              });
            }
          });

          console.log("-> Updated cellStates: ", newStates);
          return newStates;
      });

      // update PendingCellChanges
      setPendingCellChanges(new Set());

      refreshMatrixData();
    }

    function refreshMatrixData(): void {
        context.parameters.matrixDataSet.refresh();
    }

    return (
    <FluentProvider theme={webLightTheme}>
        <div className={styles.container}>

          {successMessage && (
            <div className={styles.errorContainer}>
              <MessageBar intent="success">
                <div className={styles.messageBarContent}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <div style={{ flex: 1 }}>
                      {successMessage}
                    </div>
                    <Button
                      size="small"
                      appearance="subtle"
                      className={styles.dismissButton}
                      onClick={() => setSuccessMessage(null)}
                      aria-label="Dismiss success message"
                    >
                      ×
                    </Button>
                  </div>
                </div>
              </MessageBar>
            </div>
          )}

          <Header
            onRowFilterChange={handleRowFilterChange}
            onColumnFilterChange={handleColumnFilterChange}
            disabled={false}
            rowsLabel={rowsLabel}
            columnsLabel={columnsLabel}
            loadedRowsCount={loadedRowsCount}
            loadedColumnsCount={loadedColumnsCount}
            onDropdownFilterChange={handleDropdownChange}
            dropdownItems={dropdownItems}
            selectedDropdownValue={selectedDropdownValue}
            onRefresh={refreshMatrixData}
          />
          
          <MatrixTable
                rows={filteredRows}
                columns={filteredColumns}
                cellStates={cellStates}
                onCellToggle={handleCellToggle}
        onRowToggleAll={handleRowToggleAll}
                onRowClick={handleRowClick}
                onColumnClick={handleColumnClick}
                disabled={isSaving || matrixIsReadOnly}

                //bulkSelection={state.bulkSelection}
                //onBulkRowToggle={handleBulkRowToggle}
                //onBulkSelectAll={handleBulkSelectAll}
                //onBulkClearAll={handleBulkClearAll}
                //bulkTooltipText={config.bulkSelectionTooltip || "Select rows with Draft studies for bulk operations"}
            />

            <Footer
              pendingChangesCount={pendingCellChanges.size}
              isSaving={isSaving}
              onSave={handleSave}
              onCancel={handleCancel}
              disabled={pendingCellChanges.size === 0 || matrixIsReadOnly}
            />
        </div>
    </FluentProvider>
    );
};
