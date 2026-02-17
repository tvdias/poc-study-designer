import { ViewType } from "../../types/ViewType";
import { RowEntity } from "../RowEntity";

export interface HeaderProps {
  context: ComponentFramework.Context<any>;
  view: ViewType.All | ViewType.Active | ViewType.Inactive | ViewType.Dummy; 
  updateView: (statusCode: ViewType.All | ViewType.Active | ViewType.Inactive | ViewType.Dummy) => void;
  onSearch: (value: string) => void;
  isReadOnly: boolean;
  rows: RowEntity[];
  entityName: string;    
  isScripter: boolean;
}