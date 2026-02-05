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

export interface CommissioningMarket {
    id: string;
    isoCode: string;
    name: string;
    isActive: boolean;
}

export interface CreateCommissioningMarketRequest {
    isoCode: string;
    name: string;
}

export interface UpdateCommissioningMarketRequest {
    isoCode: string;
    name: string;
    isActive: boolean;
}

export interface FieldworkMarket {
    id: string;
    isoCode: string;
    name: string;
    isActive: boolean;
}

export interface CreateFieldworkMarketRequest {
    isoCode: string;
    name: string;
}

export interface UpdateFieldworkMarketRequest {
    isoCode: string;
    name: string;
    isActive: boolean;
}

export interface Client {
    id: string;
    name: string;
    integrationMetadata: string | null;
    productsModules: string | null;
    isActive: boolean;
}

export interface CreateClientRequest {
    name: string;
    integrationMetadata?: string | null;
    productsModules?: string | null;
}

export interface UpdateClientRequest {
    name: string;
    integrationMetadata?: string | null;
    productsModules?: string | null;
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

export const commissioningMarketsApi = {
    getAll: async (query?: string): Promise<CommissioningMarket[]> => {
        const url = query ? `${API_BASE}/commissioning-markets?query=${encodeURIComponent(query)}` : `${API_BASE}/commissioning-markets`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch commissioning markets');
        return response.json();
    },

    getById: async (id: string): Promise<CommissioningMarket> => {
        const response = await fetch(`${API_BASE}/commissioning-markets/${id}`);
        if (!response.ok) throw new Error('Failed to fetch commissioning market');
        return response.json();
    },

    create: async (data: CreateCommissioningMarketRequest): Promise<CommissioningMarket> => {
        const response = await fetch(`${API_BASE}/commissioning-markets`, {
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

    update: async (id: string, data: UpdateCommissioningMarketRequest): Promise<CommissioningMarket> => {
        const response = await fetch(`${API_BASE}/commissioning-markets/${id}`, {
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
        const response = await fetch(`${API_BASE}/commissioning-markets/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete commissioning market');
    }
};

export const fieldworkMarketsApi = {
    getAll: async (query?: string): Promise<FieldworkMarket[]> => {
        const url = query ? `${API_BASE}/fieldwork-markets?query=${encodeURIComponent(query)}` : `${API_BASE}/fieldwork-markets`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch fieldwork markets');
        return response.json();
    },

    getById: async (id: string): Promise<FieldworkMarket> => {
        const response = await fetch(`${API_BASE}/fieldwork-markets/${id}`);
        if (!response.ok) throw new Error('Failed to fetch fieldwork market');
        return response.json();
    },

    create: async (data: CreateFieldworkMarketRequest): Promise<FieldworkMarket> => {
        const response = await fetch(`${API_BASE}/fieldwork-markets`, {
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

    update: async (id: string, data: UpdateFieldworkMarketRequest): Promise<FieldworkMarket> => {
        const response = await fetch(`${API_BASE}/fieldwork-markets/${id}`, {
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
        const response = await fetch(`${API_BASE}/fieldwork-markets/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete fieldwork market');
    }
};

export const clientsApi = {
    getAll: async (query?: string): Promise<Client[]> => {
        const url = query ? `${API_BASE}/clients?query=${encodeURIComponent(query)}` : `${API_BASE}/clients`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch clients');
        return response.json();
    },

    getById: async (id: string): Promise<Client> => {
        const response = await fetch(`${API_BASE}/clients/${id}`);
        if (!response.ok) throw new Error('Failed to fetch client');
        return response.json();
    },

    create: async (data: CreateClientRequest): Promise<Client> => {
        const response = await fetch(`${API_BASE}/clients`, {
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

    update: async (id: string, data: UpdateClientRequest): Promise<Client> => {
        const response = await fetch(`${API_BASE}/clients/${id}`, {
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
        const response = await fetch(`${API_BASE}/clients/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete client');
    }
};

