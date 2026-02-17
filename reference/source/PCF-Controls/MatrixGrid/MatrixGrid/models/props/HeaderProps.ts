import { DropdownItem } from "../DropdownItem";

export interface HeaderProps {
  onRowFilterChange: (value: string) => void;
  onColumnFilterChange: (value: string) => void;
  disabled: boolean;
  rowsLabel: string;
  columnsLabel: string;

  loadedRowsCount: number;
  loadedColumnsCount: number;

  onDropdownFilterChange: (value: string) => void;
  dropdownItems: DropdownItem[];
  selectedDropdownValue: string;

  onRefresh: () => void;
}