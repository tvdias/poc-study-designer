import { DataService } from "../../services/DataService";
import { ViewType } from "../../types/ViewType";
import { RowEntity } from "../RowEntity";

export interface ExpandableGridProps {
  context: ComponentFramework.Context<any>;
  rows: RowEntity[];
  dataService: DataService;
  entityName: string;
  isReadOnly: boolean;
  view: ViewType.All | ViewType.Active | ViewType.Inactive | ViewType.Dummy; 
  isScripter: boolean;
}