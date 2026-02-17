/**
 * MatrixTable.tsx
 * This code provides the ability to render a matrix table with sticky headers and row labels, displaying assignment checkboxes 
 * for each row-column intersection with status-aware styling and interactability controls. It supports bulk row selection for 
 * draft studies and provides navigation functionality to view individual records.
 */
import * as React from 'react';
import {
  Table,
  TableHeader,
  TableHeaderCell,
  TableBody,
  TableRow,
  TableCell,
  makeStyles,
  tokens,
  Checkbox,
  Tooltip,
  mergeClasses
} from '@fluentui/react-components';
import { Info24Regular } from '@fluentui/react-icons';

import { MatrixTableProps, STUDY_STATUS, StudyStatus } from '../types/MatrixTypes';
import { AssignmentCheckbox } from './AssignmentCheckbox';
import { MatrixUtils } from '../utils/MatrixUtils';

const useStyles = makeStyles({
  tableContainer: {
    flex: 1,
    overflow: 'auto',
    border: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRadius: tokens.borderRadiusMedium,
    maxHeight: '500px',
    position: 'relative'
  },

  table: {
    width: 'max-content',
    minWidth: '100%',
    borderCollapse: 'separate',
    borderSpacing: 0,
    tableLayout: 'fixed'
  },

  // Standard column headers
  headerCell: {
    backgroundColor: `${tokens.colorNeutralBackground2} !important`,
    fontWeight: tokens.fontWeightSemibold,
    padding: '8px 12px',
    textAlign: 'center',
    minWidth: '120px',
    width: 'auto',
    maxWidth: '1fr',
    position: 'sticky',
    top: '0px !important',
    zIndex: 2,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: `${tokens.colorNeutralBackground2Hover} !important`,
      color: tokens.colorBrandForeground1
    }
  },

  // Status-aware header styling
  headerCellDraft: {
    backgroundColor: `${tokens.colorNeutralBackground2} !important`,
  },
  headerCellReadyForScripting: {
    backgroundColor: '#f8f6ff !important',
  },
  headerCellApprovedForLaunch: {
    backgroundColor: '#f0eeff !important',
  },

  // FIXED: Header cell content alignment
  headerCellContent: {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  width: '100%',
  height: '100%'
  },
  // FIXED: Single sticky column (Studies + bulk selection combined)
   firstHeaderCell: {
    backgroundColor: tokens.colorNeutralBackground2,
    fontWeight: tokens.fontWeightSemibold,
    padding: '8px 12px',
    textAlign: 'left',
    minWidth: '260px', //updated
    width: '260px', // updated
    maxWidth: '260px', // Add this for consistency
    position: 'sticky',
    left: 0,
    top: 0,
    zIndex: 3,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRight: `2px solid ${tokens.colorNeutralStroke1}`
  },

  headerInfoIcon: {
    cursor: 'help',
    color: tokens.colorNeutralForeground2,
    flexShrink: 0,
    '&:hover': {
      color: tokens.colorBrandForeground1
    }
  },

  // FIXED: Single sticky row cells (Studies + checkbox combined)
  rowHeaderCell: {
    backgroundColor: `${tokens.colorNeutralBackground1} !important`,
    fontWeight: tokens.fontWeightMedium,
    padding: '8px 12px',
    textAlign: 'left',
    position: 'sticky',
    left: '0px !important',
    zIndex: 998,
    minWidth: '260px',
    width: '260px',
    maxWidth: '260px',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRight: `2px solid ${tokens.colorNeutralStroke2}`,
    cursor: 'pointer',

    // AGGRESSIVE: Solid background layers
    boxShadow: `
      4px 0 8px rgba(0, 0, 0, 0.15), 
      inset 0 0 0 1000px ${tokens.colorNeutralBackground1}
    `,

    opacity: '1 !important',
    isolation: 'isolate',

    // FLEXBOX: Text left, checkbox right
    display: 'flex !important',
    alignItems: 'center',
    justifyContent: 'space-between',

    '&:hover': {
      backgroundColor: `${tokens.colorNeutralBackground1Hover} !important`,
      color: tokens.colorBrandForeground1,
      boxShadow: `
        4px 0 8px rgba(0, 0, 0, 0.15), 
        inset 0 0 0 1000px ${tokens.colorNeutralBackground1Hover}
      `,
    }
  },

  rowHeaderCellNoModify: {
    backgroundColor: `${tokens.colorNeutralBackground1} !important`,
    fontWeight: tokens.fontWeightMedium,
    padding: '8px 12px',
    textAlign: 'left',
    position: 'sticky',
    left: '0px !important',
    zIndex: 998,
    minWidth: '260px',
    width: '260px',
    maxWidth: '260px',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRight: `2px solid ${tokens.colorNeutralStroke2}`,
    cursor: 'pointer',
    opacity: '0.6 !important',

    // AGGRESSIVE: Background with opacity
    boxShadow: `
      4px 0 8px rgba(0, 0, 0, 0.15), 
      inset 0 0 0 1000px rgba(255, 255, 255, 0.6)
    `,

    isolation: 'isolate',

    // FLEXBOX: Text left, checkbox right
    display: 'flex !important',
    alignItems: 'center',
    justifyContent: 'space-between',

    '&:hover': {
      backgroundColor: `${tokens.colorNeutralBackground1Hover} !important`,
      color: tokens.colorBrandForeground1,
      boxShadow: `
        4px 0 8px rgba(0, 0, 0, 0.15), 
        inset 0 0 0 1000px rgba(248, 248, 248, 0.6)
      `,
    }
  },

  checkboxCell: {
    textAlign: 'center',
    padding: '4px',
    minWidth: '120px',
    width: 'auto',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRight: `1px solid ${tokens.colorNeutralStroke2}`
  },

  tableRow: {
    '&:hover': {
      // Don't let row hover affect sticky cells
      '& .fui-TableCell:not([class*="rowHeaderCell"])': {
        backgroundColor: tokens.colorSubtleBackgroundHover
      }
    }
  },

  emptyState: {
    textAlign: 'center',
    padding: '40px 20px',
    color: tokens.colorNeutralForeground2,
    fontSize: tokens.fontSizeBase300
  },

  headerCellContentCenter: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    height: '100%',
    textAlign: 'center'
  }

});

export const MatrixTable: React.FC<MatrixTableProps> = ({
  rows,
  columns,
  cellStates,
  onCellToggle,
  onRowClick,
  onColumnClick,
  disabled = false,
  bulkSelection,
  onBulkRowToggle,
  onBulkSelectAll,
  onBulkClearAll,
  bulkTooltipText,
  getCellInteractable,
  getColumnStatus,
  getColumnHeaderStyle
}) => {
  const styles = useStyles();

  // FIXED: Get Draft status columns correctly
  const draftColumns = React.useMemo((): string[] => {
    if (!getColumnStatus) {
      // If no status function, assume all are draft for bulk selection purposes
      return columns.map(c => c.id);
    }

    return columns
      .filter(column => {
        const status = getColumnStatus(column.id);
        // Only Draft studies are modifiable
        return status === STUDY_STATUS.DRAFT || status === undefined;
      })
      .map(column => column.id);
  }, [columns, getColumnStatus]);

  // FIXED: Check if ANY draft studies exist in the view (for bulk selection)
  const hasDraftStudiesInView = React.useMemo((): boolean => {
    return draftColumns.length > 0;
  }, [draftColumns]);

  // Helper function to get column header class
  const getColumnHeaderClass = React.useCallback((columnId: string): string => {
    if (!getColumnStatus) return styles.headerCell;

    const status = getColumnStatus(columnId);

    if (status === STUDY_STATUS.READY_FOR_SCRIPTING) {
      return mergeClasses(styles.headerCell, styles.headerCellReadyForScripting);
    } else if (status === STUDY_STATUS.APPROVED_FOR_LAUNCH) {
      return mergeClasses(styles.headerCell, styles.headerCellApprovedForLaunch);
    } else if (status === STUDY_STATUS.REWORK) {
      return mergeClasses(styles.headerCell, styles.headerCellApprovedForLaunch);
    }
    

    return mergeClasses(styles.headerCell, styles.headerCellDraft);
  }, [getColumnStatus, styles]);

  // FIXED: Cell interactability based on specific column status
  const isCellInteractable = React.useCallback((columnId: string): boolean => {
    if (!getCellInteractable) {
      // If no function provided, check status directly
      if (!getColumnStatus) return true; // Default to interactable

      const status = getColumnStatus(columnId);
      return status === STUDY_STATUS.DRAFT || status === undefined;
    }
    return getCellInteractable(columnId);
  }, [getCellInteractable, getColumnStatus]);

  // Calculate assignment counts per column to display in headers
  const columnAssignmentCount = React.useMemo(() => {
    const counts: Record<string, number> = {};
    columns.forEach(col => (counts[col.id] = 0));

    cellStates.forEach((state, key) => {
      if (state.isAssigned) {

        // rowId = first 36 chars (GUID length)
        const rowId = key.substring(0, 36);
        // +1 skip the dash
        const columnId = key.substring(37);
        const match = columns.some(c => c.id === columnId);

        if (match) {
          counts[columnId] = (counts[columnId] || 0) + 1;
        }
      }
    });

    return counts;
  }, [cellStates, columns]);


  // Handle empty states
  if (rows.length === 0 && columns.length === 0) {
    return (
      <div className={styles.tableContainer}>
        <div className={styles.emptyState}>
          No data available. Please check your configuration.
        </div>
      </div>
    );
  }

  if (rows.length === 0) {
    return (
      <div className={styles.tableContainer}>
        <div className={styles.emptyState}>
          No rows match your search criteria.
        </div>
      </div>
    );
  }

  if (columns.length === 0) {
    return (
      <div className={styles.tableContainer}>
        <div className={styles.emptyState}>
          No columns match your search criteria.
        </div>
      </div>
    );
  }


  return (
    <div className={styles.tableContainer}>
      <Table className={styles.table} size="small">

        {/* Table Header */}
        <TableHeader>
          <TableRow>
            {/* FIXED: Single sticky header with Studies + info icon */}
            <TableHeaderCell className={styles.firstHeaderCell}>
              <div className={styles.headerCellContent}>
              <span>Studies</span>
              {bulkSelection && bulkTooltipText && (
                <Tooltip
                  content={`${bulkTooltipText}${hasDraftStudiesInView ? ' (Draft studies available)' : ' (No Draft studies in view)'}`}
                  relationship="label"
                  positioning="below-start"
                >
                  <div className={styles.headerInfoIcon}>
                    <Info24Regular />
                  </div>
                </Tooltip>
              )}
              {bulkSelection && !bulkTooltipText && (
                <Tooltip
                  content={hasDraftStudiesInView
                    ? "Select rows for bulk operations on Draft studies only"
                    : "No Draft studies available for bulk operations"
                  }
                  relationship="label"
                  positioning="below-start"
                >
                  <div className={styles.headerInfoIcon}>
                    <Info24Regular />
                  </div>
                </Tooltip>
              )}
               </div>
            </TableHeaderCell>

            {/* Column headers with status-aware styling */}
            {columns.map(column => (
              <TableHeaderCell
                key={column.id}
                className={getColumnHeaderClass(column.id)}
                title={`Click to view ${column.displayName}`}
                onClick={() => onColumnClick?.(column.id)}
              >
                <div className={styles.headerCellContentCenter}>
                  <span style={{ whiteSpace: "nowrap" }}>
                    {MatrixUtils.formatDisplayName(column.displayName, 20)}
                  </span>
                  <span style={{ fontSize: "13px", opacity: 0.7, whiteSpace: "nowrap" }}>
                  {columnAssignmentCount[column.id] ?? 0} Selected
                  </span>
                </div>
              </TableHeaderCell>
            ))}
          </TableRow>
        </TableHeader>

        {/* Table Body */}
        <TableBody>
          {rows.map(row => {
            // Visual styling: show dimmed if no draft studies available
            const rowHeaderClass = hasDraftStudiesInView ? styles.rowHeaderCell : styles.rowHeaderCellNoModify;

            return (
              <TableRow key={row.id} className={styles.tableRow}>

                {/* FIXED: Single sticky cell with text + checkbox */}
                <TableCell className={rowHeaderClass}>
                  <div
                    title={`Click to view ${row.displayName}`}
                    onClick={() => onRowClick?.(row.id)}
                    style={{
                    cursor: 'pointer',
                    flex: 1,
                    minWidth: 0,
                    whiteSpace: 'normal',   // allows wrapping
                    wordBreak: 'break-word' // breaks long words if needed
                  }}
                  >
                    {row.displayName || 'Unnamed'}
                  </div>

                  {/* FIXED: Bulk selection - enabled if ANY draft studies exist */}
                  {bulkSelection && (
                    <div style={{ flexShrink: 0 }}>
                      <Checkbox
                        checked={bulkSelection.selectedRowIds.has(row.id)}
                        onChange={() => onBulkRowToggle?.(row.id)}
                        disabled={disabled || !hasDraftStudiesInView}
                        aria-label={hasDraftStudiesInView
                          ? `Select ${row.displayName} for bulk operations on Draft studies`
                          : `No Draft studies available - bulk selection disabled`
                        }
                        title={hasDraftStudiesInView
                          ? `Select ${row.displayName} for bulk operations (will only affect Draft studies)`
                          : `No Draft studies in current view - bulk selection disabled`
                        }
                      />
                    </div>
                  )}
                </TableCell>

                {/* FIXED: Individual matrix cells - enabled/disabled per column status */}
                {columns.map(column => {
                  const cellKey = MatrixUtils.generateCellKey(row.id, column.id);
                  const cellState = cellStates.get(cellKey);
                  const isInteractable = isCellInteractable(column.id);

                  return (
                    <TableCell
                      key={cellKey}
                      className={styles.checkboxCell}
                    >
                      {cellState && (
                        <AssignmentCheckbox
                          cellState={cellState}
                          onToggle={() => onCellToggle(row.id, column.id)}
                          disabled={disabled}
                          isInteractable={isInteractable}
                        />
                      )}
                    </TableCell>
                  );
                })}
              </TableRow>
            );
          })}
        </TableBody>

      </Table>
    </div>
  );
};