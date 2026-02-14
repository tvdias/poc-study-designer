import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ModulesPage } from './ModulesPage';
import { modulesApi } from '../services/api';

// Mock the API modules
vi.mock('../services/api', () => ({
    modulesApi: {
        getAll: vi.fn(),
        getById: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    moduleQuestionsApi: {
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    questionBankApi: {
        getAll: vi.fn(),
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

describe('ModulesPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (modulesApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<ModulesPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders modules list after loading', async () => {
        const mockModules = [
            {
                id: '1',
                variableName: 'MODULE_A',
                label: 'Module A',
                description: 'Description A',
                versionNumber: 1,
                isActive: true
            },
            {
                id: '2',
                variableName: 'MODULE_B',
                label: 'Module B',
                description: 'Description B',
                versionNumber: 2,
                isActive: false
            },
        ];
        (modulesApi.getAll as any).mockResolvedValue(mockModules);

        render(<ModulesPage />);

        await waitFor(() => {
            expect(screen.getByText('Module A')).toBeInTheDocument();
            expect(screen.getByText('Module B')).toBeInTheDocument();
        });

        expect(screen.getByText('MODULE_A')).toBeInTheDocument();
        expect(screen.getByText('MODULE_B')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (modulesApi.getAll as any).mockResolvedValue([]);
        render(<ModulesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Module/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });
    });

    it('creates a module and closes panel', async () => {
        (modulesApi.getAll as any).mockResolvedValue([]);
        (modulesApi.create as any).mockResolvedValue({
            id: '3',
            variableName: 'NEW_MODULE',
            label: 'New Module',
            description: 'New Desc',
            versionNumber: 1,
            isActive: true
        });

        render(<ModulesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Module/i })).toBeInTheDocument();
        });

        // Fill form - use regex to match label
        fireEvent.change(screen.getByLabelText(/Variable Name/i), { target: { value: 'NEW_MODULE' } });
        fireEvent.change(screen.getByLabelText(/^Label/i), { target: { value: 'New Module' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(modulesApi.create).toHaveBeenCalledWith(
                expect.objectContaining({
                    variableName: 'NEW_MODULE',
                    label: 'New Module'
                })
            );
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Module' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(modulesApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (modulesApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { VariableName: ['Variable name is required'] }
        };
        (modulesApi.create as any).mockRejectedValue(errorResponse);

        render(<ModulesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Variable name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockModule = {
            id: '1',
            variableName: 'TEST_MODULE',
            label: 'Test Module',
            description: 'Test Desc',
            versionNumber: 1,
            isActive: true
        };
        const mockModuleDetail = {
            ...mockModule,
            questions: []
        };
        (modulesApi.getAll as any).mockResolvedValue([mockModule]);
        (modulesApi.getById as any).mockResolvedValue(mockModuleDetail);

        render(<ModulesPage />);
        await waitFor(() => expect(screen.getByText('Test Module')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Module'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Module' })).toBeInTheDocument();
        });
    });

    it('deletes a module with confirmation', async () => {
        const mockModule = {
            id: '1',
            variableName: 'TEST_MODULE',
            label: 'Test Module',
            description: 'Test Desc',
            versionNumber: 1,
            isActive: true
        };
        (modulesApi.getAll as any).mockResolvedValue([mockModule]);
        (modulesApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<ModulesPage />);
        await waitFor(() => expect(screen.getByText('Test Module')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(modulesApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(modulesApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters modules based on search input', async () => {
        const mockModules = [
            {
                id: '1',
                variableName: 'TEST_MODULE',
                label: 'Test Module',
                versionNumber: 1,
                isActive: true
            },
        ];
        (modulesApi.getAll as any).mockResolvedValue(mockModules);

        render(<ModulesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'Test' } });

        await waitFor(() => {
            expect(modulesApi.getAll).toHaveBeenCalledWith('Test');
        });
    });
});
