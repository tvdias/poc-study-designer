import { CellState } from "../CellState";
import { ColumnItem } from "../ColumnItem";
import { RowItem } from "../RowItem";

export interface MatrixTableProps {
  rows: RowItem[];
  columns: ColumnItem[];
  cellStates: Map<string, CellState>;
  onCellToggle: (rowId: string, columnId: string) => void;
  // Row-level toggle to select/deselect
  onRowToggleAll?: (rowId: string) => void;
  onRowClick?: (rowId: string) => void;
  onColumnClick?: (columnId: string) => void;
  disabled: boolean;

  // Bulk Selection Props (optional for backward compatibility)
  /*
  bulkSelection?: BulkSelectionState;
  onBulkRowToggle?: (rowId: string) => void;
  onBulkSelectAll?: () => void;
  onBulkClearAll?: () => void;
  bulkTooltipText?: string;
  */
}