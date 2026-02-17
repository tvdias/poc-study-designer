import { ConfirmDialog } from "./ConfirmDialog";
import { SnapshotsDataService } from "../services/SnapshotsDataService";
import { DataService } from "../services/DataService";
import * as React from "react";
import { DeleteOrDeactivateProps } from "../models/props/DeleteOrDeactivateProps";
import { Spinner } from "@fluentui/react-components";
import { QuestionDataService } from "../services/QuestionDataService";
import { ProjectDataService } from "../services/ProjectDataService";
import { EntityHelper } from "../utils/EntityHelper";



export const DeleteOrDeactivate: React.FC<DeleteOrDeactivateProps & {
    onSuccess?: (isDeactivate: boolean | null) => void;
    onError?: (isDeactivate: boolean | null) => void;
  }
> = ({ context, questionId, onSuccess, onError }) => {
  const [isDeactivate, setIsDeactivate] = React.useState<boolean | null>(null);
  const [loading, setLoading] = React.useState(false);
  
  const projectId = EntityHelper.getProjectIdFromUrl();
  const ReorderProjectQuestionnaire = async (context: ComponentFramework.Context<any>) => {
      const projectDataService = new ProjectDataService(context, context.webAPI);
      console.log("Projectid for reorder: " + projectId);
      await projectDataService.reorderProjectQuestionnaire(projectId);
  };

// Run once when dialog opens
  React.useEffect(() => {
    const checkSnapshots = async () => {
      try {
        const service = new SnapshotsDataService();
        const snapshots = await service.getAssociatedSnapshots(questionId);
        setIsDeactivate(snapshots.length > 0);
      } catch (err) {
        console.error("Error checking snapshots:", err);
        setIsDeactivate(false);
      }
    };
    checkSnapshots();
  }, [questionId]);

  const handleDeactivate = async () => {
    try {
      setLoading(true);
      const service = new DataService(context.webAPI);
      let result = await service.inactivateRecord("kt_questionnairelines", questionId);
      if (result.success)
      {
        ReorderProjectQuestionnaire(context);
        context.parameters.gridDataSet.refresh();
        onSuccess?.(isDeactivate);
      }
      else {
        console.warn("deleteQuestionnaireLines returned failure:", result);
        onError?.(isDeactivate);
      }
    } catch {
      // Show error message popup
    onError?.(isDeactivate);
     }

    finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    try {
      setLoading(true);
      const service = new QuestionDataService(context.webAPI);
      let result = await service.deleteQuestionnaireLines(questionId);
      if (result.success)
      {
        ReorderProjectQuestionnaire(context);
        context.parameters.gridDataSet.refresh();
        onSuccess?.(isDeactivate);
      } else {
        console.warn("deleteQuestionnaireLines returned failure:", result);
        onError?.(isDeactivate);
      }
    } catch {
      onError?.(isDeactivate);
    }

    finally {
      setLoading(false);
    }
  };
   // Still loading decision
  if (isDeactivate === null) return null;

  return (
    <>

      {isDeactivate ? (
        <ConfirmDialog
          context={context}
          dialogTitle="Confirm Deactivation"
          dialogText="Do you want to deactivate the selected Questionnaire Line? You can reactivate it later, if you wish. This action will change the status of the selected Questionnaire Line to Inactive."
          buttonPrimaryText="Deactivate"
          buttonSecondaryText="Cancel"
          onPrimaryActionClick={handleDeactivate}
          onSecondaryActionClick={(e) => {
            e.stopPropagation();
          }}
        />
      ) : (
        <ConfirmDialog
          context={context}
          dialogTitle="Confirm Deletion"
          dialogText="Do you want to delete the selected Questionnaire Line? You can add it later, if you wish. This action will delete the Questionnaire Line."
          buttonPrimaryText="Delete"
          buttonSecondaryText="Cancel"
          onPrimaryActionClick={handleDelete}
          onSecondaryActionClick={(e) => {
            e.stopPropagation();
          }}
        />
      )}

      {loading && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: "rgba(0,0,0,0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 2147483647, // very high to overlay everything
          }}
        >
          <div
            style={{
              background: "white",
              padding: "20px 30px",
              borderRadius: "8px",
              boxShadow: "0 4px 12px rgba(1,1,1,1)",
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              gap: "12px",
            }}
          >
            <Spinner size="large" />
            <span style={{ fontSize: "16px", fontWeight: 500, color: "#333" }}>
              Processing...
            </span>
          </div>
        </div>
      )}
      
    </>
  );
};
