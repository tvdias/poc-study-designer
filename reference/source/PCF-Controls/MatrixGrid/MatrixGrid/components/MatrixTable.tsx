import { Checkbox, makeStyles, mergeClasses, Table, TableBody, TableCell, TableHeader, TableHeaderCell, TableRow, tokens, Tooltip } from "@fluentui/react-components";
import { MatrixTableProps } from "../models/props/MatrixTableProps";
import * as React from "react";
import { Info24Regular } from "@fluentui/react-icons";
import { MatrixUtils } from "../utils/MatrixUtils";
import { AssignmentCheckbox } from "./AssignmentCheckbox";

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
  headerCellEditable: {
    backgroundColor: `${tokens.colorNeutralBackground2} !important`,
  },
  headerCellDisabled: {
    backgroundColor: '#f8f6ff !important',
  },

  // FIXED: Header cell content alignment
  headerCellContent: {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  width: '100%',
  height: '100%'
  },
  // FIXED: Single sticky column (columns + bulk selection combined)
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

  // FIXED: Single sticky row cells (columns + checkbox combined)
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
  onRowToggleAll,
  onRowClick,
  onColumnClick,
  disabled
}) => {
    const styles = useStyles();

    const bulkSelectionTooltip = "Select rows for bulk operations";
    const headerLabel = "";

    const getColumnHeaderClass = React.useCallback((columnId: string): string =>
    {
        const isDisabled = columns
            .find(col => col.id === columnId)?.disabled ?? false;

        if (isDisabled) {
            return mergeClasses(styles.headerCell, styles.headerCellDisabled);
        } 

        return mergeClasses(styles.headerCell, styles.headerCellEditable);
    }, [styles]);

    const getRowClass = React.useCallback((rowId: string): string =>
    {
        return styles.rowHeaderCell;
    }, [styles]);

 
  const getRowCheckboxState = React.useCallback((rowId: string): { checked: boolean; isDisabled: boolean } => {
    const rowCells = columns.map(column => {
      const cellKey = MatrixUtils.generateCellKey(rowId, column.id);
      return {
        column,
        cellState: cellStates.get(cellKey)
      };
    }).filter(item => !!item.cellState);

    const interactableCells = rowCells.filter(item => !item.column.disabled);

    if (disabled || interactableCells.length === 0) {
      return { checked: false, isDisabled: true };
    }

    const checkedCount = interactableCells.filter(item => item.cellState?.isChecked).length;

    // checked only when all editable cells are selected
    return { checked: checkedCount === interactableCells.length, isDisabled: false };
  }, [cellStates, columns, disabled]);


    const isCellInteractable = React.useCallback((columnId: string): boolean => {
        return !disabled;
    }, [disabled]);

    // Calculate assignment counts per column to display in headers
    const columnAssignmentCount = React.useMemo(() => {
      const counts: Record<string, number> = {};
      columns.forEach(col => (counts[col.id] = 0));

      cellStates.forEach((state, key) => {
        if (state.isChecked) {
          // Extract columnId from key (after first 36 chars of rowId + dash)
          const columnId = key.substring(37);
          const match = columns.some(c => c.id === columnId);
          if (match) {
            counts[columnId] = (counts[columnId] || 0) + 1;
          }
        }
      });

      return counts;
    }, [cellStates, columns]);


    return (
        <div className={styles.tableContainer}>
            <Table className={styles.table} size="small">
                {/* Table Header */}
                <TableHeader>
                    <TableRow>
                        <TableHeaderCell className={styles.firstHeaderCell}>
                            <div className={styles.headerCellContent}>
                            <span>{headerLabel}</span>
                            
                            <Tooltip
                                content={bulkSelectionTooltip}
                                relationship="label"
                                positioning="below-start"
                            >
                                <div className={styles.headerInfoIcon}>
                                    <Info24Regular />
                                </div>
                            </Tooltip>
                            </div>
                        </TableHeaderCell>

                        {/* Column headers with disabled-aware styling */}
                        {columns.map(column => (
                          <TableHeaderCell
                            key={column.id}
                            className={getColumnHeaderClass(column.id)}
                            title={`Click to view ${column.name}`}
                            onClick={() => onColumnClick?.(column.id)}
                          >
                            <div className={styles.headerCellContentCenter}>
                              <span style={{ whiteSpace: "nowrap" }}>
                                {MatrixUtils.formatDisplayName(column.name, 20)}
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
                        return (
                        <TableRow key={row.id} className={styles.tableRow}>

                            {/* Single sticky cell with text + checkbox */}
                            <TableCell className={getRowClass(row.id)}>
                            <div
                                title={`Click to view ${row.name}`}
                                onClick={() => onRowClick?.(row.id)}
                                style={{
                                    cursor: 'pointer',
                                    flex: 1,
                                    minWidth: 0,
                                    whiteSpace: 'normal',   // allows wrapping
                                    wordBreak: 'break-word' // breaks long words if needed
                                }}
                            >
                                {row.name}
                            </div>

                            {/* Bulk selection - enabled if ANY column is editable */}
                            <div style={{ flexShrink: 0 }}>
                                {onRowToggleAll && (() => {
                                    const rowCheckboxState = getRowCheckboxState(row.id);
                                    return (
                                        <Checkbox
                                            checked={rowCheckboxState.checked}
                                            onChange={() => onRowToggleAll(row.id)}
                                            disabled={rowCheckboxState.isDisabled}
                                            aria-label={`Select all ${row.name} items in the row`}
                                            title={`Select or deselect all ${row.name} items in this row`}
                                        />
                                    );
                                })()}
                            </div>
                            </TableCell>

                            {/* Individual matrix cells - enabled/disabled per column */}
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