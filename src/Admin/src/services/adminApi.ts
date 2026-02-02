import apiClient from './api';
import type { Question, QuestionAnswer, Module, Client, Tag, CommissioningMarket, FieldworkMarket, Product, ConfigurationQuestion } from '../types/entities';

// Questions API
export const questionsApi = {
  getAll: (search?: string, type?: string, status?: string) => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (type) params.append('type', type);
    if (status) params.append('status', status);
    return apiClient.get<Question[]>(`/questions?${params.toString()}`);
  },
  getById: (id: number) => apiClient.get<Question>(`/questions/${id}`),
  create: (question: Partial<Question>) => apiClient.post<Question>('/questions', question),
  update: (id: number, question: Partial<Question>) => apiClient.put<Question>(`/questions/${id}`, question),
  delete: (id: number) => apiClient.delete(`/questions/${id}`),
  getAnswers: (id: number) => apiClient.get<QuestionAnswer[]>(`/questions/${id}/answers`),
  addAnswer: (id: number, answer: Partial<QuestionAnswer>) => apiClient.post<QuestionAnswer>(`/questions/${id}/answers`, answer),
};

// Modules API
export const modulesApi = {
  getAll: (search?: string, status?: string) => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status) params.append('status', status);
    return apiClient.get<Module[]>(`/modules?${params.toString()}`);
  },
  getById: (id: number) => apiClient.get<Module>(`/modules/${id}`),
  create: (module: Partial<Module>) => apiClient.post<Module>('/modules', module),
  update: (id: number, module: Partial<Module>) => apiClient.put<Module>(`/modules/${id}`, module),
  delete: (id: number) => apiClient.delete(`/modules/${id}`),
  addQuestion: (moduleId: number, questionId: number, displayOrder: number) => 
    apiClient.post(`/modules/${moduleId}/questions/${questionId}`, null, { params: { displayOrder } }),
  removeQuestion: (moduleId: number, questionId: number) => 
    apiClient.delete(`/modules/${moduleId}/questions/${questionId}`),
};

// Clients API
export const clientsApi = {
  getAll: () => apiClient.get<Client[]>('/clients'),
  getById: (id: number) => apiClient.get<Client>(`/clients/${id}`),
  create: (client: Partial<Client>) => apiClient.post<Client>('/clients', client),
  update: (id: number, client: Partial<Client>) => apiClient.put<Client>(`/clients/${id}`, client),
  delete: (id: number) => apiClient.delete(`/clients/${id}`),
};

// Tags API
export const tagsApi = {
  getAll: () => apiClient.get<Tag[]>('/tags'),
  getById: (id: number) => apiClient.get<Tag>(`/tags/${id}`),
  create: (tag: Partial<Tag>) => apiClient.post<Tag>('/tags', tag),
  update: (id: number, tag: Partial<Tag>) => apiClient.put<Tag>(`/tags/${id}`, tag),
  delete: (id: number) => apiClient.delete(`/tags/${id}`),
};

// Markets API
export const marketsApi = {
  getCommissioningMarkets: () => apiClient.get<CommissioningMarket[]>('/commissioning-markets'),
  getFieldworkMarkets: () => apiClient.get<FieldworkMarket[]>('/fieldwork-markets'),
  createCommissioningMarket: (market: Partial<CommissioningMarket>) => 
    apiClient.post<CommissioningMarket>('/commissioning-markets', market),
  createFieldworkMarket: (market: Partial<FieldworkMarket>) => 
    apiClient.post<FieldworkMarket>('/fieldwork-markets', market),
  updateCommissioningMarket: (id: number, market: Partial<CommissioningMarket>) => 
    apiClient.put<CommissioningMarket>(`/commissioning-markets/${id}`, market),
  updateFieldworkMarket: (id: number, market: Partial<FieldworkMarket>) => 
    apiClient.put<FieldworkMarket>(`/fieldwork-markets/${id}`, market),
  deleteCommissioningMarket: (id: number) => apiClient.delete(`/commissioning-markets/${id}`),
  deleteFieldworkMarket: (id: number) => apiClient.delete(`/fieldwork-markets/${id}`),
};

// Products API
export const productsApi = {
  getAll: (search?: string, status?: string) => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status) params.append('status', status);
    return apiClient.get<Product[]>(`/products?${params.toString()}`);
  },
  getById: (id: number) => apiClient.get<Product>(`/products/${id}`),
  create: (product: Partial<Product>) => apiClient.post<Product>('/products', product),
  update: (id: number, product: Partial<Product>) => apiClient.put<Product>(`/products/${id}`, product),
  delete: (id: number) => apiClient.delete(`/products/${id}`),
};

// Configuration Questions API
export const configurationQuestionsApi = {
  getAll: (search?: string, status?: string) => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status) params.append('status', status);
    return apiClient.get<ConfigurationQuestion[]>(`/configuration-questions?${params.toString()}`);
  },
  getById: (id: number) => apiClient.get<ConfigurationQuestion>(`/configuration-questions/${id}`),
  create: (question: Partial<ConfigurationQuestion>) => 
    apiClient.post<ConfigurationQuestion>('/configuration-questions', question),
  update: (id: number, question: Partial<ConfigurationQuestion>) => 
    apiClient.put<ConfigurationQuestion>(`/configuration-questions/${id}`, question),
  delete: (id: number) => apiClient.delete(`/configuration-questions/${id}`),
};
