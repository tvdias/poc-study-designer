import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CommissioningMarketsPage } from './CommissioningMarketsPage';
import { commissioningMarketsApi } from '../services/api';

// Mock the API module
vi.mock('../services/api', () => ({
    commissioningMarketsApi: {
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

describe('CommissioningMarketsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (commissioningMarketsApi.getAll as any).mockImplementation(() => new Promise(() => { }));
        render(<CommissioningMarketsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders commissioning markets list after loading', async () => {
        const mockMarkets = [
            { id: '1', isoCode: 'US', name: 'United States', isActive: true },
            { id: '2', isoCode: 'GB', name: 'United Kingdom', isActive: false },
        ];
        (commissioningMarketsApi.getAll as any).mockResolvedValue(mockMarkets);

        render(<CommissioningMarketsPage />);

        await waitFor(() => {
            expect(screen.getByText('United States')).toBeInTheDocument();
            expect(screen.getByText('United Kingdom')).toBeInTheDocument();
        });

        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (commissioningMarketsApi.getAll as any).mockResolvedValue([]);
        render(<CommissioningMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Commissioning Market/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });

        expect(screen.getByLabelText('ISO Code')).toHaveValue('');
        expect(screen.getByLabelText('Name')).toHaveValue('');
    });

    it('creates a commissioning market successfully', async () => {
        (commissioningMarketsApi.getAll as any).mockResolvedValue([]);
        (commissioningMarketsApi.create as any).mockResolvedValue({ 
            id: '3', 
            isoCode: 'FR', 
            name: 'France', 
            isActive: true 
        });

        render(<CommissioningMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Commissioning Market/i })).toBeInTheDocument();
        });

        fireEvent.change(screen.getByLabelText('ISO Code'), { target: { value: 'FR' } });
        fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'France' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(commissioningMarketsApi.create).toHaveBeenCalledWith({
                isoCode: 'FR',
                name: 'France'
            });
        });
    });

    it('displays validation errors when create fails', async () => {
        (commissioningMarketsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { IsoCode: ['ISO Code is required'], Name: ['Name is required'] }
        };
        (commissioningMarketsApi.create as any).mockRejectedValue(errorResponse);

        render(<CommissioningMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('ISO Code is required')).toBeInTheDocument();
            expect(screen.getByText('Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockMarket = { id: '1', isoCode: 'DE', name: 'Germany', isActive: true };
        (commissioningMarketsApi.getAll as any).mockResolvedValue([mockMarket]);

        render(<CommissioningMarketsPage />);
        await waitFor(() => expect(screen.getByText('Germany')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Germany'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Germany' })).toBeInTheDocument();
        });
    });

    it('deletes a market with confirmation', async () => {
        const mockMarket = { id: '1', isoCode: 'ES', name: 'Spain', isActive: true };
        (commissioningMarketsApi.getAll as any).mockResolvedValue([mockMarket]);
        (commissioningMarketsApi.delete as any).mockResolvedValue();

        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<CommissioningMarketsPage />);
        await waitFor(() => expect(screen.getByText('Spain')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(commissioningMarketsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(commissioningMarketsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });
});
