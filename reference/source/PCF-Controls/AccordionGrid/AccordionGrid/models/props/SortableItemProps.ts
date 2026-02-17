import { DataService } from "../../services/DataService";
import { ViewType } from "../../types/ViewType";
import { RowEntity } from "../RowEntity";

export interface SortableItemProps {
    row: RowEntity;
    dataService: DataService;
    isReadOnly: boolean;
    entityName: string;
    context: ComponentFramework.Context<any>;
    onOpenAddPanel: (row: RowEntity) => void;
    view: ViewType.All | ViewType.Active | ViewType.Inactive| ViewType.Dummy;    
    isScripter: boolean;
}