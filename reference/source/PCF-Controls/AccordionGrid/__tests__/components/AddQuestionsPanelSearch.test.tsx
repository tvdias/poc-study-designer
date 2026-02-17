import * as React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { PanelSearch } from "../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelSearch";
import type { SearchType } from "../../AccordionGrid/types/AddQuestionsSidePanel/SearchType";

// Mock PanelSearchList to isolate PanelSearch behavior
jest.mock("../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelSearchList", () => ({
  PanelSearchList: ({ results, searchText, selectedIds, onSelectionChange }: any) => (
    <div>
      <div data-testid="results">{results.map((r: any) => r.label).join(",")}</div>
      <div data-testid="search-text">{searchText}</div>
      <div data-testid="selected">{selectedIds.join(",")}</div>
      <button data-testid="toggle-first" onClick={() => onSelectionChange?.(["id-1"])}>Toggle First</button>
    </div>
  ),
}));

describe("PanelSearch", () => {
  const mockItems: SearchType[] = [
    { id: "id-1", type: "question", label: "First Question", details: { Description: "desc1" } },
    { id: "id-2", type: "module", label: "Second Module", details: { Description: "desc2" } },
  ];

  const mockOnResults = jest.fn();
  const mockOnSelectionChange = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders search box", () => {
    render(
      <PanelSearch
        items={mockItems}
        selectedIds={[]}
        onResults={mockOnResults}
        onSelectionChange={mockOnSelectionChange}
      />
    );

    expect(screen.getByPlaceholderText("Search Questions and Modules")).toBeInTheDocument();
  });

  it("filters results when typing", () => {
    render(
      <PanelSearch
        items={mockItems}
        selectedIds={[]}
        onResults={mockOnResults}
      />
    );

    const input = screen.getByPlaceholderText("Search Questions and Modules");

    // type "First"
    fireEvent.change(input, { target: { value: "First" } });

    expect(screen.getByTestId("results").textContent).toContain("First Question");
    expect(mockOnResults).toHaveBeenCalledWith([
      expect.objectContaining({ id: "id-1" }),
    ]);
  });

  it("shows no results message when no match", () => {
    render(
      <PanelSearch
        items={mockItems}
        selectedIds={[]}
        onResults={mockOnResults}
      />
    );

    const input = screen.getByPlaceholderText("Search Questions and Modules");

    fireEvent.change(input, { target: { value: "xyz" } });

    expect(screen.getByTestId("results").textContent).toBe(""); // no items
    expect(mockOnResults).toHaveBeenCalledWith([]); // no match
  });

  it("resets search when items change", () => {
    const { rerender } = render(
      <PanelSearch
        items={mockItems}
        selectedIds={[]}
        onResults={mockOnResults}
      />
    );

    // type "Second"
    fireEvent.change(screen.getByPlaceholderText("Search Questions and Modules"), {
      target: { value: "Second" },
    });

    // rerender with new items
    rerender(
      <PanelSearch
        items={[...mockItems, { id: "id-3", type: "module", label: "Third One", details: {} }]}
        selectedIds={[]}
        onResults={mockOnResults}
      />
    );

    expect(screen.getByTestId("results").textContent).toBe(""); // cleared
    expect(screen.getByTestId("search-text").textContent).toBe(""); // search reset
  });

  it("handles selection change from list", () => {
    render(
      <PanelSearch
        items={mockItems}
        selectedIds={[]}
        onSelectionChange={mockOnSelectionChange}
      />
    );

    fireEvent.click(screen.getByTestId("toggle-first"));

    expect(mockOnSelectionChange).toHaveBeenCalledWith(["id-1"]);
  });
});
