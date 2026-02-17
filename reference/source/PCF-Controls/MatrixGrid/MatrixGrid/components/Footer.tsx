import { Badge, Button, makeStyles, Spinner, tokens } from "@fluentui/react-components";
import { FooterProps } from "../models/props/FooterProps";
import * as React from "react";

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

export const Footer: React.FC<FooterProps> = ({
  pendingChangesCount,
  isSaving,
  onSave,
  onCancel,
  disabled
}) => {
    const styles = useStyles();

    const isSaveEnabled = pendingChangesCount > 0 && !isSaving && !disabled;
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