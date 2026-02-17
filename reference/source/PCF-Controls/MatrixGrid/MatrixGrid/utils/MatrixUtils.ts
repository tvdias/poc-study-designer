import { CellState } from "../models/CellState";
import { ColumnItem } from "../models/ColumnItem";
import { JunctionItem } from "../models/JunctionItem";
import { RowItem } from "../models/RowItem";

export class MatrixUtils {
  
  /**
   * Generate unique cell key from row and column IDs
   */
  static generateCellKey(rowId: string, columnId: string): string {
    return `${rowId}-${columnId}`;
  }

  /**
   * Generate all the possible cells for the Matrix
   */
  static buildCellStates(rows: RowItem[], columns: ColumnItem[], junctions: JunctionItem[], dropdownValue: string): Map<string, CellState> {
    const cellStates = new Map<string, CellState>();

    rows.forEach(row => {
      columns.forEach(column => {
        const cellKey = this.generateCellKey(row.id, column.id);
        const junction = junctions
          .find(j => j.rowId === row.id && j.columnId === column.id && j.dropdownValueToFilter === dropdownValue);
        
        cellStates.set(cellKey, {
          rowId: row.id,
          columnId: column.id,
          junctionId: junction ? junction.id : '',

          isChecked: junction ? true : false,
          isModified: false,
          hasConflict: false,
          isInteractable: true,

          rowName: row.name,
          columnName: column.name
        });
      });
    });

    console.log('CELL STATES:', cellStates);

    return cellStates;
  }

  /**
   * Filter entities based on search term with performance optimization
   */
  static filterEntities<T extends { displayName: string }>(
    entities: T[], 
    filter: string
  ): T[] {
    if (!filter.trim()) {
      return entities;
    }

    const searchTerm = filter.toLowerCase().trim();
    
    if (entities.length > 1000) {
      const results: T[] = [];
      for (const entity of entities) {
        if (entity.displayName.toLowerCase().includes(searchTerm)) {
          results.push(entity);
        }
      }
      return results;
    }

    return entities.filter(entity => 
      entity.displayName.toLowerCase().includes(searchTerm)
    );
  }


  /**
   * Format entity display name with truncation
   */
  static formatDisplayName(displayName: string, maxLength: number): string {
    if (!displayName) return 'Unnamed';
    
    if (displayName.length <= maxLength) {
      return displayName;
    }

    return displayName.substring(0, maxLength - 3) + '...';
  }
}