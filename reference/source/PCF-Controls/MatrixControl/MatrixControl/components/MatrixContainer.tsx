/**
 * This code provides the ability to render and manage a comprehensive matrix interface for handling entity 
 * assignments between rows and columns with Dataverse entities. It manages data loading, cell state tracking, 
 * bulk selection operations, save/cancel functionality, and user permissions while supporting advanced features like version chains and performance diagnostics.
 */
import * as React from 'react';
import {
  FluentProvider,
  webLightTheme,
  makeStyles,
  Spinner,
  MessageBar,
  MessageBarIntent,
  tokens,
  Button,
  Tooltip
} from '@fluentui/react-components';

import {
  MatrixConfig,
  MatrixState,
  CellState,
  RowEntity,
  ColumnEntity,
  JunctionRecord,
  BatchOperation,
  MatrixContainerProps,
  CellKey,
  SaveStatus,
  BulkSelectionState,
  StudyChain,
  STUDY_STATUS,
  StudyStatus
} from '../types/MatrixTypes';

import { DataService } from '../services/DataService';
import { DataServiceConfig, PerformanceStats } from '../types/DataServiceTypes';
import { ErrorHandler } from '../utils/ErrorHandler';
import { PerformanceTracker } from '../utils/PerformanceTracker';

import { SearchFilters } from './SearchFilters';
import { MatrixTable } from './MatrixTable';
import { ActionBar } from './ActionBar';
import { MatrixUtils } from '../utils/MatrixUtils';
import { VersionChainProcessor } from '../utils/VersionChainProcessor';

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
  config,
  dataService,
  context,
  onNotifyOutputChanged,
  parentRecordId
}) => {
  const styles = useStyles();

  const [state, setState] = React.useState<MatrixState>({
    allRows: [],
    allColumns: [],
    loadedRows: [],
    loadedColumns: [],
    cellStates: new Map(),

    loadedRowRange: { start: 0, end: 20 },
    loadedColumnRange: { start: 0, end: 20 },
    totalAvailableRows: 0,
    totalAvailableColumns: 0,
    isLoadingMoreRows: false,
    isLoadingMoreColumns: false,

    filteredRows: [],
    filteredColumns: [],
    rowFilter: '',
    columnFilter: '',
    hasSearchExpanded: false,

    pendingChanges: new Set(),
    isLoading: true,
    isSaving: false,
    error: null,
    conflicts: [],

    visibleColumns: [],
    studyChains: [],
    bulkSelection: {
      selectedRowIds: new Set<string>(),
      isAllRowsSelected: false
    }
  });

  const [canEdit, setCanEdit] = React.useState<boolean>(true);
  const [performanceStats, setPerformanceStats] = React.useState<PerformanceStats | null>(null);
  const [diagnosticsResults, setDiagnosticsResults] = React.useState<any>(null);
  const [showDiagnostics, setShowDiagnostics] = React.useState<boolean>(false);
  const [successMessage, setSuccessMessage] = React.useState<string | null>(null);
  const [rawColumnsProcessedCount, setRawColumnsProcessedCount] = React.useState<number>(0);
  const isDebugMode = config.debugMode || false;

  const dataServiceConfig: DataServiceConfig = React.useMemo(() => ({
    debugMode: isDebugMode,
    enablePerformanceTracking: true,
    cacheTTL: 30000,
    maxRetries: 3,
    maxCacheSize: 1000
  }), [isDebugMode]);

  const hasVersionChainFeatures = React.useMemo(() => {
    return !!(config.columnParentAttrField || config.columnVersionField);
  }, [config.columnParentAttrField, config.columnVersionField]);

  const getEffectiveColumns = React.useCallback((): ColumnEntity[] => {
    return state.visibleColumns && state.visibleColumns.length > 0
      ? state.visibleColumns
      : state.loadedColumns;
  }, [state.visibleColumns, state.loadedColumns]);

  // Helper to get Draft status columns (modifiable studies)
  const getDraftColumns = React.useCallback((): ColumnEntity[] => {
    return getEffectiveColumns().filter(column => {
      if (!hasVersionChainFeatures || !column.statuscode) {
        return true; // Default to modifiable if no status information
      }
      return column.statuscode === STUDY_STATUS.DRAFT;
    });
  }, [getEffectiveColumns, hasVersionChainFeatures]);

  // Helper to check if a row can be modified (has at least one Draft study available)
  const canRowBeModified = React.useCallback((rowId: string): boolean => {
    const draftColumns = getDraftColumns();
    return draftColumns.length > 0;
  }, [getDraftColumns]);

  // Get all modifiable rows (rows that have at least one Draft study available)
  const getModifiableRows = React.useCallback((): RowEntity[] => {
    return state.filteredRows.filter(row => canRowBeModified(row.id));
  }, [state.filteredRows, canRowBeModified]);

  const getCellInteractable = React.useCallback((columnId: string): boolean => {
    // Always check if this specific column is Draft status
    const column = getEffectiveColumns().find(c => c.id === columnId);

    if (!column) return false; // Column not found

    // If no status information, assume it's interactable (backward compatibility)
    if (!column.statuscode) return true;

    // Only Draft studies are interactable
    return column.statuscode === STUDY_STATUS.DRAFT;
  }, [getEffectiveColumns]);

  // Helper: immediately apply bulk selection to Draft cells in the given rows
  const applyBulkToRows = React.useCallback((targetRowIds: string[], targetChecked: boolean) => {
    setState(prev => {
      const draftColumns = getDraftColumns();
      if (draftColumns.length === 0 || targetRowIds.length === 0) return prev;

      const updatedCellStates = new Map(prev.cellStates);
      const newPending = new Set(prev.pendingChanges);

      for (const rowId of targetRowIds) {
        for (const col of draftColumns) {
          const key = MatrixUtils.generateCellKey(rowId, col.id);
          const cur = updatedCellStates.get(key);
          if (!cur) continue;

          // honor per-column interactivity (e.g., status)
          const interactable = getCellInteractable(col.id);
          if (!interactable) continue;

          if (cur.isAssigned !== targetChecked) {
            updatedCellStates.set(key, {
              ...cur,
              isAssigned: targetChecked,
              isModified: true,
              isInteractable: interactable
            });
            newPending.add(key);
          }
        }
      }

      return { ...prev, cellStates: updatedCellStates, pendingChanges: newPending };
    });
  }, [getDraftColumns, getCellInteractable]);


  const getColumnStatus = React.useCallback((columnId: string): StudyStatus | undefined => {
    // Always try to get status, regardless of version chain features
    const column = getEffectiveColumns().find(c => c.id === columnId);
    return column?.statuscode;
  }, [getEffectiveColumns]);

  const getColumnHeaderStyle = React.useCallback((columnId: string): string => {
    const status = getColumnStatus(columnId);
    if (typeof status === 'undefined') {
      return 'column-header-draft';
    }

    return VersionChainProcessor.getStatusStyle(status);
  }, [getColumnStatus]);

  // Updated bulk selection handlers for Draft-only selection
  // UPDATED: Fixed bulk selection handlers that work with individual row toggles
  const handleBulkRowToggle = React.useCallback((rowId: string): void => {
    // Check if ANY draft studies exist in view (not row-specific)
    if (getDraftColumns().length === 0) {
      console.warn(`No Draft studies available for bulk selection`);
      return;
    }

    // Toggle the row selection
    const willBeChecked = !(state.bulkSelection?.selectedRowIds || new Set<string>()).has(rowId);

    setState(prev => {
      const newSelectedRowIds = new Set(prev.bulkSelection?.selectedRowIds || []);
      if (newSelectedRowIds.has(rowId)) newSelectedRowIds.delete(rowId);
      else newSelectedRowIds.add(rowId);

      return {
        ...prev,
        bulkSelection: {
          selectedRowIds: newSelectedRowIds,
          isAllRowsSelected: false // Update this based on the logic
        }
      };
    });

    // Apply to cells
    applyBulkToRows([rowId], willBeChecked);
  }, [getDraftColumns, applyBulkToRows, state.bulkSelection]);


  const handleBulkSelectAll = React.useCallback((): void => {
    const modifiableRows = getModifiableRows();
    const modifiableRowIds = modifiableRows.map(r => r.id);

    setState(prev => ({
      ...prev,
      bulkSelection: { selectedRowIds: new Set(modifiableRowIds), isAllRowsSelected: true }
    }));

    // immediately apply to cells
    applyBulkToRows(modifiableRowIds, true);
  }, [getModifiableRows, applyBulkToRows]);


  const handleBulkClearAll = React.useCallback((): void => {
    const modifiableRows = getModifiableRows();
    const modifiableRowIds = modifiableRows.map(r => r.id);

    setState(prev => ({
      ...prev,
      bulkSelection: { selectedRowIds: new Set<string>(), isAllRowsSelected: false }
    }));

    // immediately clear cells
    applyBulkToRows(modifiableRowIds, false);
  }, [getModifiableRows, applyBulkToRows]);


  // UPDATED: Enhanced bulk operation handler (optional - )
  const handleApplyBulkSelection = React.useCallback(async (): Promise<void> => {
    if (!state.bulkSelection || state.bulkSelection.selectedRowIds.size === 0) {
      setState(prev => ({
        ...prev,
        error: 'No rows selected for bulk operation'
      }));
      return;
    }

    try {
      setState(prev => ({ ...prev, isSaving: true, error: null }));

      const selectedRowIds = Array.from(state.bulkSelection.selectedRowIds);
      const draftColumns = getDraftColumns();

      console.log(`Applying bulk selection: ${selectedRowIds.length} rows × ${draftColumns.length} Draft studies`);

      // Create new cell states with all Draft studies assigned for selected rows
      const updatedCellStates = new Map(state.cellStates);
      const newPendingChanges = new Set(state.pendingChanges);

      selectedRowIds.forEach(rowId => {
        draftColumns.forEach(column => {
          const cellKey = MatrixUtils.generateCellKey(rowId, column.id);
          const currentCell = updatedCellStates.get(cellKey);

          if (currentCell && !currentCell.isAssigned) {
            // Only assign if not already assigned
            const updatedCell: CellState = {
              ...currentCell,
              isAssigned: true,
              isModified: true,
              isInteractable: getCellInteractable(column.id)
            };

            updatedCellStates.set(cellKey, updatedCell);
            newPendingChanges.add(cellKey);
          }
        });
      });

      const changesCount = newPendingChanges.size - state.pendingChanges.size;

      setState(prev => ({
        ...prev,
        cellStates: updatedCellStates,
        pendingChanges: newPendingChanges,
        isSaving: false,
        bulkSelection: {
          selectedRowIds: new Set<string>(),
          isAllRowsSelected: false
        }
      }));

      console.log(`Bulk operation completed: ${changesCount} new assignments created`);

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Bulk operation failed:', userMessage);
      setState(prev => ({
        ...prev,
        isSaving: false,
        error: `Bulk operation failed: ${userMessage}`
      }));
    }
  }, [state.bulkSelection, state.cellStates, state.pendingChanges, getDraftColumns, getCellInteractable]);

  const BulkSelectionSummary: React.FC = () => {
    if (!state.bulkSelection || state.bulkSelection.selectedRowIds.size === 0) {
      return null;
    }

    const selectedCount = state.bulkSelection.selectedRowIds.size;
    const modifiableRowsCount = getModifiableRows().length;
    const draftColumnsCount = getDraftColumns().length;
    const potentialAssignments = selectedCount * draftColumnsCount;

    return (
      <div className={styles.bulkSelectionSummary}>
        <div>
          <strong>{selectedCount}</strong> of <strong>{modifiableRowsCount}</strong> rows selected
          {state.bulkSelection.isAllRowsSelected && ' (All modifiable rows)'}
          <br />
          <span style={{ fontSize: '12px', color: '#666' }}>
            Up to {potentialAssignments} assignments possible ({draftColumnsCount} Draft studies per row)
          </span>
        </div>
        <div className={styles.bulkSelectionActions}>
          <Button
            size="small"
            appearance="primary"
            onClick={handleApplyBulkSelection}
            disabled={state.isSaving || !canEdit}
          >
            Assign Draft Studies
          </Button>
          <Button
            size="small"
            appearance="subtle"
            onClick={handleBulkSelectAll}
            disabled={state.isSaving || !canEdit}
          >
            Select All
          </Button>
          <Button
            size="small"
            appearance="subtle"
            onClick={handleBulkClearAll}
            disabled={state.isSaving || !canEdit}
          >
            Clear Selection
          </Button>
        </div>
      </div>
    );
  };

  React.useEffect(() => {
    console.log('Starting Enhanced Dataverse Integration');
    console.log('Parent Record ID:', parentRecordId);
    console.log('MatrixConfig:', {
      junctionRowField: config.junctionRowField,
      junctionColumnField: config.junctionColumnField,
      junctionEntityName: config.junctionEntityName,
      debugMode: isDebugMode,
      hasVersionChainFeatures
    });

    loadMatrixData();
  }, [config, parentRecordId]);

  React.useEffect(() => {
    const effectiveColumns = state.visibleColumns && state.visibleColumns.length > 0
      ? state.visibleColumns
      : state.loadedColumns;

    const filteredRows = MatrixUtils.filterEntities(state.loadedRows, state.rowFilter);
    const filteredColumns = MatrixUtils.filterEntities(effectiveColumns, state.columnFilter);

    setState(prev => ({
      ...prev,
      filteredRows,
      filteredColumns
    }));
  }, [state.loadedRows, state.loadedColumns, state.visibleColumns, state.rowFilter, state.columnFilter]);

  React.useEffect(() => {
    if (!state.isLoading && !state.isSaving && dataService && isDebugMode) {
      updatePerformanceStats();
    }
  }, [state.isLoading, state.isSaving, isDebugMode]);

  const forceStyleRefresh = React.useCallback(() => {
    const firstHeaders = document.querySelectorAll('.fui-Table .fui-TableHeaderCell:first-child');
    firstHeaders.forEach(header => {
      const element = header as HTMLElement;
      element.style.position = 'sticky';
      element.style.left = '0px';
      element.style.top = '0px';
      element.style.zIndex = '1000';
      element.style.backgroundColor = tokens.colorNeutralBackground2;
      console.log('Force-applied sticky styles to header:', element);
    });
  }, []);

  // Call this after the table renders
  React.useEffect(() => {
    if (!state.isLoading && state.loadedRows.length > 0) {
      setTimeout(forceStyleRefresh, 100);
    }
  }, [state.isLoading, state.loadedRows.length, forceStyleRefresh]);

  const updatePerformanceStats = React.useCallback(() => {
    if (dataService) {
      try {
        const stats = dataService.getServiceStats();
        setPerformanceStats(stats.performance);

        if (isDebugMode && stats.performance.calls > 0) {
          console.log('DataService Performance Update:', stats);
        }
      } catch (error) {
        console.warn('Failed to get performance stats:', error);
      }
    }
  }, [dataService, isDebugMode]);

  const loadMatrixData = async (): Promise<void> => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));

      console.log('Loading matrix data from Dataverse with enhanced DataService...');
      console.log('Configuration:', {
        rowEntity: config.rowEntityName,
        columnEntity: config.columnEntityName,
        junctionEntity: config.junctionEntityName,
        parentRecordId: parentRecordId,
        debugMode: isDebugMode,
        hasVersionChainFeatures
      });

      const result = await dataService.loadInitialMatrixData(config, parentRecordId);

      const { rows, columns, rawColumns, junctions, totalRowCount, totalColumnCount, canEdit: userCanEdit, rawColumnsProcessed } = result;

      setRawColumnsProcessedCount(rawColumnsProcessed);

      if (hasVersionChainFeatures) {
        console.log('Version chain processing enabled:');
        console.log(`   Raw columns: ${columns.length}`);
        console.log(`   Configuration: parentAttr=${config.columnParentAttrField}, version=${config.columnVersionField}`);
      }

      console.log('Enhanced DataService loaded successfully:');
      console.log(`   Matrix size: ${rows.length}×${columns.length} loaded`);
      console.log(`   Total available: ${totalRowCount} rows, ${totalColumnCount} columns`);
      console.log(`   User permissions: ${userCanEdit ? 'Can edit' : 'Read-only'}`);
      console.log(`   Form context filtering: ${parentRecordId ? 'Applied' : 'None'}`);
      console.log('   Navigation property cache: Initialized');

      const cellStatesMap = MatrixUtils.buildCellStatesMap(rows, columns, junctions);

      if (hasVersionChainFeatures) {
        cellStatesMap.forEach((cellState, cellKey) => {
          const columnId = cellKey.split('-')[1];
          cellState.isInteractable = getCellInteractable(columnId);
        });
      }

      const sortedColumns = columns.sort((a: ColumnEntity, b: ColumnEntity) => {
        const dateA = a.createdDate || new Date(0);
        const dateB = b.createdDate || new Date(0);
        return dateB.getTime() - dateA.getTime();
      });

      console.log(`Initial columns sorted: ${columns.length} columns ordered by creation date (newest first)`);
      setState(prev => ({
        ...prev,
        allRows: rows,
        allColumns: rawColumns,
        loadedRows: rows,

        loadedColumns: sortedColumns,
        cellStates: cellStatesMap,
        totalAvailableRows: totalRowCount,
        totalAvailableColumns: totalColumnCount,
        loadedRowRange: { start: 0, end: rows.length },
        loadedColumnRange: { start: 0, end: sortedColumns.length },

        visibleColumns: hasVersionChainFeatures ? sortedColumns : [],
        studyChains: [],

        isLoading: false
      }));

      setCanEdit(userCanEdit);
      updatePerformanceStats();

      if (isDebugMode && rows.length > 0 && columns.length > 0 && junctions.length === 0) {
        console.warn('No junction records found. Running diagnostics...');
        setTimeout(() => runFieldMappingDiagnostics(), 1000);
      }

    } catch (error: any) {
      console.error('Error loading matrix data:', error);

      const handledError = ErrorHandler.handleDataverseError(error);
      const userFriendlyMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      setState(prev => ({
        ...prev,
        isLoading: false,
        error: userFriendlyMessage
      }));

      if (ErrorHandler.isPermissionError(handledError)) {
        setCanEdit(false);
        console.warn('Permission error detected - user access restricted');
      }

      if (handledError.message.toLowerCase().includes('schema') ||
        handledError.message.toLowerCase().includes('field') ||
        handledError.message.toLowerCase().includes('junction')) {
        setShowDiagnostics(true);
      }
    }
  };

  const loadMoreRows = async (): Promise<void> => {
    try {
      setState(prev => ({ ...prev, isLoadingMoreRows: true }));

      console.log(`Loading more rows: current=${state.loadedRows.length}, parent=${parentRecordId}`);

      const newRows = await dataService.loadMoreRows(config, state.loadedRows.length, 10, parentRecordId);

      if (newRows.length === 0) {
        console.log('No more rows available');
        setState(prev => ({ ...prev, isLoadingMoreRows: false }));
        return;
      }

      setState(prev => {
        const updatedLoadedRows = [...prev.loadedRows, ...newRows];
        const updatedAllRows = [...prev.allRows, ...newRows];

        console.log(`Loaded ${newRows.length} more rows. Total: ${updatedLoadedRows.length}/${prev.totalAvailableRows}`);

        dataService.loadJunctionRecordsForEntities(config, newRows, prev.loadedColumns).then((newJunctions: JunctionRecord[]) => {
          setState(prevState => {
            const allJunctions = [...MatrixUtils.extractJunctionsFromCellStates(prevState.cellStates), ...newJunctions];
            const newCellStates = MatrixUtils.buildCellStatesMap(updatedLoadedRows, prevState.loadedColumns, allJunctions);

            return {
              ...prevState,
              cellStates: newCellStates
            };
          });
        }).catch((error: any) => {
          const handledError = ErrorHandler.handleDataverseError(error);
          console.error('Failed to load junction records for new rows:', ErrorHandler.getUserFriendlyMessage(handledError));
        });

        return {
          ...prev,
          allRows: updatedAllRows,
          loadedRows: updatedLoadedRows,
          loadedRowRange: { ...prev.loadedRowRange, end: updatedLoadedRows.length },
          isLoadingMoreRows: false
        };
      });

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Error loading more rows:', userMessage);
      setState(prev => ({
        ...prev,
        isLoadingMoreRows: false,
        error: `Failed to load more rows: ${userMessage}`
      }));
    }
  };

  const loadMoreColumns = async (): Promise<void> => {
    try {
      setState(prev => ({ ...prev, isLoadingMoreColumns: true }));

      console.log(`Loading more columns: visible=${getEffectiveColumns().length}, rawProcessed=${rawColumnsProcessedCount}`);

      const newRawColumnsResult = await dataService.loadEntitiesRaw(
        config.columnEntityName,
        config.columnIdField,
        config.columnDisplayField,
        config.columnParentField,
        rawColumnsProcessedCount, // Use raw count for database pagination
        10,
        parentRecordId,
        'columns'
      );

      if (newRawColumnsResult.rawEntities.length === 0) {
        console.log('No more columns available');
        setState(prev => ({ ...prev, isLoadingMoreColumns: false }));
        return;
      }

      console.log(`Received ${newRawColumnsResult.rawEntities.length} new raw columns from database`);

      // Combine ALL raw columns (existing + new)
      const allRawColumns = [...state.allColumns, ...newRawColumnsResult.rawEntities];

      console.log(`Combined raw columns: existing=${state.allColumns.length} + new=${newRawColumnsResult.rawEntities.length} = ${allRawColumns.length}`);

      // Process ALL raw columns together to get complete visible set
      const { visibleColumns: allVisibleColumns } = VersionChainProcessor.processColumns(allRawColumns, config);

      console.log(`Version processing: ${allRawColumns.length} total raw → ${allVisibleColumns.length} total visible`);

      // Sort all visible columns by creation date (newest first)
      const sortedAllVisibleColumns = allVisibleColumns.sort((a: ColumnEntity, b: ColumnEntity) => {
        const dateA = a.createdDate || new Date(0);
        const dateB = b.createdDate || new Date(0);
        return dateB.getTime() - dateA.getTime();
      });

      // Update raw processed count with actual new raw entities fetched
      setRawColumnsProcessedCount(prev => prev + newRawColumnsResult.rawEntities.length);

      // Load junction records for ALL visible columns (not just new ones)
      const newJunctions = await dataService.loadJunctionRecordsForEntities(config, state.loadedRows, sortedAllVisibleColumns);

      setState(prev => {
        const existingJunctions = MatrixUtils.extractJunctionsFromCellStates(prev.cellStates);
        const allJunctions = [...existingJunctions, ...newJunctions];
        const newCellStates = MatrixUtils.buildCellStatesMap(prev.loadedRows, sortedAllVisibleColumns, allJunctions);

        // Apply interactability to all cell states
        sortedAllVisibleColumns.forEach(column => {
          prev.loadedRows.forEach(row => {
            const cellKey = MatrixUtils.generateCellKey(row.id, column.id);
            const cellState = newCellStates.get(cellKey);
            if (cellState) {
              cellState.isInteractable = getCellInteractable(column.id);
            }
          });
        });

        const filteredColumns = MatrixUtils.filterEntities(sortedAllVisibleColumns, prev.columnFilter);

        return {
          ...prev,
          // Store ALL raw columns and ALL visible columns
          allColumns: allRawColumns,  // All raw columns processed so far
          loadedColumns: sortedAllVisibleColumns,  // All visible columns
          visibleColumns: sortedAllVisibleColumns, // Same as loadedColumns for version chain mode
          cellStates: newCellStates,
          filteredColumns: filteredColumns,
          loadedColumnRange: {
            ...prev.loadedColumnRange,
            end: sortedAllVisibleColumns.length
          },
          isLoadingMoreColumns: false
        };
      });

      console.log(`Load more completed: ${allRawColumns.length} raw, ${sortedAllVisibleColumns.length} visible`);

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Error loading more columns:', userMessage);
      setState(prev => ({
        ...prev,
        isLoadingMoreColumns: false,
        error: `Failed to load more columns: ${userMessage}`
      }));
    }
  };

  const loadAllRows = async (): Promise<void> => {
    try {
      setState(prev => ({ ...prev, isLoadingMoreRows: true }));

      console.log(`Loading all remaining rows: current=${state.loadedRows.length}, parent=${parentRecordId}`);

      const allRemainingRows = await dataService.loadAllRows(config, state.loadedRows.length, parentRecordId);

      setState(prev => {
        const updatedLoadedRows = [...prev.loadedRows, ...allRemainingRows];
        console.log(`Loaded all rows: ${updatedLoadedRows.length}/${prev.totalAvailableRows}`);

        dataService.loadJunctionRecordsForEntities(config, allRemainingRows, prev.loadedColumns).then((newJunctions: JunctionRecord[]) => {
          setState(prevState => {
            const allJunctions = [...MatrixUtils.extractJunctionsFromCellStates(prevState.cellStates), ...newJunctions];
            const newCellStates = MatrixUtils.buildCellStatesMap(updatedLoadedRows, prevState.loadedColumns, allJunctions);

            return {
              ...prevState,
              cellStates: newCellStates
            };
          });
        }).catch((error: any) => {
          const handledError = ErrorHandler.handleDataverseError(error);
          console.error('Failed to load junction records for all rows:', ErrorHandler.getUserFriendlyMessage(handledError));
        });

        return {
          ...prev,
          allRows: updatedLoadedRows,
          loadedRows: updatedLoadedRows,
          loadedRowRange: { start: 0, end: updatedLoadedRows.length },
          isLoadingMoreRows: false
        };
      });

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Error loading all rows:', userMessage);
      setState(prev => ({
        ...prev,
        isLoadingMoreRows: false,
        error: `Failed to load all rows: ${userMessage}`
      }));
    }
  };

  const loadAllColumns = async (): Promise<void> => {
    try {
      setState(prev => ({ ...prev, isLoadingMoreColumns: true }));

      console.log(`Loading all remaining columns: current visible=${getEffectiveColumns().length}, rawProcessed=${rawColumnsProcessedCount}`);

      // Load all remaining raw columns in batches
      const allRemainingRawColumns: ColumnEntity[] = [];
      let currentSkip = rawColumnsProcessedCount;
      const batchSize = 50;

      // eslint-disable-next-line no-constant-condition
      while (true) {
        const batchResult = await dataService.loadEntitiesRaw(
          config.columnEntityName,
          config.columnIdField,
          config.columnDisplayField,
          config.columnParentField,
          currentSkip,
          batchSize,
          parentRecordId,
          'columns'
        );

        if (batchResult.rawEntities.length === 0) {
          break; // No more entities to load
        }

        allRemainingRawColumns.push(...batchResult.rawEntities);
        currentSkip += batchResult.rawEntities.length;

        if (batchResult.rawEntities.length < batchSize) {
          break;
        }

        console.log(`Loaded batch: ${batchResult.rawEntities.length} raw columns, total so far: ${allRemainingRawColumns.length}`);
      }

      // Combine with existing raw columns
      const allRawColumns = [...state.allColumns, ...allRemainingRawColumns];

      // Process all raw columns together
      const { visibleColumns } = VersionChainProcessor.processColumns(allRawColumns, config);

      // Sort visible columns by creation date (newest first)  
      const sortedVisibleColumns = visibleColumns.sort((a: ColumnEntity, b: ColumnEntity) => {
        const dateA = a.createdDate || new Date(0);
        const dateB = b.createdDate || new Date(0);
        return dateB.getTime() - dateA.getTime();
      });

      console.log(`Version chain processing: ${allRawColumns.length} raw → ${sortedVisibleColumns.length} visible (sorted)`);

      // Update raw processed count
      setRawColumnsProcessedCount(prev => prev + allRemainingRawColumns.length);

      const newJunctions = await dataService.loadJunctionRecordsForEntities(config, state.loadedRows, sortedVisibleColumns);

      setState(prev => {
        const existingJunctions = MatrixUtils.extractJunctionsFromCellStates(prev.cellStates);
        const allJunctions = [...existingJunctions, ...newJunctions];
        const newCellStates = MatrixUtils.buildCellStatesMap(prev.loadedRows, sortedVisibleColumns, allJunctions);

        sortedVisibleColumns.forEach(column => {
          prev.loadedRows.forEach(row => {
            const cellKey = MatrixUtils.generateCellKey(row.id, column.id);
            const cellState = newCellStates.get(cellKey);
            if (cellState) {
              cellState.isInteractable = getCellInteractable(column.id);
            }
          });
        });

        const filteredColumns = MatrixUtils.filterEntities(sortedVisibleColumns, prev.columnFilter);

        console.log(`Loaded all columns: ${allRawColumns.length} raw, ${sortedVisibleColumns.length} visible (sorted)`);

        return {
          ...prev,
          allColumns: allRawColumns,
          loadedColumns: sortedVisibleColumns,
          visibleColumns: sortedVisibleColumns,
          cellStates: newCellStates,
          filteredColumns: filteredColumns,
          loadedColumnRange: { start: 0, end: sortedVisibleColumns.length },
          isLoadingMoreColumns: false
        };
      });

      console.log(`Load all completed: ${allRemainingRawColumns.length} new raw columns loaded`);

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Error loading all columns:', userMessage);
      setState(prev => ({
        ...prev,
        isLoadingMoreColumns: false,
        error: `Failed to load all columns: ${userMessage}`
      }));
    }
  };

  const handleCellToggle = React.useCallback((rowId: string, columnId: string): void => {
    if (!canEdit) {
      console.warn('User does not have permission to edit junction records');
      setState(prev => ({
        ...prev,
        error: 'You do not have permission to edit junction records. Contact your administrator.'
      }));
      return;
    }

    if (!getCellInteractable(columnId)) {
      const status = getColumnStatus(columnId);
      console.warn(`Cell ${rowId}-${columnId} is not interactable due to status: ${status || 'unknown'}`);

      setState(prev => ({
        ...prev,
        error: 'Cannot edit assignments for studies that are not in Draft status.'
      }));
      return;
    }

    const cellKey = MatrixUtils.generateCellKey(rowId, columnId);

    setState(prev => {
      const newCellStates = new Map(prev.cellStates);
      const newPendingChanges = new Set(prev.pendingChanges);

      const currentCell = newCellStates.get(cellKey);
      if (!currentCell) {
        console.warn(`Cell not found: ${cellKey}`);
        return prev;
      }

      const updatedCell: CellState = {
        ...currentCell,
        isAssigned: !currentCell.isAssigned,
        isModified: true,
        hasConflict: false,
        isInteractable: getCellInteractable(columnId)
      };

      newCellStates.set(cellKey, updatedCell);
      newPendingChanges.add(cellKey);

      console.log(`Toggled cell ${cellKey}: ${currentCell.isAssigned} → ${updatedCell.isAssigned}`);

      return {
        ...prev,
        cellStates: newCellStates,
        pendingChanges: newPendingChanges,
        error: null
      };
    });
  }, [canEdit, getCellInteractable, getColumnStatus]);

  const handleRowClick = React.useCallback((rowId: string): void => {
    try {
      const entityName = config.rowEntityName;
      context.navigation.openForm({
        entityName: entityName,
        entityId: rowId
      });
      console.log(`Navigating to ${entityName} record: ${rowId}`);
    } catch (error) {
      console.error('Error navigating to row record:', error);
      const fallbackUrl = `/main.aspx?etn=${config.rowEntityName}&id=${rowId}&pagetype=entityrecord`;
      window.open(fallbackUrl, '_blank');
      console.log(`Fallback navigation to: ${fallbackUrl}`);
    }
  }, [config.rowEntityName, context.navigation]);

  const handleColumnClick = React.useCallback((columnId: string): void => {
    try {
      const entityName = config.columnEntityName;
      context.navigation.openForm({
        entityName: entityName,
        entityId: columnId
      });
      console.log(`Navigating to ${entityName} record: ${columnId}`);
    } catch (error) {
      console.error('Error navigating to column record:', error);
      const fallbackUrl = `/main.aspx?etn=${config.columnEntityName}&id=${columnId}&pagetype=entityrecord`;
      window.open(fallbackUrl, '_blank');
      console.log(`Fallback navigation to: ${fallbackUrl}`);
    }
  }, [config.columnEntityName, context.navigation]);

  const handleSave = async (): Promise<void> => {
    try {
      setSuccessMessage(null);
      setState(prev => ({ ...prev, isSaving: true, error: null }));

      console.log(`Saving ${state.pendingChanges.size} changes using Enhanced DataService...`);

      const batchOperation = MatrixUtils.buildBatchOperation(
        state.cellStates,
        state.pendingChanges,
        config
      );

      console.log('Batch operation details:', {
        creates: batchOperation.creates.length,
        deletes: batchOperation.deletes.length,
        updates: batchOperation.updates.length
      });

      console.log('Batch operation details:', {
        creates: batchOperation.creates,
        deletes: batchOperation.deletes,
        updates: batchOperation.updates
      });

      const result = await dataService.executeBatchSave(config, batchOperation);

      if (result.success) {
        setState(prev => {
          const newCellStates = new Map(prev.cellStates);

          prev.pendingChanges.forEach(cellKey => {
            const cell = newCellStates.get(cellKey);
            if (cell) {
              newCellStates.set(cellKey, {
                ...cell,
                isModified: false,
                hasConflict: false
              });
            }
          });

          return {
            ...prev,
            cellStates: newCellStates,
            pendingChanges: new Set(),
            isSaving: false,
            conflicts: []
          };
        });

        console.log('Save completed successfully with enhanced DataService');
        updatePerformanceStats();

        onNotifyOutputChanged();

        // Reload data to reflect any server-side changes
        loadMatrixData();

        // Show success notification
        const changeCount = state.pendingChanges.size;
        setSuccessMessage(`Successfully saved ${changeCount} changes`);

        // Auto-dismiss success message after 3 seconds
        setTimeout(() => setSuccessMessage(null), 3000);

      } else {
        const errorMessage = result.errors?.join('; ') || 'Save operation failed';

        setState(prev => ({
          ...prev,
          isSaving: false,
          error: errorMessage,
          conflicts: result.conflicts || []
        }));

        console.error('Save failed:', result.errors);

        if (result.errors?.some((error: string) =>
          error.toLowerCase().includes('schema') ||
          error.toLowerCase().includes('field') ||
          error.toLowerCase().includes('odata')
        )) {
          setShowDiagnostics(true);
        }
      }

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      const userMessage = ErrorHandler.getUserFriendlyMessage(handledError);

      console.error('Unexpected save error:', userMessage);
      setState(prev => ({
        ...prev,
        isSaving: false,
        error: `Save failed: ${userMessage}`
      }));

      if (ErrorHandler.isPermissionError(handledError)) {
        setCanEdit(false);
      }
    }
  };

  const handleCancel = (): void => {
    setState(prev => {
      const newCellStates = new Map(prev.cellStates);

      prev.pendingChanges.forEach(cellKey => {
        const cell = newCellStates.get(cellKey);
        if (cell) {
          newCellStates.set(cellKey, {
            ...cell,
            isAssigned: !cell.isAssigned,
            isModified: false,
            hasConflict: false
          });
        }
      });

      console.log(`Cancelled ${prev.pendingChanges.size} changes`);

      return {
        ...prev,
        cellStates: newCellStates,
        pendingChanges: new Set(),
        error: null,
        conflicts: []
      };
    });
  };

  const handleRowFilterChange = React.useCallback((value: string): void => {
    setState(prev => ({ ...prev, rowFilter: value }));
  }, []);

  const handleColumnFilterChange = React.useCallback((value: string): void => {
    setState(prev => ({ ...prev, columnFilter: value }));
  }, []);

  const runFieldMappingDiagnostics = async (): Promise<void> => {
    try {
      console.log('Running field mapping diagnostics...');
      const diagnosis = await dataService.diagnoseJunctionFieldMapping(config, 3);

      setDiagnosticsResults(diagnosis);
      console.log('Field Mapping Diagnosis:', diagnosis);

      if (isDebugMode && diagnosis.recommendations.some((r: string) => r.includes('setSchemaNameOverride'))) {
        console.log('Auto-applying recommended schema name overrides...');

        if (diagnosis.recommendations.some((r: string) => r.includes('row'))) {
          dataService.setSchemaNameOverride(
            config.junctionEntityName,
            config.junctionRowField,
            `${config.junctionRowField.charAt(0).toUpperCase()}${config.junctionRowField.slice(1)}`
          );
        }

        if (diagnosis.recommendations.some((r: string) => r.includes('column'))) {
          dataService.setSchemaNameOverride(
            config.junctionEntityName,
            config.junctionColumnField,
            `${config.junctionColumnField.charAt(0).toUpperCase()}${config.junctionColumnField.slice(1)}`
          );
        }
      }

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      console.error('Diagnostics failed:', ErrorHandler.getUserFriendlyMessage(handledError));
      setDiagnosticsResults({
        error: ErrorHandler.getUserFriendlyMessage(handledError),
        recommendations: ['Unable to run diagnostics. Check entity permissions and configuration.']
      });
    }
  };

  const analyzeVersionChains = async (): Promise<void> => {
    if (!hasVersionChainFeatures) {
      console.warn('Version chain features not configured');
      return;
    }

    try {
      console.log('Running version chain analysis...');

      if (dataService && typeof dataService.analyzeVersionChains === 'function') {
        const analysis = await dataService.analyzeVersionChains(config, parentRecordId);

        console.log('=== VERSION CHAIN ANALYSIS ===');
        console.log(`Total Studies: ${analysis.totalStudies}`);
        console.log(`Active Studies: ${analysis.activeStudies}`);
        console.log(`Abandoned Studies: ${analysis.abandonedStudies}`);
        console.log(`Total Chains: ${analysis.totalChains}`);
        console.log(`Standalone Studies: ${analysis.standaloneStudies}`);
        console.log(`Versioned Chains: ${analysis.versionedChains}`);
        console.log('Status Breakdown:', analysis.statusBreakdown);
        console.log('============================');

        setState(prev => ({
          ...prev,
          error: `Version Analysis: ${analysis.totalChains} chains found (${analysis.versionedChains} versioned, ${analysis.standaloneStudies} standalone). Check console for details.`
        }));

      } else {
        console.log('DataService version chain analysis not available');
      }

    } catch (error: any) {
      console.error('Version chain analysis failed:', error);
      setState(prev => ({
        ...prev,
        error: `Version chain analysis failed: ${error.message}`
      }));
    }
  };

  const handleClearCacheAndReload = async (): Promise<void> => {
    console.log('Clearing DataService cache and reloading...');
    dataService.clearCache();
    await loadMatrixData();
  };

  const showPerformanceReport = (): void => {
    if (dataService) {
      dataService.logServiceReport();
      updatePerformanceStats();
    }
  };

  const getErrorIntent = (error: string): MessageBarIntent => {
    if (error.toLowerCase().includes('permission')) return 'warning';
    if (error.toLowerCase().includes('network')) return 'error';
    if (error.toLowerCase().includes('timeout')) return 'warning';
    return 'error';
  };

  if (state.isLoading) {
    return (
      <FluentProvider theme={webLightTheme}>
        <div className={styles.container}>
          <div className={styles.loadingContainer}>
            <Spinner label={`Loading matrix data${parentRecordId ? ` for record ${parentRecordId}` : ''}...`} />
          </div>
        </div>
      </FluentProvider>
    );
  }

  return (
    <FluentProvider theme={webLightTheme}>
      <div className={styles.container}>

        {isDebugMode && (
          <div className={styles.debugActions}>
            <Tooltip content="Run diagnostics to troubleshoot field mapping issues" relationship="label">
              <Button
                size="small"
                appearance="subtle"
                onClick={runFieldMappingDiagnostics}
                disabled={state.isSaving}
              >
                Diagnostics
              </Button>
            </Tooltip>

            <Tooltip content="Clear all caches and reload fresh data" relationship="label">
              <Button
                size="small"
                appearance="subtle"
                onClick={handleClearCacheAndReload}
                disabled={state.isSaving}
              >
                Clear Cache
              </Button>
            </Tooltip>

            <Tooltip content="Show detailed performance statistics in console" relationship="label">
              <Button
                size="small"
                appearance="subtle"
                onClick={showPerformanceReport}
              >
                Performance
              </Button>
            </Tooltip>

            {hasVersionChainFeatures && (
              <Tooltip content="Analyze version chains and study status distribution" relationship="label">
                <Button
                  size="small"
                  appearance="subtle"
                  onClick={analyzeVersionChains}
                  disabled={state.isSaving}
                >
                  Version Analysis
                </Button>
              </Tooltip>
            )}
          </div>
        )}

        {(showDiagnostics || diagnosticsResults) && diagnosticsResults && (
          <div className={styles.diagnosticsContainer}>
            <MessageBar intent={diagnosticsResults.error ? "error" : "info"}>
              <div className={styles.messageBarContent}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ flex: 1 }}>
                    <strong>Field Mapping Diagnostics:</strong>
                    <ul style={{ marginTop: '8px', marginBottom: '0' }}>
                      {diagnosticsResults.recommendations?.map((rec: string, idx: number) => (
                        <li key={idx} style={{ fontSize: '12px' }}>{rec}</li>
                      ))}
                    </ul>
                    {diagnosticsResults.sampleRecords && diagnosticsResults.sampleRecords.length > 0 && (
                      <details style={{ marginTop: '8px', fontSize: '12px' }}>
                        <summary>Sample Record Fields</summary>
                        <pre style={{ fontSize: '11px', marginTop: '4px' }}>
                          {JSON.stringify(diagnosticsResults.sampleRecords[0]?.availableFields, null, 2)}
                        </pre>
                      </details>
                    )}
                  </div>
                  <Button
                    size="small"
                    appearance="subtle"
                    className={styles.dismissButton}
                    onClick={() => {
                      setShowDiagnostics(false);
                      setDiagnosticsResults(null);
                    }}
                    aria-label="Dismiss diagnostics"
                  >
                    ×
                  </Button>
                </div>
              </div>
            </MessageBar>
          </div>
        )}

        {state.error && (
          <div className={styles.errorContainer}>
            <MessageBar intent={getErrorIntent(state.error)}>
              <div className={styles.messageBarContent}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ flex: 1 }}>
                    {state.error}
                    {state.error.toLowerCase().includes('schema') || state.error.toLowerCase().includes('field') ? (
                      <div style={{ marginTop: '8px' }}>
                        <Button
                          size="small"
                          appearance="subtle"
                          onClick={runFieldMappingDiagnostics}
                          disabled={state.isSaving}
                        >
                          Run Diagnostics
                        </Button>
                      </div>
                    ) : null}
                  </div>
                  <Button
                    size="small"
                    appearance="subtle"
                    className={styles.dismissButton}
                    onClick={() => setState(prev => ({ ...prev, error: null }))}
                    aria-label="Dismiss error"
                  >
                    ×
                  </Button>
                </div>
              </div>
            </MessageBar>
          </div>
        )}

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

        {!canEdit && (
          <div className={styles.readonlyBanner}>
            <MessageBar intent="warning">
              You have read-only access. Contact your administrator to edit junction records.
            </MessageBar>
          </div>
        )}

        <SearchFilters
          rowFilter={state.rowFilter}
          columnFilter={state.columnFilter}
          onRowFilterChange={handleRowFilterChange}
          onColumnFilterChange={handleColumnFilterChange}
          disabled={state.isSaving || !canEdit}
          loadedRowCount={state.loadedRows.length}
          totalAvailableRows={state.totalAvailableRows}
          isLoadingMoreRows={state.isLoadingMoreRows}
          onLoadMoreRows={loadMoreRows}
          loadedColumnCount={state.loadedColumns.length}
          loadedVisibleColumnCount={getEffectiveColumns().length}
          totalAvailableColumns={state.totalAvailableColumns}
          isLoadingMoreColumns={state.isLoadingMoreColumns}
          onLoadMoreColumns={loadMoreColumns}
          onLoadAllRows={loadAllRows}
          onLoadAllColumns={loadAllColumns}
          parentEntityName={config.parentEntityName}
        />

        {state.bulkSelection && state.bulkSelection.selectedRowIds.size > 0 && (
          <div className={styles.bulkSelectionSummary}>
            <div>
              <strong>{state.bulkSelection.selectedRowIds.size}</strong> rows selected (Draft studies only)
              {state.bulkSelection.isAllRowsSelected && ' (All modifiable rows)'}
            </div>
            <div className={styles.bulkSelectionActions}>
              <Button
                size="small"
                appearance="subtle"
                onClick={handleBulkSelectAll}
                disabled={state.isSaving || !canEdit}
              >
                Select All Draft Rows
              </Button>
              <Button
                size="small"
                appearance="subtle"
                onClick={handleBulkClearAll}
                disabled={state.isSaving || !canEdit}
              >
                Clear Selection
              </Button>
            </div>
          </div>
        )}

        <MatrixTable
          rows={state.filteredRows}
          columns={state.filteredColumns}
          cellStates={state.cellStates}
          onCellToggle={handleCellToggle}
          onRowClick={handleRowClick}
          onColumnClick={handleColumnClick}
          disabled={state.isSaving || !canEdit}

          bulkSelection={state.bulkSelection}
          onBulkRowToggle={handleBulkRowToggle}
          onBulkSelectAll={handleBulkSelectAll}
          onBulkClearAll={handleBulkClearAll}
          bulkTooltipText={config.bulkSelectionTooltip || "Select rows with Draft studies for bulk operations"}

          getCellInteractable={getCellInteractable}
          getColumnStatus={getColumnStatus}
          getColumnHeaderStyle={getColumnHeaderStyle}
        />

        <ActionBar
          pendingChangesCount={state.pendingChanges.size}
          isSaving={state.isSaving}
          error={state.error}
          onSave={handleSave}
          onCancel={handleCancel}
          disabled={state.pendingChanges.size === 0 || !canEdit}
        />

        {state.loadedRows.length > 0 && isDebugMode && (
          <div className={styles.performanceStats}>
            <div>
              <strong>Matrix:</strong> {state.loadedRows.length} × {state.filteredColumns.length} = {state.loadedRows.length * state.filteredColumns.length} cells |
              <strong> Available:</strong> {state.totalAvailableRows} × {state.totalAvailableColumns} total |
              <strong> Assigned:</strong> {Array.from(state.cellStates.values()).filter(c => c.isAssigned).length} |
              <strong> Pending:</strong> {state.pendingChanges.size} changes |
              <strong> Mode:</strong> {canEdit ? 'Edit' : 'Read-only'}

              {state.bulkSelection && state.bulkSelection.selectedRowIds.size > 0 && (
                <>
                  | <strong> Selected:</strong> {state.bulkSelection.selectedRowIds.size} rows (Draft-compatible)
                </>
              )}

              {parentRecordId && (
                <>
                  <br />
                  <strong>Context:</strong> Filtered for record {parentRecordId} |
                  <strong> Filtered:</strong> {state.filteredRows.length} rows × {state.filteredColumns.length} columns visible
                </>
              )}

              {hasVersionChainFeatures && isDebugMode && (
                <>
                  <br />
                  <strong>Version 4.0</strong>
                  <strong>Version Processing:</strong> Enabled |
                  <strong> Raw Columns:</strong> {state.allColumns.length} →
                  <strong> Visible:</strong> {getEffectiveColumns().length} |
                  <strong> Filtered:</strong> {state.allColumns.length - getEffectiveColumns().length}
                  {config.columnParentAttrField && (
                    <> | <strong>Parent Field:</strong> {config.columnParentAttrField}</>
                  )}
                  {config.columnVersionField && (
                    <> | <strong>Version Field:</strong> {config.columnVersionField}</>
                  )}
                </>
              )}
            </div>

            {performanceStats && performanceStats.calls > 0 && (
              <div style={{ marginTop: '4px', fontSize: '11px' }}>
                <strong>Performance:</strong> {performanceStats.calls} calls |
                Avg: {performanceStats.avgTime.toFixed(1)}ms |
                Errors: {performanceStats.errors} |
                Success Rate: {(((performanceStats.calls - performanceStats.errors) / performanceStats.calls) * 100).toFixed(1)}%
                {isDebugMode && (
                  <span> | <strong>Debug:</strong> Enabled</span>
                )}
              </div>
            )}
          </div>
        )}

      </div>
    </FluentProvider>
  );
};

export type { MatrixConfig };