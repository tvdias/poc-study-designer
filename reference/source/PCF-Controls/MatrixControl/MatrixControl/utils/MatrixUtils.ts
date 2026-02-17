import {
  RowEntity,
  ColumnEntity,
  JunctionRecord,
  CellState,
  CellKey,
  BatchOperation,
  MatrixConfig,
  BulkSelectionState,
  STUDY_STATUS,
  StudyStatus
} from '../types/MatrixTypes';

export class MatrixUtils {
  /**
   * Generate unique cell key from row and column IDs
   */
  static generateCellKey(rowId: string, columnId: string): CellKey {
    return `${rowId}-${columnId}`;
  }

  /**
   * Parse cell key back to row and column IDs
   */
  static parseCellKey(cellKey: CellKey): { rowId: string; columnId: string } {
    const [rowId, columnId] = cellKey.split('-');
    return { rowId, columnId };
  }

  /**
   * Filter entities based on search term with performance optimization
   */
  static filterEntities<T extends { displayName: string }>(
    entities: T[], 
    filter: string
  ): T[] {
    if (!filter.trim()) {
      return entities;
    }

    const searchTerm = filter.toLowerCase().trim();
    
    // Performance optimization: use more efficient filtering for large datasets
    if (entities.length > 1000) {
      const results: T[] = [];
      for (let i = 0; i < entities.length; i++) {
        if (entities[i].displayName.toLowerCase().includes(searchTerm)) {
          results.push(entities[i]);
        }
      }
      return results;
    }

    return entities.filter(entity => 
      entity.displayName.toLowerCase().includes(searchTerm)
    );
  }

  /**
   * Build cell states map from entities and junction records with performance optimization
   */
  static buildCellStatesMap(
    rows: RowEntity[],
    columns: ColumnEntity[],
    junctions: JunctionRecord[]
  ): Map<CellKey, CellState> {
    const cellStates = new Map<CellKey, CellState>();

    // Create junction lookup for faster access
    const junctionLookup = new Map<CellKey, JunctionRecord>();
    junctions.forEach(junction => {
      const cellKey = this.generateCellKey(junction.rowId, junction.columnId);
      junctionLookup.set(cellKey, junction);
    });

    // Build all possible cell combinations with performance optimization
    rows.forEach(row => {
      columns.forEach(column => {
        const cellKey = this.generateCellKey(row.id, column.id);
        const junction = junctionLookup.get(cellKey);

        const cellState: CellState = {
          rowId: row.id,
          columnId: column.id,
          isAssigned: !!junction,
          isModified: false,
          hasConflict: false,
          junctionId: junction?.id,
          sortOrder: row?.sortOrder ?? 0,
          // Enhanced: Add interactability based on column status
          isInteractable: this.isColumnInteractable(column)
        };

        cellStates.set(cellKey, cellState);
      });
    });

    return cellStates;
  }

  /**
   * Check if a column (study) is interactable based on its status
   */
  static isColumnInteractable(column: ColumnEntity): boolean {
    // If no status is set, assume it's interactable (backward compatibility)
    if (!column.statuscode) return true;
    
    // Only Draft status studies are interactable
    return column.statuscode === STUDY_STATUS.DRAFT;
  }

  /**
   * Get columns that are in Draft status (modifiable)
   */
  static getDraftColumns(columns: ColumnEntity[]): ColumnEntity[] {
    return columns.filter(column => 
      !column.statuscode || column.statuscode === STUDY_STATUS.DRAFT
    );
  }

  /**
   * Create initial bulk selection state
   */
  static createBulkSelectionState(): BulkSelectionState {
    return {
      selectedRowIds: new Set<string>(),
      isAllRowsSelected: false
    };
  }

  /**
   * Toggle row selection in bulk selection state
   */
  static toggleRowInBulkSelection(
    currentState: BulkSelectionState,
    rowId: string
  ): BulkSelectionState {
    const newSelectedRowIds = new Set(currentState.selectedRowIds);
    
    if (newSelectedRowIds.has(rowId)) {
      newSelectedRowIds.delete(rowId);
    } else {
      newSelectedRowIds.add(rowId);
    }
    
    return {
      selectedRowIds: newSelectedRowIds,
      isAllRowsSelected: false // Reset master selection when individual rows are toggled
    };
  }

  /**
   * Select all modifiable rows
   */
  static selectAllModifiableRows(
    rows: RowEntity[],
    columns: ColumnEntity[]
  ): BulkSelectionState {
    const draftColumns = this.getDraftColumns(columns);
    
    // Only select rows that have at least one Draft study available
    const modifiableRowIds = rows
      .filter(() => draftColumns.length > 0) // All rows are modifiable if any Draft columns exist
      .map(row => row.id);
    
    return {
      selectedRowIds: new Set(modifiableRowIds),
      isAllRowsSelected: true
    };
  }

  /**
   * Clear all row selections
   */
  static clearAllRowSelections(): BulkSelectionState {
    return {
      selectedRowIds: new Set<string>(),
      isAllRowsSelected: false
    };
  }

  /**
   * Enhanced: Build batch operation from pending changes with bulk selection support
   */
  static buildBatchOperation(
    cellStates: Map<CellKey, CellState>,
    pendingChanges: Set<CellKey>,
    config: MatrixConfig,
    bulkSelectionState?: BulkSelectionState
  ): BatchOperation {
    const operation: BatchOperation = {
      creates: [],
      deletes: [],
      updates: []
    };

    pendingChanges.forEach(cellKey => {
      const cellState = cellStates.get(cellKey);
      if (!cellState || !cellState.isModified) {
        return;
      }

      // Only process changes for interactable cells (Draft status)
      if (cellState.isInteractable === false) {
        return;
      }

      if (cellState.isAssigned && !cellState.junctionId) {
        // Need to create new junction record
        operation.creates.push({
          rowId: cellState.rowId,
          columnId: cellState.columnId,
          sortOrder: cellState.sortOrder ?? 0
        });
      } else if (!cellState.isAssigned && cellState.junctionId) {
        // Need to delete existing junction record
        operation.deletes.push(cellState.junctionId);
      }
    });

    return operation;
  }

  /**
   * Apply bulk selection to cell states (assign all Draft studies for selected rows)
   */
  static applyBulkSelectionToCellStates(
    cellStates: Map<CellKey, CellState>,
    bulkSelectionState: BulkSelectionState,
    columns: ColumnEntity[]
  ): { updatedCellStates: Map<CellKey, CellState>; pendingChanges: Set<CellKey> } {
    const updatedCellStates = new Map(cellStates);
    const pendingChanges = new Set<CellKey>();
    const draftColumns = this.getDraftColumns(columns);

    bulkSelectionState.selectedRowIds.forEach(rowId => {
      draftColumns.forEach(column => {
        const cellKey = this.generateCellKey(rowId, column.id);
        const currentCellState = updatedCellStates.get(cellKey);
        
        if (currentCellState && !currentCellState.isAssigned) {
          // Assign this cell (only if not already assigned)
          const updatedCellState: CellState = {
            ...currentCellState,
            isAssigned: true,
            isModified: true
          };
          
          updatedCellStates.set(cellKey, updatedCellState);
          pendingChanges.add(cellKey);
        }
      });
    });

    return { updatedCellStates, pendingChanges };
  }

  /**
   * Get study status CSS class for column header styling
   */
  static getStatusCssClass(status?: StudyStatus): string {
    if (!status) return 'column-header-draft';
    
    switch (status) {
      case STUDY_STATUS.READY_FOR_SCRIPTING:
        return 'column-header-ready';
      case STUDY_STATUS.APPROVED_FOR_LAUNCH:
        return 'column-header-approved';
      case STUDY_STATUS.REWORK:
        return 'column-header-rework';
      case STUDY_STATUS.ABANDONED:
        return 'column-header-disabled';
      case STUDY_STATUS.DRAFT:
      default:
        return 'column-header-draft';
    }
  }

  /**
   * Enhanced: Get cells that have pending changes with filtering
   */
  static getPendingCells(
    cellStates: Map<CellKey, CellState>,
    pendingChanges: Set<CellKey>,
    onlyInteractable: boolean = true
  ): CellState[] {
    const pendingCells: CellState[] = [];
    
    pendingChanges.forEach(cellKey => {
      const cellState = cellStates.get(cellKey);
      if (cellState && cellState.isModified) {
        // Filter by interactability if requested
        if (!onlyInteractable || cellState.isInteractable !== false) {
          pendingCells.push(cellState);
        }
      }
    });

    return pendingCells;
  }

  /**
   * Get cells that have conflicts
   */
  static getConflictCells(cellStates: Map<CellKey, CellState>): CellState[] {
    const conflictCells: CellState[] = [];
    
    cellStates.forEach(cellState => {
      if (cellState.hasConflict) {
        conflictCells.push(cellState);
      }
    });

    return conflictCells;
  }

  /**
   * Enhanced: Calculate statistics for the matrix with status awareness
   */
  static calculateMatrixStats(
    cellStates: Map<CellKey, CellState>,
    columns: ColumnEntity[]
  ): {
    totalCells: number;
    assignedCells: number;
    pendingChanges: number;
    conflicts: number;
    interactableCells: number;
    draftColumns: number;
  } {
    let assignedCells = 0;
    let pendingChanges = 0;
    let conflicts = 0;
    let interactableCells = 0;

    cellStates.forEach(cellState => {
      if (cellState.isAssigned) assignedCells++;
      if (cellState.isModified) pendingChanges++;
      if (cellState.hasConflict) conflicts++;
      if (cellState.isInteractable !== false) interactableCells++;
    });

    const draftColumns = this.getDraftColumns(columns).length;

    return {
      totalCells: cellStates.size,
      assignedCells,
      pendingChanges,
      conflicts,
      interactableCells,
      draftColumns
    };
  }

  /**
   * Validate matrix configuration
   */
  static validateConfig(config: MatrixConfig): string[] {
    const errors: string[] = [];

    // Required fields
    if (!config.rowEntityName) errors.push('Row entity name is required');
    if (!config.columnEntityName) errors.push('Column entity name is required');
    if (!config.junctionEntityName) errors.push('Junction entity name is required');
    
    if (!config.rowIdField) errors.push('Row ID field is required');
    if (!config.rowDisplayField) errors.push('Row display field is required');
    if (!config.columnIdField) errors.push('Column ID field is required');
    if (!config.columnDisplayField) errors.push('Column display field is required');
    
    if (!config.junctionRowField) errors.push('Junction row field is required');
    if (!config.junctionColumnField) errors.push('Junction column field is required');

    // Enhanced validation
    if (!config.entityId) errors.push('Entity ID is required for form context');
    if (!config.entityName) errors.push('Entity name is required for form context');

    // Validation rules
    if (config.pageSize && config.pageSize < 1) {
      errors.push('Page size must be greater than 0');
    }

    return errors;
  }

  /**
   * Enhanced debounce function with immediate option
   */
  static debounce<T extends (...args: any[]) => any>(
    func: T,
    wait: number,
    immediate: boolean = false
  ): (...args: Parameters<T>) => void {
    let timeout: ReturnType<typeof setTimeout> | null = null;
    
    return (...args: Parameters<T>) => {
      const callNow = immediate && !timeout;
      
      if (timeout) {
        clearTimeout(timeout);
      }
      
      timeout = setTimeout(() => {
        timeout = null;
        if (!immediate) func(...args);
      }, wait);
      
      if (callNow) func(...args);
    };
  }

  /**
   * Deep clone an object (for state management)
   */
  static deepClone<T>(obj: T): T {
    if (obj === null || typeof obj !== 'object') {
      return obj;
    }

    if (obj instanceof Date) {
      return new Date(obj.getTime()) as unknown as T;
    }

    if (obj instanceof Array) {
      return obj.map(item => this.deepClone(item)) as unknown as T;
    }

    if (obj instanceof Map) {
      const clonedMap = new Map();
      obj.forEach((value, key) => {
        clonedMap.set(key, this.deepClone(value));
      });
      return clonedMap as unknown as T;
    }

    if (obj instanceof Set) {
      const clonedSet = new Set();
      obj.forEach(value => {
        clonedSet.add(this.deepClone(value));
      });
      return clonedSet as unknown as T;
    }

    if (typeof obj === 'object') {
      const clonedObj: any = {};
      Object.keys(obj).forEach(key => {
        clonedObj[key] = this.deepClone((obj as any)[key]);
      });
      return clonedObj as T;
    }

    return obj;
  }

  /**
   * Format entity display name with truncation
   */
  static formatDisplayName(displayName: string, maxLength: number = 30): string {
    if (!displayName) return 'Unnamed';
    
    if (displayName.length <= maxLength) {
      return displayName;
    }

    return displayName.substring(0, maxLength - 3) + '...';
  }

  /**
   * Extract junction records from current cell states (for progressive loading)
   */
  static extractJunctionsFromCellStates(cellStates: Map<CellKey, CellState>): JunctionRecord[] {
    const junctions: JunctionRecord[] = [];
    
    cellStates.forEach(cellState => {
      if (cellState.isAssigned && cellState.junctionId) {
        junctions.push({
          id: cellState.junctionId,
          rowId: cellState.rowId,
          columnId: cellState.columnId
        });
      }
    });
    
    return junctions;
  }

  /**
   * Sort entities by display name with enhanced options
   */
  static sortEntitiesByName<T extends { displayName: string }>(
    entities: T[],
    descending: boolean = false
  ): T[] {
    const sorted = [...entities].sort((a, b) => 
      a.displayName.localeCompare(b.displayName, undefined, { 
        numeric: true, 
        sensitivity: 'base' 
      })
    );
    
    return descending ? sorted.reverse() : sorted;
  }

  /**
   * Performance monitoring utilities
   */
  static measurePerformance<T>(
    operation: () => T,
    label: string = 'Operation'
  ): { result: T; duration: number } {
    const start = performance.now();
    const result = operation();
    const duration = performance.now() - start;
    
    console.log(`${label} took ${duration.toFixed(2)}ms`);
    
    return { result, duration };
  }

  /**
   * Memory usage optimization for large datasets
   */
  static optimizeForLargeDataset<T>(
    items: T[],
    threshold: number = 1000
  ): { 
    isLarge: boolean; 
    shouldVirtualize: boolean; 
    chunkSize: number; 
  } {
    const isLarge = items.length > threshold;
    const shouldVirtualize = items.length > threshold * 5;
    const chunkSize = Math.min(Math.max(Math.floor(items.length / 10), 50), 200);
    
    return { isLarge, shouldVirtualize, chunkSize };
  }
}