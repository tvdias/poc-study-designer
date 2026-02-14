import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ProductTemplatesPage } from './ProductTemplatesPage';
import { productTemplatesApi, productsApi } from '../services/api';

// Mock the API modules
vi.mock('../services/api', () => ({
    productTemplatesApi: {
        getAll: vi.fn(),
        getById: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    productsApi: {
        getAll: vi.fn(),
    },
    productTemplateLinesApi: {
        getAll: vi.fn().mockResolvedValue([]),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    modulesApi: {
        getAll: vi.fn(),
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

describe('ProductTemplatesPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        (productsApi.getAll as any).mockResolvedValue([]);
    });

    it('shows loading state initially', async () => {
        (productTemplatesApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<ProductTemplatesPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders product templates list after loading', async () => {
        const mockTemplates = [
            {
                id: '1',
                name: 'Template A',
                version: 1,
                productId: 'prod-1',
                productName: 'Product 1',
                isActive: true
            },
            {
                id: '2',
                name: 'Template B',
                version: 2,
                productId: 'prod-2',
                productName: 'Product 2',
                isActive: false
            },
        ];
        (productTemplatesApi.getAll as any).mockResolvedValue(mockTemplates);

        render(<ProductTemplatesPage />);

        await waitFor(() => {
            expect(screen.getByText('Template A')).toBeInTheDocument();
            expect(screen.getByText('Template B')).toBeInTheDocument();
        });

        expect(screen.getByText('Product 1')).toBeInTheDocument();
        expect(screen.getByText('Product 2')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (productTemplatesApi.getAll as any).mockResolvedValue([]);
        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Product Template/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });
    });

    it('creates a product template and closes panel', async () => {
        const mockProducts = [{ id: 'prod-1', name: 'Product 1', isActive: true }];
        (productTemplatesApi.getAll as any).mockResolvedValue([]);
        (productsApi.getAll as any).mockResolvedValue(mockProducts);
        (productTemplatesApi.create as any).mockResolvedValue({
            id: '3',
            name: 'New Template',
            version: 1,
            productId: 'prod-1',
            productName: 'Product 1',
            isActive: true
        });

        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Product Template/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText(/Name/i), { target: { value: 'New Template' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(productTemplatesApi.create).toHaveBeenCalledWith(
                expect.objectContaining({
                    name: 'New Template'
                })
            );
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Product Template' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(productTemplatesApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (productTemplatesApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { Name: ['Name is required'] }
        };
        (productTemplatesApi.create as any).mockRejectedValue(errorResponse);

        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockTemplate = {
            id: '1',
            name: 'Test Template',
            version: 1,
            productId: 'prod-1',
            productName: 'Product 1',
            isActive: true
        };
        (productTemplatesApi.getAll as any).mockResolvedValue([mockTemplate]);

        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.getByText('Test Template')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Template'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Template' })).toBeInTheDocument();
        });
    });

    it('deletes a product template with confirmation', async () => {
        const mockTemplate = {
            id: '1',
            name: 'Test Template',
            version: 1,
            productId: 'prod-1',
            productName: 'Product 1',
            isActive: true
        };
        (productTemplatesApi.getAll as any).mockResolvedValue([mockTemplate]);
        (productTemplatesApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.getByText('Test Template')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(productTemplatesApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(productTemplatesApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters templates based on search input', async () => {
        const mockTemplates = [
            {
                id: '1',
                name: 'Alpha Template',
                version: 1,
                productId: 'prod-1',
                productName: 'Product 1',
                isActive: true
            },
        ];
        (productTemplatesApi.getAll as any).mockResolvedValue(mockTemplates);

        render(<ProductTemplatesPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'Alpha' } });

        await waitFor(() => {
            expect(productTemplatesApi.getAll).toHaveBeenCalledWith('Alpha');
        });
    });
});
