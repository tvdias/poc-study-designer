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
    accountName: string;
    companyNumber: string | null;
    customerNumber: string | null;
    companyCode: string | null;
    isActive: boolean;
}

export interface CreateClientRequest {
    accountName: string;
    companyNumber?: string | null;
    customerNumber?: string | null;
    companyCode?: string | null;
}

export interface UpdateClientRequest {
    accountName: string;
    companyNumber?: string | null;
    customerNumber?: string | null;
    companyCode?: string | null;
    isActive: boolean;
}

export interface Module {
    id: string;
    variableName: string;
    label: string;
    description?: string;
    versionNumber: number;
    parentModuleId?: string;
    instructions?: string;
    isActive: boolean;
}

export interface CreateModuleRequest {
    variableName: string;
    label: string;
    description?: string;
    versionNumber: number;
    parentModuleId?: string;
    instructions?: string;
}

export interface UpdateModuleRequest {
    variableName: string;
    label: string;
    description?: string;
    versionNumber: number;
    parentModuleId?: string;
    instructions?: string;
}

export interface ConfigurationQuestion {
    id: string;
    question: string;
    aiPrompt?: string;
    ruleType: 'SingleCoded' | 'MultiCoded';
    isActive: boolean;
    version: number;
}

export interface ConfigurationQuestionDetail extends ConfigurationQuestion {
    answersCount?: number;
    answers?: ConfigurationAnswer[];
    dependencyRules?: DependencyRule[];
}

export interface CreateConfigurationQuestionRequest {
    question: string;
    aiPrompt?: string;
    ruleType: 'SingleCoded' | 'MultiCoded';
}

export interface UpdateConfigurationQuestionRequest {
    question: string;
    aiPrompt?: string;
    ruleType: 'SingleCoded' | 'MultiCoded';
    isActive: boolean;
}

export interface ConfigurationAnswer {
    id: string;
    name: string;
    configurationQuestionId: string;
    isActive: boolean;
    createdOn?: string;
    createdBy?: string;
}

export interface CreateConfigurationAnswerRequest {
    name: string;
    configurationQuestionId: string;
}

export interface UpdateConfigurationAnswerRequest {
    name: string;
    isActive: boolean;
}

export interface DependencyRule {
    id: string;
    name: string;
    configurationQuestionId: string;
    triggeringAnswerId?: string;
    triggeringAnswerName?: string;
    classification?: string;
    type?: string;
    contentType?: string;
    module?: string;
    questionBank?: string;
    tag?: string;
    statusReason?: string;
    isActive: boolean;
    createdOn?: string;
    createdBy?: string;
}

export interface CreateDependencyRuleRequest {
    name: string;
    configurationQuestionId: string;
    triggeringAnswerId?: string;
    classification?: string;
    type?: string;
    contentType?: string;
    module?: string;
    questionBank?: string;
    tag?: string;
}

export interface UpdateDependencyRuleRequest {
    name: string;
    triggeringAnswerId?: string;
    classification?: string;
    type?: string;
    contentType?: string;
    module?: string;
    questionBank?: string;
    tag?: string;
    statusReason?: string;
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

export const modulesApi = {
    getAll: async (query?: string): Promise<Module[]> => {
        const url = query ? `${API_BASE}/modules?query=${encodeURIComponent(query)}` : `${API_BASE}/modules`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch modules');
        return response.json();
    },

    getById: async (id: string): Promise<Module> => {
        const response = await fetch(`${API_BASE}/modules/${id}`);
        if (!response.ok) throw new Error('Failed to fetch module');
        return response.json();
    },

    create: async (data: CreateModuleRequest): Promise<Module> => {
        const response = await fetch(`${API_BASE}/modules`, {
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

    update: async (id: string, data: UpdateModuleRequest): Promise<Module> => {
        const response = await fetch(`${API_BASE}/modules/${id}`, {
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
        const response = await fetch(`${API_BASE}/modules/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete module');
    }
};

export const configurationQuestionsApi = {
    getAll: async (query?: string): Promise<ConfigurationQuestionDetail[]> => {
        const url = query ? `${API_BASE}/configuration-questions?query=${encodeURIComponent(query)}` : `${API_BASE}/configuration-questions`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch configuration questions');
        return response.json();
    },

    getById: async (id: string): Promise<ConfigurationQuestionDetail> => {
        const response = await fetch(`${API_BASE}/configuration-questions/${id}`);
        if (!response.ok) throw new Error('Failed to fetch configuration question');
        return response.json();
    },

    create: async (data: CreateConfigurationQuestionRequest): Promise<ConfigurationQuestion> => {
        const response = await fetch(`${API_BASE}/configuration-questions`, {
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

    update: async (id: string, data: UpdateConfigurationQuestionRequest): Promise<ConfigurationQuestion> => {
        const response = await fetch(`${API_BASE}/configuration-questions/${id}`, {
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
        const response = await fetch(`${API_BASE}/configuration-questions/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete configuration question');
    }
};

export const configurationAnswersApi = {
    getAll: async (questionId?: string): Promise<ConfigurationAnswer[]> => {
        const url = questionId ? `${API_BASE}/configuration-answers?questionId=${questionId}` : `${API_BASE}/configuration-answers`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch configuration answers');
        return response.json();
    },

    getById: async (id: string): Promise<ConfigurationAnswer> => {
        const response = await fetch(`${API_BASE}/configuration-answers/${id}`);
        if (!response.ok) throw new Error('Failed to fetch configuration answer');
        return response.json();
    },

    create: async (data: CreateConfigurationAnswerRequest): Promise<ConfigurationAnswer> => {
        const response = await fetch(`${API_BASE}/configuration-answers`, {
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

    update: async (id: string, data: UpdateConfigurationAnswerRequest): Promise<ConfigurationAnswer> => {
        const response = await fetch(`${API_BASE}/configuration-answers/${id}`, {
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
        const response = await fetch(`${API_BASE}/configuration-answers/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete configuration answer');
    }
};

export const dependencyRulesApi = {
    getAll: async (questionId?: string): Promise<DependencyRule[]> => {
        const url = questionId ? `${API_BASE}/dependency-rules?questionId=${questionId}` : `${API_BASE}/dependency-rules`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch dependency rules');
        return response.json();
    },

    getById: async (id: string): Promise<DependencyRule> => {
        const response = await fetch(`${API_BASE}/dependency-rules/${id}`);
        if (!response.ok) throw new Error('Failed to fetch dependency rule');
        return response.json();
    },

    create: async (data: CreateDependencyRuleRequest): Promise<DependencyRule> => {
        const response = await fetch(`${API_BASE}/dependency-rules`, {
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

    update: async (id: string, data: UpdateDependencyRuleRequest): Promise<DependencyRule> => {
        const response = await fetch(`${API_BASE}/dependency-rules/${id}`, {
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
        const response = await fetch(`${API_BASE}/dependency-rules/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete dependency rule');
    }
};
