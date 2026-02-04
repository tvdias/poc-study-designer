export interface Tag {
    id: string;
    name: string;
    isActive: boolean;
}

export interface CreateTagRequest {
    name: string;
}

export interface UpdateTagRequest {
    name: string;
    isActive: boolean;
}

export interface ValidationErrorResponse {
    type: string;
    title: string;
    status: number;
    errors: Record<string, string[]>;
}

const API_BASE = '/api';

export const tagsApi = {
    getAll: async (query?: string): Promise<Tag[]> => {
        const url = query ? `${API_BASE}/tags?query=${encodeURIComponent(query)}` : `${API_BASE}/tags`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch tags');
        return response.json();
    },

    getById: async (id: string): Promise<Tag> => {
        const response = await fetch(`${API_BASE}/tags/${id}`);
        if (!response.ok) throw new Error('Failed to fetch tag');
        return response.json();
    },

    create: async (data: CreateTagRequest): Promise<Tag> => {
        const response = await fetch(`${API_BASE}/tags`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw { status: response.status, ...errorData };
        }
        return response.json();
    },

    update: async (id: string, data: UpdateTagRequest): Promise<Tag> => {
        const response = await fetch(`${API_BASE}/tags/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw { status: response.status, ...errorData };
        }
        return response.json();
    },

    delete: async (id: string): Promise<void> => {
        const response = await fetch(`${API_BASE}/tags/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete tag');
    }
};
