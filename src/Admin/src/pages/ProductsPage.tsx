import { useState, useEffect } from 'react';
import { productsApi, productConfigQuestionsApi, configurationQuestionsApi, productTemplatesApi } from '../services/api';
import type { ProductDetail, Product, ProductConfigQuestionInfo, ProductTemplateInfo, ConfigurationQuestion } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ProductsPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';
type TabMode = 'general' | 'config-questions' | 'templates';
type ConfigQuestionMode = 'none' | 'create';
type TemplateMode = 'none' | 'create';

export function ProductsPage() {
    const [products, setProducts] = useState<Product[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [tabMode, setTabMode] = useState<TabMode>('general');
    const [selectedProduct, setSelectedProduct] = useState<ProductDetail | null>(null);
    const [formData, setFormData] = useState({ name: '', description: '', isActive: true });

    // Config Question State
    const [configQuestionMode, setConfigQuestionMode] = useState<ConfigQuestionMode>('none');
    const [availableQuestions, setAvailableQuestions] = useState<ConfigurationQuestion[]>([]);
    const [configQuestionFormData, setConfigQuestionFormData] = useState({
        configurationQuestionId: '',
        statusReason: ''
    });

    // Template State
    const [templateMode, setTemplateMode] = useState<TemplateMode>('none');
    const [templateFormData, setTemplateFormData] = useState({
        name: '',
        version: 1
    });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchProducts();
    }, [search]);

    const fetchProducts = async () => {
        setIsLoading(true);
        try {
            const data = await productsApi.getAll(search);
            setProducts(data);
        } catch (error) {
            console.error('Failed to fetch products', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedProduct(null);
        setFormData({ name: '', description: '', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
        setTabMode('general');
    };

    const openView = async (product: Product) => {
        try {
            // Fetch full details before opening
            const fullProduct = await productsApi.getById(product.id);
            setSelectedProduct(fullProduct);
            setMode('view');
            setTabMode('general');
        } catch (error) {
            console.error('Failed to fetch product details', error);
        }
    };

    const openEdit = async (product?: Product) => {
        const target = product || selectedProduct;
        if (!target) return;

        // If opening from row action, fetch full product details first
        if (product) {
            try {
                const fullProduct = await productsApi.getById(product.id);
                setSelectedProduct(fullProduct);
                setFormData({
                    name: fullProduct.name,
                    description: fullProduct.description || '',
                    isActive: fullProduct.isActive
                });
            } catch (error) {
                console.error(`Failed to fetch product details for product ${product.id}`, error);
                return;
            }
        } else {
            // Already have full details from view mode
            setFormData({
                name: target.name,
                description: target.description || '',
                isActive: target.isActive
            });
        }

        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedProduct(null);
        setConfigQuestionMode('none');
        setTemplateMode('none');
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedProduct: Product;
            if (mode === 'edit' && selectedProduct) {
                savedProduct = await productsApi.update(selectedProduct.id, {
                    name: formData.name,
                    description: formData.description || undefined,
                    isActive: formData.isActive
                });
            } else {
                savedProduct = await productsApi.create({
                    name: formData.name,
                    description: formData.description || undefined
                });
            }

            await fetchProducts();
            const fullProduct = await productsApi.getById(savedProduct.id);
            setSelectedProduct(fullProduct);
            setMode('view');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]>; detail?: string };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else if (error.status === 409) {
                setServerError(error.detail || "Product already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (product?: Product) => {
        const target = product || selectedProduct;
        if (!target || !confirm(`Are you sure you want to delete product '${target.name}'?`)) return;

        try {
            await productsApi.delete(target.id);
            closePanel();
            fetchProducts();
        } catch (error) {
            console.error('Failed to delete product', error);
        }
    };

    // --- Config Question Actions ---

    const openConfigQuestionCreate = async () => {
        try {
            const questions = await configurationQuestionsApi.getAll();
            setAvailableQuestions(questions);
            setConfigQuestionFormData({
                configurationQuestionId: '',
                statusReason: ''
            });
            setConfigQuestionMode('create');
        } catch (error) {
            console.error('Failed to fetch configuration questions', error);
        }
    };

    const handleConfigQuestionSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedProduct) return;

        setErrors({});
        setServerError('');

        try {
            await productConfigQuestionsApi.create({
                productId: selectedProduct.id,
                configurationQuestionId: configQuestionFormData.configurationQuestionId,
                statusReason: configQuestionFormData.statusReason || undefined
            });

            const updatedProduct = await productsApi.getById(selectedProduct.id);
            setSelectedProduct(updatedProduct);
            setConfigQuestionMode('none');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleConfigQuestionDelete = async (pcq: ProductConfigQuestionInfo) => {
        if (!confirm(`Are you sure you want to remove this configuration question?`)) return;

        try {
            await productConfigQuestionsApi.delete(pcq.id);
            if (selectedProduct) {
                const updatedProduct = await productsApi.getById(selectedProduct.id);
                setSelectedProduct(updatedProduct);
            }
        } catch (error) {
            console.error('Failed to delete product config question', error);
        }
    };

    // --- Template Actions ---

    const openTemplateCreate = () => {
        setTemplateFormData({
            name: '',
            version: 1
        });
        setTemplateMode('create');
    };

    const handleTemplateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedProduct) return;

        setErrors({});
        setServerError('');

        try {
            await productTemplatesApi.create({
                name: templateFormData.name,
                version: templateFormData.version,
                productId: selectedProduct.id
            });

            const updatedProduct = await productsApi.getById(selectedProduct.id);
            setSelectedProduct(updatedProduct);
            setTemplateMode('none');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleTemplateDelete = async (template: ProductTemplateInfo) => {
        if (!confirm(`Are you sure you want to delete template '${template.name}'?`)) return;

        try {
            await productTemplatesApi.delete(template.id);
            if (selectedProduct) {
                const updatedProduct = await productsApi.getById(selectedProduct.id);
                setSelectedProduct(updatedProduct);
            }
        } catch (error) {
            console.error('Failed to delete template', error);
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
                <button className="cmd-btn" onClick={fetchProducts}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search products..."
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
                                <th>Description</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {products.map((product) => (
                                <tr key={product.id} onClick={() => openView(product)} className="clickable-row">
                                    <td>{product.name}</td>
                                    <td>{product.description || '-'}</td>
                                    <td>
                                        <span className={`status-text ${product.isActive ? 'active' : 'inactive'}`}>
                                            {product.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(product); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(product); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(product); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {products.length === 0 && (
                                <tr><td colSpan={4} className="empty-state">No products found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Product' : mode === 'edit' ? 'Edit Product' : selectedProduct?.name || 'Product Details'}
                footer={
                    (mode === 'create' || mode === 'edit') && tabMode === 'general' ? (
                        <>
                            <button key="save-btn" className="btn primary" type="submit" form="product-form">Save</button>
                            <button key="cancel-btn" type="button" className="btn" onClick={mode === 'edit' ? () => setMode('view') : closePanel}>Cancel</button>
                        </>
                    ) : (
                        mode === 'view' && (
                            <>
                                <button key="edit-btn" type="button" className="btn primary" onClick={() => openEdit()}>Edit</button>
                                <button key="delete-btn" type="button" className="btn danger" onClick={() => handleDelete()}>Delete</button>
                            </>
                        )
                    )
                }
            >
                {/* Tabs */}
                {mode !== 'create' && selectedProduct && (
                    <div className="tabs">
                        <button
                            className={`tab ${tabMode === 'general' ? 'active' : ''}`}
                            onClick={() => setTabMode('general')}
                        >
                            General
                        </button>
                        <button
                            className={`tab ${tabMode === 'config-questions' ? 'active' : ''}`}
                            onClick={() => setTabMode('config-questions')}
                        >
                            Configuration Questions
                        </button>
                        <button
                            className={`tab ${tabMode === 'templates' ? 'active' : ''}`}
                            onClick={() => setTabMode('templates')}
                        >
                            Product Templates
                        </button>
                    </div>
                )}

                {/* General Tab */}
                {tabMode === 'general' && (
                    <>
                        {/* View Mode */}
                        {mode === 'view' && selectedProduct && (
                            <div className="view-details">
                                <div className="detail-item">
                                    <label>Name</label>
                                    <div className="value">{selectedProduct.name}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Description</label>
                                    <div className="value">{selectedProduct.description || '-'}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Status</label>
                                    <div className="value">
                                        {selectedProduct.isActive ? 'Active' : 'Inactive'}
                                    </div>
                                </div>
                                <div className="detail-item">
                                    <label>ID</label>
                                    <div className="value monospace">{selectedProduct.id}</div>
                                </div>
                            </div>
                        )}

                        {/* Form Mode */}
                        {(mode === 'create' || mode === 'edit') && (
                            <form id="product-form" className="panel-form" onSubmit={handleSubmit}>
                                <div className="form-field">
                                    <label htmlFor="productName">Name <span className="required">*</span></label>
                                    <input
                                        id="productName"
                                        type="text"
                                        value={formData.name}
                                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                        className={errors.Name ? 'error' : ''}
                                        autoFocus
                                    />
                                    {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                                </div>

                                <div className="form-field">
                                    <label htmlFor="productDescription">Description</label>
                                    <textarea
                                        id="productDescription"
                                        value={formData.description}
                                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                        className={errors.Description ? 'error' : ''}
                                        rows={4}
                                    />
                                    {errors.Description && <span className="field-error">{errors.Description[0]}</span>}
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
                    </>
                )}

                {/* Configuration Questions Tab */}
                {tabMode === 'config-questions' && mode === 'view' && selectedProduct && (
                    <div className="tab-content">
                        <div className="tab-header">
                            <button className="btn primary" onClick={openConfigQuestionCreate}>
                                <PlusIcon /> Add Configuration Question
                            </button>
                        </div>

                        {configQuestionMode === 'create' && (
                            <form className="panel-form" onSubmit={handleConfigQuestionSubmit} style={{ marginBottom: '1rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '4px' }}>
                                <h4>Add Configuration Question</h4>
                                <div className="form-field">
                                    <label htmlFor="configQuestionId">Configuration Question <span className="required">*</span></label>
                                    <select
                                        id="configQuestionId"
                                        value={configQuestionFormData.configurationQuestionId}
                                        onChange={(e) => setConfigQuestionFormData({ ...configQuestionFormData, configurationQuestionId: e.target.value })}
                                        className={errors.ConfigurationQuestionId ? 'error' : ''}
                                    >
                                        <option value="">-- Select Question --</option>
                                        {availableQuestions.map(q => (
                                            <option key={q.id} value={q.id}>{q.question}</option>
                                        ))}
                                    </select>
                                    {errors.ConfigurationQuestionId && <span className="field-error">{errors.ConfigurationQuestionId[0]}</span>}
                                </div>

                                <div className="form-field">
                                    <label htmlFor="statusReason">Status Reason</label>
                                    <input
                                        id="statusReason"
                                        type="text"
                                        value={configQuestionFormData.statusReason}
                                        onChange={(e) => setConfigQuestionFormData({ ...configQuestionFormData, statusReason: e.target.value })}
                                    />
                                </div>

                                <div style={{ marginTop: '1rem' }}>
                                    <button type="submit" className="btn primary">Add</button>
                                    <button type="button" className="btn" onClick={() => setConfigQuestionMode('none')}>Cancel</button>
                                </div>
                            </form>
                        )}

                        <table className="details-list">
                            <thead>
                                <tr>
                                    <th>Question</th>
                                    <th>Status Reason</th>
                                    <th style={{ width: '100px' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {selectedProduct.configurationQuestions.map((pcq) => (
                                    <tr key={pcq.id}>
                                        <td>{pcq.question}</td>
                                        <td>{pcq.statusReason || '-'}</td>
                                        <td>
                                            <div className="row-actions">
                                                <button
                                                    className="action-btn danger"
                                                    onClick={() => handleConfigQuestionDelete(pcq)}
                                                    title="Remove"
                                                >
                                                    <TrashIcon />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                                {selectedProduct.configurationQuestions.length === 0 && (
                                    <tr><td colSpan={3} className="empty-state">No configuration questions.</td></tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Product Templates Tab */}
                {tabMode === 'templates' && mode === 'view' && selectedProduct && (
                    <div className="tab-content">
                        <div className="tab-header">
                            <button className="btn primary" onClick={openTemplateCreate}>
                                <PlusIcon /> New Template
                            </button>
                        </div>

                        {templateMode === 'create' && (
                            <form className="panel-form" onSubmit={handleTemplateSubmit} style={{ marginBottom: '1rem', padding: '1rem', border: '1px solid #ddd', borderRadius: '4px' }}>
                                <h4>New Product Template</h4>
                                <div className="form-field">
                                    <label htmlFor="templateName">Template Name <span className="required">*</span></label>
                                    <input
                                        id="templateName"
                                        type="text"
                                        value={templateFormData.name}
                                        onChange={(e) => setTemplateFormData({ ...templateFormData, name: e.target.value })}
                                        className={errors.Name ? 'error' : ''}
                                    />
                                    {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                                </div>

                                <div className="form-field">
                                    <label htmlFor="templateVersion">Version <span className="required">*</span></label>
                                    <input
                                        id="templateVersion"
                                        type="number"
                                        min="1"
                                        value={templateFormData.version}
                                        onChange={(e) => setTemplateFormData({ ...templateFormData, version: parseInt(e.target.value) || 1 })}
                                        className={errors.Version ? 'error' : ''}
                                    />
                                    {errors.Version && <span className="field-error">{errors.Version[0]}</span>}
                                </div>

                                <div style={{ marginTop: '1rem' }}>
                                    <button type="submit" className="btn primary">Create</button>
                                    <button type="button" className="btn" onClick={() => setTemplateMode('none')}>Cancel</button>
                                </div>
                            </form>
                        )}

                        <table className="details-list">
                            <thead>
                                <tr>
                                    <th>Name</th>
                                    <th style={{ width: '100px' }}>Version</th>
                                    <th style={{ width: '100px' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {selectedProduct.productTemplates.map((template) => (
                                    <tr key={template.id}>
                                        <td>{template.name}</td>
                                        <td>{template.version}</td>
                                        <td>
                                            <div className="row-actions">
                                                <button
                                                    className="action-btn danger"
                                                    onClick={() => handleTemplateDelete(template)}
                                                    title="Delete"
                                                >
                                                    <TrashIcon />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                                {selectedProduct.productTemplates.length === 0 && (
                                    <tr><td colSpan={3} className="empty-state">No product templates.</td></tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                )}
            </SidePanel>
        </div>
    );
}
