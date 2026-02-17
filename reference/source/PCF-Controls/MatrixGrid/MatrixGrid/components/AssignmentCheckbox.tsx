import { Checkbox, makeStyles, tokens, Tooltip } from "@fluentui/react-components";
import { AssignmentCheckboxProps } from "../models/props/AssignmentCheckboxProps";
import * as React from "react";
import { CheckboxStatus } from "../models/CheckboxStatus";

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
  disabled,
  isInteractable
}) => {
    const styles = useStyles();

     // Determine cell status for visual feedback
    const getCellStatus = React.useCallback((): CheckboxStatus => {
        if (cellState.hasConflict) return CheckboxStatus.Conflict;
        if (cellState.isModified) return CheckboxStatus.Pending;
        return CheckboxStatus.Normal;
    }, [cellState.hasConflict, cellState.isModified]);

    const getTooltipText = React.useCallback((): string =>
    {
        const status = getCellStatus();
        const baseText = `${cellState.isChecked ? 'Assigned' : 'Not assigned'}`;
        
        // Enhanced tooltips for non-interactable states
        if (!isInteractable) {
            const statusSuffix = (() => {
                switch (status) {
                    case CheckboxStatus.Pending:
                        return ' - Pending save';
                    case CheckboxStatus.Conflict:
                        return ' - Conflict detected';
                    default:
                        return ' - Cannot be modified';
                }
            })();
            
            return baseText + statusSuffix;
        }
        
        // Tooltips for interactable states
        switch (status) {
            case CheckboxStatus.Pending:
                return `${baseText} - Pending save (Click to toggle)`;
            case CheckboxStatus.Conflict:
                return `${baseText} - Conflict detected (Click to resolve)`;
            default:
                return `${baseText} (Click to toggle)`;
        }
    }, [getCellStatus, cellState.isChecked, isInteractable]);

    const getContainerClassName = React.useCallback((): string =>
    {
        const status = getCellStatus();
        
        // Handle non-interactable states (not in Draft status, for e.g.)
        if (!isInteractable) {
            switch (status) {
                case CheckboxStatus.Pending:
                    return `${styles.container} ${styles.containerPendingNonInteractable}`;
                case CheckboxStatus.Conflict:
                    return `${styles.container} ${styles.containerConflictNonInteractable}`;
                default:
                    return `${styles.container} ${styles.containerNonInteractable}`;
            }
        }
        
        // Handle interactable states (is editable)
        switch (status) {
            case CheckboxStatus.Pending:
                return `${styles.container} ${styles.containerPending}`;
            case CheckboxStatus.Conflict:
                return `${styles.container} ${styles.containerConflict}`;
            default:
                return `${styles.container} ${styles.containerNormal}`;
        }
    }, [getCellStatus, isInteractable, styles]);

    const effectivelyDisabled = disabled || !isInteractable;
    const getCheckboxClassName = React.useCallback((): string =>
    {
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

    const handleToggle = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        
        if (effectivelyDisabled) {
            event.preventDefault();
            return;
        }
        
        // Call parent toggle handler
        onToggle();
    }, [effectivelyDisabled, onToggle]);

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
                    checked={cellState.isChecked}
                    onChange={handleToggle}
                    disabled={effectivelyDisabled}
                    size="medium"
                    aria-invalid={cellState.hasConflict}
                    {...getAccessibilityProps()}
                />
            </div>
        </Tooltip>
  );
}