import type { RowEntity } from "../models/RowEntity";
import { ModulesInProject } from "../models/ModulesInProject";

export class GirdRowsHelper {

  private STATUS_ACTIVE = 1;

  /**
   * Get modules in the project
   */
  getModulesInProject(rows: RowEntity[]): ModulesInProject[] {
    const map = new Map<string, RowEntity[]>();

    rows.forEach(r => {
      const moduleName = r.middleLabelText?.trim();
      if (moduleName && r.statusCode === this.STATUS_ACTIVE) {  // <-- only include rows with statusCode = 1
        if (!map.has(moduleName)) {
          map.set(moduleName, []);
        }
        map.get(moduleName)!.push(r);
      }
    });

    return Array.from(map.entries()).map(([moduleName, relatedRows]) => ({
      moduleName,
      count: relatedRows.length,
      rows: relatedRows,
    }));
  }
}