export interface Module {
  id: string;
  variableName: string;
  label: string;
  description?: string;
  versionNumber: number;
  parentModuleId?: string;
  instructions?: string;
  status: string;
  statusReason?: string;
  isActive: boolean;
}

export interface ModuleVersion {
  id: string;
  versionNumber: number;
  changeDescription?: string;
  createdOn: string;
  createdBy: string;
}
