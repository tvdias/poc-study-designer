/**
 * VirtualizedMatrixTable.tsx
 * 
 * DO NOT USE - 
 */
import * as React from 'react';
import {
  makeStyles,
  tokens
} from '@fluentui/react-components';

import { MatrixTableProps } from '../types/MatrixTypes';
import { AssignmentCheckbox } from './AssignmentCheckbox';
import { MatrixUtils } from '../utils/MatrixUtils';

const useStyles = makeStyles({
  virtualContainer: {
    flex: 1,
    overflow: 'auto',
    border: `1px solid ${tokens.colorNeutralStroke2}`,
    borderRadius: tokens.borderRadiusMedium,
    maxHeight: '600px',
    position: 'relative',
    backgroundColor: tokens.colorNeutralBackground1
  },
  virtualContent: {
    position: 'relative'
  },
  stickyHeader: {
    position: 'sticky',
    top: 0,
    left: 0,
    zIndex: 10,
    backgroundColor: tokens.colorNeutralBackground2,
    borderBottom: `2px solid ${tokens.colorNeutralStroke1}`,
    display: 'flex'
  },
  cornerCell: {
    minWidth: '200px',
    width: '200px',
    padding: '12px',
    fontWeight: tokens.fontWeightSemibold,
    borderRight: `2px solid ${tokens.colorNeutralStroke1}`,
    backgroundColor: tokens.colorNeutralBackground2,
    position: 'sticky',
    left: 0,
    zIndex: 11
  },
  headerCell: {
    minWidth: '120px',
    width: '120px',
    padding: '8px 4px',
    textAlign: 'center',
    fontWeight: tokens.fontWeightSemibold,
    fontSize: tokens.fontSizeBase200,
    borderRight: `1px solid ${tokens.colorNeutralStroke2}`,
    backgroundColor: tokens.colorNeutralBackground2,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap'
  },
  virtualRow: {
    position: 'absolute',
    left: 0,
    display: 'flex',
    alignItems: 'center',
    height: '40px',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    '&:hover': {
      backgroundColor: tokens.colorSubtleBackgroundHover
    }
  },
  rowHeader: {
    minWidth: '200px',
    width: '200px',
    padding: '8px 12px',
    fontWeight: tokens.fontWeightMedium,
    backgroundColor: tokens.colorNeutralBackground1,
    borderRight: `2px solid ${tokens.colorNeutralStroke1}`,
    position: 'sticky',
    left: 0,
    zIndex: 2,
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap'
  },
  virtualCell: {
    minWidth: '120px',
    width: '120px',
    height: '40px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    borderRight: `1px solid ${tokens.colorNeutralStroke2}`,
    backgroundColor: tokens.colorNeutralBackground1
  },
  scrollIndicator: {
    position: 'absolute',
    bottom: '10px',
    right: '10px',
    padding: '4px 8px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRadius: tokens.borderRadiusSmall,
    fontSize: tokens.fontSizeBase100,
    color: tokens.colorNeutralForeground2,
    pointerEvents: 'none',
    zIndex: 15,
    border: `1px solid ${tokens.colorNeutralStroke1}`
  },
  emptyState: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    height: '200px',
    color: tokens.colorNeutralForeground2,
    fontSize: tokens.fontSizeBase300
  }
});

// Configuration for virtualization
const ROW_HEIGHT = 40;
const COLUMN_WIDTH = 120;
const HEADER_HEIGHT = 50;
const OVERSCAN = 5; // Render extra rows/columns for smooth scrolling

export const VirtualizedMatrixTable: React.FC<MatrixTableProps> = ({
  rows,
  columns,
  cellStates,
  onCellToggle,
  disabled = false
}) => {
  const styles = useStyles();
  const containerRef = React.useRef<HTMLDivElement>(null);
  
  const [scrollState, setScrollState] = React.useState({
    scrollTop: 0,
    scrollLeft: 0,
    containerHeight: 600,
    containerWidth: 800
  });

  // Calculate visible ranges
  const visibleStartRow = Math.floor(scrollState.scrollTop / ROW_HEIGHT);
  const visibleEndRow = Math.min(
    rows.length - 1,
    visibleStartRow + Math.ceil(scrollState.containerHeight / ROW_HEIGHT) + OVERSCAN
  );

  const visibleStartCol = Math.floor(scrollState.scrollLeft / COLUMN_WIDTH);
  const visibleEndCol = Math.min(
    columns.length - 1,
    visibleStartCol + Math.ceil(scrollState.containerWidth / COLUMN_WIDTH) + OVERSCAN
  );

  // Handle scroll events
  const handleScroll = React.useCallback((e: React.UIEvent<HTMLDivElement>) => {
    const target = e.target as HTMLDivElement;
    setScrollState(prev => ({
      ...prev,
      scrollTop: target.scrollTop,
      scrollLeft: target.scrollLeft
    }));
  }, []);

  // Update container dimensions
  React.useLayoutEffect(() => {
    if (containerRef.current) {
      const updateDimensions = () => {
        const rect = containerRef.current!.getBoundingClientRect();
        setScrollState(prev => ({
          ...prev,
          containerHeight: rect.height - HEADER_HEIGHT,
          containerWidth: rect.width - 200 // Subtract row header width
        }));
      };

      updateDimensions();
      window.addEventListener('resize', updateDimensions);
      return () => window.removeEventListener('resize', updateDimensions);
    }
  }, []);

  // Handle empty states
  if (rows.length === 0 && columns.length === 0) {
    return (
      <div className={styles.virtualContainer}>
        <div className={styles.emptyState}>
          No data available. Please check your configuration.
        </div>
      </div>
    );
  }

  if (rows.length === 0) {
    return (
      <div className={styles.virtualContainer}>
        <div className={styles.emptyState}>
          No rows match your search criteria.
        </div>
      </div>
    );
  }

  if (columns.length === 0) {
    return (
      <div className={styles.virtualContainer}>
        <div className={styles.emptyState}>
          No columns match your search criteria.
        </div>
      </div>
    );
  }

  const totalHeight = rows.length * ROW_HEIGHT + HEADER_HEIGHT;
  const totalWidth = 200 + (columns.length * COLUMN_WIDTH); // Row header + columns

  return (
    <div 
      ref={containerRef}
      className={styles.virtualContainer} 
      onScroll={handleScroll}
    >
      {/* Virtual Content Container */}
      <div 
        className={styles.virtualContent}
        style={{ 
          height: totalHeight,
          width: totalWidth
        }}
      >
        
        {/* Sticky Headers */}
        <div className={styles.stickyHeader}>
          {/* Corner cell */}
          <div className={styles.cornerCell}>
            Questions / Studies
          </div>
          
          {/* Column headers - only render visible ones */}
          <div style={{ display: 'flex', marginLeft: scrollState.scrollLeft * -1 }}>
            {columns.slice(visibleStartCol, visibleEndCol + 1).map((column, index) => {
              const actualIndex = visibleStartCol + index;
              return (
                <div
                  key={column.id}
                  className={styles.headerCell}
                  title={column.displayName}
                  style={{ left: actualIndex * COLUMN_WIDTH }}
                >
                  {MatrixUtils.formatDisplayName(column.displayName, 12)}
                </div>
              );
            })}
          </div>
        </div>

        {/* Virtual Rows - only render visible ones */}
        {rows.slice(visibleStartRow, visibleEndRow + 1).map((row, rowIndex) => {
          const actualRowIndex = visibleStartRow + rowIndex;
          const rowTop = HEADER_HEIGHT + (actualRowIndex * ROW_HEIGHT);
          
          return (
            <div
              key={row.id}
              className={styles.virtualRow}
              style={{ top: rowTop }}
            >
              {/* Row header */}
              <div 
                className={styles.rowHeader}
                title={row.displayName}
              >
                {MatrixUtils.formatDisplayName(row.displayName, 25)}
              </div>

              {/* Virtual cells - only render visible ones */}
              <div style={{ display: 'flex', marginLeft: scrollState.scrollLeft * -1 }}>
                {columns.slice(visibleStartCol, visibleEndCol + 1).map((column, colIndex) => {
                  const actualColIndex = visibleStartCol + colIndex;
                  const cellKey = MatrixUtils.generateCellKey(row.id, column.id);
                  const cellState = cellStates.get(cellKey);
                  
                  return (
                    <div
                      key={cellKey}
                      className={styles.virtualCell}
                      style={{ left: actualColIndex * COLUMN_WIDTH }}
                    >
                      {cellState && (
                        <AssignmentCheckbox
                          cellState={cellState}
                          onToggle={() => onCellToggle(row.id, column.id)}
                          disabled={disabled}
                        />
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}

      </div>

      {/* Scroll Position Indicator */}
      <div className={styles.scrollIndicator}>
        Row {visibleStartRow + 1}-{visibleEndRow + 1} / {rows.length} • 
        Col {visibleStartCol + 1}-{visibleEndCol + 1} / {columns.length}
      </div>
    </div>
  );
};