// API Types
export interface Project {
    id: string;
    name: string;
    description?: string;
    clientId?: string;
    clientName?: string;
    commissioningMarketId?: string;
    commissioningMarketName?: string;
    methodology?: 'CATI' | 'CAPI' | 'CAWI' | 'Online' | 'Mixed';
    productId?: string;
    productName?: string;
    owner?: string;
    status: 'Active' | 'OnHold' | 'Closed';
    costManagementEnabled: boolean;
    modifiedOn?: string;
    createdOn: string;
}

export interface CreateProjectRequest {
    name: string;
    description?: string;
    clientId?: string;
    commissioningMarketId?: string;
    methodology?: 'CATI' | 'CAPI' | 'CAWI' | 'Online' | 'Mixed';
    productId?: string;
    owner?: string;
    status?: 'Active' | 'OnHold' | 'Closed';
    costManagementEnabled?: boolean;
}

export interface UpdateProjectRequest {
    name: string;
    description?: string;
    clientId?: string;
    commissioningMarketId?: string;
    methodology?: 'CATI' | 'CAPI' | 'CAWI' | 'Online' | 'Mixed';
    productId?: string;
    owner?: string;
    status: 'Active' | 'OnHold' | 'Closed';
    costManagementEnabled: boolean;
}

export interface ValidationErrorResponse {
    type: string;
    title: string;
    status: number;
    errors: Record<string, string[]>;
}

const API_BASE = '/api';

export const projectsApi = {
    getAll: async (query?: string): Promise<Project[]> => {
        const url = query ? `${API_BASE}/projects?query=${encodeURIComponent(query)}` : `${API_BASE}/projects`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch projects');
        return response.json();
    },

    getById: async (id: string): Promise<Project> => {
        const response = await fetch(`${API_BASE}/projects/${id}`);
        if (!response.ok) {
            if (response.status === 404) throw new Error('Project not found');
            throw new Error('Failed to fetch project');
        }
        return response.json();
    },

    create: async (data: CreateProjectRequest): Promise<Project> => {
        const response = await fetch(`${API_BASE}/projects`, {
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

    update: async (id: string, data: UpdateProjectRequest): Promise<Project> => {
        const response = await fetch(`${API_BASE}/projects/${id}`, {
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
        const response = await fetch(`${API_BASE}/projects/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete project');
    }
};

export interface Client {
    id: string;
    accountName: string;
    companyNumber?: string;
    customerNumber?: string;
    companyCode?: string;
    createdOn: string;
}

export interface CommissioningMarket {
    id: string;
    isoCode: string;
    name: string;
}

export const clientsApi = {
    getAll: async (query?: string): Promise<Client[]> => {
        const url = query ? `${API_BASE}/clients?query=${encodeURIComponent(query)}` : `${API_BASE}/clients`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch clients');
        return response.json();
    }
};

export const commissioningMarketsApi = {
    getAll: async (query?: string): Promise<CommissioningMarket[]> => {
        const url = query ? `${API_BASE}/commissioning-markets?query=${encodeURIComponent(query)}` : `${API_BASE}/commissioning-markets`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch commissioning markets');
        return response.json();
    }
};
