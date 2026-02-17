// models/ModulesInProject.ts
import { RowEntity } from "./RowEntity";

export interface ModulesInProject {
  moduleName: string;
  count: number;
  rows: RowEntity[]
}
