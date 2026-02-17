import * as React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { Header } from "../../AccordionGrid/components/Header";
import { ViewType } from "../../AccordionGrid/types/ViewType";
import { MocksHelper } from "../helpers/MocksHelper";

// Mock SearchFilter so we can focus on Header behavior
jest.mock("../../AccordionGrid/components/SearchFilter", () => ({
  SearchFilter: ({ onSearch }: any) => (
    <input data-testid="search-filter" onChange={(e) => onSearch(e.target.value)} />
  ),
}));

describe("Header Component", () => {
  const mockContext = MocksHelper.getMockContext();

  const viewTypeActiveMock = ViewType.Active;

  const mockUpdateView = jest.fn();
  const mockOnSearch = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders the Header and its elements", () => {
    render(
      <Header
        context={mockContext}
        view={viewTypeActiveMock}
        updateView={mockUpdateView}
        onSearch={mockOnSearch}
        isReadOnly={false}
        rows={[]}
        entityName=""
        isScripter= {false}
      />
    );

    // Subtitle
    expect(screen.getByText("Questions")).toBeInTheDocument();

    // Select dropdown options
    expect(screen.getByRole("combobox")).toBeInTheDocument();
    expect(screen.getByText("Active Questions")).toBeInTheDocument();
    expect(screen.getByText("Inactive Questions")).toBeInTheDocument();
    expect(screen.getByText("All Questions")).toBeInTheDocument();

    // SearchFilter
    expect(screen.getByTestId("search-filter")).toBeInTheDocument();
  });

  it("calls updateView when select value changes", () => {
    render(
      <Header
        context={mockContext}
        view={viewTypeActiveMock}
        updateView={mockUpdateView}
        onSearch={mockOnSearch}
        isReadOnly={false}
        rows={[]}
        entityName=""
        isScripter= {false}

      />
    );

    const select = screen.getByRole("combobox");

    // Simulate changing the value to 'inactive'
    fireEvent.change(select, { target: { value: "inactive" } });

    expect(mockUpdateView).toHaveBeenCalledWith("inactive");
  });

  it("calls onSearch when typing in SearchFilter", () => {
    render(
      <Header
        context={mockContext}
        view={viewTypeActiveMock}
        updateView={mockUpdateView}
        onSearch={mockOnSearch}
        isReadOnly={false}
        rows={[]}
        entityName=""
        isScripter={false}
      />
    );

    const searchInput = screen.getByTestId("search-filter");
    fireEvent.change(searchInput, { target: { value: "test query" } });

    expect(mockOnSearch).toHaveBeenCalledWith("test query");
  });
});
