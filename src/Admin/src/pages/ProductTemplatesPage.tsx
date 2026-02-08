import { useState, useEffect } from 'react';
import { productTemplatesApi, productsApi, productTemplateLinesApi, modulesApi, questionBankApi } from '../services/api';
import type { ProductTemplate, Product, ProductTemplateLine, Module, QuestionBankItem } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ProductTemplatesPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';
type TabMode = 'general' | 'template-lines';
type TemplateLineMode = 'none' | 'create' | 'edit';

export function ProductTemplatesPage() {
    const [templates, setTemplates] = useState<ProductTemplate[]>([]);
    const [products, setProducts] = useState<Product[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [tabMode, setTabMode] = useState<TabMode>('general');
    const [selectedTemplate, setSelectedTemplate] = useState<ProductTemplate | null>(null);
    const [formData, setFormData] = useState({ name: '', version: 1, productId: '', isActive: true });

    // Template Line State
    const [templateLineMode, setTemplateLineMode] = useState<TemplateLineMode>('none');
    const [templateLines, setTemplateLines] = useState<ProductTemplateLine[]>([]);
    const [selectedTemplateLine, setSelectedTemplateLine] = useState<ProductTemplateLine | null>(null);
    const [templateLineFormData, setTemplateLineFormData] = useState({
        name: '',
        type: 'Module' as 'Module' | 'Question',
        includeByDefault: true,
        sortOrder: 0,
        moduleId: '',
        questionBankItemId: ''
    });
    const [moduleSearch, setModuleSearch] = useState('');
    const [questionBankSearch, setQuestionBankSearch] = useState('');
    const [availableModules, setAvailableModules] = useState<Module[]>([]);
    const [availableQuestions, setAvailableQuestions] = useState<QuestionBankItem[]>([]);

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchTemplates();
        fetchProducts();
    }, [search]);

    const fetchTemplates = async () => {
        setIsLoading(true);
        try {
            const data = await productTemplatesApi.getAll(search);
            setTemplates(data);
        } catch (error) {
            console.error('Failed to fetch product templates', error);
        } finally {
            setIsLoading(false);
        }
    };

    const fetchProducts = async () => {
        try {
            const data = await productsApi.getAll();
            setProducts(data);
        } catch (error) {
            console.error('Failed to fetch products', error);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedTemplate(null);
        setFormData({ name: '', version: 1, productId: '', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
        setTabMode('general');
    };

    const openView = async (template: ProductTemplate) => {
        setSelectedTemplate(template);
        setMode('view');
        setTabMode('general');
        // Fetch template lines for this template
        await fetchTemplateLines(template.id);
    };

    const openEdit = (template?: ProductTemplate) => {
        const target = template || selectedTemplate;
        if (!target) return;

        if (template) setSelectedTemplate(template);

        setFormData({ 
            name: target.name, 
            version: target.version,
            productId: target.productId,
            isActive: target.isActive 
        });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedTemplate(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedTemplate: ProductTemplate;
            if (mode === 'edit' && selectedTemplate) {
                savedTemplate = await productTemplatesApi.update(selectedTemplate.id, { 
                    name: formData.name,
                    version: formData.version,
                    productId: formData.productId,
                    isActive: formData.isActive 
                });
            } else {
                savedTemplate = await productTemplatesApi.create({ 
                    name: formData.name,
                    version: formData.version,
                    productId: formData.productId
                });
            }

            await fetchTemplates();
            setSelectedTemplate(savedTemplate);
            setMode('view');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]>; detail?: string };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else if (error.status === 409) {
                setServerError(error.detail || "Product template already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (template?: ProductTemplate) => {
        const target = template || selectedTemplate;
        if (!target || !confirm(`Are you sure you want to delete product template '${target.name}'?`)) return;

        try {
            await productTemplatesApi.delete(target.id);
            closePanel();
            fetchTemplates();
        } catch (error) {
            console.error('Failed to delete product template', error);
        }
    };

    // --- Template Line Actions ---

    const fetchTemplateLines = async (templateId: string) => {
        try {
            const lines = await productTemplateLinesApi.getAll(templateId);
            setTemplateLines(lines);
        } catch (error) {
            console.error('Failed to fetch template lines', error);
        }
    };

    const searchModules = async (searchTerm: string) => {
        if (searchTerm.length < 3) {
            setAvailableModules([]);
            return;
        }
        
        try {
            const modules = await modulesApi.getAll(searchTerm);
            setAvailableModules(modules);
        } catch (error) {
            console.error('Failed to search modules', error);
        }
    };

    const searchQuestionBank = async (searchTerm: string) => {
        if (searchTerm.length < 3) {
            setAvailableQuestions([]);
            return;
        }
        
        try {
            const questions = await questionBankApi.getAll(searchTerm);
            setAvailableQuestions(questions);
        } catch (error) {
            console.error('Failed to search question bank', error);
        }
    };

    const openTemplateLineCreate = () => {
        setTemplateLineFormData({
            name: '',
            type: 'Module',
            includeByDefault: true,
            sortOrder: templateLines.length,
            moduleId: '',
            questionBankItemId: ''
        });
        setModuleSearch('');
        setQuestionBankSearch('');
        setAvailableModules([]);
        setAvailableQuestions([]);
        setTemplateLineMode('create');
        setSelectedTemplateLine(null);
    };

    const openTemplateLineEdit = (line: ProductTemplateLine) => {
        setSelectedTemplateLine(line);
        setTemplateLineFormData({
            name: line.name,
            type: line.type,
            includeByDefault: line.includeByDefault,
            sortOrder: line.sortOrder,
            moduleId: line.moduleId || '',
            questionBankItemId: line.questionBankItemId || ''
        });
        setModuleSearch(line.moduleName || '');
        setQuestionBankSearch(line.questionBankItemName || '');
        setAvailableModules([]);
        setAvailableQuestions([]);
        setTemplateLineMode('edit');
    };

    const handleTemplateLineSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedTemplate) return;

        setErrors({});
        setServerError('');

        try {
            if (templateLineMode === 'edit' && selectedTemplateLine) {
                await productTemplateLinesApi.update(selectedTemplateLine.id, {
                    name: templateLineFormData.name,
                    type: templateLineFormData.type,
                    includeByDefault: templateLineFormData.includeByDefault,
                    sortOrder: templateLineFormData.sortOrder,
                    moduleId: templateLineFormData.type === 'Module' ? templateLineFormData.moduleId : undefined,
                    questionBankItemId: templateLineFormData.type === 'Question' ? templateLineFormData.questionBankItemId : undefined,
                    isActive: true
                });
            } else {
                await productTemplateLinesApi.create({
                    productTemplateId: selectedTemplate.id,
                    name: templateLineFormData.name,
                    type: templateLineFormData.type,
                    includeByDefault: templateLineFormData.includeByDefault,
                    sortOrder: templateLineFormData.sortOrder,
                    moduleId: templateLineFormData.type === 'Module' ? templateLineFormData.moduleId : undefined,
                    questionBankItemId: templateLineFormData.type === 'Question' ? templateLineFormData.questionBankItemId : undefined
                });
            }

            await fetchTemplateLines(selectedTemplate.id);
            setTemplateLineMode('none');
            setModuleSearch('');
            setQuestionBankSearch('');
            setAvailableModules([]);
            setAvailableQuestions([]);

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleTemplateLineDelete = async (line: ProductTemplateLine) => {
        if (!confirm(`Are you sure you want to delete template line '${line.name}'?`)) return;

        try {
            await productTemplateLinesApi.delete(line.id);
            if (selectedTemplate) {
                await fetchTemplateLines(selectedTemplate.id);
            }
        } catch (error) {
            console.error('Failed to delete template line', error);
        }
    };

    // --- Renders ---

    return (
        <div className="page-container">
            {/* Command Bar */}
            <div className="command-bar card">
                <button className="cmd-btn primary" onClick={openCreate}>
                    <PlusIcon /> <span className="label">New</span>
                </button>
                <button className="cmd-btn" onClick={fetchTemplates}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search templates..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Content Area */}
            <div className="content-area card">
                {isLoading ? (
                    <div className="loading-state">Loading...</div>
                ) : (
                    <table className="details-list">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Product</th>
                                <th style={{ width: '100px' }}>Version</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {templates.map((template) => (
                                <tr key={template.id} onClick={() => openView(template)} className="clickable-row">
                                    <td>{template.name}</td>
                                    <td>{template.productName}</td>
                                    <td>{template.version}</td>
                                    <td>
                                        <span className={`status-text ${template.isActive ? 'active' : 'inactive'}`}>
                                            {template.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(template); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(template); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(template); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {templates.length === 0 && (
                                <tr><td colSpan={5} className="empty-state">No product templates found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Product Template' : mode === 'edit' ? 'Edit Product Template' : selectedTemplate?.name || 'Template Details'}
                footer={
                    (mode === 'create' || mode === 'edit') && tabMode === 'general' ? (
                        <>
                            <button className="btn primary" onClick={(e) => handleSubmit(e as React.FormEvent)}>Save</button>
                            <button className="btn" onClick={mode === 'edit' ? () => setMode('view') : closePanel}>Cancel</button>
                        </>
                    ) : (
                        mode === 'view' && tabMode === 'general' && (
                            <>
                                <button className="btn primary" onClick={() => openEdit()}>Edit</button>
                                <button className="btn danger" onClick={() => handleDelete()}>Delete</button>
                            </>
                        )
                    )
                }
            >
                {/* Tabs */}
                {mode === 'view' && selectedTemplate && (
                    <>
                        <div className="panel-tabs">
                            <button
                                className={`tab ${tabMode === 'general' ? 'active' : ''}`}
                                onClick={() => setTabMode('general')}
                            >
                                General
                            </button>
                            <button
                                className={`tab ${tabMode === 'template-lines' ? 'active' : ''}`}
                                onClick={() => setTabMode('template-lines')}
                            >
                                Template Lines
                            </button>
                        </div>

                        {/* General Tab */}
                        {tabMode === 'general' && (
                            <div className="view-details">
                                <div className="detail-item">
                                    <label>Name</label>
                                    <div className="value">{selectedTemplate.name}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Product</label>
                                    <div className="value">{selectedTemplate.productName}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Version</label>
                                    <div className="value">{selectedTemplate.version}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Status</label>
                                    <div className="value">
                                        {selectedTemplate.isActive ? 'Active' : 'Inactive'}
                                    </div>
                                </div>
                                <div className="detail-item">
                                    <label>ID</label>
                                    <div className="value monospace">{selectedTemplate.id}</div>
                                </div>
                            </div>
                        )}

                        {/* Template Lines Tab */}
                        {tabMode === 'template-lines' && (
                            <div className="tab-content">
                                <div className="tab-header">
                                    <button className="btn primary" onClick={openTemplateLineCreate}>
                                        <PlusIcon /> Add Template Line
                                    </button>
                                </div>

                                {(templateLineMode === 'create' || templateLineMode === 'edit') && (
                                    <form className="panel-form" onSubmit={handleTemplateLineSubmit} style={{ marginBottom: '1rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '4px' }}>
                                        <h4>{templateLineMode === 'create' ? 'Add' : 'Edit'} Template Line</h4>
                                        
                                        <div className="form-field">
                                            <label htmlFor="templateLineName">Name <span className="required">*</span></label>
                                            <input
                                                id="templateLineName"
                                                type="text"
                                                value={templateLineFormData.name}
                                                onChange={(e) => setTemplateLineFormData({ ...templateLineFormData, name: e.target.value })}
                                                className={errors.Name ? 'error' : ''}
                                            />
                                            {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                                        </div>

                                        <div className="form-field">
                                            <label htmlFor="type">Type <span className="required">*</span></label>
                                            <select
                                                id="type"
                                                value={templateLineFormData.type}
                                                onChange={(e) => setTemplateLineFormData({ 
                                                    ...templateLineFormData, 
                                                    type: e.target.value as 'Module' | 'Question',
                                                    moduleId: '',
                                                    questionBankItemId: ''
                                                })}
                                                className={errors.Type ? 'error' : ''}
                                            >
                                                <option value="Module">Module</option>
                                                <option value="Question">Question</option>
                                            </select>
                                            {errors.Type && <span className="field-error">{errors.Type[0]}</span>}
                                        </div>

                                        {templateLineFormData.type === 'Module' && (
                                            <div className="form-field">
                                                <label htmlFor="moduleSearch">Module <span className="required">*</span></label>
                                                <input
                                                    id="moduleSearch"
                                                    type="text"
                                                    placeholder="Type at least 3 characters to search..."
                                                    value={moduleSearch}
                                                    onChange={(e) => {
                                                        setModuleSearch(e.target.value);
                                                        searchModules(e.target.value);
                                                    }}
                                                    className={errors.ModuleId ? 'error' : ''}
                                                />
                                                {errors.ModuleId && <span className="field-error">{errors.ModuleId[0]}</span>}
                                                {moduleSearch.length > 0 && moduleSearch.length < 3 && (
                                                    <small style={{ color: '#666' }}>Type at least 3 characters to search</small>
                                                )}
                                                {availableModules.length > 0 && (
                                                    <div style={{ marginTop: '0.5rem', border: '1px solid #ddd', borderRadius: '4px', maxHeight: '200px', overflowY: 'auto' }}>
                                                        {availableModules.map(m => (
                                                            <div
                                                                key={m.id}
                                                                onClick={() => {
                                                                    setTemplateLineFormData({ ...templateLineFormData, moduleId: m.id });
                                                                    setModuleSearch(m.name);
                                                                    setAvailableModules([]);
                                                                }}
                                                                style={{
                                                                    padding: '0.5rem',
                                                                    cursor: 'pointer',
                                                                    borderBottom: '1px solid #eee',
                                                                    backgroundColor: templateLineFormData.moduleId === m.id ? '#e3f2fd' : 'transparent'
                                                                }}
                                                                onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#f5f5f5'}
                                                                onMouseLeave={(e) => e.currentTarget.style.backgroundColor = templateLineFormData.moduleId === m.id ? '#e3f2fd' : 'transparent'}
                                                            >
                                                                {m.name}
                                                            </div>
                                                        ))}
                                                    </div>
                                                )}
                                                {moduleSearch.length >= 3 && availableModules.length === 0 && (
                                                    <small style={{ color: '#666' }}>No modules found</small>
                                                )}
                                            </div>
                                        )}

                                        {templateLineFormData.type === 'Question' && (
                                            <div className="form-field">
                                                <label htmlFor="questionBankSearch">Question Bank <span className="required">*</span></label>
                                                <input
                                                    id="questionBankSearch"
                                                    type="text"
                                                    placeholder="Type at least 3 characters to search..."
                                                    value={questionBankSearch}
                                                    onChange={(e) => {
                                                        setQuestionBankSearch(e.target.value);
                                                        searchQuestionBank(e.target.value);
                                                    }}
                                                    className={errors.QuestionBankItemId ? 'error' : ''}
                                                />
                                                {errors.QuestionBankItemId && <span className="field-error">{errors.QuestionBankItemId[0]}</span>}
                                                {questionBankSearch.length > 0 && questionBankSearch.length < 3 && (
                                                    <small style={{ color: '#666' }}>Type at least 3 characters to search</small>
                                                )}
                                                {availableQuestions.length > 0 && (
                                                    <div style={{ marginTop: '0.5rem', border: '1px solid #ddd', borderRadius: '4px', maxHeight: '200px', overflowY: 'auto' }}>
                                                        {availableQuestions.map(q => (
                                                            <div
                                                                key={q.id}
                                                                onClick={() => {
                                                                    setTemplateLineFormData({ ...templateLineFormData, questionBankItemId: q.id });
                                                                    setQuestionBankSearch(q.variableName);
                                                                    setAvailableQuestions([]);
                                                                }}
                                                                style={{
                                                                    padding: '0.5rem',
                                                                    cursor: 'pointer',
                                                                    borderBottom: '1px solid #eee',
                                                                    backgroundColor: templateLineFormData.questionBankItemId === q.id ? '#e3f2fd' : 'transparent'
                                                                }}
                                                                onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#f5f5f5'}
                                                                onMouseLeave={(e) => e.currentTarget.style.backgroundColor = templateLineFormData.questionBankItemId === q.id ? '#e3f2fd' : 'transparent'}
                                                            >
                                                                {q.variableName}
                                                            </div>
                                                        ))}
                                                    </div>
                                                )}
                                                {questionBankSearch.length >= 3 && availableQuestions.length === 0 && (
                                                    <small style={{ color: '#666' }}>No questions found</small>
                                                )}
                                            </div>
                                        )}

                                        <div className="form-field">
                                            <label htmlFor="sortOrder">Sort Order <span className="required">*</span></label>
                                            <input
                                                id="sortOrder"
                                                type="number"
                                                min="0"
                                                value={templateLineFormData.sortOrder}
                                                onChange={(e) => setTemplateLineFormData({ ...templateLineFormData, sortOrder: parseInt(e.target.value) || 0 })}
                                                className={errors.SortOrder ? 'error' : ''}
                                            />
                                            {errors.SortOrder && <span className="field-error">{errors.SortOrder[0]}</span>}
                                        </div>

                                        <div className="form-field checkbox">
                                            <label>
                                                <input
                                                    type="checkbox"
                                                    checked={templateLineFormData.includeByDefault}
                                                    onChange={(e) => setTemplateLineFormData({ ...templateLineFormData, includeByDefault: e.target.checked })}
                                                />
                                                <span>Include By Default</span>
                                            </label>
                                        </div>

                                        <div style={{ marginTop: '1rem' }}>
                                            <button type="submit" className="btn primary" disabled={
                                                (templateLineFormData.type === 'Module' && !templateLineFormData.moduleId) ||
                                                (templateLineFormData.type === 'Question' && !templateLineFormData.questionBankItemId)
                                            }>
                                                {templateLineMode === 'create' ? 'Add' : 'Update'}
                                            </button>
                                            <button type="button" className="btn" onClick={() => { 
                                                setTemplateLineMode('none'); 
                                                setModuleSearch(''); 
                                                setQuestionBankSearch(''); 
                                                setAvailableModules([]);
                                                setAvailableQuestions([]);
                                            }}>Cancel</button>
                                        </div>

                                        {serverError && <div className="server-error">{serverError}</div>}
                                    </form>
                                )}

                                <table className="details-list">
                                    <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th>Type</th>
                                            <th>Include By Default</th>
                                            <th>Sort Order</th>
                                            <th style={{ width: '100px' }}>Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {templateLines.map((line) => (
                                            <tr key={line.id}>
                                                <td>{line.name}</td>
                                                <td>{line.type}</td>
                                                <td>{line.includeByDefault ? 'Yes' : 'No'}</td>
                                                <td>{line.sortOrder}</td>
                                                <td>
                                                    <div className="row-actions">
                                                        <button 
                                                            type="button"
                                                            className="action-btn" 
                                                            onClick={() => openTemplateLineEdit(line)} 
                                                            title="Edit"
                                                        >
                                                            <EditIcon />
                                                        </button>
                                                        <button 
                                                            type="button"
                                                            className="action-btn danger" 
                                                            onClick={() => handleTemplateLineDelete(line)} 
                                                            title="Delete"
                                                        >
                                                            <TrashIcon />
                                                        </button>
                                                    </div>
                                                </td>
                                            </tr>
                                        ))}
                                        {templateLines.length === 0 && (
                                            <tr><td colSpan={5} className="empty-state">No template lines.</td></tr>
                                        )}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </>
                )}

                {/* View Mode - Old content removed, now in tabs */}
                {mode === 'view' && !selectedTemplate && (
                    <div className="loading-state">Loading...</div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="templateName">Name <span className="required">*</span></label>
                            <input
                                id="templateName"
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className={errors.Name ? 'error' : ''}
                                autoFocus
                            />
                            {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="productId">Product <span className="required">*</span></label>
                            <select
                                id="productId"
                                value={formData.productId}
                                onChange={(e) => setFormData({ ...formData, productId: e.target.value })}
                                className={errors.ProductId ? 'error' : ''}
                            >
                                <option value="">-- Select Product --</option>
                                {products.map(p => (
                                    <option key={p.id} value={p.id}>{p.name}</option>
                                ))}
                            </select>
                            {errors.ProductId && <span className="field-error">{errors.ProductId[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="version">Version <span className="required">*</span></label>
                            <input
                                id="version"
                                type="number"
                                min="1"
                                value={formData.version}
                                onChange={(e) => setFormData({ ...formData, version: parseInt(e.target.value) || 1 })}
                                className={errors.Version ? 'error' : ''}
                            />
                            {errors.Version && <span className="field-error">{errors.Version[0]}</span>}
                        </div>

                        {mode === 'edit' && (
                            <div className="form-field checkbox">
                                <label>
                                    <input
                                        type="checkbox"
                                        checked={formData.isActive}
                                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                    />
                                    <span>Is Active</span>
                                </label>
                            </div>
                        )}

                        {serverError && <div className="server-error">{serverError}</div>}
                    </form>
                )}
            </SidePanel>
        </div>
    );
}
