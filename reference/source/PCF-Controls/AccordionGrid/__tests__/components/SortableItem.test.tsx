/// <reference types="jest" />

import * as React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { SortableItem } from '../../AccordionGrid/components/SortableItem';
import { RowEntity } from '../../AccordionGrid/models/RowEntity';
import { ViewType } from '../../AccordionGrid/types/ViewType';
import { QuestionType } from '../../AccordionGrid/types/QuestionType';
import { StandardOrCustom } from '../../AccordionGrid/types/DetailView/QuestionnaireLinesChoices';
import { DndContext } from '@dnd-kit/core';
import { MocksHelper } from '../helpers/MocksHelper';

// --- Mock ProjectDataService ---
const mockReorderProjectQuestionnaire = jest.fn();
jest.mock("../../AccordionGrid/services/ProjectDataService", () => {
  return {
    ProjectDataService: jest.fn().mockImplementation(() => ({
      reorderProjectQuestionnaire: mockReorderProjectQuestionnaire,
    })),
  };
});

// Mock the required modules
jest.mock('../../AccordionGrid/components/ConfirmDialog', () => ({
    ConfirmDialog: ({ onPrimaryActionClick, buttonPrimaryText }: any) => (
        <div data-testid="confirm-dialog">
            <button onClick={(e) => onPrimaryActionClick(e)} data-testid="confirm-primary-button">
                {buttonPrimaryText}
            </button>
        </div>
    )
}));

jest.mock('../../AccordionGrid/components/DetailView/DetailsView', () => ({
    DetailsView: () => <div data-testid="details-view">Details View</div>
}));

jest.mock('../../AccordionGrid/utils/EntityHelper', () => ({
    EntityHelper: {
        generateEditUrl: jest.fn().mockResolvedValue('https://test-url.com/edit/123'),
        getProjectIdFromUrl: jest.fn().mockReturnValue('project-123')
    }
}));

// Mock window.open
Object.defineProperty(window, 'open', {
    writable: true,
    value: jest.fn()
});

describe('SortableItem Button Functionality Tests', () => {
    // Mock data
    const mockActiveRow: RowEntity = {
        id: 'test-id-1',
        name: 'Test Question',
        sortOrder: 1,
        statusCode: 1, // Active
        firstLabelText: 'Single Choice',
        firstLabelId: QuestionType.SingleChoice,
        middleLabelText: 'Test Module',
        lastLabelText: 'Q1',
        projectId: 'project-1',
        questionTitle: 'Test Question Title',
        questionFormatDetail: 'Test Format',
        answerMin: 1,
        answerMax: 5,
        isDummy: 'No',
        answerList: 'Yes,No,Maybe',
        scripterNotes: 'Test notes',
        rowSortOrder: '1',
        columnSortOrder: '1',
        standardOrCustomText: 'Standard',
        standardOrCustomId: StandardOrCustom.Standard,
        questionVersion: '1.0',
        questionRationale: 'Test rationale'
    };

    const mockInactiveRow: RowEntity = {
        ...mockActiveRow,
        id: 'test-id-2',
        statusCode: 2 // Inactive
    };

    const mockDataService = MocksHelper.getMockDataService();

    const mockContext = MocksHelper.getMockContext();

    const mockOnOpenAddPanel = jest.fn();

    const defaultProps = {
        dataService: mockDataService,
        isReadOnly: false,
        entityName: 'test_entity',
        context: mockContext,
        onOpenAddPanel: mockOnOpenAddPanel,
        view: ViewType.All,
        isScripter:false
    };

    // Wrapper component for DndContext
    const DndWrapper = ({ children }: { children: React.ReactNode }) => (
        <DndContext>{children}</DndContext>
    );

    beforeEach(() => {
        jest.clearAllMocks();
    });

    test('should render component successfully', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockActiveRow} />
            </DndWrapper>
        );

        expect(screen.getByText('Test Question')).toBeInTheDocument();
        expect(screen.getByText('Single Choice')).toBeInTheDocument();
    });

    test('should render delete button for active rows when not readonly', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockActiveRow} />
            </DndWrapper>
        );

        const deleteButton = screen.getByTestId('delete-button');
        expect(deleteButton).toBeInTheDocument();
    });

    test('should render reactivate button for inactive rows when not readonly', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockInactiveRow} />
            </DndWrapper>
        );

        const reactivateButton = screen.getByTestId('reactivate-button');
        expect(reactivateButton).toBeInTheDocument();
    });

    test('should not render delete button for inactive rows', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockInactiveRow} />
            </DndWrapper>
        );

        const deleteButton = screen.queryByTestId('delete-button');
        expect(deleteButton).not.toBeInTheDocument();
    });

    test('should not render reactivate button for active rows', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockActiveRow} />
            </DndWrapper>
        );

        const reactivateButton = screen.queryByTestId('reactivate-button');
        expect(reactivateButton).not.toBeInTheDocument();
    });

    test('should not render action buttons when readonly', () => {
        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockActiveRow} isReadOnly={true} />
            </DndWrapper>
        );

        const deleteButton = screen.queryByTestId('delete-button');
        const reactivateButton = screen.queryByTestId('reactivate-button');
        
        expect(deleteButton).not.toBeInTheDocument();
        expect(reactivateButton).not.toBeInTheDocument();
    });

    test('should call reactivateRecord when reactivate button is confirmed', async () => {
        (mockDataService.reactivateRecord as jest.Mock).mockResolvedValue({ success: true });

        render(
            <DndWrapper>
                <SortableItem {...defaultProps} row={mockInactiveRow} />
            </DndWrapper>
        );

        // Click reactivate button to open dialog
        const reactivateButton = screen.getByTestId('reactivate-button');
        fireEvent.click(reactivateButton);

        // Click confirm button in dialog
        const confirmButton = screen.getByTestId('confirm-primary-button');
        fireEvent.click(confirmButton);

        await waitFor(() => {
            expect(mockDataService.reactivateRecord).toHaveBeenCalledWith('test_entity', 'test-id-2');
            expect(mockContext.parameters.gridDataSet.refresh).toHaveBeenCalled();
        });
    });
});

// Note: Edit button tests are not included here as the edit button is located 
// inside the AccordionPanel which requires complex accordion expansion handling in tests.
// The edit button functionality can be tested manually or with E2E tests.
// Current tests focus on the delete/inactivate and reactivate button functionality 
// which are directly accessible in the accordion header.