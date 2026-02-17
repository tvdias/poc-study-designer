import { DropdownItem } from "../DropdownItem";

export interface DropdownFilterProps {
  onDropdownFilterChange: (value: string) => void;
  dropdownItems: DropdownItem[];
  selectedValue: string;
}