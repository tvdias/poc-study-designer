import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TagsPage } from './TagsPage';
import { tagsApi } from '../services/api';

// Mock the API module
vi.mock('../services/api', () => ({
    tagsApi: {
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

describe('TagsPage', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('shows loading state initially', async () => {
        (tagsApi.getAll as any).mockImplementation(() => new Promise(() => { })); // Never resolves
        render(<TagsPage />);
        expect(screen.getByText('Loading...')).toBeInTheDocument();
    });

    it('renders tags list after loading', async () => {
        const mockTags = [
            { id: '1', name: 'Tag 1', isActive: true },
            { id: '2', name: 'Tag 2', isActive: false },
        ];
        (tagsApi.getAll as any).mockResolvedValue(mockTags);

        render(<TagsPage />);

        await waitFor(() => {
            expect(screen.getByText('Tag 1')).toBeInTheDocument();
            expect(screen.getByText('Tag 2')).toBeInTheDocument();
        });

        expect(screen.getByText('Active')).toBeInTheDocument();
        expect(screen.getByText('Inactive')).toBeInTheDocument();
    });

    it('opens create panel when New button is clicked', async () => {
        (tagsApi.getAll as any).mockResolvedValue([]);
        render(<TagsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            // Check for the Heading and the Save button
            expect(screen.getByRole('heading', { name: /New Tag/i })).toBeInTheDocument();
            expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
        });

        expect(screen.getByLabelText('Name')).toHaveValue('');
    });

    it('creates a tag and refreshes list', async () => {
        (tagsApi.getAll as any).mockResolvedValue([]);
        (tagsApi.create as any).mockResolvedValue({ id: '3', name: 'New Tag', isActive: true });

        render(<TagsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        // Open create
        fireEvent.click(screen.getByRole('button', { name: /new/i }));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: /New Tag/i })).toBeInTheDocument();
        });

        // Fill form
        fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'New Tag' } });
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(tagsApi.create).toHaveBeenCalledWith({ name: 'New Tag' });
        });

        // Should switch to view mode (Title becomes the tag name)
        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'New Tag' })).toBeInTheDocument();
            // Ensure 'Edit' button is present (marks view mode)
            expect(screen.getByRole('button', { name: /edit/i })).toBeInTheDocument();
        });

        // Check list refresh
        expect(tagsApi.getAll).toHaveBeenCalledTimes(2);
    });

    it('displays validation errors when create fails', async () => {
        (tagsApi.getAll as any).mockResolvedValue([]);
        const errorResponse = {
            status: 400,
            errors: { Name: ['Name is required'] }
        };
        (tagsApi.create as any).mockRejectedValue(errorResponse);

        render(<TagsPage />);
        await waitFor(() => expect(screen.queryByText('Loading...')).not.toBeInTheDocument());

        fireEvent.click(screen.getByRole('button', { name: /new/i }));
        fireEvent.click(screen.getByRole('button', { name: /save/i }));

        await waitFor(() => {
            expect(screen.getByText('Name is required')).toBeInTheDocument();
        });
    });

    it('opens view details when row is clicked', async () => {
        const mockTag = { id: '1', name: 'Test Tag', isActive: true };
        (tagsApi.getAll as any).mockResolvedValue([mockTag]);

        render(<TagsPage />);
        await waitFor(() => expect(screen.getByText('Test Tag')).toBeInTheDocument());

        fireEvent.click(screen.getByText('Test Tag'));

        await waitFor(() => {
            expect(screen.getByRole('heading', { name: 'Test Tag' })).toBeInTheDocument();
        });
    });

    it('deletes a tag with confirmation', async () => {
        const mockTag = { id: '1', name: 'Test Tag', isActive: true };
        (tagsApi.getAll as any).mockResolvedValue([mockTag]);
        (tagsApi.delete as any).mockResolvedValue();

        // Mock confirm
        const confirmSpy = vi.spyOn(window, 'confirm');
        confirmSpy.mockImplementation(() => true);

        render(<TagsPage />);
        await waitFor(() => expect(screen.getByText('Test Tag')).toBeInTheDocument());

        // Use title (from Action button title attribute, assuming icons have no text content)
        const deleteBtns = screen.getAllByTitle('Delete');
        fireEvent.click(deleteBtns[0]);

        expect(confirmSpy).toHaveBeenCalled();
        expect(tagsApi.delete).toHaveBeenCalledWith('1');

        await waitFor(() => {
            expect(tagsApi.getAll).toHaveBeenCalledTimes(2);
        });

        confirmSpy.mockRestore();
    });
});
