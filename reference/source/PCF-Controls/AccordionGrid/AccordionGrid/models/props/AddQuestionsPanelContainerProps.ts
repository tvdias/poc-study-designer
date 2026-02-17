// models/props/AddQuestionsPanelContainerProps.ts
import { RowEntity } from "../RowEntity";
export interface AddQuestionsPanelContainerProps {
  isOpen: boolean;
  onClose: () => void;
  row?: RowEntity;
  existingRows?: RowEntity[]; 
  onRefresh: () => void; 
  addFromHeader: boolean;
  context: ComponentFramework.Context<any>;
  isScripter: boolean;
}
