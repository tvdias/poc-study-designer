import { DataService } from "../../services/DataService";
import { RowEntity } from "../RowEntity";

export interface GridContainerProps {
  context: ComponentFramework.Context<any>;
  onNotifyOutputChanged: () => void;
  dataItems: RowEntity[];
  dataService: DataService;
  isReadOnly: boolean;
}