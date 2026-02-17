// QuestionnaireReadView.test.tsx
import * as React from "react";
import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import { DetailsView } from "../../AccordionGrid/components/DetailView/DetailsView";
import { QuestionType } from "../../AccordionGrid/types/QuestionType";
import { StandardOrCustom } from "../../AccordionGrid/types/DetailView/QuestionnaireLinesChoices";
import { MocksHelper } from "../helpers/MocksHelper";

// --- Mock AnswerDataService ---
const mockFetchAnswers = jest.fn().mockResolvedValue([]);
const mockFetchManagedLists = jest.fn().mockResolvedValue([]);
jest.mock("../../AccordionGrid/services/AnswerDataService", () => {
  return {
    AnswerDataService: jest.fn().mockImplementation(() => ({
      fetchAnswers: mockFetchAnswers,
      fetchManagedLists: mockFetchManagedLists,
    })),
  };
});

describe("DetailsView", () => {
  const baseRow = {
    firstLabelId: QuestionType.SingleChoice,
    firstLabelText: "Single Choice",
    lastLabelText: "Var_Name",
    standardOrCustomId: StandardOrCustom.Standard,
    standardOrCustomText: "Standard",
    statusCode: 1,
    isDummy: "True",
    questionTitle: "Sample Question Title",
    name: "This is the question text",
    answerMin: 1,
    answerMax: 5,
    answerList: "<p>Option 1</p><p>Option 2</p>",
    rowSortOrder: "10",
    columnSortOrder: "20",
    scripterNotes: "Some notes that should wrap properly",
    questionFormatDetail: "Format ABC",
    id: "1",
    sortOrder: 1,
    middleLabelText: "Module A",
    projectId: "00000000-0000-0000-0000-000000000010",
  };

  const mockContext = MocksHelper.getMockContext();

  beforeEach(() => {
    jest.clearAllMocks();
    mockFetchAnswers.mockResolvedValue([]);
    mockFetchManagedLists.mockResolvedValue([]);
  });

  it("renders base fields", () => {
    render(<DetailsView row={baseRow} context={mockContext} />);

    expect(screen.getByText("Question Type")).toBeInTheDocument();
    expect(screen.getByText("Variable Name")).toBeInTheDocument();
    expect(screen.getByText("Question Title")).toBeInTheDocument();
    expect(screen.getByText("Question Text")).toBeInTheDocument();
    expect(screen.getByText("Scripter Notes")).toBeInTheDocument();
    expect(screen.getByText("Question Format Details")).toBeInTheDocument();

    // Values
    expect(screen.getByText("Standard")).toBeInTheDocument();
    expect(screen.getByText("Single Choice")).toBeInTheDocument();
    expect(screen.getByText("Var_Name")).toBeInTheDocument();
    expect(screen.getByText("Sample Question Title")).toBeInTheDocument();
    expect(screen.getByText("This is the question text")).toBeInTheDocument();
    expect(
      screen.getByText("Some notes that should wrap properly")
    ).toBeInTheDocument();
    expect(screen.getByText("Format ABC")).toBeInTheDocument();
  });

  it("hides restriction when question type does not support it", () => {
    const row = { ...baseRow, firstLabelId: QuestionType.SingleChoice, answerMin: 1, answerMax: 5 };
    render(<DetailsView row={row} context={mockContext} />);

    const restrictionHeading = screen.queryByText("Restrictions");
    // The container div has display:none inline when hidden; ensure the parent or heading is not rendered in layout
    if (restrictionHeading) {
      const container = restrictionHeading.closest('div');
      expect(container?.getAttribute('style') || '').toMatch(/display:\s*none/);
    }
  });


  it("shows row and column sort order when applicable", () => {
    const row = { ...baseRow, firstLabelId: QuestionType.MultipleChoiceMatrix };
    render(<DetailsView row={row} context={mockContext} />);
    expect(screen.getByText("Row Sort Order")).toBeInTheDocument();
    expect(screen.getByText("Column Sort Order")).toBeInTheDocument();
    expect(screen.getByText("10")).toBeInTheDocument();
    expect(screen.getByText("20")).toBeInTheDocument();
  });

  it("renders Dummy tag when isDummy = True", () => {
    const row = { ...baseRow, isDummy: "True" };
    render(<DetailsView row={row} context={mockContext} />);
    expect(screen.getByText("Dummy")).toBeInTheDocument();
  });

});
