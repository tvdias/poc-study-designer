import { useState, useEffect } from 'react';
import { modulesApi, moduleQuestionsApi, questionBankApi } from '../services/api';
import type { Module, ModuleQuestion, QuestionBankItem } from '../services/api';
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
    const [activeTab, setActiveTab] = useState<'general' | 'questions'>('general');
    const [formData, setFormData] = useState({
        variableName: '',
        label: '',
        description: '',
        versionNumber: 1,
        parentModuleId: '',
        instructions: '',
        isActive: true
    });

    // Questions State
    const [questions, setQuestions] = useState<ModuleQuestion[]>([]);
    const [questionSearch, setQuestionSearch] = useState('');
    const [questionSearchResults, setQuestionSearchResults] = useState<QuestionBankItem[]>([]);
    const [isSearching, setIsSearching] = useState(false);

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
        setQuestions([]);
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

    const openView = async (module: Module) => {
        setIsLoading(true);
        try {
            const fullModule = await modulesApi.getById(module.id);
            setSelectedModule(fullModule);
            setQuestions(fullModule.questions || []);
            setActiveTab('general');
            setMode('view');
        } catch (error) {
            console.error('Failed to fetch module details', error);
        } finally {
            setIsLoading(false);
        }
    };

    const openEdit = async (module?: Module) => {
        const targetId = module?.id || selectedModule?.id;
        if (!targetId) return;

        setIsLoading(true);
        try {
            const target = await modulesApi.getById(targetId);
            setSelectedModule(target);
            setQuestions(target.questions || []);

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
        } catch (error) {
            console.error('Failed to fetch module details', error);
        } finally {
            setIsLoading(false);
        }
    };

    const closePanel = () => {
        setMode('list');
        setSelectedModule(null);
        setQuestions([]);
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

    const handleQuestionSearch = async (query: string) => {
        setQuestionSearch(query);
        if (query.length < 3) {
            setQuestionSearchResults([]);
            return;
        }
        setIsSearching(true);
        try {
            const results = await questionBankApi.getAll(query);
            // Filter out questions that are already in the module
            const filteredResults = results.filter(
                (item) => !questions.some((q) => q.questionBankItemId === item.id)
            );
            setQuestionSearchResults(filteredResults.slice(0, 10)); // Limit to 10 results
        } catch (error) {
            console.error('Failed to search questions', error);
        } finally {
            setIsSearching(false);
        }
    };

    const handleAddQuestion = async (questionBankItemId: string) => {
        if (!selectedModule) return;
        try {
            const newQuestion = await moduleQuestionsApi.create(selectedModule.id, {
                questionBankItemId,
                displayOrder: questions.length
            });
            setQuestions([...questions, newQuestion]);
            setQuestionSearch('');
            setQuestionSearchResults([]);
        } catch (err: any) {
            if (err.status === 409) {
                alert(err.detail || 'This question is already added to this module.');
            } else {
                alert('Failed to add question');
            }
        }
    };

    const handleDeleteQuestion = async (questionId: string) => {
        if (!selectedModule || !confirm('Remove this question from the module?')) return;
        try {
            await moduleQuestionsApi.delete(selectedModule.id, questionId);
            setQuestions(questions.filter(q => q.id !== questionId));
        } catch (error) {
            console.error('Failed to delete question', error);
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
                            <button key="save-btn" className="btn primary" type="submit" form="modules-form">Save</button>
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
                {mode === 'view' && selectedModule && (
                    <>
                        {/* Tabs */}
                        <div className="tabs">
                            <button
                                className={`tab ${activeTab === 'general' ? 'active' : ''}`}
                                onClick={() => setActiveTab('general')}
                                type="button"
                            >
                                General
                            </button>
                            <button
                                className={`tab ${activeTab === 'questions' ? 'active' : ''}`}
                                onClick={() => setActiveTab('questions')}
                                type="button"
                            >
                                Questions
                            </button>
                        </div>

                        {/* General Tab */}
                        {activeTab === 'general' && (
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

                        {/* Questions Tab */}
                        {activeTab === 'questions' && (
                            <div className="tab-content">
                                <div className="add-question-section">
                                    <div className="form-field">
                                        <label htmlFor="questionSearch">Add Question</label>
                                        <div className="search-input-wrapper">
                                            <input
                                                id="questionSearch"
                                                type="text"
                                                placeholder="Search by variable name..."
                                                value={questionSearch}
                                                onChange={(e) => handleQuestionSearch(e.target.value)}
                                                autoComplete="off"
                                            />
                                            {isSearching && <div className="searching-indicator">Searching...</div>}

                                            {questionSearchResults.length > 0 && (
                                                <div className="lookup-results">
                                                    {questionSearchResults.map((item) => (
                                                        <div
                                                            key={item.id}
                                                            className="lookup-result-item"
                                                            onClick={() => {
                                                                setQuestionSearch('');
                                                                setQuestionSearchResults([]);
                                                                handleAddQuestion(item.id);
                                                            }}
                                                        >
                                                            <div className="result-primary">{item.variableName}</div>
                                                            <div className="result-secondary">{item.questionText || item.questionType}</div>
                                                        </div>
                                                    ))}
                                                </div>
                                            )}
                                        </div>
                                        <small className="field-help">Enter at least 3 characters to search</small>
                                    </div>
                                </div>

                                {questions.length > 0 ? (
                                    <table className="details-list">
                                        <thead>
                                            <tr>
                                                <th>Question Variable Name</th>
                                                <th>Question Type</th>
                                                <th>Question Text</th>
                                                <th>Classification</th>
                                                <th style={{ width: '100px' }}>Actions</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {questions.map((q) => (
                                                <tr key={q.id}>
                                                    <td>{q.questionVariableName}</td>
                                                    <td>{q.questionType || '—'}</td>
                                                    <td>{q.questionText || '—'}</td>
                                                    <td>{q.classification || '—'}</td>
                                                    <td>
                                                        <div className="row-actions">
                                                            <button
                                                                type="button"
                                                                className="action-btn danger"
                                                                onClick={() => handleDeleteQuestion(q.id)}
                                                                title="Remove"
                                                            >
                                                                <TrashIcon />
                                                            </button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                ) : (
                                    <div className="empty-state">No questions added yet.</div>
                                )}
                            </div>
                        )}
                    </>
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
