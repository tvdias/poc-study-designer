import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { QuestionBankPage } from './QuestionBankPage';
import { questionBankApi } from '../services/api';

// Mock the API modules
vi.mock('../services/api', () => ({
    questionBankApi: {
        getAll: vi.fn(),
        getById: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    questionAnswerApi: {
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    metricGroupsApi: {
        getAll: vi.fn(),
        create: vi.fn(),
    },
}));

// Mock Icons
vi.mock('../components/ui/Icons', () => ({
    EyeIcon: () => <span data-testid="icon-eye">Eye</span>,
    EditIcon: () => <span data-testid="icon-edit">Edit</span>,
    TrashIcon: () => <span data-testid="icon-trash">Trash</span>,
    PlusIcon: () => <span data-testid="icon-plus">Plus</span>,
    RefreshIcon: () => <span data-testid="icon-refresh">Refresh</span>,
}));

describe('QuestionBankPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (questionBankApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<QuestionBankPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders question bank items list after loading', async () => {
        const mockQuestions = [
            {
                id: '1',
                variableName: 'Q1',
                version: 1,
                questionType: 'Single',
                questionText: 'What is your name?',
                classification: 'Personal',
                isDummy: false
            },
            {
                id: '2',
                variableName: 'Q2',
                version: 1,
                questionType: 'Multiple',
                questionText: 'Select your interests',
                classification: 'Preferences',
                isDummy: false
            },
        ];
        (questionBankApi.getAll as any).mockResolvedValue(mockQuestions);

        render(<QuestionBankPage />);

        await waitFor(() => {
            expect(screen.getByText('What is your name?')).toBeInTheDocument();
            expect(screen.getByText('Select your interests')).toBeInTheDocument();
        });

        expect(screen.getByText('Q1')).toBeInTheDocument();
        expect(screen.getByText('Q2')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (questionBankApi.getAll as any).mockResolvedValue([]);
        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Question/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });
    });

    it('creates a question bank item and closes panel', async () => {
        (questionBankApi.getAll as any).mockResolvedValue([]);
        (questionBankApi.create as any).mockResolvedValue({
            id: '3',
            variableName: 'Q3',
            version: 1,
            questionType: 'Single',
            questionText: 'New Question',
            classification: 'Test',
            isDummy: false
        });

        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Question/i })).toBeInTheDocument();
        });

        // Fill form - minimal required fields
        fireEvent.change(screen.getByLabelText(/Variable Name/i), { target: { value: 'Q3' } });
        fireEvent.change(screen.getByLabelText(/Question Text/i), { target: { value: 'New Question' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(questionBankApi.create).toHaveBeenCalledWith(
                expect.objectContaining({
                    variableName: 'Q3',
                    questionText: 'New Question'
                })
            );
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Question' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(questionBankApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (questionBankApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { VariableName: ['Variable name is required'] }
        };
        (questionBankApi.create as any).mockRejectedValue(errorResponse);

        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Variable name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockQuestion = {
            id: '1',
            variableName: 'TEST_Q',
            version: 1,
            questionType: 'Single',
            questionText: 'Test Question',
            classification: 'Test',
            isDummy: false
        };
        const mockQuestionDetail = {
            ...mockQuestion,
            answers: []
        };
        (questionBankApi.getAll as any).mockResolvedValue([mockQuestion]);
        (questionBankApi.getById as any).mockResolvedValue(mockQuestionDetail);

        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.getByText('Test Question')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Question'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /TEST_Q v1/i })).toBeInTheDocument();
        });
    });

    it('deletes a question bank item with confirmation', async () => {
        const mockQuestion = {
            id: '1',
            variableName: 'TEST_Q',
            version: 1,
            questionType: 'Single',
            questionText: 'Test Question',
            classification: 'Test',
            isDummy: false
        };
        (questionBankApi.getAll as any).mockResolvedValue([mockQuestion]);
        (questionBankApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.getByText('Test Question')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(questionBankApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(questionBankApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters questions based on search input', async () => {
        const mockQuestions = [
            {
                id: '1',
                variableName: 'TEST_Q',
                version: 1,
                questionType: 'Single',
                questionText: 'Test Question',
                classification: 'Test',
                isDummy: false
            },
        ];
        (questionBankApi.getAll as any).mockResolvedValue(mockQuestions);

        render(<QuestionBankPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'Test' } });

        await waitFor(() => {
            expect(questionBankApi.getAll).toHaveBeenCalledWith('Test');
        });
    });
});
