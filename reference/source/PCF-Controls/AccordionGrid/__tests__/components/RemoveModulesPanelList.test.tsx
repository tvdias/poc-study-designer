import * as React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { RemoveModulesPanelList } from "../../AccordionGrid/components/RemoveModulesSidePanel/RemoveModulesPanelList";
import type { ModulesInProject } from "../../AccordionGrid/models/ModulesInProject";
import type { RowEntity } from "../../AccordionGrid/models/RowEntity";

describe("RemoveModulesPanelList", () => {
  const mockRows: RowEntity[] = [
    {
      id: "row-1",
      firstLabelId: "qtype1",
      firstLabelText: "MCQ",
      questionTitle: "What is React?",
    } as any,
    {
      id: "row-2",
      firstLabelId: "qtype2",
      firstLabelText: "Text",
      questionTitle: "Explain hooks",
    } as any,
  ];

  const mockModules: ModulesInProject[] = [
    {
      moduleName: "Module A",
      count: 2,
      rows: mockRows,
    },
  ];

  const onSelectionChange = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders no modules message when list is empty", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={[]}
        selectedIds={[]}
        onSelectionChange={onSelectionChange}
      />
    );

    expect(
      screen.getByText(/No modules available to remove/i)
    ).toBeInTheDocument();
  });

  it("renders modules with questions", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={[]}
        onSelectionChange={onSelectionChange}
      />
    );

    // Module header should render
    expect(screen.getByText("Module A")).toBeInTheDocument();
    expect(screen.getByText("2 questions")).toBeInTheDocument();

    // Expand accordion to reveal questions
    fireEvent.click(screen.getByRole("button", { name: /Module A/i }));

    // Now the questions should be visible
    expect(screen.getByText("MCQ")).toBeInTheDocument();
    expect(screen.getByText("Text")).toBeInTheDocument();
  });

  it("selects all modules when Select All clicked", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={[]}
        onSelectionChange={onSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("Select All Modules"));
    expect(onSelectionChange).toHaveBeenCalledWith(["row-1", "row-2"]);
  });

  it("unselects all modules when Select All clicked again", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={["row-1", "row-2"]}
        onSelectionChange={onSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("Select All Modules"));
    expect(onSelectionChange).toHaveBeenCalledWith([]);
  });

  it("selects all rows in a module when module checkbox clicked", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={[]}
        onSelectionChange={onSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("Select Module A"));
    expect(onSelectionChange).toHaveBeenCalledWith(["row-1", "row-2"]);
  });

  it("unselects all rows in a module when clicked again", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={["row-1", "row-2"]}
        onSelectionChange={onSelectionChange}
      />
    );

    fireEvent.click(screen.getByLabelText("Select Module A"));
    expect(onSelectionChange).toHaveBeenCalledWith([]);
  });

  it("expands module accordion to show questions", () => {
    render(
      <RemoveModulesPanelList
        modulesInProject={mockModules}
        selectedIds={[]}
        onSelectionChange={onSelectionChange}
      />
    );

    fireEvent.click(screen.getByRole("button", { name: /Module A/i }));
    expect(screen.getByText("What is React?")).toBeInTheDocument();
    expect(screen.getByText("Explain hooks")).toBeInTheDocument();
  });
});
