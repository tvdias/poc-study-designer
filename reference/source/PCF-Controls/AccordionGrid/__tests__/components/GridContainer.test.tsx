import { render, screen, waitFor } from "@testing-library/react";
import { GridContainer } from "../../AccordionGrid/components/GridContainer";
import { GridContainerProps } from "../../AccordionGrid/models/props/GridContainerProps";
import { RowEntity } from "../../AccordionGrid/models/RowEntity";
import { DataService } from "../../AccordionGrid/services/DataService";
import * as React from "react";
import { MocksHelper } from "../helpers/MocksHelper";

// Mock child components to avoid rendering complexity
jest.mock("../../AccordionGrid/components/Header", () => ({
  Header: ({ onSearch, updateView }: any) => (
    <div>
      <button onClick={() => updateView("all")}>All</button>
      <button onClick={() => updateView("active")}>Active</button>
      <button onClick={() => updateView("inactive")}>Inactive</button>
      <button onClick={() => updateView("dummy")}>Dummy</button>
      <input
        data-testid="search"
        onChange={(e) => onSearch(e.target.value)}
      />
    </div>
  ),
}));

jest.mock("../../AccordionGrid/components/ExpandableGrid", () => ({
  ExpandableGrid: ({ rows }: any) => (
    <div data-testid="grid">
      {rows.map((r: any) => (
        <div key={r.id}>{r.name}</div>
      ))}
    </div>
  ),
}));

describe("GridContainer", () => {

  let mockDataService = MocksHelper.getMockDataService();
  let mockContext: any;
  let mockOnNotifyOutputChanged: jest.Mock;
  let defaultProps: GridContainerProps;
  let mockRowsEntities: RowEntity[];

  // Mock PCF parameter
  const mockGridDataSet = {
    getTargetEntityType: jest.fn(() => "kt_questionnaireLines"),
    // add other dataset methods you might be using inside GridContainer:
    paging: { hasNextPage: false, loadNextPage: jest.fn() },
    columns: [],
    sortedRecordIds: ["1", "2", "3"],
    records: {
      "1": { getRecordId: () => "1", getFormattedValue: () => "Item A", getValue: () => "val" },
      "2": { getRecordId: () => "2", getFormattedValue: () => "Item B", getValue: () => "val" },
      "3": { getRecordId: () => "3", getFormattedValue: () => "Item C", getValue: () => "val" },
    },
  } as any;

  // Mock context
  mockContext = {
    navigation: {
      openForm: jest.fn()
    },
    parameters: {
      gridDataSet: mockGridDataSet,
    },
    userSettings: {
      securityRoles: ["3a27fcc5-cc0a-f011-bae2-000d3a2274a5"],
    }
  };

  // Mock callback
  mockOnNotifyOutputChanged = jest.fn();

  // Mock rows
  mockRowsEntities = [
      { id: '1', name: 'Item A', sortOrder: 1, statusCode: 1, firstLabelText: "SingleChoice", firstLabelId: 0, middleLabelText: 'Module A', lastLabelText: 'Intro', projectId: '', questionTitle: "Test" , questionFormatDetail:"Test", answerMin:0, answerMax: 90, isDummy : "true", answerList: "AnswerList" , scripterNotes: "Scripter Notes", rowSortOrder:"Normal", columnSortOrder:"Normal", standardOrCustomId:1, standardOrCustomText: "custom"  },
      { id: '2', name: 'Item B', sortOrder: 2, statusCode: 2, firstLabelText: "Standard", firstLabelId: 1, middleLabelText: '', lastLabelText: 'Work_Sector', projectId: '', questionTitle: "Test" , questionFormatDetail:"Test", answerMin:0, answerMax: 90, isDummy : "true", answerList: "AnswerList" , scripterNotes: "Scripter Notes", rowSortOrder:"Normal", columnSortOrder:"Normal", standardOrCustomId:1, standardOrCustomText: "custom"  },
      { id: '3', name: 'Item C', sortOrder: 3, statusCode: 1, firstLabelText: "NumericInput", firstLabelId: 1, middleLabelText: '', lastLabelText: 'brand_More', projectId: '', questionTitle: "Test" , questionFormatDetail:"Test", answerMin:0, answerMax: 90, isDummy : "true", answerList: "AnswerList" , scripterNotes: "Scripter Notes", rowSortOrder:"Normal", columnSortOrder:"Normal", standardOrCustomId:1, standardOrCustomText: "custom"  }
  ];

  // Setup default props
  defaultProps = {
    context: mockContext,
    onNotifyOutputChanged: mockOnNotifyOutputChanged,
    dataItems: mockRowsEntities,
    dataService: mockDataService,
    isReadOnly: false,
  };

  describe("Display data in rows", () => {
    it("should render 'Active' rows sorted by sortOrder", () => {
      render(<GridContainer {...defaultProps} />);
      const rows = screen.getByTestId("grid").children;
      expect(rows.length).toBe(2);
      expect(rows[0].textContent).toBe("Item A"); // sortOrder 1
      expect(rows[1].textContent).toBe("Item C"); // sortOrder 2
    });
  });
});