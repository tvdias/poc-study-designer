import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { FieldworkMarketsPage } from './FieldworkMarketsPage';
import { fieldworkMarketsApi } from '../services/api';

// Mock the API module
vi.mock('../services/api', () => ({
    fieldworkMarketsApi: {
        getAll: vi.fn(),
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

describe('FieldworkMarketsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (fieldworkMarketsApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<FieldworkMarketsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders fieldwork markets list after loading', async () => {
        const mockMarkets = [
            { id: '1', isoCode: 'US', name: 'United States', isActive: true },
            { id: '2', isoCode: 'GB', name: 'United Kingdom', isActive: false },
        ];
        (fieldworkMarketsApi.getAll as any).mockResolvedValue(mockMarkets);

        render(<FieldworkMarketsPage />);

        await waitFor(() => {
            expect(screen.getByText('United States')).toBeInTheDocument();
            expect(screen.getByText('United Kingdom')).toBeInTheDocument();
        });

        expect(screen.getByText('US')).toBeInTheDocument();
        expect(screen.getByText('GB')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([]);
        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Fieldwork Market/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });

        expect(screen.getByLabelText('ISO Code')).toHaveValue('');
        expect(screen.getByLabelText('Name')).toHaveValue('');
    });

    it('creates a fieldwork market and closes panel', async () => {
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([]);
        (fieldworkMarketsApi.create as any).mockResolvedValue({ id: '3', isoCode: 'FR', name: 'France', isActive: true });

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Fieldwork Market/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText('ISO Code'), { target: { value: 'FR' } });
        fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'France' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(fieldworkMarketsApi.create).toHaveBeenCalledWith({ isoCode: 'FR', name: 'France' });
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Fieldwork Market' })).not.toBeInTheDocument();
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(fieldworkMarketsApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { IsoCode: ['ISO Code is required'] }
        };
        (fieldworkMarketsApi.create as any).mockRejectedValue(errorResponse);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('ISO Code is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockMarket = { id: '1', isoCode: 'US', name: 'United States', isActive: true };
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([mockMarket]);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('United States')).toBeInTheDocument());

        fireEvent.click(screen.getByText('United States'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'United States' })).toBeInTheDocument();
        });
    });

    it('deletes a fieldwork market with confirmation', async () => {
        const mockMarket = { id: '1', isoCode: 'US', name: 'United States', isActive: true };
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([mockMarket]);
        (fieldworkMarketsApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('United States')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(fieldworkMarketsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(fieldworkMarketsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters markets based on search input', async () => {
        const mockMarkets = [
            { id: '1', isoCode: 'US', name: 'United States', isActive: true },
        ];
        (fieldworkMarketsApi.getAll as any).mockResolvedValue(mockMarkets);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
        fireEvent.change(searchInput, { target: { value: 'United' } });

        await waitFor(() => {
            expect(fieldworkMarketsApi.getAll).toHaveBeenCalledWith('United');
        });
    });

    it('updates a fieldwork market and returns to view mode', async () => {
        const mockMarket = { id: '1', isoCode: 'US', name: 'United States', isActive: true };
        const updatedMarket = { id: '1', isoCode: 'US', name: 'USA', isActive: true };
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([mockMarket]);
        (fieldworkMarketsApi.update as any).mockResolvedValue(updatedMarket);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('United States')).toBeInTheDocument());

        // Open view
        fireEvent.click(screen.getByText('United States'));
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'United States' })).toBeInTheDocument();
        });

        // Click Edit
        const editBtns = screen.getAllByTitle('Edit');
        fireEvent.click(editBtns[0]);

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /Edit Fieldwork Market/i })).toBeInTheDocument();
        });

        // Update name
        fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'USA' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(fieldworkMarketsApi.update).toHaveBeenCalled();
        });

        // Should return to view mode
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'USA' })).toBeInTheDocument();
        });
    });

    it('refreshes the list when refresh button is clicked', async () => {
        const mockMarkets = [{ id: '1', isoCode: 'US', name: 'United States', isActive: true }];
        (fieldworkMarketsApi.getAll as any).mockResolvedValue(mockMarkets);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('United States')).toBeInTheDocument());

        const refreshBtn = screen.getByRole('button', { name: /refresh/i });
        fireEvent.click(refreshBtn);

        await waitFor(() => {
            expect(fieldworkMarketsApi.getAll).toHaveBeenCalledTimes(2);
        });
    });
});
