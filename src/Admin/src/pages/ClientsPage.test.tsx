import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
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
        (clientsApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<ClientsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders clients list after loading', async () => {
        const mockClients = [
            {
                id: '1',
                accountName: 'Client 1',
                companyNumber: '12345',
                customerNumber: 'CUST-001',
                companyCode: 'C1',
                isActive: true
            },
            {
                id: '2',
                accountName: 'Client 2',
                companyNumber: '67890',
                customerNumber: 'CUST-002',
                companyCode: 'C2',
                isActive: false
            },
        ];
        (clientsApi.getAll as any).mockResolvedValue(mockClients);

        render(<ClientsPage />);

        await waitFor(() => {
            expect(screen.getByText('Client 1')).toBeInTheDocument();
            expect(screen.getByText('Client 2')).toBeInTheDocument();
        });

        expect(screen.getByText('CUST-001')).toBeInTheDocument();
        expect(screen.getByText('CUST-002')).toBeInTheDocument();
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

        expect(screen.getByLabelText(/Account Name/i)).toHaveValue('');
        expect(screen.getByLabelText(/Customer Number/i)).toHaveValue('');
    });

    it('creates a client and closes panel', async () => {
        (clientsApi.getAll as any).mockResolvedValue([]);
        (clientsApi.create as any).mockResolvedValue({
            id: '3',
            accountName: 'New Client',
            customerNumber: 'CUST-003',
            companyNumber: null,
            companyCode: null,
            isActive: true
        });

        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Client/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText(/Account Name/i), { target: { value: 'New Client' } });
        fireEvent.change(screen.getByLabelText(/Customer Number/i), { target: { value: 'CUST-003' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(clientsApi.create).toHaveBeenCalledWith(
                expect.objectContaining({
                    accountName: 'New Client',
                    customerNumber: 'CUST-003'
                })
            );
        });

        // Should close the panel and return to list
        await waitFor(() => {
            expect(screen.queryByRole('heading', { name: 'New Client' })).not.toBeInTheDocument();
            // The creation button (New) should be visible (meaning we are back to list)
            expect(screen.getByRole('button', { name: /new/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(clientsApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (clientsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { AccountName: ['Account name is required.'] }
        };
        (clientsApi.create as any).mockRejectedValue(errorResponse);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Account name is required.')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockClient = {
            id: '1',
            accountName: 'Test Client',
            companyNumber: '12345',
            customerNumber: 'CUST-001',
            companyCode: 'TC',
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
            companyNumber: '12345',
            customerNumber: 'CUST-001',
            companyCode: 'TC',
            isActive: true
        };
        (clientsApi.getAll as any).mockResolvedValue([mockClient]);
        (clientsApi.delete as any).mockResolvedValue();

        // Mock confirm
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

    it('filters clients based on search input', async () => {
        const mockClients = [
            {
                id: '1',
                accountName: 'Alpha Client',
                customerNumber: 'CUST-001',
                companyNumber: null,
                companyCode: null,
                isActive: true
            },
        ];
        (clientsApi.getAll as any).mockResolvedValue(mockClients);

        render(<ClientsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        const searchInput = screen.getByPlaceholderText(/search/i);
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
