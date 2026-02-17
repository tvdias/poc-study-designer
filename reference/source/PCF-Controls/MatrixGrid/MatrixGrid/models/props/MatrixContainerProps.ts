import { IInputs } from "../../generated/ManifestTypes";
import { MatrixService } from "../../services/MatrixService";
import { ColumnItem } from "../ColumnItem";
import { DropdownItem } from "../DropdownItem";
import { JunctionItem } from "../JunctionItem";
import { RowItem } from "../RowItem";

export interface MatrixContainerProps {
  context: ComponentFramework.Context<IInputs>;
  
  rowsItems: RowItem[];
  columnItems: ColumnItem[];
  junctionItems: JunctionItem[];

  rowsLabel: string;
  columnsLabel: string;

  dropdownItems: DropdownItem[];

  matrixService: MatrixService;

  isReadOnly: boolean;
}