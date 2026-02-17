import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import * as React from "react";
import { AddQuestionsPanelContainer } from "../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelContainer";
import type { AddQuestionsPanelContainerProps } from "../../AccordionGrid/models/props/AddQuestionsPanelContainerProps";
import { MocksHelper } from "../helpers/MocksHelper";

// --- Mock dataservice ---
jest.mock("../../AccordionGrid/services/QuestionDataService", () => {
  return {
    QuestionDataService: jest.fn().mockImplementation(() => ({
      getActiveQuestions: jest.fn().mockResolvedValue({ questions: [] }),
    })),
  };
});

jest.mock("../../AccordionGrid/services/ModuleDataService", () => {
  return {
    ModuleDataService: jest.fn().mockImplementation(() => ({
      getActiveModules: jest.fn().mockResolvedValue({ modules: [] }),
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

// --- Mock child components ---
jest.mock("../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelTabFilters", () => ({
  PanelTabFilters: ({ selectedTab, onTabChange }: any) => (
    <div>
      <button data-testid="questions-tab" onClick={() => onTabChange("questions")}>Questions</button>
      <button data-testid="modules-tab" onClick={() => onTabChange("modules")}>Modules</button>
      <div data-testid="active-tab">{selectedTab}</div>
    </div>
  ),
}));

jest.mock("../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelSearch", () => ({
  PanelSearch: ({ items = [], selectedIds = [], onSelectionChange }: any) => (
    <div>
      <div data-testid="search-items">{items.map((i: any) => i.label).join(",")}</div>
      <button
        data-testid="select-first"
        onClick={() => onSelectionChange([...(selectedIds || []), "id-1"])}
      >
        Select First
      </button>
    </div>
  ),
}));

jest.mock("../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelActionButtons", () => ({
  PanelActionButtons: ({ onCancel, onSave }: any) => (
    <div>
      <button data-testid="cancel" onClick={onCancel}>Cancel</button>
      <button data-testid="save" onClick={onSave}>Save</button>
    </div>
  ),
}));

describe("AddQuestionsPanelContainer", () => {
  const mockContext = MocksHelper.getMockContext();

  const defaultProps: AddQuestionsPanelContainerProps = {
    isOpen: true,
    onClose: jest.fn(),
    onRefresh: jest.fn(),
    row: { projectId: "proj-123", sortOrder: 5 } as any,
    existingRows: [],
    addFromHeader: false,
    context: mockContext,
    isScripter: false
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders and defaults to Questions tab", () => {
    render(<AddQuestionsPanelContainer {...defaultProps} />);
    expect(screen.getByTestId("active-tab").textContent).toBe("questions");
  });

  it("switches to Modules tab", () => {
    render(<AddQuestionsPanelContainer {...defaultProps} />);
    fireEvent.click(screen.getByTestId("modules-tab"));
    expect(screen.getByTestId("active-tab").textContent).toBe("modules");
  });

  it("resets to Questions tab when reopened", () => {
    const { rerender } = render(<AddQuestionsPanelContainer {...defaultProps} isOpen={false} />);
    rerender(<AddQuestionsPanelContainer {...defaultProps} isOpen={true} />);
    expect(screen.getByTestId("active-tab").textContent).toBe("questions");
  });

  it("calls onClose when cancel clicked", () => {
    const mockClose = jest.fn();
    render(<AddQuestionsPanelContainer {...defaultProps} onClose={mockClose} />);
    fireEvent.click(screen.getByTestId("cancel"));
    expect(mockClose).toHaveBeenCalled();
  });

  it("calls API on save and shows success notification", async () => {
    const mockExecute = jest.fn().mockResolvedValue({ ok: true });
    (window as any).Xrm = {
      WebApi: { online: { execute: mockExecute } },
    };

    render(<AddQuestionsPanelContainer {...defaultProps} />);

    fireEvent.click(screen.getByTestId("select-first"));
    fireEvent.click(screen.getByTestId("save"));

    await waitFor(() => {
      expect(mockExecute).toHaveBeenCalled();
      expect(screen.getByText(/added to the project successfully/i)).toBeInTheDocument();
    });
  });

  it("shows error notification when API fails", async () => {
    const mockExecute = jest.fn().mockResolvedValue({ ok: false });
    (window as any).Xrm = {
      WebApi: { online: { execute: mockExecute } },
    };

    render(<AddQuestionsPanelContainer {...defaultProps} />);

    fireEvent.click(screen.getByTestId("select-first"));
    fireEvent.click(screen.getByTestId("save"));

    await waitFor(() => {
      expect(mockExecute).toHaveBeenCalled();
      expect(screen.getByText(/failed to add records/i)).toBeInTheDocument();
    });
  });

  it("shows validation message when no records selected", async () => {
    const mockExecute = jest.fn();
    (window as any).Xrm = {
      WebApi: { online: { execute: mockExecute } },
    };

    render(<AddQuestionsPanelContainer {...defaultProps} />);

    // directly click save without selecting anything
    fireEvent.click(screen.getByTestId("save"));

    await waitFor(() => {
      expect(mockExecute).not.toHaveBeenCalled();
      expect(
        screen.getByText((content) =>
          content.toLowerCase().includes("please select at least one")
        )
      ).toBeInTheDocument();
    });
  });

  it("uses projectId from URL when addFromHeader is true", async () => {
    (window as any).Xrm = { WebApi: { online: { execute: jest.fn().mockResolvedValue({ ok: true }) } } };
    delete (window as any).location;
    (window as any).location = new URL("https://test.crm.dynamics.com/main.aspx?appid=abc&id=proj-999");

    render(<AddQuestionsPanelContainer {...defaultProps} addFromHeader={true} row={undefined as any} />);

    fireEvent.click(screen.getByTestId("select-first"));
    fireEvent.click(screen.getByTestId("save"));

    await waitFor(() => {
      expect(screen.getByText(/added to the project successfully/i)).toBeInTheDocument();
    });
  });

});
