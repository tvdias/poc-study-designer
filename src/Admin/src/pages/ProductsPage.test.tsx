import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ProductsPage } from './ProductsPage';
import { productsApi } from '../services/api';

// Mock the API module
vi.mock('../services/api', () => ({
    productsApi: {
        getAll: vi.fn(),
        getById: vi.fn(),
        create: vi.fn(),
        update: vi.fn(),
        delete: vi.fn(),
    },
    productConfigQuestionsApi: {
        create: vi.fn(),
        delete: vi.fn(),
    },
    configurationQuestionsApi: {
        getAll: vi.fn(),
    },
    productTemplatesApi: {
        create: vi.fn(),
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

describe('ProductsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (productsApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<ProductsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders products list after loading', async () => {
        const mockProducts = [
            { id: '1', name: 'Product 1', description: 'Description 1', isActive: true },
            { id: '2', name: 'Product 2', description: 'Description 2', isActive: false },
        ];
        (productsApi.getAll as any).mockResolvedValue(mockProducts);

        render(<ProductsPage />);

        await waitFor(() => {
            expect(screen.getByText('Product 1')).toBeInTheDocument();
            expect(screen.getByText('Product 2')).toBeInTheDocument();
        });

        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (productsApi.getAll as any).mockResolvedValue([]);
        render(<ProductsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Product/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });

        expect(screen.getByLabelText(/Name/i)).toHaveValue('');
    });

    it('creates a product and closes panel', async () => {
        (productsApi.getAll as any).mockResolvedValue([]);
        (productsApi.create as any).mockResolvedValue({ id: '3', name: 'New Product', description: 'New Desc', isActive: true });

        render(<ProductsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Product/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText(/Name/i), { target: { value: 'New Product' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(productsApi.create).toHaveBeenCalledWith({ name: 'New Product', description: undefined });
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Product' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(productsApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (productsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { Name: ['Name is required'] }
        };
        (productsApi.create as any).mockRejectedValue(errorResponse);

        render(<ProductsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockProduct = { id: '1', name: 'Test Product', description: 'Test Desc', isActive: true };
        const mockProductDetail = {
            ...mockProduct,
            productTemplates: [],
            configurationQuestions: []
        };
        (productsApi.getAll as any).mockResolvedValue([mockProduct]);
        (productsApi.getById as any).mockResolvedValue(mockProductDetail);

        render(<ProductsPage />);
        await waitFor(() => expect(screen.getByText('Test Product')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Product'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Product' })).toBeInTheDocument();
        });
    });

    it('deletes a product with confirmation', async () => {
        const mockProduct = { id: '1', name: 'Test Product', description: 'Test Desc', isActive: true };
        (productsApi.getAll as any).mockResolvedValue([mockProduct]);
        (productsApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<ProductsPage />);
        await waitFor(() => expect(screen.getByText('Test Product')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(productsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(productsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters products based on search input', async () => {
        const mockProducts = [
            { id: '1', name: 'Alpha Product', description: 'Desc', isActive: true },
        ];
        (productsApi.getAll as any).mockResolvedValue(mockProducts);

        render(<ProductsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'Alpha' } });

        await waitFor(() => {
            expect(productsApi.getAll).toHaveBeenCalledWith('Alpha');
        });
    });
});
