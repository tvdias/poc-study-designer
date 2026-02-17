/**
 * RemoveModulesPanelActionButtons.tsx
 * 
 * This component renders remove and cancel buttons on Remove module panel
 */
import * as React from "react";
import {
  Button,
  makeStyles,
  shorthands,
  Dialog,
  DialogTrigger,
} from "@fluentui/react-components";
import { ConfirmDialog } from "./../ConfirmDialog";

interface PanelActionButtonsProps {
  onCancel: () => void;
  handleRemove: (e: React.MouseEvent<HTMLButtonElement>) => void;
  context: ComponentFramework.Context<any>;
  disabled?: boolean;
}

const useStyles = makeStyles({
  container: {
    display: "flex",
    justifyContent: "flex-end",
    ...shorthands.gap("8px"),
    width: "100%",
  },
});

export const PanelActionButtons: React.FC<PanelActionButtonsProps> = ({
  onCancel,
  handleRemove,
  context,
  disabled = false,
}) => {
  const styles = useStyles();

  return (
    <div className={styles.container}>
      <Button appearance="secondary" data-close-btn="true" onClick={onCancel}>
        Cancel
      </Button>

      <Dialog>
        <DialogTrigger disableButtonEnhancement>
          <Button
            appearance="primary"
            style={{
              backgroundColor: disabled ? "white" : "red",
              color: disabled ? "#666" : "white",
              border: "none"
            }}
            disabled={disabled}
          >
            Remove Selected
          </Button>
        </DialogTrigger>

        <ConfirmDialog
          context={context}
          dialogTitle="Remove Modules"
          dialogText="Do you really want to remove the selected Modules from the Questionnaire?"
          buttonPrimaryText="Yes"
          buttonSecondaryText="No"
          onPrimaryActionClick={handleRemove}
          onSecondaryActionClick={() => {}}
        />
      </Dialog>
    </div>
  );
};
