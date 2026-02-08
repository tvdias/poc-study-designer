import { useState, useEffect } from 'react';
import { productTemplatesApi, productsApi } from '../services/api';
import type { ProductTemplate, Product } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ProductTemplatesPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';

export function ProductTemplatesPage() {
    const [templates, setTemplates] = useState<ProductTemplate[]>([]);
    const [products, setProducts] = useState<Product[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedTemplate, setSelectedTemplate] = useState<ProductTemplate | null>(null);
    const [formData, setFormData] = useState({ name: '', version: 1, productId: '', isActive: true });

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
    };

    const openView = async (template: ProductTemplate) => {
        setSelectedTemplate(template);
        setMode('view');
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
            if (mode === 'edit' && selectedTemplate) {
                const savedTemplate = await productTemplatesApi.update(selectedTemplate.id, {
                    name: formData.name,
                    version: formData.version,
                    productId: formData.productId,
                    isActive: formData.isActive
                });
                await fetchTemplates();
                setSelectedTemplate(savedTemplate);
                setMode('view');
            } else {
                await productTemplatesApi.create({
                    name: formData.name,
                    version: formData.version,
                    productId: formData.productId
                });
                await fetchTemplates();
                closePanel();
            }

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
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button key="save-btn" className="btn primary" type="submit" form="product-template-form">Save</button>
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
                {/* View Mode */}
                {mode === 'view' && selectedTemplate && (
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

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form id="product-template-form" className="panel-form" onSubmit={handleSubmit}>
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
