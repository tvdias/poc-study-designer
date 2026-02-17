/**
 * AssignmentCheckbox.tsx
 * 
 * This component renders a checkbox for assignment status with enhanced visual feedback,
 * tooltips, and accessibility features. It supports different states (normal, pending, conflict)
 * and handles interaction based on the study's draft status.
 */
import * as React from 'react';
import {
  Checkbox,
  makeStyles,
  tokens,
  Tooltip
} from '@fluentui/react-components';

import { AssignmentCheckboxProps, CellStatus } from '../types/MatrixTypes';

const useStyles = makeStyles({
  // Container styles optimized for Fluent UI V9
  container: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    position: 'relative',
    padding: '4px'
  },
  
  // Normal state - clean and minimal
  containerNormal: {
    borderRadius: tokens.borderRadiusSmall,
    border: `1px solid transparent`,
    padding: '2px',
    backgroundColor: 'transparent'
  },
  
  // Pending changes state - subtle yellow highlight
  containerPending: {
    borderRadius: tokens.borderRadiusSmall,
    backgroundColor: tokens.colorPaletteYellowBackground1,
    border: `2px solid ${tokens.colorPaletteDarkOrangeBorder1}`,
    padding: '2px'
  },
  
  // Conflict state - attention-grabbing red highlight
  containerConflict: {
    borderRadius: tokens.borderRadiusSmall, 
    backgroundColor: tokens.colorPaletteRedBackground1,
    border: `2px solid ${tokens.colorPaletteRedBorder1}`,
    padding: '2px'
  },
  
  // Non-interactable states (Study not in Draft status)
  containerNonInteractable: {
    borderRadius: tokens.borderRadiusSmall,
    border: `1px solid transparent`,
    padding: '2px',
    opacity: 0.6,
    backgroundColor: tokens.colorNeutralBackground3,
    cursor: 'not-allowed'
  },
  
  containerPendingNonInteractable: {
    borderRadius: tokens.borderRadiusSmall,
    backgroundColor: tokens.colorPaletteYellowBackground1,
    border: `2px solid ${tokens.colorPaletteDarkOrangeBorder1}`,
    padding: '2px',
    opacity: 0.6,
    cursor: 'not-allowed'
  },
  
  containerConflictNonInteractable: {
    borderRadius: tokens.borderRadiusSmall, 
    backgroundColor: tokens.colorPaletteRedBackground1,
    border: `2px solid ${tokens.colorPaletteRedBorder1}`,
    padding: '2px',
    opacity: 0.6,
    cursor: 'not-allowed'
  },
  
  // Enhanced checkbox styling for better visual feedback
  checkbox: {
    '&:hover': {
      transform: 'scale(1.05)',
      transition: 'transform 0.1s ease-in-out'
    },
    '&:focus-visible': {
      outline: `2px solid ${tokens.colorBrandStroke1}`,
      outlineOffset: '2px'
    }
  },
  
  checkboxDisabled: {
    cursor: 'not-allowed',
    '&:hover': {
      transform: 'none'
    }
  }
});

export const AssignmentCheckbox: React.FC<AssignmentCheckboxProps> = ({
  cellState,
  onToggle,
  disabled = false,
  isInteractable = true
}) => {
  const styles = useStyles();

  // Determine cell status for visual feedback
  const getCellStatus = React.useCallback((): CellStatus => {
    if (cellState.hasConflict) return CellStatus.Conflict;
    if (cellState.isModified) return CellStatus.Pending;
    return CellStatus.Normal;
  }, [cellState.hasConflict, cellState.isModified]);

  // Get appropriate container CSS class based on status and interactability
  const getContainerClassName = React.useCallback((): string => {
    const status = getCellStatus();
    
    // Handle non-interactable states (Study not in Draft status)
    if (!isInteractable) {
      switch (status) {
        case CellStatus.Pending:
          return `${styles.container} ${styles.containerPendingNonInteractable}`;
        case CellStatus.Conflict:
          return `${styles.container} ${styles.containerConflictNonInteractable}`;
        default:
          return `${styles.container} ${styles.containerNonInteractable}`;
      }
    }
    
    // Handle interactable states (Draft studies)
    switch (status) {
      case CellStatus.Pending:
        return `${styles.container} ${styles.containerPending}`;
      case CellStatus.Conflict:
        return `${styles.container} ${styles.containerConflict}`;
      default:
        return `${styles.container} ${styles.containerNormal}`;
    }
  }, [getCellStatus, isInteractable, styles]);

  // Get comprehensive tooltip text
  const getTooltipText = React.useCallback((): string => {
    const status = getCellStatus();
    const baseText = `${cellState.isAssigned ? 'Assigned' : 'Not assigned'}`;
    
    // Enhanced tooltips for non-interactable states
    if (!isInteractable) {
      const statusSuffix = (() => {
        switch (status) {
          case CellStatus.Pending:
            return ' - Pending save (Study not in Draft status)';
          case CellStatus.Conflict:
            return ' - Conflict detected (Study not in Draft status)';
          default:
            return ' - Study not in Draft status and cannot be modified';
        }
      })();
      
      return baseText + statusSuffix;
    }
    
    // Tooltips for interactable states
    switch (status) {
      case CellStatus.Pending:
        return `${baseText} - Pending save (Click to toggle)`;
      case CellStatus.Conflict:
        return `${baseText} - Conflict detected (Click to resolve)`;
      default:
        return `${baseText} (Click to toggle)`;
    }
  }, [getCellStatus, cellState.isAssigned, isInteractable]);

  // Determine effective disabled state
  const effectivelyDisabled = disabled || !isInteractable;

  // Enhanced toggle handler with validation
  const handleToggle = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    // Prevent toggle when disabled or non-interactable
    if (effectivelyDisabled) {
      event.preventDefault();
      return;
    }
    
    // Call parent toggle handler
    onToggle();
  }, [effectivelyDisabled, onToggle]);

  // Get appropriate checkbox CSS class
  const getCheckboxClassName = React.useCallback((): string => {
    return effectivelyDisabled ? 
      `${styles.checkbox} ${styles.checkboxDisabled}` : 
      styles.checkbox;
  }, [effectivelyDisabled, styles]);

  // Enhanced accessibility props (Fluent UI V9 compatible)
  const getAccessibilityProps = React.useCallback(() => {
    return {
      'aria-label': getTooltipText(),
      'aria-describedby': cellState.isModified ? `pending-${cellState.rowId}-${cellState.columnId}` : undefined,
      'data-testid': `assignment-checkbox-${cellState.rowId}-${cellState.columnId}`,
      'data-status': getCellStatus(),
      'data-interactable': isInteractable.toString()
    };
  }, [getTooltipText, cellState, getCellStatus, isInteractable]);

  return (
    <Tooltip 
      content={getTooltipText()} 
      relationship="description"
      positioning="above"
      hideDelay={100}
      showDelay={300}
    >
      <div className={getContainerClassName()}>
        <Checkbox
          className={getCheckboxClassName()}
          checked={cellState.isAssigned}
          onChange={handleToggle}
          disabled={effectivelyDisabled}
          size="medium"
          aria-invalid={cellState.hasConflict}
          {...getAccessibilityProps()}
        />
      </div>
    </Tooltip>
  );
};