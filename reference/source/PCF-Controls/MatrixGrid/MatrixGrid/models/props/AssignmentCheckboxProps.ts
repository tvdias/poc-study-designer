import { CellState } from "../CellState";

export interface AssignmentCheckboxProps {
  cellState: CellState;
  onToggle: () => void;
  disabled: boolean;

  isInteractable: boolean;
}