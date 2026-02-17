export interface CellState {
  rowId: string;
  columnId: string;
  junctionId: string;
  
  isChecked: boolean;
  isModified: boolean;
  hasConflict: boolean;

  isInteractable: boolean;          // Based on column status (Draft = true, for e.g.)

  rowName: string;
  columnName: string;
}