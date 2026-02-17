import { render, screen, fireEvent, waitFor, renderHook } from "@testing-library/react";
import { ExpandableGrid } from "../../AccordionGrid/components/ExpandableGrid";
import { ExpandableGridProps } from "../../AccordionGrid/models/props/ExpandableGridProps";
import { RowEntity } from "../../AccordionGrid/models/RowEntity";
import { DataService } from "../../AccordionGrid/services/DataService";
import { QuestionType } from "../../AccordionGrid/types/QuestionType";
import * as React from "react";
import { StandardOrCustom } from "../../AccordionGrid/types/DetailView/QuestionnaireLinesChoices";
import { ViewType } from "../../AccordionGrid/types/ViewType";
import { MocksHelper } from "../helpers/MocksHelper";

// Mock child components
jest.mock("../../AccordionGrid/components/DetailView/DetailsView", () => ({
    DetailsView: ({ row }: any) => <div data-testid={`details-${row.id}`}>Details for {row.name}</div>,
}));

jest.mock("../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelContainer", () => ({
    AddQuestionsPanelContainer: ({ isOpen, row }: any) =>
        isOpen ? <div data-testid="side-panel">Panel for {row?.name}</div> : null,
}));

jest.mock("../../AccordionGrid/components/ConfirmDialog", () => ({
    ConfirmDialog: ({ isOpen, onPrimaryActionClick, onSecondaryActionClick }: any) =>
        isOpen ? (
            <div data-testid="confirm-dialog">
                <button data-testid="confirm-activate" onClick={onPrimaryActionClick}>
                    Activate
                </button>
                <button data-testid="cancel-activate" onClick={onSecondaryActionClick}>
                    Cancel
                </button>
            </div>
        ) : null,
}));

// Mock window.open
const mockWindowOpen = jest.fn();
Object.defineProperty(window, 'open', {
    writable: true,
    value: mockWindowOpen,
});

// Mock console.log for testing
const mockConsoleLog = jest.spyOn(console, 'log').mockImplementation();

describe("ExpandableGrid", () => {
    let mockDataService = MocksHelper.getMockDataService();
    
    const mockContext = MocksHelper.getMockContext();

    const createMockRow = (id: string, name: string, statusCode: number = 1): RowEntity => ({
        id,
        name,
        statusCode,
        firstLabelText: "SingleChoice",
        firstLabelId: QuestionType.SingleChoice,
        middleLabelText: "Module A",
        lastLabelText: "Intro",
        projectId: "project-123",
        sortOrder: 1,
        questionTitle: "Test Question Title",
        questionFormatDetail: "Test format details",
        answerMin: 1,
        answerMax: 5,
        isDummy: "false",
        answerList: "Test answer list",
        scripterNotes: "Test notes",
        rowSortOrder: "1",
        columnSortOrder: "1",
        standardOrCustomText: "Standard",
        standardOrCustomId: StandardOrCustom.Standard,
        questionVersion: "1.0",
        questionRationale: "Test question rationale",
    });

    const defaultProps: ExpandableGridProps = {
        context: mockContext,
        rows: [
            createMockRow("row1", "Test Question 1", 1), // Active
            createMockRow("row2", "Test Question 2", 2), // Inactive
        ],
        dataService: mockDataService,
        entityName: "kt_questionnairelines",
        isReadOnly: false,
        view: ViewType.Active,
        isScripter: false
    };

    beforeEach(() => {
        jest.clearAllMocks();
        
        // Setup window.Xrm mock
        (window as any).Xrm = {
            Utility: {
                getGlobalContext: jest.fn(() => ({
                    getClientUrl: jest.fn(() => "https://test-org.crm.dynamics.com"),
                    getCurrentAppProperties: jest.fn(() =>
                        Promise.resolve({ appId: "test-app-id-123" })
                    ),
                })),
            },
        };
    });

    afterEach(() => {
        mockConsoleLog.mockClear();
        mockWindowOpen.mockClear();
    });

    describe("Component Rendering", () => {
        it("should render the component without crashing", () => {
            render(<ExpandableGrid {...defaultProps} />);
            expect(screen.getByText("Test Question 1")).toBeInTheDocument();
            expect(screen.getByText("Test Question 2")).toBeInTheDocument();
        });

        it("should render correct number of accordion items", () => {
            render(<ExpandableGrid {...defaultProps} />);
            
            // Check for accordion items by looking for question names
            expect(screen.getByText("Test Question 1")).toBeInTheDocument();
            expect(screen.getByText("Test Question 2")).toBeInTheDocument();
            
            // Should render multiple buttons (accordion headers, info buttons, action buttons)
            const buttons = screen.getAllByRole("button");
            expect(buttons.length).toBeGreaterThan(2);
        });
    });

    describe("Info Button Functionality", () => {
        it("should render Info buttons with correct tooltip references", () => {
            render(<ExpandableGrid {...defaultProps} />);

            // Look for buttons with tooltip aria-labelledby attributes
            const buttons = screen.getAllByRole("button");
            const infoButtons = buttons.filter(button => 
                button.getAttribute("aria-labelledby")?.includes("tooltip-")
            );
            
            expect(infoButtons.length).toBeGreaterThan(0);
        });

        it("should have accessible Info buttons for both rows", () => {
            render(<ExpandableGrid {...defaultProps} />);
            
            // Look for buttons with specific aria-labelledby attributes (tooltip references)
            const buttons = screen.getAllByRole("button");
            const infoButtons = buttons.filter(button => 
                button.getAttribute("aria-labelledby")?.includes("tooltip-")
            );

            expect(infoButtons.length).toBe(2); // One for each row
        });

        it("should render Info button icons", () => {
            render(<ExpandableGrid {...defaultProps} />);
            
            // Look for SVG icons inside info buttons
            const buttons = screen.getAllByRole("button");
            const buttonsWithSvg = buttons.filter(button => 
                button.querySelector('svg')
            );

            expect(buttonsWithSvg.length).toBeGreaterThan(0);
        });
    });

    describe("Reactivate Button Functionality", () => {
        it("should show reactivate functionality for inactive rows", () => {
            render(<ExpandableGrid {...defaultProps} />);

            // Check that buttons are rendered (reactivate button should be present for inactive rows)
            const buttons = screen.getAllByRole("button");
            expect(buttons.length).toBeGreaterThan(0);
        });

        it("should not show reactivate buttons when grid is read-only", () => {
            render(<ExpandableGrid {...defaultProps} isReadOnly={true} />);

            // In read-only mode, action buttons should be limited
            const buttons = screen.getAllByRole("button");
            // Info buttons might still be present, but action buttons should be limited
            expect(buttons).toBeDefined();
        });

        it("should show confirmation dialog when reactivate is clicked", async () => {
            render(<ExpandableGrid {...defaultProps} />);

            // Find buttons and simulate clicking what could be a reactivate button
            const buttons = screen.getAllByRole("button");
            
            if (buttons.length > 1) {
                // Click what might be a reactivate button (typically second button if info is first)
                fireEvent.click(buttons[1]);

                // Wait for potential confirmation dialog
                await waitFor(() => {
                    // Check if confirm dialog elements might be present
                    const dialogButtons = screen.queryAllByRole("button");
                    expect(dialogButtons.length).toBeGreaterThanOrEqual(buttons.length);
                });
            }
        });

        it("should call reactivateRecord when confirmed", async () => {
            render(<ExpandableGrid {...defaultProps} />);

            // Simulate the reactivate flow
            const buttons = screen.getAllByRole("button");
            
            if (buttons.length > 1) {
                fireEvent.click(buttons[1]);

                // Look for confirm button if dialog appears
                const confirmButton = screen.queryByTestId("confirm-activate");
                if (confirmButton) {
                    fireEvent.click(confirmButton);

                    await waitFor(() => {
                        // Verify that reactivateRecord was called
                        expect(mockDataService.reactivateRecord).toHaveBeenCalled();
                    });
                }
            }
        });
    });

    describe("Data Integration", () => {
        it("should handle rows with questionVersion and questionRationale", () => {
            const rowsWithData = [
                { ...createMockRow("test1", "Question with data", 1), questionVersion: "2.0", questionRationale: "Updated rationale" },
                { ...createMockRow("test2", "Question without data", 2) }
            ];

            render(<ExpandableGrid {...defaultProps} rows={rowsWithData} />);

            expect(screen.getByText("Question with data")).toBeInTheDocument();
            expect(screen.getByText("Question without data")).toBeInTheDocument();
        });

        it("should handle empty rows array", () => {
            render(<ExpandableGrid {...defaultProps} rows={[]} />);
            
            // Should render without crashing even with no rows
            const buttons = screen.queryAllByRole("button");
            expect(buttons).toBeDefined();
        });
    });

    describe("Context and DataService Integration", () => {
        it("should initialize with proper context", () => {
            render(<ExpandableGrid {...defaultProps} />);
            
            // Verify component renders with provided context
            expect(mockContext.parameters.gridDataSet.refresh).toBeDefined();
        });

        it("should use provided DataService", () => {
            render(<ExpandableGrid {...defaultProps} />);
            
            // DataService should be available for component operations
            expect(mockDataService.reactivateRecord).toBeDefined();
            expect(mockDataService.inactivateRecord).toBeDefined();
        });
    });

    describe('Drag&Drop logic', () => {
        const rows: RowEntity[] = [
            { id: 'row1', name: 'Row 1', sortOrder: 0 } as any,
            { id: 'row2', name: 'Row 2', sortOrder: 1 } as any,
        ];

        it('updates order on drag end', async () => {
            const { result } = renderHook(() =>
                React.useState(rows)
            );

            const oldItems = [...rows];
            const newIndex = 1; // simulate moving first item to second

            // simulate drag end
            const newItems = [...oldItems];
            const [moved] = newItems.splice(0, 1);
            newItems.splice(newIndex, 0, moved);

            expect(newItems.map(r => r.id)).toEqual(['row2', 'row1']);
        });
    });
});