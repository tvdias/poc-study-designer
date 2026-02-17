import { MatrixConfig, ColumnEntity, StudyChain, STUDY_STATUS, StudyStatus } from '../types/MatrixTypes';

/**
 * VersionChainProcessor - Handles complex study version chain logic
 * This is a completely new utility that doesn't affect existing functionality
 */
export class VersionChainProcessor {
  
  /**
   * Main entry point - processes raw columns into filtered, latest-version-only columns
   * Only runs when version chain fields are configured
   */
  static processColumns(
    rawColumns: ColumnEntity[], 
    config: MatrixConfig
  ): {
    visibleColumns: ColumnEntity[];
    studyChains: StudyChain[];
  } {
    // If no version chain configuration, return original columns unchanged
    if (!config.columnParentAttrField && !config.columnVersionField) {
      return {
        visibleColumns: rawColumns,
        studyChains: []
      };
    }

    console.log('VersionChainProcessor: Processing', rawColumns.length, 'raw columns');
    
    // Step 1: Filter out abandoned studies (status 847,610,005)
    const activeColumns = this.filterByStatus(rawColumns);
    console.log('VersionChainProcessor: Filtered to', activeColumns.length, 'active columns');
    
    // Step 2: Group into study chains
    const studyChains = this.groupIntoChains(activeColumns, config.columnParentAttrField);
    console.log('VersionChainProcessor: Created', studyChains.length, 'study chains');
    
    // Step 3: Extract latest active version from each chain
    const visibleColumns = studyChains.map(chain => chain.latestActiveStudy);
    console.log('VersionChainProcessor: Final visible columns:', visibleColumns.length);
    
    return {
      visibleColumns,
      studyChains
    };
  }

  /**
   * Filter columns by status - remove abandoned studies
   */
  static filterByStatus(columns: ColumnEntity[]): ColumnEntity[] {
    return columns.filter(column => {
      const status = column.statuscode;
      
      // If statuscode is missing, include it (backward compatibility)
      if (!status) {
        return true;
      }
      
      // Exclude abandoned studies
      return status !== STUDY_STATUS.ABANDONED;
    });
  }

  /**
   * Group columns into study chains based on parent relationships
   */
  static groupIntoChains(
    columns: ColumnEntity[], 
    parentAttrField?: string
  ): StudyChain[] {
    if (!parentAttrField) {
      // No parent field configured - treat each column as standalone
      return columns.map(column => ({
        chainId: column.id,
        studies: [column],
        latestActiveStudy: column,
        isStandalone: true
      }));
    }

    // Build parent-child relationship map
    const parentToChildren = new Map<string, ColumnEntity[]>();
    const childToParent = new Map<string, string>();
    const allStudies = new Map<string, ColumnEntity>();
    
    // Index all studies
    columns.forEach(study => {
      allStudies.set(study.id, study);
      
      const parentId = this.extractParentId(study, parentAttrField);
      if (parentId) {
        childToParent.set(study.id, parentId);
        
        if (!parentToChildren.has(parentId)) {
          parentToChildren.set(parentId, []);
        }
        parentToChildren.get(parentId)!.push(study);
      }
    });

    // Find root studies (no parent or parent not in current dataset)
    const rootStudies = columns.filter(study => {
      const parentId = this.extractParentId(study, parentAttrField);
      return !parentId || !allStudies.has(parentId);
    });

    // Build chains starting from each root
    const chains: StudyChain[] = [];
    
    for (const root of rootStudies) {
      const chainStudies = this.buildChainFromRoot(root, parentToChildren);
      const latestActive = this.getLatestInChain(chainStudies);
      
      chains.push({
        chainId: root.id,
        studies: chainStudies,
        latestActiveStudy: latestActive,
        isStandalone: chainStudies.length === 1
      });
    }

    return chains;
  }

  /**
   * Extract parent ID from study entity using configured field
   */
  private static extractParentId(study: ColumnEntity, parentAttrField: string): string | undefined {
    // Try multiple possible field name patterns - filter out undefined values
    const possibleFields = [
      parentAttrField,
      `_${parentAttrField}_value`,
      `${parentAttrField}@odata.bind`,
      study.parentAttrId // Mapped field from DataService
    ].filter((field): field is string => field !== undefined && field !== null);

    for (const field of possibleFields) {
      if (field in study) {
        const value = study[field];
        if (value) {
          // Extract GUID from OData bind format if needed
          if (typeof value === 'string' && value.includes('(') && value.includes(')')) {
            const match = value.match(/\(([^)]+)\)/);
            return match ? match[1] : value;
          }
          return String(value);
        }
      }
    }

    return undefined;
  }

  /**
   * Build complete chain starting from root study
   */
  private static buildChainFromRoot(
    root: ColumnEntity,
    parentToChildren: Map<string, ColumnEntity[]>
  ): ColumnEntity[] {
    const chain: ColumnEntity[] = [root];
    const visited = new Set<string>([root.id]);

    // Recursively add all descendants
    const addChildren = (parentId: string) => {
      const children = parentToChildren.get(parentId) || [];
      
      for (const child of children) {
        if (!visited.has(child.id)) {
          visited.add(child.id);
          chain.push(child);
          addChildren(child.id); // Recursive for deeper chains
        }
      }
    };

    addChildren(root.id);
    return chain;
  }

  /**
   * Get the latest active study in a chain
   */
  static getLatestInChain(
    chainStudies: ColumnEntity[], 
    versionField?: string
  ): ColumnEntity {
    if (chainStudies.length === 1) {
      return chainStudies[0];
    }

    // Sort by version field if specified
    if (versionField) {
      const sortedStudies = [...chainStudies].sort((a, b) => {
        const valueA = a.versionValue || a[versionField];
        const valueB = b.versionValue || b[versionField];
        
        return this.compareVersionValues(valueA, valueB);
      });
      
      return sortedStudies[sortedStudies.length - 1]; // Return highest version
    }

    // Fallback: sort by creation date (newest first)
    const sortedByDate = [...chainStudies].sort((a, b) => {
      const dateA = a.createdDate || new Date(0);
      const dateB = b.createdDate || new Date(0);
      return dateB.getTime() - dateA.getTime();
    });

    return sortedByDate[0];
  }

  /**
   * Compare version values (handles numbers, dates, strings)
   */
  private static compareVersionValues(a: any, b: any): number {
    if (a === undefined && b === undefined) return 0;
    if (a === undefined) return -1;
    if (b === undefined) return 1;

    // Try numeric comparison first
    const numA = Number(a);
    const numB = Number(b);
    
    if (!isNaN(numA) && !isNaN(numB)) {
      return numA - numB;
    }

    // Try date comparison
    const dateA = new Date(a);
    const dateB = new Date(b);
    
    if (!isNaN(dateA.getTime()) && !isNaN(dateB.getTime())) {
      return dateA.getTime() - dateB.getTime();
    }

    // Fallback to string comparison
    return String(a).localeCompare(String(b));
  }

  /**
   * Check if a study status allows interaction (Draft only)
   */
  static isInteractable(status: StudyStatus): boolean {
    return status === STUDY_STATUS.DRAFT;
  }

  /**
   * Get CSS style class for column header based on status
   */
  static getStatusStyle(status: StudyStatus): string {
    switch (status) {
      case STUDY_STATUS.DRAFT:
        return 'column-header-draft'; // Default styling
      case STUDY_STATUS.READY_FOR_SCRIPTING:
        return 'column-header-ready';
      case STUDY_STATUS.APPROVED_FOR_LAUNCH:
        return 'column-header-approved';
      case STUDY_STATUS.REWORK:
        return 'column-header-rework';
      default:
        return 'column-header-disabled';
    }
  }

  /**
   * Debug helper - analyze study chains for troubleshooting
   */
  static analyzeChains(
    rawColumns: ColumnEntity[],
    config: MatrixConfig
  ): {
    totalStudies: number;
    abandonedStudies: number;
    activeStudies: number;
    totalChains: number;
    standaloneStudies: number;
    versionedChains: number;
    statusBreakdown: Record<string, number>;
  } {
    const activeColumns = this.filterByStatus(rawColumns);
    const chains = this.groupIntoChains(activeColumns, config.columnParentAttrField);
    
    const statusBreakdown: Record<string, number> = {};
    rawColumns.forEach(column => {
      const status = column.statuscode;
      const statusKey = status !== undefined ? String(status) : 'unknown';
      statusBreakdown[statusKey] = (statusBreakdown[statusKey] || 0) + 1;
    });

    return {
      totalStudies: rawColumns.length,
      abandonedStudies: rawColumns.length - activeColumns.length,
      activeStudies: activeColumns.length,
      totalChains: chains.length,
      standaloneStudies: chains.filter(c => c.isStandalone).length,
      versionedChains: chains.filter(c => !c.isStandalone).length,
      statusBreakdown
    };
  }
}