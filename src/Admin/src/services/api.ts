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
    createdOn: string;
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
    isActive: boolean;
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

// Product interfaces
export interface Product {
    id: string;
    name: string;
    description?: string;
    isActive: boolean;
}

export interface ProductDetail extends Product {
    productTemplates: ProductTemplateInfo[];
    configurationQuestions: ProductConfigQuestionInfo[];
}

export interface ProductTemplateInfo {
    id: string;
    name: string;
    version: number;
}

export interface ProductConfigQuestionInfo {
    id: string;
    configurationQuestionId: string;
    question: string;
    statusReason?: string;
}

export interface CreateProductRequest {
    name: string;
    description?: string;
}

export interface UpdateProductRequest {
    name: string;
    description?: string;
    isActive: boolean;
}

// ProductTemplate interfaces
export interface ProductTemplate {
    id: string;
    name: string;
    version: number;
    productId: string;
    productName: string;
    isActive: boolean;
}

export interface CreateProductTemplateRequest {
    name: string;
    version: number;
    productId: string;
}

export interface UpdateProductTemplateRequest {
    name: string;
    version: number;
    productId: string;
    isActive: boolean;
}

// ProductConfigQuestion interfaces
export interface ProductConfigQuestion {
    id: string;
    productId: string;
    configurationQuestionId: string;
    question: string;
    statusReason?: string;
    isActive: boolean;
}

export interface CreateProductConfigQuestionRequest {
    productId: string;
    configurationQuestionId: string;
    statusReason?: string;
}

export interface UpdateProductConfigQuestionRequest {
    statusReason?: string;
    isActive: boolean;
}

// ProductTemplateLine interfaces
export interface ProductTemplateLine {
    id: string;
    productTemplateId: string;
    name: string;
    type: 'Module' | 'Question';
    includeByDefault: boolean;
    sortOrder: number;
    moduleId?: string;
    questionBankItemId?: string;
    isActive: boolean;
    createdOn: string;
    moduleName?: string;
    questionBankItemName?: string;
}

export interface CreateProductTemplateLineRequest {
    productTemplateId: string;
    name: string;
    type: 'Module' | 'Question';
    includeByDefault: boolean;
    sortOrder: number;
    moduleId?: string;
    questionBankItemId?: string;
}

export interface UpdateProductTemplateLineRequest {
    name: string;
    type: 'Module' | 'Question';
    includeByDefault: boolean;
    sortOrder: number;
    moduleId?: string;
    questionBankItemId?: string;
    isActive: boolean;
}

// ProductConfigQuestionDisplayRule interfaces
export interface ProductConfigQuestionDisplayRule {
    id: string;
    productConfigQuestionId: string;
    triggeringConfigurationQuestionId: string;
    triggeringAnswerId?: string;
    displayCondition: 'Show' | 'Hide';
    isActive: boolean;
    createdOn: string;
    triggeringQuestionText?: string;
    triggeringAnswerName?: string;
}

export interface CreateProductConfigQuestionDisplayRuleRequest {
    productConfigQuestionId: string;
    triggeringConfigurationQuestionId: string;
    triggeringAnswerId?: string;
    displayCondition: 'Show' | 'Hide';
}

export interface UpdateProductConfigQuestionDisplayRuleRequest {
    triggeringConfigurationQuestionId: string;
    triggeringAnswerId?: string;
    displayCondition: 'Show' | 'Hide';
    isActive: boolean;
}

// ModuleQuestion interfaces
export interface ModuleQuestion {
    id: string;
    moduleId: string;
    questionBankItemId: string;
    sortOrder: number;
    isActive: boolean;
    createdOn: string;
    questionVariableName?: string;
    questionText?: string;
}

export interface CreateModuleQuestionRequest {
    moduleId: string;
    questionBankItemId: string;
    sortOrder: number;
}

export interface UpdateModuleQuestionRequest {
    sortOrder: number;
    isActive: boolean;
}

// Product API client
export const productsApi = {
    getAll: async (query?: string): Promise<Product[]> => {
        const url = query ? `${API_BASE}/products?query=${encodeURIComponent(query)}` : `${API_BASE}/products`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch products');
        return response.json();
    },

    getById: async (id: string): Promise<ProductDetail> => {
        const response = await fetch(`${API_BASE}/products/${id}`);
        if (!response.ok) throw new Error('Failed to fetch product');
        return response.json();
    },

    create: async (data: CreateProductRequest): Promise<Product> => {
        const response = await fetch(`${API_BASE}/products`, {
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

    update: async (id: string, data: UpdateProductRequest): Promise<Product> => {
        const response = await fetch(`${API_BASE}/products/${id}`, {
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
        const response = await fetch(`${API_BASE}/products/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete product');
    }
};

// ProductTemplate API client
export const productTemplatesApi = {
    getAll: async (query?: string, productId?: string): Promise<ProductTemplate[]> => {
        let url = `${API_BASE}/product-templates`;
        const params = new URLSearchParams();
        if (query) params.append('query', query);
        if (productId) params.append('productId', productId);
        if (params.toString()) url += `?${params.toString()}`;
        
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch product templates');
        return response.json();
    },

    getById: async (id: string): Promise<ProductTemplate> => {
        const response = await fetch(`${API_BASE}/product-templates/${id}`);
        if (!response.ok) throw new Error('Failed to fetch product template');
        return response.json();
    },

    create: async (data: CreateProductTemplateRequest): Promise<ProductTemplate> => {
        const response = await fetch(`${API_BASE}/product-templates`, {
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

    update: async (id: string, data: UpdateProductTemplateRequest): Promise<ProductTemplate> => {
        const response = await fetch(`${API_BASE}/product-templates/${id}`, {
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
        const response = await fetch(`${API_BASE}/product-templates/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete product template');
    }
};

// ProductConfigQuestion API client
export const productConfigQuestionsApi = {
    getById: async (id: string): Promise<ProductConfigQuestionInfo> => {
        const response = await fetch(`${API_BASE}/product-config-questions/${id}`);
        if (!response.ok) throw new Error('Failed to fetch product config question');
        return response.json();
    },

    create: async (data: CreateProductConfigQuestionRequest): Promise<ProductConfigQuestionInfo> => {
        const response = await fetch(`${API_BASE}/product-config-questions`, {
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

    update: async (id: string, data: UpdateProductConfigQuestionRequest): Promise<ProductConfigQuestion> => {
        const response = await fetch(`${API_BASE}/product-config-questions/${id}`, {
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
        const response = await fetch(`${API_BASE}/product-config-questions/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete product config question');
    }
};

// Question Bank Types
export interface QuestionBankItem {
    id: string;
    variableName: string;
    version: number;
    questionType: string | null;
    questionText: string | null;
    classification: string | null;
    isDummy: boolean;
    questionTitle: string | null;
    status: string | null;
    methodology: string | null;
    createdOn: string;
    createdBy: string | null;
}

export interface QuestionBankItemDetail extends QuestionBankItem {
    dataQualityTag: string | null;
    rowSortOrder: number | null;
    columnSortOrder: number | null;
    answerMin: number | null;
    answerMax: number | null;
    questionFormatDetails: string | null;
    scraperNotes: string | null;
    customNotes: string | null;
    metricGroup: string | null;
    tableTitle: string | null;
    questionRationale: string | null;
    singleOrMulticode: string | null;
    managedListReferences: string | null;
    isTranslatable: boolean;
    isHidden: boolean;
    isQuestionActive: boolean;
    isQuestionOutOfUse: boolean;
    answerRestrictionMin: number | null;
    answerRestrictionMax: number | null;
    restrictionDataType: string | null;
    restrictedToClient: string | null;
    answerTypeCode: string | null;
    isAnswerRequired: boolean;
    scalePoint: string | null;
    scaleType: string | null;
    displayType: string | null;
    instructionText: string | null;
    parentQuestionId: string | null;
    questionFacet: string | null;
    modifiedOn: string | null;
    modifiedBy: string | null;
    answers: QuestionAnswer[];
}

export interface QuestionAnswer {
    id: string;
    answerText: string;
    answerCode: string | null;
    answerLocation: string | null;
    isOpen: boolean;
    isFixed: boolean;
    isExclusive: boolean;
    isActive: boolean;
    customProperty: string | null;
    facets: string | null;
    version: number;
    displayOrder: number | null;
    createdOn: string;
    createdBy: string | null;
}

export interface CreateQuestionBankItemRequest {
    variableName: string;
    version: number;
    questionType?: string | null;
    questionText?: string | null;
    classification?: string | null;
    isDummy: boolean;
    questionTitle?: string | null;
    status?: string | null;
    methodology?: string | null;
    dataQualityTag?: string | null;
    rowSortOrder?: number | null;
    columnSortOrder?: number | null;
    answerMin?: number | null;
    answerMax?: number | null;
    questionFormatDetails?: string | null;
    scraperNotes?: string | null;
    customNotes?: string | null;
    metricGroup?: string | null;
    tableTitle?: string | null;
    questionRationale?: string | null;
    singleOrMulticode?: string | null;
    managedListReferences?: string | null;
    isTranslatable: boolean;
    isHidden: boolean;
    isQuestionActive: boolean;
    isQuestionOutOfUse: boolean;
    answerRestrictionMin?: number | null;
    answerRestrictionMax?: number | null;
    restrictionDataType?: string | null;
    restrictedToClient?: string | null;
    answerTypeCode?: string | null;
    isAnswerRequired: boolean;
    scalePoint?: string | null;
    scaleType?: string | null;
    displayType?: string | null;
    instructionText?: string | null;
    parentQuestionId?: string | null;
    questionFacet?: string | null;
}

export type UpdateQuestionBankItemRequest = CreateQuestionBankItemRequest;

export interface CreateQuestionAnswerRequest {
    answerText: string;
    answerCode?: string | null;
    answerLocation?: string | null;
    isOpen: boolean;
    isFixed: boolean;
    isExclusive: boolean;
    isActive: boolean;
    customProperty?: string | null;
    facets?: string | null;
    version: number;
    displayOrder?: number | null;
}

export type UpdateQuestionAnswerRequest = CreateQuestionAnswerRequest;

// Question Bank API
export const questionBankApi = {
    getAll: async (query?: string): Promise<QuestionBankItem[]> => {
        const url = query ? `${API_BASE}/question-bank?query=${encodeURIComponent(query)}` : `${API_BASE}/question-bank`;
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch question bank items');
        return response.json();
    },

    getById: async (id: string): Promise<QuestionBankItemDetail> => {
        const response = await fetch(`${API_BASE}/question-bank/${id}`);
        if (!response.ok) throw new Error('Failed to fetch question bank item');
        return response.json();
    },

    create: async (data: CreateQuestionBankItemRequest): Promise<{ id: string; variableName: string; version: number }> => {
        const response = await fetch(`${API_BASE}/question-bank`, {
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

    update: async (id: string, data: UpdateQuestionBankItemRequest): Promise<{ id: string; variableName: string; version: number }> => {
        const response = await fetch(`${API_BASE}/question-bank/${id}`, {
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
        const response = await fetch(`${API_BASE}/question-bank/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete question bank item');
    }
};

// Question Answer API
export const questionAnswerApi = {
    create: async (questionId: string, data: CreateQuestionAnswerRequest): Promise<{ id: string; answerText: string }> => {
        const response = await fetch(`${API_BASE}/question-bank/${questionId}/answers`, {
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

    update: async (questionId: string, answerId: string, data: UpdateQuestionAnswerRequest): Promise<{ id: string; answerText: string }> => {
        const response = await fetch(`${API_BASE}/question-bank/${questionId}/answers/${answerId}`, {
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

    delete: async (questionId: string, answerId: string): Promise<void> => {
        const response = await fetch(`${API_BASE}/question-bank/${questionId}/answers/${answerId}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete answer');
    }
};

// ProductTemplateLine API client
export const productTemplateLinesApi = {
    getAll: async (productTemplateId?: string): Promise<ProductTemplateLine[]> => {
        let url = `${API_BASE}/product-template-lines`;
        if (productTemplateId) url += `?productTemplateId=${productTemplateId}`;
        
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch product template lines');
        return response.json();
    },

    getById: async (id: string): Promise<ProductTemplateLine> => {
        const response = await fetch(`${API_BASE}/product-template-lines/${id}`);
        if (!response.ok) throw new Error('Failed to fetch product template line');
        return response.json();
    },

    create: async (data: CreateProductTemplateLineRequest): Promise<ProductTemplateLine> => {
        const response = await fetch(`${API_BASE}/product-template-lines`, {
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

    update: async (id: string, data: UpdateProductTemplateLineRequest): Promise<ProductTemplateLine> => {
        const response = await fetch(`${API_BASE}/product-template-lines/${id}`, {
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
        const response = await fetch(`${API_BASE}/product-template-lines/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete product template line');
    }
};

// ProductConfigQuestionDisplayRule API client
export const productConfigQuestionDisplayRulesApi = {
    getAll: async (productConfigQuestionId?: string): Promise<ProductConfigQuestionDisplayRule[]> => {
        let url = `${API_BASE}/product-config-question-display-rules`;
        if (productConfigQuestionId) url += `?productConfigQuestionId=${productConfigQuestionId}`;
        
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch product config question display rules');
        return response.json();
    },

    getById: async (id: string): Promise<ProductConfigQuestionDisplayRule> => {
        const response = await fetch(`${API_BASE}/product-config-question-display-rules/${id}`);
        if (!response.ok) throw new Error('Failed to fetch product config question display rule');
        return response.json();
    },

    create: async (data: CreateProductConfigQuestionDisplayRuleRequest): Promise<ProductConfigQuestionDisplayRule> => {
        const response = await fetch(`${API_BASE}/product-config-question-display-rules`, {
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

    update: async (id: string, data: UpdateProductConfigQuestionDisplayRuleRequest): Promise<ProductConfigQuestionDisplayRule> => {
        const response = await fetch(`${API_BASE}/product-config-question-display-rules/${id}`, {
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
        const response = await fetch(`${API_BASE}/product-config-question-display-rules/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete product config question display rule');
    }
};

// ModuleQuestion API client
export const moduleQuestionsApi = {
    getAll: async (moduleId?: string): Promise<ModuleQuestion[]> => {
        let url = `${API_BASE}/module-questions`;
        if (moduleId) url += `?moduleId=${moduleId}`;
        
        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch module questions');
        return response.json();
    },

    getById: async (id: string): Promise<ModuleQuestion> => {
        const response = await fetch(`${API_BASE}/module-questions/${id}`);
        if (!response.ok) throw new Error('Failed to fetch module question');
        return response.json();
    },

    create: async (data: CreateModuleQuestionRequest): Promise<ModuleQuestion> => {
        const response = await fetch(`${API_BASE}/module-questions`, {
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

    update: async (id: string, data: UpdateModuleQuestionRequest): Promise<ModuleQuestion> => {
        const response = await fetch(`${API_BASE}/module-questions/${id}`, {
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
        const response = await fetch(`${API_BASE}/module-questions/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete module question');
    }
};
