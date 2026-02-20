import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Plus, RefreshCw, X, CheckCircle2, Info } from 'lucide-react';
import {
    managedListsApi,
    studiesApi,
    questionnaireLinesApi,
    type ManagedList,
    type ManagedListItemRequest,
    type StudySummary,
    type QuestionnaireLine
} from '../services/api';
import './ManagedListDetailPage.css';

type Tab = 'general' | 'entities' | 'study_allocation' | 'question_allocation';

export function ManagedListDetailPage() {
    const { projectId, listId } = useParams<{ projectId: string; listId: string }>();
    const navigate = useNavigate();

    const [list, setList] = useState<ManagedList | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<Tab>('general');

    // Entities tab state
    const [newItemData, setNewItemData] = useState<ManagedListItemRequest>({
        code: '',
        label: '',
        sortOrder: 10,
    });
    const [showAddForm, setShowAddForm] = useState(false);
    const [itemSubmitting, setItemSubmitting] = useState(false);
    const [itemError, setItemError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [filterText, setFilterText] = useState('');

    // Allocation tab state
    const [studies, setStudies] = useState<StudySummary[]>([]);
    const [allocations, setAllocations] = useState<Record<string, Record<string, boolean>>>({});
    const [originalAllocations, setOriginalAllocations] = useState<Record<string, Record<string, boolean>>>({});
    const [hasAllocationChanges, setHasAllocationChanges] = useState(false);
    const [allocationSubmitting, setAllocationSubmitting] = useState(false);

    // Question Allocation tab state
    const [questions, setQuestions] = useState<QuestionnaireLine[]>([]);
    const [questionAllocations, setQuestionAllocations] = useState<Record<string, Record<string, boolean>>>({});
    const [originalQuestionAllocations, setOriginalQuestionAllocations] = useState<Record<string, Record<string, boolean>>>({});
    const [hasQuestionAllocationChanges, setHasQuestionAllocationChanges] = useState(false);
    const [questionAllocationSubmitting, setQuestionAllocationSubmitting] = useState(false);

    const loadList = useCallback(async () => {
        if (!listId) return;
        try {
            setLoading(true);
            setError(null);
            const data = await managedListsApi.getById(listId);
            setList(data);

            if (projectId) {
                const studiesData = await studiesApi.getAll(projectId);
                // Based on UI screenshot, only Draft studies are actionable, others could be shown but disabled.
                // We'll show all and disable non-draft checkboxes.
                setStudies(studiesData);

                // Build initial allocation matrix. Load from localStorage if available.
                const savedAllocationsRaw = localStorage.getItem(`ml_${listId}_studyAllocations`);
                const savedAllocations = savedAllocationsRaw ? JSON.parse(savedAllocationsRaw) : null;

                const initialMap: Record<string, Record<string, boolean>> = {};
                for (const study of studiesData) {
                    initialMap[study.studyId] = {};
                    for (const item of (data.items || [])) {
                        initialMap[study.studyId][item.id] = savedAllocations?.[study.studyId]?.[item.id] || false;
                    }
                }
                setAllocations(initialMap);
                setOriginalAllocations(JSON.parse(JSON.stringify(initialMap))); // deep copy
                setHasAllocationChanges(false);

                // Load questions for the question allocation tab
                const qsData = await questionnaireLinesApi.getAll(projectId);
                setQuestions(qsData);

                // Build initial question allocation matrix. Load from localStorage if available.
                const savedQAllocationsRaw = localStorage.getItem(`ml_${listId}_questionAllocations`);
                const savedQAllocations = savedQAllocationsRaw ? JSON.parse(savedQAllocationsRaw) : null;

                const initialQMap: Record<string, Record<string, boolean>> = {};
                for (const q of qsData) {
                    initialQMap[q.id] = {};
                    for (const item of (data.items || [])) {
                        initialQMap[q.id][item.id] = savedQAllocations?.[q.id]?.[item.id] || false;
                    }
                }
                setQuestionAllocations(initialQMap);
                setOriginalQuestionAllocations(JSON.parse(JSON.stringify(initialQMap)));
                setHasQuestionAllocationChanges(false);
            }
        } catch (err) {
            setError('Failed to load managed list');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [listId]);

    useEffect(() => {
        loadList();
    }, [loadList]);

    // Compute next sort order suggestion
    useEffect(() => {
        if (list) {
            const max = list.items?.length > 0
                ? Math.max(...list.items.map(i => i.sortOrder)) + 10
                : 10;
            setNewItemData(prev => ({ ...prev, sortOrder: max }));
        }
    }, [list]);

    const showSuccess = (msg: string) => {
        setSuccessMessage(msg);
        setTimeout(() => setSuccessMessage(null), 3000);
    };

    const handleAddItem = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!listId) return;
        setItemSubmitting(true);
        setItemError(null);
        setFieldErrors({});
        try {
            const newItem = await managedListsApi.addItem(listId, newItemData);
            setList(prev => prev ? {
                ...prev,
                items: [...(prev.items || []), newItem].sort((a, b) => a.sortOrder - b.sortOrder)
            } : prev);
            setNewItemData(prev => ({ code: '', label: '', sortOrder: prev.sortOrder + 10 }));
            setShowAddForm(false);
            showSuccess('Item added successfully');
            // refocus for quick entry
        } catch (err: any) {
            if (err.errors) {
                setFieldErrors(err.errors);
                setItemError('Validation failed. Please fix the highlighted fields.');
            } else {
                setItemError(err.message || err.title || 'Failed to add item');
            }
        } finally {
            setItemSubmitting(false);
        }
    };

    const handleDeleteItem = async (itemId: string) => {
        if (!listId) return;
        if (!confirm('Remove this item?')) return;
        try {
            await managedListsApi.deleteItem(listId, itemId);
            setList(prev => prev ? {
                ...prev,
                items: prev.items.filter(i => i.id !== itemId)
            } : prev);
            showSuccess('Item removed');
        } catch {
            alert('Failed to delete item');
        }
    };

    const handleDeleteList = async () => {
        if (!listId || !projectId) return;
        if (!confirm(`Are you sure you want to delete "${list?.name}"? This cannot be undone.`)) return;
        try {
            await managedListsApi.delete(listId);
            navigate(`/projects/${projectId}?section=lists`);
        } catch {
            showSuccess('Failed to remove item');
        }
    };

    const handleAllocationToggle = (studyId: string, itemId: string) => {
        setAllocations(prev => {
            const next = { ...prev };
            next[studyId] = { ...next[studyId], [itemId]: !next[studyId][itemId] };

            // Check if there are changes compared to the original map
            let changed = false;
            for (const sId of Object.keys(next)) {
                for (const iId of Object.keys(next[sId])) {
                    if (next[sId][iId] !== originalAllocations[sId]?.[iId]) {
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }
            setHasAllocationChanges(changed);

            return next;
        });
    };

    const handleStudyToggleAll = (studyId: string, isSelectAll: boolean) => {
        if (!list?.items) return;
        setAllocations(prev => {
            const next = { ...prev };
            next[studyId] = { ...next[studyId] };
            for (const item of list.items) {
                next[studyId][item.id] = isSelectAll;
            }
            let changed = false;
            for (const sId of Object.keys(next)) {
                for (const iId of Object.keys(next[sId])) {
                    if (next[sId][iId] !== originalAllocations[sId]?.[iId]) { changed = true; break; }
                }
                if (changed) break;
            }
            setHasAllocationChanges(changed);
            return next;
        });
    };

    const handleEntityToggleAllStudies = (itemId: string, isSelectAll: boolean) => {
        const draftStudies = studies.filter(s => s.status === 'Draft');
        setAllocations(prev => {
            const next = { ...prev };
            for (const study of draftStudies) {
                next[study.studyId] = { ...next[study.studyId], [itemId]: isSelectAll };
            }
            let changed = false;
            for (const sId of Object.keys(next)) {
                for (const iId of Object.keys(next[sId])) {
                    if (next[sId][iId] !== originalAllocations[sId]?.[iId]) { changed = true; break; }
                }
                if (changed) break;
            }
            setHasAllocationChanges(changed);
            return next;
        });
    };

    const handleSaveAllocations = async () => {
        setAllocationSubmitting(true);
        setTimeout(() => {
            // Persist to local storage mock
            if (listId) {
                localStorage.setItem(`ml_${listId}_studyAllocations`, JSON.stringify(allocations));
            }
            setOriginalAllocations(JSON.parse(JSON.stringify(allocations)));
            setHasAllocationChanges(false);
            setAllocationSubmitting(false);
            showSuccess('Allocations saved successfully!');
        }, 800);
    };

    const handleCopyAllCodes = () => {
        const draftStudies = studies.filter(s => s.status === 'Draft');
        if (draftStudies.length === 0 || !list?.items) return;

        setAllocations(prev => {
            const next = { ...prev };

            for (const study of draftStudies) {
                // clone the study object to avoid mutating previous state
                next[study.studyId] = { ...next[study.studyId] };
                for (const item of list.items) {
                    next[study.studyId][item.id] = true;
                }
            }

            let changed = false;
            for (const sId of Object.keys(next)) {
                for (const iId of Object.keys(next[sId])) {
                    if (next[sId][iId] !== originalAllocations[sId]?.[iId]) {
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }

            setHasAllocationChanges(changed);
            return next;
        });
    };

    const handleQuestionAllocationToggle = (questionId: string, itemId: string) => {
        setQuestionAllocations(prev => {
            const next = { ...prev };
            next[questionId] = { ...next[questionId], [itemId]: !next[questionId][itemId] };

            let changed = false;
            for (const qId of Object.keys(next)) {
                for (const iId of Object.keys(next[qId])) {
                    if (next[qId][iId] !== originalQuestionAllocations[qId]?.[iId]) {
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }
            setHasQuestionAllocationChanges(changed);

            return next;
        });
    };

    const handleQuestionToggleAll = (questionId: string, isSelectAll: boolean) => {
        if (!list?.items) return;
        setQuestionAllocations(prev => {
            const next = { ...prev };
            next[questionId] = { ...next[questionId] };
            for (const item of list.items) {
                next[questionId][item.id] = isSelectAll;
            }
            let changed = false;
            for (const qId of Object.keys(next)) {
                for (const iId of Object.keys(next[qId])) {
                    if (next[qId][iId] !== originalQuestionAllocations[qId]?.[iId]) { changed = true; break; }
                }
                if (changed) break;
            }
            setHasQuestionAllocationChanges(changed);
            return next;
        });
    };

    const handleEntityToggleAllQuestions = (itemId: string, isSelectAll: boolean) => {
        setQuestionAllocations(prev => {
            const next = { ...prev };
            for (const q of questions) {
                next[q.id] = { ...next[q.id], [itemId]: isSelectAll };
            }
            let changed = false;
            for (const qId of Object.keys(next)) {
                for (const iId of Object.keys(next[qId])) {
                    if (next[qId][iId] !== originalQuestionAllocations[qId]?.[iId]) { changed = true; break; }
                }
                if (changed) break;
            }
            setHasQuestionAllocationChanges(changed);
            return next;
        });
    };

    const handleSaveQuestionAllocations = async () => {
        setQuestionAllocationSubmitting(true);
        setTimeout(() => {
            // Persist to local storage mock
            if (listId) {
                localStorage.setItem(`ml_${listId}_questionAllocations`, JSON.stringify(questionAllocations));
            }
            setOriginalQuestionAllocations(JSON.parse(JSON.stringify(questionAllocations)));
            setHasQuestionAllocationChanges(false);
            setQuestionAllocationSubmitting(false);
            showSuccess('Question allocations saved successfully!');
        }, 800);
    };

    const handleCopyAllCodesToQuestions = () => {
        if (questions.length === 0 || !list?.items) return;

        setQuestionAllocations(prev => {
            const next = { ...prev };
            for (const q of questions) {
                next[q.id] = { ...next[q.id] };
                for (const item of list.items) {
                    next[q.id][item.id] = true;
                }
            }

            let changed = false;
            for (const qId of Object.keys(next)) {
                for (const iId of Object.keys(next[qId])) {
                    if (next[qId][iId] !== originalQuestionAllocations[qId]?.[iId]) {
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }
            setHasQuestionAllocationChanges(changed);
            return next;
        });
    };

    const filteredItems = (list?.items || []).filter(item =>
        !filterText ||
        item.label.toLowerCase().includes(filterText.toLowerCase()) ||
        item.code.toLowerCase().includes(filterText.toLowerCase())
    );

    if (loading) return <div className="ml-detail-loading">Loading managed list…</div>;
    if (error || !list) return <div className="ml-detail-error">{error || 'Not found'}</div>;

    return (
        <div className="ml-detail-page">
            {/* Page Header */}
            <div className="ml-detail-header">
                <div className="ml-detail-breadcrumb">
                    <button
                        className="ml-back-btn"
                        onClick={() => navigate(`/projects/${projectId}?section=lists`)}
                    >
                        <ArrowLeft size={16} />
                    </button>
                    <div className="ml-title-block">
                        <h1 className="ml-title">{list.name}</h1>
                        <span className="ml-subtitle">Managed List</span>
                    </div>
                    <span className={`ml-status-pill ml-status-pill--${list.status.toLowerCase()}`}>
                        {list.status}
                    </span>
                </div>

                <div className="ml-header-actions">
                    <button className="ml-action-btn ml-action-btn--danger" onClick={handleDeleteList}>
                        Delete
                    </button>
                    <button className="ml-action-btn" onClick={loadList}>
                        <RefreshCw size={14} />
                        Refresh
                    </button>
                </div>
            </div>

            {/* Tabs */}
            <div className="ml-tabs">
                <button
                    className={`ml-tab ${activeTab === 'general' ? 'ml-tab--active' : ''}`}
                    onClick={() => setActiveTab('general')}
                >
                    General
                </button>
                <button
                    className={`ml-tab ${activeTab === 'entities' ? 'ml-tab--active' : ''}`}
                    onClick={() => setActiveTab('entities')}
                >
                    Entities
                </button>
                <button
                    className={`ml-tab ${activeTab === 'study_allocation' ? 'ml-tab--active' : ''}`}
                    onClick={() => setActiveTab('study_allocation')}
                >
                    Study Allocation
                </button>
                <button
                    className={`ml-tab ${activeTab === 'question_allocation' ? 'ml-tab--active' : ''}`}
                    onClick={() => setActiveTab('question_allocation')}
                >
                    Question Allocation
                </button>
            </div>

            {/* Tab Content */}
            <div className="ml-tab-content">
                {/* Global Success banner */}
                {successMessage && (
                    <div className="ml-success-banner">
                        <CheckCircle2 size={16} />
                        Successfully saved — {successMessage}
                        <button className="ml-banner-close" onClick={() => setSuccessMessage(null)}>
                            <X size={14} />
                        </button>
                    </div>
                )}

                {/* ── General Tab ── */}
                {activeTab === 'general' && (
                    <div className="ml-general-tab">
                        {/* Fields grid */}
                        <div className="ml-fields-section">
                            <div className="ml-fields-grid">
                                {/* Left column */}
                                <div className="ml-fields-col">
                                    <div className="ml-field">
                                        <label className="ml-field-label">
                                            <span className="ml-required">*</span> Name
                                        </label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.name}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Description</label>
                                        <textarea
                                            className="ml-field-textarea"
                                            value={list.description || ''}
                                            readOnly
                                            rows={3}
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Source Type</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.sourceType || 'Question Bank'}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Last Updated</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.modifiedOn ? new Date(list.modifiedOn).toLocaleDateString() : 'N/A'}
                                            readOnly
                                        />
                                    </div>
                                </div>

                                {/* Right column */}
                                <div className="ml-fields-col">
                                    <div className="ml-field">
                                        <label className="ml-field-label">Question Count</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.items?.length ?? 0}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Status</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.status}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Is Auto Generated</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.isAutoGenerated ? 'Yes' : 'No'}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">Ever In Snapshot</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.everInSnapshot ? 'Yes' : 'No'}
                                            readOnly
                                        />
                                    </div>
                                    <div className="ml-field">
                                        <label className="ml-field-label">First Snapshot Date</label>
                                        <input
                                            type="text"
                                            className="ml-field-input"
                                            value={list.firstSnapshotDate ? new Date(list.firstSnapshotDate).toLocaleDateString() : 'N/A'}
                                            readOnly
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Questionnaire Lines sub-panel (read-only info) */}
                        <div className="ml-sub-panel">
                            <div className="ml-sub-panel-header">
                                <h3 className="ml-sub-panel-title">Questionnaire Lines</h3>
                                <div className="ml-sub-panel-actions">
                                    <button className="toolbar-btn" onClick={loadList} title="Refresh">
                                        <RefreshCw size={14} />
                                        Refresh
                                    </button>
                                </div>
                            </div>

                            {list.items?.length === 0 ? (
                                <div className="ml-empty-state">
                                    <div className="ml-empty-icon">
                                        <svg viewBox="0 0 64 64" fill="none" width="64" height="64">
                                            <rect x="8" y="8" width="48" height="48" rx="4" stroke="#cbd5e1" strokeWidth="2" fill="#f8fafc" />
                                            <rect x="16" y="20" width="32" height="4" rx="2" fill="#e2e8f0" />
                                            <rect x="16" y="30" width="24" height="4" rx="2" fill="#e2e8f0" />
                                            <rect x="16" y="40" width="28" height="4" rx="2" fill="#e2e8f0" />
                                        </svg>
                                    </div>
                                    <p>We didn't find anything to show here</p>
                                    <span className="ml-empty-rows">Rows: 0</span>
                                </div>
                            ) : (
                                <div className="ml-sub-table-wrapper">
                                    <table className="ml-sub-table">
                                        <thead>
                                            <tr>
                                                <th>Code</th>
                                                <th>Label</th>
                                                <th>Sort Order</th>
                                                <th>Active</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {list.items.map(item => (
                                                <tr key={item.id}>
                                                    <td className="ml-code-cell">{item.code}</td>
                                                    <td>{item.label}</td>
                                                    <td>{item.sortOrder}</td>
                                                    <td>{item.isActive ? 'Yes' : 'No'}</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                    <div className="ml-sub-panel-footer">Rows: {list.items.length}</div>
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {/* ── Entities Tab ── */}
                {activeTab === 'entities' && (
                    <div className="ml-entities-tab">
                        {/* Entities toolbar */}
                        <div className="ml-entities-toolbar">
                            <div className="ml-entities-toolbar__left">
                                <span className="ml-entities-count">
                                    Active Managed List Entities
                                </span>
                                <span className="ml-count-pill">{filteredItems.length}</span>
                            </div>
                            <div className="ml-entities-toolbar__right">
                                <div className="ml-search-box">
                                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" /></svg>
                                    <input
                                        type="text"
                                        placeholder="Search entities…"
                                        value={filterText}
                                        onChange={e => setFilterText(e.target.value)}
                                        className="ml-search-input"
                                    />
                                </div>
                                <button className="toolbar-btn toolbar-btn--primary" onClick={() => setShowAddForm(true)}>
                                    <Plus size={14} />
                                    New Entity
                                </button>
                                <button className="toolbar-btn" onClick={loadList}>
                                    <RefreshCw size={14} />
                                    Refresh
                                </button>
                            </div>
                        </div>

                        {/* Add Item Form (inline panel) */}
                        {showAddForm && (
                            <div className="ml-add-form-panel">
                                <div className="ml-add-form-header">
                                    <h4>New Managed List Entity</h4>
                                    <button className="ml-close-form-btn" onClick={() => { setShowAddForm(false); setItemError(null); setFieldErrors({}); }}>
                                        <X size={16} />
                                    </button>
                                </div>
                                {itemError && (
                                    <div className="ml-item-error">
                                        <Info size={14} />
                                        {itemError}
                                    </div>
                                )}
                                <form onSubmit={handleAddItem} className="ml-add-form">
                                    <div className="ml-add-form-grid">
                                        <div className="ml-add-field">
                                            <label>Name (Label) <span className="ml-required">*</span></label>
                                            <input
                                                type="text"
                                                className={`ml-field-input ${fieldErrors.Label ? 'ml-field-input--invalid' : ''}`}
                                                placeholder="e.g. Coca Cola"
                                                value={newItemData.label}
                                                onChange={e => setNewItemData({ ...newItemData, label: e.target.value })}
                                                required
                                                autoFocus
                                            />
                                            {fieldErrors.Label && <div className="ml-field-error-text">{fieldErrors.Label[0]}</div>}
                                        </div>
                                        <div className="ml-add-field">
                                            <label>Answer Code <span className="ml-required">*</span></label>
                                            <input
                                                type="text"
                                                className={`ml-field-input ${fieldErrors.Code ? 'ml-field-input--invalid' : ''}`}
                                                placeholder="e.g. COCACOLA"
                                                value={newItemData.code}
                                                onChange={e => setNewItemData({ ...newItemData, code: e.target.value })}
                                                required
                                            />
                                            {fieldErrors.Code && <div className="ml-field-error-text">{fieldErrors.Code[0]}</div>}
                                        </div>
                                        <div className="ml-add-field ml-add-field--small">
                                            <label>Sort Order</label>
                                            <input
                                                type="number"
                                                className="ml-field-input"
                                                value={newItemData.sortOrder}
                                                onChange={e => setNewItemData({ ...newItemData, sortOrder: parseInt(e.target.value) || 0 })}
                                            />
                                        </div>
                                    </div>
                                    <div className="ml-add-form-footer">
                                        <button type="button" className="ml-cancel-btn" onClick={() => { setShowAddForm(false); setItemError(null); }}>
                                            Cancel
                                        </button>
                                        <button type="submit" className="ml-save-btn" disabled={itemSubmitting}>
                                            {itemSubmitting ? 'Saving…' : 'Save'}
                                        </button>
                                    </div>
                                </form>
                            </div>
                        )}

                        {/* Entities Table */}
                        {filteredItems.length === 0 && !showAddForm ? (
                            <div className="ml-empty-state">
                                <div className="ml-empty-icon">
                                    <svg viewBox="0 0 64 64" fill="none" width="64" height="64">
                                        <rect x="8" y="8" width="48" height="48" rx="4" stroke="#cbd5e1" strokeWidth="2" fill="#f8fafc" />
                                        <rect x="16" y="20" width="32" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="30" width="24" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="40" width="28" height="4" rx="2" fill="#e2e8f0" />
                                    </svg>
                                </div>
                                <p>We didn't find anything to show here</p>
                                <span className="ml-empty-rows">Rows: 0</span>
                            </div>
                        ) : (
                            <div className="ml-entities-table-wrapper">
                                <table className="ml-entities-table">
                                    <thead>
                                        <tr>
                                            <th className="col-name">Name</th>
                                            <th className="col-code">Answer Code</th>
                                            <th className="col-source">Source</th>
                                            <th className="col-snapshot">Ever in Snapshot</th>
                                            <th className="col-order">Sort Order</th>
                                            <th className="col-active">Active</th>
                                            <th className="col-actions"></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {filteredItems.map(item => (
                                            <tr key={item.id}>
                                                <td className="col-name col-link">{item.label}</td>
                                                <td className="col-code ml-code-cell">{item.code}</td>
                                                <td className="col-source">{item.sourceType || 'Managed List'}</td>
                                                <td className="col-snapshot">{item.everInSnapshot ? 'Yes' : 'No'}</td>
                                                <td className="col-order">{item.sortOrder}</td>
                                                <td className="col-active">
                                                    <span className={`ml-active-pill ${item.isActive ? 'ml-active-pill--yes' : 'ml-active-pill--no'}`}>
                                                        {item.isActive ? 'Active' : 'Inactive'}
                                                    </span>
                                                </td>
                                                <td className="col-actions">
                                                    <button
                                                        className="ml-delete-item-btn"
                                                        onClick={() => handleDeleteItem(item.id)}
                                                        title="Remove"
                                                    >
                                                        <X size={14} />
                                                    </button>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                                <div className="ml-entities-footer">Rows: {filteredItems.length}</div>
                            </div>
                        )}
                    </div>
                )}

                {/* ── Study Allocation Tab ── */}
                {activeTab === 'study_allocation' && (
                    <div className="ml-allocation-tab">
                        <div className="ml-allocation-toolbar">
                            <div className="ml-allocation-toolbar__left">
                                <span className="ml-allocation-count">Study Selection</span>
                            </div>
                            <div className="ml-allocation-toolbar__right">
                                <button
                                    className="toolbar-btn toolbar-btn--primary"
                                    disabled={!hasAllocationChanges || allocationSubmitting}
                                    onClick={handleSaveAllocations}
                                >
                                    {allocationSubmitting ? 'Saving…' : 'Save Options'}
                                </button>
                            </div>
                        </div>

                        {studies.length === 0 || !list?.items?.length ? (
                            <div className="ml-empty-state">
                                <div className="ml-empty-icon">
                                    <svg viewBox="0 0 64 64" fill="none" width="64" height="64">
                                        <rect x="8" y="8" width="48" height="48" rx="4" stroke="#cbd5e1" strokeWidth="2" fill="#f8fafc" />
                                        <rect x="16" y="20" width="32" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="30" width="24" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="40" width="28" height="4" rx="2" fill="#e2e8f0" />
                                    </svg>
                                </div>
                                <p>Ensure both studies and entities exist to allocate them.</p>
                            </div>
                        ) : (
                            <div className="ml-allocation-matrix-wrapper">
                                <table className="ml-allocation-matrix">
                                    <thead>
                                        <tr>
                                            <th className="alloc-col-entity">
                                                <div>Answer Code</div>
                                                <button
                                                    className="alloc-copy-btn"
                                                    onClick={handleCopyAllCodes}
                                                    disabled={!studies.some(s => s.status === 'Draft')}
                                                >
                                                    Copy all codes to all draft studies
                                                </button>
                                            </th>
                                            {studies.map(study => {
                                                const isDraft = study.status === 'Draft';
                                                const isAllSelected = list?.items?.length > 0 && list.items.every(item => allocations[study.studyId]?.[item.id]);

                                                return (
                                                    <th key={study.studyId} className="alloc-col-study">
                                                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                                                            <input
                                                                type="checkbox"
                                                                checked={isAllSelected}
                                                                disabled={!isDraft}
                                                                onChange={(e) => handleStudyToggleAll(study.studyId, e.target.checked)}
                                                                className="alloc-header-checkbox"
                                                            />
                                                            <div className="alloc-study-name">{study.name}</div>
                                                        </div>
                                                        <div className={`alloc-study-status alloc-study-status--${study.status.toLowerCase()}`}>
                                                            {study.status}
                                                        </div>
                                                    </th>
                                                );
                                            })}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {list.items.map(item => {
                                            const draftStudies = studies.filter(s => s.status === 'Draft');
                                            const isAllDraftSelected = draftStudies.length > 0 && draftStudies.every(s => allocations[s.studyId]?.[item.id]);

                                            return (
                                                <tr key={item.id}>
                                                    <td className="alloc-cell-entity">
                                                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                                            <input
                                                                type="checkbox"
                                                                checked={isAllDraftSelected}
                                                                disabled={draftStudies.length === 0}
                                                                onChange={(e) => handleEntityToggleAllStudies(item.id, e.target.checked)}
                                                                className="alloc-row-checkbox"
                                                            />
                                                            <div>
                                                                <div>{item.label}</div>
                                                                <div className="alloc-entity-code">{item.code}</div>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    {studies.map(study => {
                                                        const isSelected = allocations[study.studyId]?.[item.id] || false;
                                                        const isOriginal = originalAllocations[study.studyId]?.[item.id] || false;
                                                        const isChanged = isSelected !== isOriginal;
                                                        const isDraft = study.status === 'Draft';

                                                        return (
                                                            <td key={`${item.id}-${study.studyId}`} className="alloc-cell-checkbox">
                                                                <div className={`alloc-checkbox-wrapper ${isChanged ? 'is-changed' : ''}`}>
                                                                    <input
                                                                        type="checkbox"
                                                                        checked={isSelected}
                                                                        disabled={!isDraft}
                                                                        onChange={() => handleAllocationToggle(study.studyId, item.id)}
                                                                    />
                                                                </div>
                                                            </td>
                                                        );
                                                    })}
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                )}

                {/* ── Question Allocation Tab ── */}
                {activeTab === 'question_allocation' && (
                    <div className="ml-allocation-tab">
                        <div className="ml-allocation-toolbar">
                            <div className="ml-allocation-toolbar__left">
                                <span className="ml-allocation-count">Question Selection</span>
                            </div>
                            <div className="ml-allocation-toolbar__right">
                                <button
                                    className="toolbar-btn toolbar-btn--primary"
                                    disabled={!hasQuestionAllocationChanges || questionAllocationSubmitting}
                                    onClick={handleSaveQuestionAllocations}
                                >
                                    {questionAllocationSubmitting ? 'Saving…' : 'Save Options'}
                                </button>
                            </div>
                        </div>

                        {questions.length === 0 || !list?.items?.length ? (
                            <div className="ml-empty-state">
                                <div className="ml-empty-icon">
                                    <svg viewBox="0 0 64 64" fill="none" width="64" height="64">
                                        <rect x="8" y="8" width="48" height="48" rx="4" stroke="#cbd5e1" strokeWidth="2" fill="#f8fafc" />
                                        <rect x="16" y="20" width="32" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="30" width="24" height="4" rx="2" fill="#e2e8f0" />
                                        <rect x="16" y="40" width="28" height="4" rx="2" fill="#e2e8f0" />
                                    </svg>
                                </div>
                                <p>Ensure both questions and entities exist to allocate them.</p>
                            </div>
                        ) : (
                            <div className="ml-allocation-matrix-wrapper">
                                <table className="ml-allocation-matrix">
                                    <thead>
                                        <tr>
                                            <th className="alloc-col-entity">
                                                <div>Answer Code</div>
                                                <button className="alloc-copy-btn" onClick={handleCopyAllCodesToQuestions} disabled={questions.length === 0}>
                                                    Copy all codes to all questions
                                                </button>
                                            </th>
                                            {questions.map(q => {
                                                const isAllSelected = list?.items?.length > 0 && list.items.every(item => questionAllocations[q.id]?.[item.id]);

                                                return (
                                                    <th key={q.id} className="alloc-col-study">
                                                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                                                            <input
                                                                type="checkbox"
                                                                checked={isAllSelected}
                                                                onChange={(e) => handleQuestionToggleAll(q.id, e.target.checked)}
                                                                className="alloc-header-checkbox"
                                                            />
                                                            <div className="alloc-study-name">{q.variableName}</div>
                                                        </div>
                                                        <div className="alloc-study-status alloc-study-status--draft" style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', maxWidth: '140px' }} title={q.questionText || q.questionTitle || ''}>
                                                            {q.questionTitle || q.questionText || 'Target'}
                                                        </div>
                                                    </th>
                                                );
                                            })}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {list.items.map(item => {
                                            const isAllSelected = questions.length > 0 && questions.every(q => questionAllocations[q.id]?.[item.id]);

                                            return (
                                                <tr key={item.id}>
                                                    <td className="alloc-cell-entity">
                                                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                                            <input
                                                                type="checkbox"
                                                                checked={isAllSelected}
                                                                disabled={questions.length === 0}
                                                                onChange={(e) => handleEntityToggleAllQuestions(item.id, e.target.checked)}
                                                                className="alloc-row-checkbox"
                                                            />
                                                            <div>
                                                                <div>{item.label}</div>
                                                                <div className="alloc-entity-code">{item.code}</div>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    {questions.map(q => {
                                                        const isSelected = questionAllocations[q.id]?.[item.id] || false;
                                                        const isOriginal = originalQuestionAllocations[q.id]?.[item.id] || false;
                                                        const isChanged = isSelected !== isOriginal;

                                                        return (
                                                            <td key={`${item.id}-${q.id}`} className="alloc-cell-checkbox">
                                                                <div className={`alloc-checkbox-wrapper ${isChanged ? 'is-changed' : ''}`}>
                                                                    <input
                                                                        type="checkbox"
                                                                        checked={isSelected}
                                                                        onChange={() => handleQuestionAllocationToggle(q.id, item.id)}
                                                                    />
                                                                </div>
                                                            </td>
                                                        );
                                                    })}
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
