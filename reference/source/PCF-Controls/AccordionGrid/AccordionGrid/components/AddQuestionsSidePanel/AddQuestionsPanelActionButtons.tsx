/**
 * AddQuestionsPanelActionButtons.tsx
 * 
 * This component renders the Cancel and Save buttons in the footer of the side panel.
 */
import * as React from "react";
import { Button, makeStyles, shorthands } from "@fluentui/react-components";

interface PanelActionButtonsProps {
  onCancel: () => void;
  onSave: () => void;
}

const useStyles = makeStyles({
  container: {
    display: "flex",
    justifyContent: "flex-end",
    ...shorthands.gap("8px"),
    width: "100%",
  },
});

export const PanelActionButtons: React.FC<PanelActionButtonsProps> = ({ onCancel, onSave }) => {
  const styles = useStyles();

  return (
    <div className={styles.container}>
      <Button appearance="secondary" data-close-btn="true" onClick={onCancel}>
        Cancel
      </Button>
      <Button
        appearance="primary"
        onClick={onSave}
        style={{ backgroundColor: "#1E95FF", color: "white", border: "none" }}
      >
        Save
      </Button>
    </div>
  );
};
