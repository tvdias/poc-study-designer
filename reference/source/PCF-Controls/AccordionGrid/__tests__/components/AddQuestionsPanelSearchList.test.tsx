import * as React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { PanelSearchList } from "../../AccordionGrid/components/AddQuestionsSidePanel/AddQuestionsPanelSearchList";
import type { SearchType } from "../../AccordionGrid/types/AddQuestionsSidePanel/SearchType";

describe("PanelSearchList", () => {
  const mockResults: SearchType[] = [
    {
      id: "id-1",
      type: "question",
      label: "First Question",
      details: { Description: "Detail 1", Extra: "Extra Info" },
    },
    {
      id: "id-2",
      type: "module",
      label: "Second Module",
      details: {},
    },
  ];

  const mockOnSelectionChange = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders 'no results' message when search text given but no results", () => {
    render(
      <PanelSearchList results={[]} searchText="abc" selectedIds={[]} />
    );

    expect(
      screen.getByText(/No results matching your Search/i)
    ).toBeInTheDocument();
  });

  it("renders results with labels", () => {
    render(
      <PanelSearchList results={mockResults} searchText="" selectedIds={[]} />
    );

    expect(screen.getByText("First Question")).toBeInTheDocument();
    expect(screen.getByText("Second Module")).toBeInTheDocument();
  });

  it("calls onSelectionChange when checkbox toggled", () => {
    render(
      <PanelSearchList
        results={mockResults}
        searchText=""
        selectedIds={[]}
        onSelectionChange={mockOnSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("First Question"));

    expect(mockOnSelectionChange).toHaveBeenCalledWith(["id-1"]);
  });

  it("removes from selection if already selected", () => {
    render(
      <PanelSearchList
        results={mockResults}
        searchText=""
        selectedIds={["id-1"]}
        onSelectionChange={mockOnSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("First Question"));

    expect(mockOnSelectionChange).toHaveBeenCalledWith([]);
  });

  it("shows details when available", () => {
    render(
      <PanelSearchList
        results={[mockResults[0]]}
        searchText=""
        selectedIds={[]}
      />
    );

    // expand accordion
    fireEvent.click(screen.getByRole("button", { name: /First Question/i }));

    expect(screen.getByText("Detail 1")).toBeInTheDocument();
    expect(screen.getByText("Extra Info")).toBeInTheDocument();
  });

  it("shows fallback message when no details", () => {
    render(
      <PanelSearchList
        results={[mockResults[1]]}
        searchText=""
        selectedIds={[]}
      />
    );

    // expand accordion
    fireEvent.click(screen.getByRole("button", { name: /Second Module/i }));

    expect(
      screen.getByText(/Details about Second Module will go here/i)
    ).toBeInTheDocument();
  });
});