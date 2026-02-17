import * as React from "react";
import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
import { DeleteOrDeactivate } from "../../AccordionGrid/components/DeleteOrDeactivateQuestions";
import { SnapshotsDataService } from "../../AccordionGrid/services/SnapshotsDataService";
import { DataService } from "../../AccordionGrid/services/DataService";
import { QuestionDataService } from "../../AccordionGrid/services/QuestionDataService";
import { MocksHelper } from "../helpers/MocksHelper";


// Mock ConfirmDialog to render two buttons with deterministic testids.
// It forwards received props so DeleteOrDeactivate still controls the flow.
jest.mock("../../AccordionGrid/components/ConfirmDialog", () => ({
  ConfirmDialog: (props: any) => {
    return (
      <div data-testid="mock-confirm-dialog">
        <div data-testid="mock-dialog-title">{props.dialogTitle}</div>
        <div data-testid="mock-dialog-text">{props.dialogText}</div>

        {/* Primary button */}
        <button
          data-testid="confirm-primary-button"
          onClick={(e) => {
            e.stopPropagation();
            // Allow ConfirmDialog consumer to provide onPrimaryActionClick
            if (typeof props.onPrimaryActionClick === "function") {
              props.onPrimaryActionClick(e);
            }
          }}
        >
          {props.buttonPrimaryText || "Confirm"}
        </button>

        {/* Secondary button */}
        <button
          data-testid="confirm-secondary-button"
          onClick={(e) => {
            e.stopPropagation();
            if (typeof props.onSecondaryActionClick === "function") {
              props.onSecondaryActionClick(e);
            }
          }}
        >
          {props.buttonSecondaryText || "Cancel"}
        </button>
      </div>
    );
  },
}));

// --- Mock ProjectDataService ---
const mockReorderProjectQuestionnaire = jest.fn();
jest.mock("../../AccordionGrid/services/ProjectDataService", () => {
  return {
    ProjectDataService: jest.fn().mockImplementation(() => ({
      reorderProjectQuestionnaire: mockReorderProjectQuestionnaire,
    })),
  };
});

describe("DeleteOrDeactivate", () => {
  const mockContext = MocksHelper.getMockContext();

  beforeEach(() => {
    jest.restoreAllMocks();
    jest.clearAllMocks();
  });

  it("renders deactivation dialog when snapshots exist", async () => {
    // snapshot check returns non-empty -> deactivation flow
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([{}]);

    render(<DeleteOrDeactivate context={mockContext} questionId="q1" />);

    // Wait for the mocked dialog to appear with deactivation title
    const title = await screen.findByTestId("mock-dialog-title");
    expect(title).toHaveTextContent("Confirm Deactivation");

    // Primary button should show "Deactivate"
    const primary = screen.getByTestId("confirm-primary-button");
    expect(primary).toHaveTextContent("Deactivate");
  });

  it("renders deletion dialog when no snapshots exist", async () => {
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([]);

    render(<DeleteOrDeactivate context={mockContext} questionId="q1" />);

    const title = await screen.findByTestId("mock-dialog-title");
    expect(title).toHaveTextContent("Confirm Deletion");

    const primary = screen.getByTestId("confirm-primary-button");
    expect(primary).toHaveTextContent("Delete");
  });

  it("calls inactivateRecord and triggers onSuccess with true when deactivate succeeds", async () => {
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([{}]); // go to deactivate branch
    const inactivateSpy = jest
      .spyOn(DataService.prototype, "inactivateRecord")
      .mockResolvedValue({ success: true });

    const onSuccess = jest.fn();
    render(
      <DeleteOrDeactivate
        context={mockContext}
        questionId="q1"
        onSuccess={onSuccess}
      />
    );

    // Wait for dialog to render and click primary
    await screen.findByTestId("mock-dialog-title");
    fireEvent.click(screen.getByTestId("confirm-primary-button"));

    await waitFor(() => {
      expect(inactivateSpy).toHaveBeenCalledWith("kt_questionnairelines", "q1");
      expect(mockContext.parameters.gridDataSet.refresh).toHaveBeenCalled();
      expect(onSuccess).toHaveBeenCalledWith(true);
    });
  });

  it("calls deleteQuestionnaireLines and triggers onSuccess with false when delete succeeds", async () => {
    // make snapshots empty -> delete branch
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([]);

    // spy on deleteQuestionnaireLines
    const deleteSpy = jest
      .spyOn(QuestionDataService.prototype, "deleteQuestionnaireLines")
      .mockResolvedValue({ success: true });

    const onSuccess = jest.fn();
    render(
      <DeleteOrDeactivate
        context={mockContext}
        questionId="q1"
        onSuccess={onSuccess}
      />
    );

    await screen.findByTestId("mock-dialog-title");
    fireEvent.click(screen.getByTestId("confirm-primary-button"));

    await waitFor(() => {
      expect(deleteSpy).toHaveBeenCalledWith("q1");
      expect(mockContext.parameters.gridDataSet.refresh).toHaveBeenCalled();
      expect(onSuccess).toHaveBeenCalledWith(false);
    });
  });

  it("calls onError(true) when inactivateRecord returns failure", async () => {
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([{}]); // deactivate path

    jest
      .spyOn(DataService.prototype, "inactivateRecord")
      .mockResolvedValue({ success: false }); // simulate service failure

    const onError = jest.fn();
    render(
      <DeleteOrDeactivate context={mockContext} questionId="q1" onError={onError} />
    );

    await screen.findByTestId("mock-dialog-title");
    fireEvent.click(screen.getByTestId("confirm-primary-button"));

    await waitFor(() => {
      expect(onError).toHaveBeenCalledWith(true);
    });
  });

  it("calls onError(false) when deleteQuestionnaireLines returns failure", async () => {
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([]); // delete path

    jest
      .spyOn(QuestionDataService.prototype, "deleteQuestionnaireLines")
      .mockResolvedValue({ success: false }); // failure

    const onError = jest.fn();
    render(
      <DeleteOrDeactivate context={mockContext} questionId="q1" onError={onError} />
    );

    await screen.findByTestId("mock-dialog-title");
    fireEvent.click(screen.getByTestId("confirm-primary-button"));

    await waitFor(() => {
      expect(onError).toHaveBeenCalledWith(false);
    });
  });

  it("shows loading overlay while service is pending (deactivate flow)", async () => {
    jest
      .spyOn(SnapshotsDataService.prototype, "getAssociatedSnapshots")
      .mockResolvedValue([{}]); // deactivate path

    // Create a deferred promise for inactivateRecord
    let resolveInactivate: (value?: any) => void;
    const deferred = new Promise((res) => {
      resolveInactivate = res;
    });
    const inactivateSpy = jest
      .spyOn(DataService.prototype, "inactivateRecord")
      .mockImplementation(() => deferred as any);

    render(<DeleteOrDeactivate context={mockContext} questionId="q1" />);

    await screen.findByTestId("mock-dialog-title");

    // Click primary to start inactivate (sets loading = true before awaiting)
    act(() => {
      fireEvent.click(screen.getByTestId("confirm-primary-button"));
    });

    // "Processing..." overlay should be present while promise unresolved
    expect(screen.getByText(/Processing\.\.\./i)).toBeInTheDocument();

    // Resolve the deferred promise to finish the flow
    await act(async () => {
      resolveInactivate?.({ success: true });
      // allow microtask queue to flush
      await Promise.resolve();
    });

    // After resolution, overlay should disappear
    await waitFor(() => {
      expect(screen.queryByText(/Processing\.\.\./i)).not.toBeInTheDocument();
    });

    // Ensure service was called
    expect(inactivateSpy).toHaveBeenCalled();
  });
});
