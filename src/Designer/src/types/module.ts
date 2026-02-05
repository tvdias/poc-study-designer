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
  questions?: ModuleQuestion[];
}

export interface ModuleQuestion {
  questionId: string;
  variableName: string;
  questionType: string;
  questionText: string;
  questionSource: string;
  displayOrder: number;
  createdBy: string;
}

export interface Question {
  id: string;
  variableName: string;
  questionType: string;
  questionText: string;
  questionSource: string;
  isActive: boolean;
}

export interface ModuleVersion {
  id: string;
  versionNumber: number;
  changeDescription?: string;
  createdOn: string;
  createdBy: string;
}
