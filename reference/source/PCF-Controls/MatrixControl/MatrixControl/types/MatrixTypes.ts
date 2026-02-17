// Configuration Types
export interface MatrixConfig {
  // Entity Configuration
  rowEntityName: string;          // e.g., "ktr_questionnaireline"
  columnEntityName: string;       // e.g., "ktr_study" 
  junctionEntityName: string;     // e.g., "ktr_studyquestionnaireline"

  // Add to MatrixConfig interface:
  entityId: string;        // Current record ID (parent record)
  entityName: string;      // Current entity logical name

  // Field Mappings
  rowIdField: string;             // Primary key field for row entity
  rowDisplayField: string;        // Display name field for row entity
  columnIdField: string;          // Primary key field for column entity
  columnDisplayField: string;     // Display name field for column entity
  junctionIdField: string;        // Primary key field for junction entity
  junctionRowField: string;       // Foreign key to row entity in junction
  junctionColumnField: string;    // Foreign key to column entity in junction

  // Parent Relationship Fields (NEW - for form context filtering)
  rowParentField: string;         // Lookup field in row entity pointing to parent record (e.g., "ktr_project")
  columnParentField: string;      // Lookup field in column entity pointing to parent record (e.g., "ktr_project")

  // NEW: Hierarchy & Version Support
  columnParentAttrField?: string; // Lookup field for hierarchy chain (Study V2 → Study V1)
  columnVersionField?: string;    // Version field for ordering (versionnumber, createdon, etc.)

  // NEW: UI Configuration
  bulkSelectionTooltip?: string;  // Tooltip text for bulk selection info icon

  // UI Configuration
  pageSize?: number;              // Default: 50
  enableBatchSave?: boolean;      // Default: true
  autoSaveDelay?: number;         // Default: disabled (explicit save only)

  // Debugging Options
  debugMode?: boolean;            // Default: false

  // Optional Parent Entity Context - for StudyMLE or similar use cases
  parentEntityId?: string;     // Parent record (e.g., "ManagedList")
  parentEntityName?: string;   // Parent entity logical name (e.g., ""ktr_managedlist")
}

// NEW: Status Constants (hardcoded statuscode values)
export const STUDY_STATUS = {
  DRAFT: 1,
  READY_FOR_SCRIPTING: 847610001,
  APPROVED_FOR_LAUNCH: 847610002,
  ABANDONED: 847610005,
  REWORK: 847610006,
} as const;

export type StudyStatus = typeof STUDY_STATUS[keyof typeof STUDY_STATUS];

// Study Hierarchy Support
export interface StudyChain {
  chainId: string;                   // Root study ID (studies with no parent)
  studies: ColumnEntity[];           // All studies in this chain (ordered by version)
  latestActiveStudy: ColumnEntity;   // Latest non-abandoned study
  isStandalone: boolean;             // True if chain has only one study
}

// Bulk Selection Support
export interface BulkSelectionState {
  selectedRowIds: Set<string>;       // Individual row selections
  isAllRowsSelected: boolean;        // Master selection state (for UI feedback)
}

// Entity Data Models
export interface RowEntity {
  id: string;
  displayName: string;
  createdDate?: Date;
  sortOrder?: number;
  [key: string]: any;
}

export interface ColumnEntity {
  id: string;
  displayName: string;
  createdDate?: Date;

  // Status & Version Support (hardcoded field names)
  statuscode?: StudyStatus;        // Status using STUDY_STATUS constants (properly typed)
  parentAttrId?: string;             // Parent study in hierarchy chain (from columnParentAttrField)
  versionValue?: any;                // Version field value (from columnVersionField)

  [key: string]: any;
}

export interface JunctionRecord {
  id?: string;
  rowId: string;
  columnId: string;
  [key: string]: any;
}

// Cell State Management
export interface CellState {
  rowId: string;
  columnId: string;
  isAssigned: boolean;
  isModified: boolean;
  hasConflict: boolean;
  junctionId?: string;
  sortOrder: number;

  // Status-Aware Interaction (optional for backward compatibility)
  isInteractable?: boolean;          // Based on column status (Draft = true)
}

// Application State
export interface MatrixState {
  // Data
  allRows: RowEntity[];              // All available rows from server
  allColumns: ColumnEntity[];        // All available columns from server  
  loadedRows: RowEntity[];           // Currently loaded/visible rows
  loadedColumns: ColumnEntity[];     // Currently loaded/visible columns
  cellStates: Map<string, CellState>; // Key: `${rowId}-${columnId}`

  // Progressive Loading State
  loadedRowRange: { start: number; end: number };
  loadedColumnRange: { start: number; end: number };
  totalAvailableRows: number;
  totalAvailableColumns: number;
  isLoadingMoreRows: boolean;
  isLoadingMoreColumns: boolean;

  // UI State
  filteredRows: RowEntity[];
  filteredColumns: ColumnEntity[];
  rowFilter: string;
  columnFilter: string;
  hasSearchExpanded: boolean;        // Track if search auto-expanded

  // Operation State
  pendingChanges: Set<string>;       // Cell keys with unsaved changes
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  conflicts: ConflictInfo[];

  // Enhanced State (optional for backward compatibility)
  visibleColumns?: ColumnEntity[];   // After status filtering & latest-version-only logic
  studyChains?: StudyChain[];        // Processed hierarchy chains
  bulkSelection?: BulkSelectionState; // Bulk row selection state
}

// Conflict Resolution
export interface ConflictInfo {
  cellKey: string;
  userValue: boolean;
  serverValue: boolean;
  junctionId?: string;
}

// Batch Operations
export interface BatchOperation {
  creates: JunctionRecord[];      // New assignments
  deletes: string[];              // Junction IDs to delete
  updates: JunctionRecord[];      // Modified assignments (if applicable)
}

// Component Props - Enhanced
export interface MatrixContainerProps {
  config: MatrixConfig;
  dataService: any; // Will be typed as DataService
  context: ComponentFramework.Context<any>;
  onNotifyOutputChanged: () => void;
  parentRecordId?: string; // Optional parent record ID for form context
}

export interface SearchFiltersProps {
  rowFilter: string;
  columnFilter: string;
  onRowFilterChange: (value: string) => void;
  onColumnFilterChange: (value: string) => void;
  disabled?: boolean;

  // Progressive loading props
  loadedRowCount: number;
  totalAvailableRows: number;
  isLoadingMoreRows: boolean;
  onLoadMoreRows: () => void;
  loadedColumnCount: number;
  totalAvailableColumns: number;
  isLoadingMoreColumns: boolean;
  onLoadMoreColumns: () => void;
  onLoadAllRows: () => void;
  onLoadAllColumns: () => void;
}

export interface MatrixTableProps {
  rows: RowEntity[];
  columns: ColumnEntity[];
  cellStates: Map<string, CellState>;
  onCellToggle: (rowId: string, columnId: string) => void;
  onRowClick?: (rowId: string) => void;
  onColumnClick?: (columnId: string) => void;
  disabled?: boolean;

  // Bulk Selection Props (optional for backward compatibility)
  bulkSelection?: BulkSelectionState;
  onBulkRowToggle?: (rowId: string) => void;
  onBulkSelectAll?: () => void;
  onBulkClearAll?: () => void;
  bulkTooltipText?: string;

  // Status-Aware Interaction Helpers (optional)
  getCellInteractable?: (columnId: string) => boolean;  // Check if Draft status
  getColumnStatus?: (columnId: string) => StudyStatus | undefined;
  getColumnHeaderStyle?: (columnId: string) => string; // CSS class for status styling
}

export interface AssignmentCheckboxProps {
  cellState: CellState;
  onToggle: () => void;
  disabled?: boolean;

  // Status-aware styling (optional)
  isInteractable?: boolean;          // Show as disabled if non-Draft status
}

// Bulk Selection Components
export interface BulkSelectionColumnProps {
  bulkSelection: BulkSelectionState;
  onBulkRowToggle: (rowId: string) => void;
  onBulkSelectAll: () => void;
  onBulkClearAll: () => void;
  tooltipText?: string;
  rows: RowEntity[];                 // For individual row checkboxes
  disabled?: boolean;
}

export interface BulkRowCheckboxProps {
  rowId: string;
  isSelected: boolean;
  onToggle: (rowId: string) => void;
  disabled?: boolean;
}

export interface InfoTooltipProps {
  text: string;
  position?: 'top' | 'bottom' | 'left' | 'right';
}

export interface ActionBarProps {
  pendingChangesCount: number;
  isSaving: boolean;
  error: string | null;
  onSave: () => void;
  onCancel: () => void;
  disabled?: boolean;
}

export interface ProgressiveLoaderProps {
  // Row loading
  loadedRowCount: number;
  totalAvailableRows: number;
  isLoadingMoreRows: boolean;
  onLoadMoreRows: () => void;

  // Column loading  
  loadedColumnCount: number;
  totalAvailableColumns: number;    // Based on visibleColumns count
  isLoadingMoreColumns: boolean;
  onLoadMoreColumns: () => void;

  // Bulk loading
  onLoadAllRows: () => void;
  onLoadAllColumns: () => void;

  disabled?: boolean;
}

// Version Chain Processing Helper Types
export interface VersionChainProcessor {
  processColumns: (columns: ColumnEntity[], config: MatrixConfig) => {
    visibleColumns: ColumnEntity[];
    studyChains: StudyChain[];
  };

  filterByStatus: (columns: ColumnEntity[]) => ColumnEntity[];
  groupIntoChains: (columns: ColumnEntity[], parentField?: string) => StudyChain[];
  getLatestInChain: (chain: ColumnEntity[], versionField?: string) => ColumnEntity;
  isInteractable: (status: StudyStatus) => boolean;
  getStatusStyle: (status: StudyStatus) => string;
}

// Utility Types
export type CellKey = string; // Format: `${rowId}-${columnId}`

export enum SaveStatus {
  Idle = 'idle',
  Saving = 'saving',
  Success = 'success',
  Error = 'error'
}

export enum CellStatus {
  Normal = 'normal',
  Pending = 'pending',
  Conflict = 'conflict'
}

// Status-based CSS Classes
export enum StatusStyle {
  Draft = 'column-header-draft',
  ReadyForScripting = 'column-header-ready',
  ApprovedForLaunch = 'column-header-approved',
  Rework = 'column-header-rework',
  Disabled = 'column-header-disabled'
}

// API Response Types
export interface EntityResponse<T> {
  value: T[];
  '@odata.count'?: number;
  '@odata.nextLink'?: string;
}

export interface SaveResponse {
  success: boolean;
  errors?: string[];
  conflicts?: ConflictInfo[];
}

// Utility Functions Type Definitions
export type CellKeyGenerator = (rowId: string, columnId: string) => CellKey;
export type FilterFunction<T> = (items: T[], filter: string) => T[];

// Version Chain Utility Functions
export type VersionComparator = (a: any, b: any) => number;
export type StatusFilter = (status: StudyStatus) => boolean;
export type ChainGrouper<T> = (items: T[], parentField: string) => Map<string, T[]>;