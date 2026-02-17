import { render, screen, fireEvent, waitFor, within } from "@testing-library/react";
import * as React from "react";
import { RemoveModulesPanelContainer } from "../../AccordionGrid/components/RemoveModulesSidePanel/RemoveModulesPanelContainer";
import type { RemoveModulesPanelContainerProps } from "../../AccordionGrid/components/RemoveModulesSidePanel/RemoveModulesPanelContainer";

// --- Mock DataService ---
const mockInactivateRecord = jest.fn();
jest.mock("../../AccordionGrid/services/DataService", () => {
  return {
    DataService: jest.fn().mockImplementation(() => ({
      inactivateRecord: mockInactivateRecord,
    })),
  };
});

// --- Mock ProjectDataService ---
const mockReorderProjectQuestionnaire = jest.fn();
jest.mock("../../AccordionGrid/services/ProjectDataService", () => {
  return {
    ProjectDataService: jest.fn().mockImplementation(() => ({
      reorderProjectQuestionnaire: mockReorderProjectQuestionnaire,
    })),
  };
});

// --- Mock GirdRowsHelper ---
jest.mock("../../AccordionGrid/utils/GridRowsHelper", () => {
  return {
    GirdRowsHelper: jest.fn().mockImplementation(() => ({
      getModulesInProject: jest.fn((rows) => [
        { moduleName: "m1", rows, count: rows.length },
      ]),
    })),
  };
});

// --- Mock EntityHelper ---
jest.mock("../../AccordionGrid/utils/EntityHelper", () => {
  return {
    EntityHelper: {
      getProjectId: jest.fn(() => "project-123"),
    },
  };
});

// --- Mock child components ---
jest.mock("../../AccordionGrid/components/RemoveModulesSidePanel/RemoveModulesPanelList", () => ({
  RemoveModulesPanelList: ({ onSelectionChange }: any) => (
    <div>
      <button
        data-testid="select-row"
        onClick={() => onSelectionChange(["row-1"])}
      >
        Select Row
      </button>
    </div>
  ),
}));

jest.mock("../../AccordionGrid/components/RemoveModulesSidePanel/RemoveModulesPanelActionButtons", () => ({
  PanelActionButtons: ({ onCancel, handleRemove, disabled }: any) => (
    <div>
      <button data-testid="cancel" onClick={onCancel}>Cancel</button>
      <button data-testid="remove" disabled={disabled} onClick={handleRemove}>
        Remove
      </button>
    </div>
  ),
}));

describe("RemoveModulesPanelContainer", () => {
  const defaultProps: RemoveModulesPanelContainerProps = {
    isOpen: true,
    onClose: jest.fn(),
    rows: [{ id: "row-1" }] as any,
    context: {
      webAPI: {},
      parameters: { gridDataSet: { refresh: jest.fn() } },
    } as any,
    isReadOnly: false,
    entityName: "testentity",
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders with header and helper text", () => {
    render(<RemoveModulesPanelContainer {...defaultProps} />);
    expect(screen.getByText("Remove Modules")).toBeInTheDocument();
    expect(
      screen.getByText(/select the modules you want to remove/i)
    ).toBeInTheDocument();
  });

  it("calls onClose when Cancel clicked", () => {
    const mockClose = jest.fn();
    render(<RemoveModulesPanelContainer {...defaultProps} onClose={mockClose} />);
    fireEvent.click(screen.getByTestId("cancel"));
    expect(mockClose).toHaveBeenCalled();
  });

  it("disables Remove button when no selection", () => {
    render(<RemoveModulesPanelContainer {...defaultProps} />);
    expect(screen.getByTestId("remove")).toBeDisabled();
  });

  it("removes modules successfully and shows dialog", async () => {
    mockInactivateRecord.mockResolvedValueOnce({});
    render(<RemoveModulesPanelContainer {...defaultProps} />);
    fireEvent.click(screen.getByTestId("select-row"));
    fireEvent.click(screen.getByTestId("remove"));

    await waitFor(() =>
      expect(screen.getByText(/successfully deactivated/i)).toBeInTheDocument()
    );
  });

  it("shows loading overlay during removal", async () => {
    let resolveFn: any;
    const promise = new Promise((res) => { resolveFn = res; });
    mockInactivateRecord.mockReturnValue(promise);

    render(<RemoveModulesPanelContainer {...defaultProps} />);
    fireEvent.click(screen.getByTestId("select-row"));
    fireEvent.click(screen.getByTestId("remove"));

    expect(screen.getByText(/processing/i)).toBeInTheDocument();

    resolveFn({});
  });
});
