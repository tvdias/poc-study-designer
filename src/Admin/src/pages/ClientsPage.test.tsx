import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ClientsPage } from './ClientsPage';
import { clientsApi } from '../services/api';

// Mock the API module
vi.mock('../services/api', () => ({
    clientsApi: {
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

describe('ClientsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (clientsApi.getAll as any).mockImplementation(() => new Promise(() => { }));
        render(<ClientsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders clients list after loading', async () => {
        const mockClients = [
            { id: '1', accountName: 'Client 1', companyNumber: 'CN001', customerNumber: 'CU001', companyCode: 'CC001', isActive: true },
            { id: '2', accountName: 'Client 2', companyNumber: 'CN002', customerNumber: 'CU002', companyCode: 'CC002', isActive: false },
        ];
        (clientsApi.getAll as any).mockResolvedValue(mockClients);

        render(<ClientsPage />);

        await waitFor(() => {
            expect(screen.getByText('Client 1')).toBeInTheDocument();
            expect(screen.getByText('Client 2')).toBeInTheDocument();
        });

        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (clientsApi.getAll as any).mockResolvedValue([]);
        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Client/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Account Name *')).toHaveValue('');
    });

    it('creates a client successfully', async () => {
        (clientsApi.getAll as any).mockResolvedValue([]);
        (clientsApi.create as any).mockResolvedValue({ 
            id: '3', 
            accountName: 'New Client', 
            companyNumber: 'CN003',
            customerNumber: 'CU003',
            companyCode: 'CC003',
            isActive: true 
        });

        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Client/i })).toBeInTheDocument();
        });

        fireEvent.change(screen.getByLabelText('Account Name *'), { target: { value: 'New Client' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(clientsApi.create).toHaveBeenCalledWith({
                accountName: 'New Client',
                companyNumber: null,
                customerNumber: null,
                companyCode: null,
                isActive: true
            });
        });
    });

    it('displays validation errors when create fails', async () => {
        (clientsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { AccountName: ['Account Name is required'] }
        };
        (clientsApi.create as any).mockRejectedValue(errorResponse);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Account Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockClient = { 
            id: '1', 
            accountName: 'Test Client', 
            companyNumber: 'CN001',
            customerNumber: 'CU001',
            companyCode: 'CC001',
            isActive: true 
        };
        (clientsApi.getAll as any).mockResolvedValue([mockClient]);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.getByText('Test Client')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Client'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Client' })).toBeInTheDocument();
        });
    });

    it('deletes a client with confirmation', async () => {
        const mockClient = { 
            id: '1', 
            accountName: 'Test Client', 
            companyNumber: 'CN001',
            customerNumber: 'CU001',
            companyCode: 'CC001',
            isActive: true 
        };
        (clientsApi.getAll as any).mockResolvedValue([mockClient]);
        (clientsApi.delete as any).mockResolvedValue();

        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.getByText('Test Client')).toBeInTheDocument());

        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(clientsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(clientsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });

    it('filters clients based on search', async () => {
        const mockClients = [
            { id: '1', accountName: 'Client Alpha', companyNumber: 'CN001', customerNumber: 'CU001', companyCode: 'CC001', isActive: true },
        ];
        (clientsApi.getAll as any).mockResolvedValue(mockClients);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.getByText('Client Alpha')).toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText('Search clients...');
        fireEvent.change(searchInput, { target: { value: 'Alpha' } });

        await waitFor(() => {
            expect(clientsApi.getAll).toHaveBeenCalledWith('Alpha');
        });
    });

    it('updates a client and returns to view mode', async () => {
        const mockClient = {
            id: '1',
            accountName: 'Original Client',
            customerNumber: 'CUST-001',
            companyNumber: null,
            companyCode: null,
            isActive: true
        };
        const updatedClient = {
            id: '1',
            accountName: 'Updated Client',
            customerNumber: 'CUST-001',
            companyNumber: null,
            companyCode: null,
            isActive: true
        };
        (clientsApi.getAll as any).mockResolvedValue([mockClient]);
        (clientsApi.update as any).mockResolvedValue(updatedClient);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.getByText('Original Client')).toBeInTheDocument());

        // Open view
        fireEvent.click(screen.getByText('Original Client'));
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Original Client' })).toBeInTheDocument();
        });

        // Click Edit
        const editBtns = screen.getAllByTitle('Edit');
        fireEvent.click(editBtns[0]);

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /Edit Client/i })).toBeInTheDocument();
        });

        // Update name
        fireEvent.change(screen.getByLabelText(/Account Name/i), { target: { value: 'Updated Client' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(clientsApi.update).toHaveBeenCalled();
        });

        // Should return to view mode
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Updated Client' })).toBeInTheDocument();
        });
    });

    it('refreshes the list when refresh button is clicked', async () => {
        const mockClients = [
            {
                id: '1',
                accountName: 'Test Client',
                customerNumber: 'CUST-001',
                companyNumber: null,
                companyCode: null,
                isActive: true
            }
        ];
        (clientsApi.getAll as any).mockResolvedValue(mockClients);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.getByText('Test Client')).toBeInTheDocument());

        const refreshBtn = screen.getByRole('button', { name: /refresh/i });
        fireEvent.click(refreshBtn);

        await waitFor(() => {
            expect(clientsApi.getAll).toHaveBeenCalledTimes(2);
        });
    });
});
