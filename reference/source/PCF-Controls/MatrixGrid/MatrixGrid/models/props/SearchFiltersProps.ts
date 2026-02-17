export interface SearchFiltersProps {
  onRowFilterChange: (value: string) => void;
  onColumnFilterChange: (value: string) => void;
  disabled: boolean;
  rowsLabel: string;
  columnsLabel: string;
}