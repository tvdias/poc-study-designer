import { useState, useEffect } from 'react';
import { modulesApi } from '../services/api';
import type { Module } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ModulesPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';

export function ModulesPage() {
    const [modules, setModules] = useState<Module[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedModule, setSelectedModule] = useState<Module | null>(null);
    const [formData, setFormData] = useState({
        variableName: '',
        label: '',
        description: '',
        versionNumber: 1,
        parentModuleId: '',
        instructions: '',
        isActive: true
    });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchModules();
    }, [search]);

    const fetchModules = async () => {
        setIsLoading(true);
        try {
            const data = await modulesApi.getAll(search);
            setModules(data);
        } catch (error) {
            console.error('Failed to fetch modules', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedModule(null);
        setFormData({
            variableName: '',
            label: '',
            description: '',
            versionNumber: 1,
            parentModuleId: '',
            instructions: '',
            isActive: true
        });
        setErrors({});
        setServerError('');
        setMode('create');
    };

    const openView = (module: Module) => {
        setSelectedModule(module);
        setMode('view');
    };

    const openEdit = (module?: Module) => {
        const target = module || selectedModule;
        if (!target) return;

        if (module) setSelectedModule(module);

        setFormData({
            variableName: target.variableName,
            label: target.label,
            description: target.description || '',
            versionNumber: target.versionNumber,
            parentModuleId: target.parentModuleId || '',
            instructions: target.instructions || '',
            isActive: target.isActive
        });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedModule(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedModule: Module;
            if (mode === 'edit' && selectedModule) {
                savedModule = await modulesApi.update(selectedModule.id, {
                    variableName: formData.variableName,
                    label: formData.label,
                    description: formData.description,
                    versionNumber: formData.versionNumber,
                    parentModuleId: formData.parentModuleId || undefined,
                    instructions: formData.instructions,
                    isActive: formData.isActive
                });
            } else {
                savedModule = await modulesApi.create({
                    variableName: formData.variableName,
                    label: formData.label,
                    description: formData.description,
                    versionNumber: formData.versionNumber,
                    parentModuleId: formData.parentModuleId || undefined,
                    instructions: formData.instructions
                });
            }

            await fetchModules();
            setSelectedModule(savedModule);
            setMode('view');

        } catch (err: any) {
            if (err.status === 400 && err.errors) {
                setErrors(err.errors);
            } else if (err.status === 409) {
                setServerError(err.detail || "Module already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (module?: Module) => {
        const target = module || selectedModule;
        if (!target || !confirm(`Are you sure you want to delete module '${target.label}'?`)) return;

        try {
            await modulesApi.delete(target.id);
            closePanel();
            fetchModules();
        } catch (error) {
            console.error('Failed to delete module', error);
        }
    };

    return (
        <div className="page-container">
            {/* Command Bar */}
            <div className="command-bar card">
                <button className="cmd-btn primary" onClick={openCreate}>
                    <PlusIcon /> <span className="label">New</span>
                </button>
                <button className="cmd-btn" onClick={fetchModules}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search modules..."
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
                                <th>Variable Name</th>
                                <th>Label</th>
                                <th style={{ width: '80px' }}>Version</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {modules.map((module) => (
                                <tr key={module.id} onClick={() => openView(module)} className="clickable-row">
                                    <td>{module.variableName}</td>
                                    <td>{module.label}</td>
                                    <td>{module.versionNumber}</td>
                                    <td>
                                        <span className={`status-text ${module.isActive ? 'active' : 'inactive'}`}>
                                            {module.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(module); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(module); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(module); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {modules.length === 0 && (
                                <tr><td colSpan={5} className="empty-state">No modules found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Module' : mode === 'edit' ? 'Edit Module' : selectedModule?.label || 'Module Details'}
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" type="submit" form="modules-form">Save</button>
                            <button className="btn" onClick={mode === 'edit' ? () => setMode('view') : closePanel}>Cancel</button>
                        </>
                    ) : (
                        mode === 'view' && (
                            <>
                                <button className="btn primary" onClick={() => openEdit()}>Edit</button>
                                <button className="btn danger" onClick={() => handleDelete()}>Delete</button>
                            </>
                        )
                    )
                }
            >
                {/* View Mode */}
                {mode === 'view' && selectedModule && (
                    <div className="view-details">
                        <div className="detail-item">
                            <label>Variable Name</label>
                            <div className="value">{selectedModule.variableName}</div>
                        </div>
                        <div className="detail-item">
                            <label>Label</label>
                            <div className="value">{selectedModule.label}</div>
                        </div>
                        <div className="detail-item">
                            <label>Description</label>
                            <div className="value">{selectedModule.description || '—'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Version Number</label>
                            <div className="value">{selectedModule.versionNumber}</div>
                        </div>
                        <div className="detail-item">
                            <label>Parent Module ID</label>
                            <div className="value monospace">{selectedModule.parentModuleId || '—'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Instructions</label>
                            <div className="value">{selectedModule.instructions || '—'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Status</label>
                            <div className="value">
                                {selectedModule.isActive ? 'Active' : 'Inactive'}
                            </div>
                        </div>
                        <div className="detail-item">
                            <label>ID</label>
                            <div className="value monospace">{selectedModule.id}</div>
                        </div>
                    </div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form id="modules-form" className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="variableName">Variable Name *</label>
                            <input
                                id="variableName"
                                type="text"
                                value={formData.variableName}
                                onChange={(e) => setFormData({ ...formData, variableName: e.target.value })}
                                className={errors.VariableName ? 'error' : ''}
                                autoFocus
                            />
                            {errors.VariableName && <span className="field-error">{errors.VariableName[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="label">Label *</label>
                            <input
                                id="label"
                                type="text"
                                value={formData.label}
                                onChange={(e) => setFormData({ ...formData, label: e.target.value })}
                                className={errors.Label ? 'error' : ''}
                            />
                            {errors.Label && <span className="field-error">{errors.Label[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="description">Description</label>
                            <textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                                rows={3}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="versionNumber">Version Number *</label>
                            <input
                                id="versionNumber"
                                type="number"
                                min="1"
                                value={formData.versionNumber}
                                onChange={(e) => setFormData({ ...formData, versionNumber: parseInt(e.target.value) || 1 })}
                                className={errors.VersionNumber ? 'error' : ''}
                            />
                            {errors.VersionNumber && <span className="field-error">{errors.VersionNumber[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="parentModuleId">Parent Module ID</label>
                            <input
                                id="parentModuleId"
                                type="text"
                                value={formData.parentModuleId}
                                onChange={(e) => setFormData({ ...formData, parentModuleId: e.target.value })}
                                placeholder="Leave empty for no parent"
                            />
                            <small>When creating a new version (greater than 1), enter the ID of the previous version</small>
                        </div>

                        <div className="form-field">
                            <label htmlFor="instructions">Instructions</label>
                            <textarea
                                id="instructions"
                                value={formData.instructions}
                                onChange={(e) => setFormData({ ...formData, instructions: e.target.value })}
                                rows={4}
                            />
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
