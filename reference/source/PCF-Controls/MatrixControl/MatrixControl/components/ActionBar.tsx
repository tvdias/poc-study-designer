/**
 * This code provides the ability to display a save/cancel action bar with real-time status tracking 
 * and conditional button states. It manages pending changes count display, saving state indicators, 
 * and automatically enables/disables action buttons based on current operation status.
 */
import * as React from 'react';
import {
  Button,
  Badge,
  Spinner,
  MessageBar,
  makeStyles,
  tokens
} from '@fluentui/react-components';

import { ActionBarProps } from '../types/MatrixTypes';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '16px 0',
    borderTop: `1px solid ${tokens.colorNeutralStroke2}`,
    gap: '16px'
  },
  leftSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px'
  },
  rightSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px'
  },
  statusText: {
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground2
  },
  buttonWithBadge: {
    position: 'relative'
  },
  badge: {
    position: 'absolute',
    top: '-6px',
    right: '-6px',
    minWidth: '18px',
    height: '18px'
  },
  savingContainer: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    color: tokens.colorNeutralForeground2,
    fontSize: tokens.fontSizeBase200
  }
});

/**
 * Renders an action bar with status information and action buttons for saving or cancelling changes.
 *
 * @param pendingChangesCount - The number of unsaved changes.
 * @param isSaving - Indicates whether a save operation is currently in progress.
 * @param error - An optional error message to display (not currently rendered).
 * @param onSave - Callback invoked when the "Save Changes" button is clicked.
 * @param onCancel - Callback invoked when the "Cancel Changes" button is clicked.
 * @param disabled - Optional flag to disable all actions and buttons (defaults to false).
 *
 * The component displays:
 * - A status message indicating the number of unsaved changes or that all changes are saved.
 * - A spinner and message when saving is in progress.
 * - "Cancel Changes" and "Save Changes" buttons, which are enabled only when there are pending changes and not saving.
 * - A badge on the "Save Changes" button showing the number of pending changes.
 */
export const ActionBar: React.FC<ActionBarProps> = ({
  pendingChangesCount,
  isSaving,
  error,
  onSave,
  onCancel,
  disabled = false
}) => {
  const styles = useStyles();

  // Determine if save button should be enabled
  const isSaveEnabled = pendingChangesCount > 0 && !isSaving && !disabled;
  
  // Determine if cancel button should be enabled
  const isCancelEnabled = pendingChangesCount > 0 && !isSaving && !disabled;

  return (
    <div className={styles.container}>
      
      {/* Left Section - Status Information */}
      <div className={styles.leftSection}>
        
        {/* Saving Status */}
        {isSaving && (
          <div className={styles.savingContainer}>
            <Spinner size="tiny" />
            <span>Saving changes...</span>
          </div>
        )}

        {/* Pending Changes Status */}
        {!isSaving && (
          <span className={styles.statusText}>
            {pendingChangesCount > 0 
              ? `${pendingChangesCount} unsaved change${pendingChangesCount === 1 ? '' : 's'}`
              : 'All changes saved'
            }
          </span>
        )}

      </div>

      {/* Right Section - Action Buttons */}
      <div className={styles.rightSection}>
        
        {/* Cancel Button */}
        <Button
          appearance="secondary"
          onClick={onCancel}
          disabled={!isCancelEnabled}
        >
          Cancel Changes
        </Button>

        {/* Save Button with Badge */}
        <div className={styles.buttonWithBadge}>
          <Button
            appearance="primary"
            onClick={onSave}
            disabled={!isSaveEnabled}
          >
            Save Changes
          </Button>
          
          {/* Badge showing pending changes count */}
          {pendingChangesCount > 0 && !isSaving && (
            <Badge
              className={styles.badge}
              appearance="filled"
              color="important"
              size="small"
            >
              {pendingChangesCount}
            </Badge>
          )}
        </div>

      </div>

    </div>
  );
};