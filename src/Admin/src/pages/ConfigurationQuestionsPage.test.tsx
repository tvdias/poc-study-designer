import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ConfigurationQuestionsPage } from './ConfigurationQuestionsPage';
import { configurationQuestionsApi } from '../services/api';

// Mock the API modules
vi.mock('../services/api', () => ({
    configurationQuestionsApi: {
        getAll: vi.fn(),
        getById: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    configurationAnswersApi: {
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    dependencyRulesApi: {
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
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

describe('ConfigurationQuestionsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (configurationQuestionsApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<ConfigurationQuestionsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders configuration questions list after loading', async () => {
        const mockQuestions = [
            {
                id: '1',
                question: 'What is your preferred color?',
                aiPrompt: '',
                ruleType: 'SingleCoded' as const,
                isActive: true,
                version: 1
            },
            {
                id: '2',
                question: 'Select all that apply',
                aiPrompt: '',
                ruleType: 'MultiCoded' as const,
                isActive: false,
                version: 1
            },
        ];
        (configurationQuestionsApi.getAll as any).mockResolvedValue(mockQuestions);

        render(<ConfigurationQuestionsPage />);

        await waitFor(() => {
            expect(screen.getByText('What is your preferred color?')).toBeInTheDocument();
            expect(screen.getByText('Select all that apply')).toBeInTheDocument();
        });
    });

    it('opens create panel when New button is clicked', async () => {
        (configurationQuestionsApi.getAll as any).mockResolvedValue([]);
        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Configuration Question/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });
    });

    it('creates a configuration question and closes panel', async () => {
        (configurationQuestionsApi.getAll as any).mockResolvedValue([]);
        (configurationQuestionsApi.create as any).mockResolvedValue({
            id: '3',
            question: 'New Question',
            aiPrompt: '',
            ruleType: 'SingleCoded',
            isActive: true,
            version: 1
        });

        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Configuration Question/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText(/^Question/i), { target: { value: 'New Question' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(configurationQuestionsApi.create).toHaveBeenCalledWith(
                expect.objectContaining({
                    question: 'New Question'
                })
            );
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Configuration Question' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(configurationQuestionsApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (configurationQuestionsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { Question: ['Question is required'] }
        };
        (configurationQuestionsApi.create as any).mockRejectedValue(errorResponse);

        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Question is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockQuestion = {
            id: '1',
            question: 'Test Question',
            aiPrompt: '',
            ruleType: 'SingleCoded' as const,
            isActive: true,
            version: 1
        };
        const mockQuestionDetail = {
            ...mockQuestion,
            answers: [],
            dependencyRules: []
        };
        (configurationQuestionsApi.getAll as any).mockResolvedValue([mockQuestion]);
        (configurationQuestionsApi.getById as any).mockResolvedValue(mockQuestionDetail);

        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.getByText('Test Question')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Question'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Question' })).toBeInTheDocument();
        });
    });

    it('deletes a configuration question with confirmation', async () => {
        const mockQuestion = {
            id: '1',
            question: 'Test Question',
            aiPrompt: '',
            ruleType: 'SingleCoded' as const,
            isActive: true,
            version: 1
        };
        (configurationQuestionsApi.getAll as any).mockResolvedValue([mockQuestion]);
        (configurationQuestionsApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.getByText('Test Question')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(configurationQuestionsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(configurationQuestionsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters questions based on search input', async () => {
        const mockQuestions = [
            {
                id: '1',
                question: 'Test Question',
                aiPrompt: '',
                ruleType: 'SingleCoded' as const,
                isActive: true,
                version: 1
            },
        ];
        (configurationQuestionsApi.getAll as any).mockResolvedValue(mockQuestions);

        render(<ConfigurationQuestionsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'Test' } });

        await waitFor(() => {
            expect(configurationQuestionsApi.getAll).toHaveBeenCalledWith('Test');
        });
    });
});
