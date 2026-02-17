import { render, fireEvent, screen, act } from "@testing-library/react";
import "@testing-library/jest-dom";
import { MatrixContainer } from "../../MatrixGrid/components/MatrixContainer";
import * as React from "react";
import { MocksHelper } from "../helpers/MocksHelper";

jest.mock("../../MatrixGrid/utils/UrlHelpers", () => ({
  UrlHelpers: {
    navigateToEntityForm: jest.fn(),
    getMainEntityName: () => "testentity"
  }
}));

jest.mock("../../MatrixGrid/utils/MatrixUtils", () => ({
  MatrixUtils: {
    generateCellKey: (r: string, c: string) => `${r}-${c}`,
    buildCellStates: jest.fn((rows, cols, junctions) => {
      const map = new Map();

      rows.forEach((r: any) =>
        cols.forEach((c: any) => {
          map.set(`${r.id}-${c.id}`, {
            rowId: r.id,
            columnId: c.id,
            isChecked: false,
            isModified: false,
            hasConflict: false,
            isInteractable: true
          });
        })
      );
      return map;
    })
  }
}));

jest.mock("../../MatrixGrid/utils/SearchFilterHelpers", () => ({
  SearchFiltersHelpers: {
    filterRowItems: jest.fn((rows) => rows),
    filterColumnItems: jest.fn((cols) => cols)
  }
}));

// Mock children components to simplify rendering
jest.mock("../../MatrixGrid/components/Header", () => ({
  Header: (props: any) => (
    <div data-testid="header">
      <button data-testid="dropdown-change" onClick={() => props.onDropdownFilterChange("dropdownB")} />
    </div>
  )
}));

jest.mock("../../MatrixGrid/components/MatrixTable", () => ({
  MatrixTable: (props: any) => (
    <div>
      {props.rows.map((r: any) =>
        props.columns.map((c: any) => (
          <button
            key={`${r.id}-${c.id}`}
            data-testid={`cell-${r.id}-${c.id}`}
            onClick={() => props.onCellToggle(r.id, c.id)}
          >
            Cell
          </button>
        ))
      )}
    </div>
  )
}));

jest.mock("../../MatrixGrid/components/Footer", () => ({
  Footer: (props: any) => (
    <div>
      <button
        data-testid="save-btn"
        disabled={props.disabled}
        onClick={props.onSave}
      >
        Save
      </button>
      <button
        data-testid="cancel-btn"
        disabled={props.disabled}
        onClick={props.onCancel}
      >
        Cancel
      </button>
    </div>
  )
}));


// -------------------------
// Test Setup
// -------------------------
const mockContext: any = MocksHelper.getMockContext();

const mockMatrixService = MocksHelper.getMockMatrixService();

const rows = [
  { id: "r1", name: "Row 1", sortOrder: 1, dropdownValueToFilter: "dropdownA" }
];

const columns = [
  { id: "c1", name: "Column 1", dropdownValueToFilter: "dropdownA", disabled: false }
];

const junctionItems: any[] = [];

const dropdownItems = [
  { id: "dropdownA", name: "A", isReadOnly: false },
  { id: "dropdownB", name: "B", isReadOnly: true }
];

// -------------------------
//       TEST SUITE
// -------------------------
describe("MatrixContainer", () => {

  const setup = () =>
    render(
      <MatrixContainer
        context={mockContext}
        rowsItems={rows}
        columnItems={columns}
        junctionItems={junctionItems}
        rowsLabel="Rows"
        columnsLabel="Cols"
        dropdownItems={dropdownItems}
        matrixService={mockMatrixService}
        isReadOnly={false}
      />
    );

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test("renders the component successfully", () => {
    setup();
    expect(screen.getByTestId("header")).toBeInTheDocument();
  });

  test("clicking a cell toggles its state and adds to pending changes", () => {
    setup();

    const cell = screen.getByTestId("cell-r1-c1");

    act(() => {
      fireEvent.click(cell);
    });

    // After toggle, Save button should be enabled
    expect(screen.getByTestId("save-btn")).not.toBeDisabled();
  });

  test("dropdown change triggers rebuild of cell states", () => {
    setup();

    const dropdownButton = screen.getByTestId("dropdown-change");

    act(() => {
      fireEvent.click(dropdownButton);
    });

    // New dropdown value should make Save disabled again (no pending changes)
    expect(screen.getByTestId("save-btn")).toBeDisabled();
  });

  test("saving calls matrixService and shows success message", async () => {
    setup();

    const cell = screen.getByTestId("cell-r1-c1");

    act(() => {
      fireEvent.click(cell);
    });

    await act(async () => {
      fireEvent.click(screen.getByTestId("save-btn"));
    });

    expect(mockMatrixService.saveJunctionItems).toHaveBeenCalledTimes(1);
    expect(screen.getByText(/successfully saved/i)).toBeInTheDocument();
  });

  test("cancel reverts modified cell states", () => {
    setup();

    const cell = screen.getByTestId("cell-r1-c1");

    act(() => {
      fireEvent.click(cell);
    });

    act(() => {
      fireEvent.click(screen.getByTestId("cancel-btn"));
    });

    // After cancel, save should again be disabled
    expect(screen.getByTestId("save-btn")).toBeDisabled();
  });

  test("matrix becomes read-only when dropdown changes to readonly item", () => {
    setup();

    act(() => {
      fireEvent.click(screen.getByTestId("dropdown-change")); // switches to dropdownB (readonly)
    });

    expect(screen.getByTestId("save-btn")).toBeDisabled();
  });
});
