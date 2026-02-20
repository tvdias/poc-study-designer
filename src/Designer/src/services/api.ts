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
    hasStudies: boolean;
    studyCount: number;
    lastStudyModifiedOn?: string;
    questionnaireLineCount: number;
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

export interface FieldworkMarket {
    id: string;
    isoCode: string;
    name: string;
}

export const fieldworkMarketsApi = {
    getAll: async (query?: string): Promise<FieldworkMarket[]> => {
        const url = query ? `${API_BASE}/fieldwork-markets?query=${encodeURIComponent(query)}` : `${API_BASE}/fieldwork-markets`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch fieldwork markets');
        return response.json();
    }
};

export interface QuestionBankItemSummary {
    id: string;
    variableName: string;
    version: number;
    questionText?: string;
    questionType?: string;
    classification?: string;
    questionRationale?: string;
}

export interface QuestionBankItem {
    id: string;
    variableName: string;
    version: number;
    questionText?: string;
    questionTitle?: string;
    questionType?: string;
    classification?: string;
    status?: string;
    methodology?: string;
    questionRationale?: string;
    scaleType?: string;
    displayType?: string;
    instructionText?: string;
}

export interface QuestionnaireLine {
    id: string;
    projectId: string;
    questionBankItemId: string;
    sortOrder: number;
    variableName: string;
    version: number;
    questionText?: string;
    questionTitle?: string;
    questionType?: string;
    classification?: string;
    questionRationale?: string;
    scraperNotes?: string;
    customNotes?: string;
    rowSortOrder?: number;
    columnSortOrder?: number;
    answerMin?: number;
    answerMax?: number;
    questionFormatDetails?: string;
    isDummy: boolean;
}

export interface AddQuestionnaireLineRequest {
    questionBankItemId: string;
}

export interface UpdateQuestionnaireLineRequest {
    questionText?: string;
    questionTitle?: string;
    questionRationale?: string;
    scraperNotes?: string;
    customNotes?: string;
    rowSortOrder?: number;
    columnSortOrder?: number;
    answerMin?: number;
    answerMax?: number;
    questionFormatDetails?: string;
}

export interface UpdateQuestionnaireLinesSortOrderRequest {
    items: { id: string; sortOrder: number }[];
}

export const questionBankApi = {
    getAll: async (query?: string): Promise<QuestionBankItem[]> => {
        const url = query ? `${API_BASE}/question-bank?query=${encodeURIComponent(query)}` : `${API_BASE}/question-bank`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch question bank items');
        return response.json();
    }
};

export const questionnaireLinesApi = {
    getAll: async (projectId: string): Promise<QuestionnaireLine[]> => {
        const response = await fetch(`${API_BASE}/projects/${projectId}/questionnairelines`);
        if (!response.ok) throw new Error('Failed to fetch questionnaire lines');
        return response.json();
    },

    add: async (projectId: string, data: AddQuestionnaireLineRequest): Promise<QuestionnaireLine> => {
        const response = await fetch(`${API_BASE}/projects/${projectId}/questionnairelines`, {
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

    update: async (projectId: string, id: string, data: UpdateQuestionnaireLineRequest): Promise<QuestionnaireLine> => {
        const response = await fetch(`${API_BASE}/projects/${projectId}/questionnairelines/${id}`, {
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

    updateSortOrder: async (projectId: string, data: UpdateQuestionnaireLinesSortOrderRequest): Promise<void> => {
        const response = await fetch(`${API_BASE}/projects/${projectId}/questionnairelines/sort-order`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw { status: response.status, ...errorData };
        }
    },

    delete: async (projectId: string, id: string): Promise<void> => {
        const response = await fetch(`${API_BASE}/projects/${projectId}/questionnairelines/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete questionnaire line');
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
export interface ManagedList {
    id: string;
    projectId: string;
    name: string;
    description?: string;
    status: 'Active' | 'Inactive';
    items: ManagedListItem[];
    sourceType?: string;
    isAutoGenerated?: boolean;
    everInSnapshot?: boolean;
    firstSnapshotDate?: string | null;
    modifiedOn?: string | null;
}

export interface ManagedListItem {
    id: string;
    managedListId: string;
    code: string;
    label: string;
    sortOrder: number;
    isActive: boolean;
    metadata?: string;
    sourceType?: string;
    everInSnapshot?: boolean;
}

export interface CreateManagedListRequest {
    projectId: string;
    name: string;
    description?: string;
}

export interface UpdateManagedListRequest {
    name?: string;
    description?: string;
}

export interface ManagedListItemRequest {
    code: string;
    label: string;
    sortOrder: number;
    metadata?: string;
}

export interface StudySummary {
    studyId: string;
    name: string;
    version: number;
    status: 'Draft' | 'Final' | 'Archived' | 'Completed' | 'Abandoned'; // Updated enum
    createdOn: string;
    createdBy: string;
    questionCount: number;
    category?: string;
    fieldworkMarketName?: string;
}

export interface CreateStudyRequest {
    projectId: string;
    name: string;
    category: string;
    maconomyJobNumber: string;
    projectOperationsUrl: string;
    scripterNotes?: string;
    fieldworkMarketId: string;
}

export const managedListsApi = {
    getAll: async (projectId: string): Promise<ManagedList[]> => {
        const response = await fetch(`${API_BASE}/managedlists?projectId=${projectId}`);
        if (!response.ok) throw new Error('Failed to fetch managed lists');
        return response.json();
    },

    getById: async (id: string): Promise<ManagedList> => {
        const response = await fetch(`${API_BASE}/managedlists/${id}`);
        if (!response.ok) throw new Error('Failed to fetch managed list');
        return response.json();
    },

    create: async (data: CreateManagedListRequest): Promise<ManagedList> => {
        const response = await fetch(`${API_BASE}/managedlists`, {
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

    update: async (id: string, data: UpdateManagedListRequest): Promise<ManagedList> => {
        const response = await fetch(`${API_BASE}/managedlists/${id}`, {
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
        const response = await fetch(`${API_BASE}/managedlists/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete managed list');
    },

    addItem: async (managedListId: string, data: ManagedListItemRequest): Promise<ManagedListItem> => {
        const response = await fetch(`${API_BASE}/managedlists/${managedListId}/items`, {
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

    updateItem: async (managedListId: string, itemId: string, data: ManagedListItemRequest): Promise<ManagedListItem> => {
        const response = await fetch(`${API_BASE}/managedlists/${managedListId}/items/${itemId}`, {
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

    deleteItem: async (managedListId: string, itemId: string): Promise<void> => {
        const response = await fetch(`${API_BASE}/managedlists/${managedListId}/items/${itemId}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete managed list item');
    }
};

export interface CreateStudyResponse {
    studyId: string;
}

export const studiesApi = {
    getAll: async (projectId: string): Promise<StudySummary[]> => {
        const response = await fetch(`${API_BASE}/studies?projectId=${projectId}`);
        if (!response.ok) throw new Error('Failed to fetch studies');
        const data = await response.json();
        return data.studies; // Response wrapper
    },

    create: async (data: CreateStudyRequest): Promise<CreateStudyResponse> => {
        const response = await fetch(`${API_BASE}/studies`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw { status: response.status, ...errorData };
        }
        return response.json();
    }
};
