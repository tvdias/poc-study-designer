import { render, screen, fireEvent, waitFor } from '@testing-library/react';
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
        (fieldworkMarketsApi.getAll as any).mockImplementation(() => new Promise(() => { }));
        render(<FieldworkMarketsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders fieldwork markets list after loading', async () => {
        const mockMarkets = [
            { id: '1', isoCode: 'US', name: 'United States', isActive: true },
            { id: '2', isoCode: 'CA', name: 'Canada', isActive: false },
        ];
        (fieldworkMarketsApi.getAll as any).mockResolvedValue(mockMarkets);

        render(<FieldworkMarketsPage />);

        await waitFor(() => {
            expect(screen.getByText('United States')).toBeInTheDocument();
            expect(screen.getByText('Canada')).toBeInTheDocument();
        });

        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
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

    it('creates a fieldwork market successfully', async () => {
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([]);
        (fieldworkMarketsApi.create as any).mockResolvedValue({ 
            id: '3', 
            isoCode: 'AU', 
            name: 'Australia', 
            isActive: true 
        });

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Fieldwork Market/i })).toBeInTheDocument();
        });

        fireEvent.change(screen.getByLabelText('ISO Code'), { target: { value: 'AU' } });
        fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'Australia' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(fieldworkMarketsApi.create).toHaveBeenCalledWith({
                isoCode: 'AU',
                name: 'Australia'
            });
        });
    });

    it('displays validation errors when create fails', async () => {
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { IsoCode: ['ISO Code is required'], Name: ['Name is required'] }
        };
        (fieldworkMarketsApi.create as any).mockRejectedValue(errorResponse);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('ISO Code is required')).toBeInTheDocument();
            expect(screen.getByText('Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockMarket = { id: '1', isoCode: 'JP', name: 'Japan', isActive: true };
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([mockMarket]);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('Japan')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Japan'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Japan' })).toBeInTheDocument();
        });
    });

    it('deletes a market with confirmation', async () => {
        const mockMarket = { id: '1', isoCode: 'NZ', name: 'New Zealand', isActive: true };
        (fieldworkMarketsApi.getAll as any).mockResolvedValue([mockMarket]);
        (fieldworkMarketsApi.delete as any).mockResolvedValue();

        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<FieldworkMarketsPage />);
        await waitFor(() => expect(screen.getByText('New Zealand')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(fieldworkMarketsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(fieldworkMarketsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });
});
