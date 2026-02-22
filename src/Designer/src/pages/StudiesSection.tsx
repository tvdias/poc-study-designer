import React, { useState, useEffect, useCallback } from 'react';
import { Plus, RefreshCw, X, FlaskConical, ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import {
    studiesApi,
    fieldworkMarketsApi,
    type StudySummary,
    type CreateStudyRequest,
    type FieldworkMarket
} from '../services/api';
import './StudiesSection.css';

interface StudiesSectionProps {
    projectId: string;
    onListUpdate?: () => void;
}

type SortField = 'name' | 'version' | 'status' | 'category' | 'fieldworkMarketName' | 'createdOn';
type SortDirection = 'asc' | 'desc';

export function StudiesSection({ projectId, onListUpdate }: StudiesSectionProps) {
    const navigate = useNavigate();
    const [studies, setStudies] = useState<StudySummary[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [serverError, setServerError] = useState<string | null>(null);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [fieldworkMarkets, setFieldworkMarkets] = useState<FieldworkMarket[]>([]);
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [sortField, setSortField] = useState<SortField>('name');
    const [sortDir, setSortDir] = useState<SortDirection>('asc');

    // Create Form State
    const [createFormData, setCreateFormData] = useState<CreateStudyRequest>({
        projectId,
        name: '',
        category: '',
        maconomyJobNumber: '',
        projectOperationsUrl: '',
        scripterNotes: '',
        fieldworkMarketId: ''
    });
    const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});

    useEffect(() => {
        fieldworkMarketsApi.getAll().then(setFieldworkMarkets).catch(err => console.error('Failed to load markets', err));
    }, []);

    const loadStudies = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await studiesApi.getAll(projectId);
            setStudies(data);
        } catch (err) {
            setError('Failed to load studies');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => {
        loadStudies();
    }, [loadStudies]);

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setServerError(null);
        setValidationErrors({});
        try {
            await studiesApi.create(createFormData);
            await loadStudies();
            if (onListUpdate) onListUpdate();
            setShowCreateModal(false);
            setCreateFormData({
                projectId,
                name: '',
                category: '',
                maconomyJobNumber: '',
                projectOperationsUrl: '',
                scripterNotes: '',
                fieldworkMarketId: ''
            });
        } catch (err: any) {
            console.error('Failed to create study', err);
            if (err.status === 400 && err.errors) {
                setValidationErrors(err.errors);
            } else if (err.detail) {
                setServerError(err.detail);
            } else if (err.title) {
                setServerError(err.title);
            } else {
                setServerError('Failed to create study');
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const toggleSort = (field: SortField) => {
        if (sortField === field) {
            setSortDir(d => d === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDir('asc');
        }
    };

    const sortedStudies = [...studies].sort((a, b) => {
        let aVal: string | number = '';
        let bVal: string | number = '';
        switch (sortField) {
            case 'name': aVal = a.name; bVal = b.name; break;
            case 'version': aVal = a.version; bVal = b.version; break;
            case 'status': aVal = a.status; bVal = b.status; break;
            case 'category': aVal = a.category || ''; bVal = b.category || ''; break;
            case 'fieldworkMarketName': aVal = a.fieldworkMarketName || ''; bVal = b.fieldworkMarketName || ''; break;
            case 'createdOn': aVal = a.createdOn; bVal = b.createdOn; break;
        }
        if (aVal < bVal) return sortDir === 'asc' ? -1 : 1;
        if (aVal > bVal) return sortDir === 'asc' ? 1 : -1;
        return 0;
    });

    const allChecked = sortedStudies.length > 0 && selectedIds.size === sortedStudies.length;
    const someChecked = selectedIds.size > 0 && !allChecked;

    const toggleAll = () => {
        if (allChecked || someChecked) {
            setSelectedIds(new Set());
        } else {
            setSelectedIds(new Set(sortedStudies.map(s => s.studyId)));
        }
    };

    const toggleRow = (id: string) => {
        setSelectedIds(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    };

    const SortIcon = ({ field }: { field: SortField }) => {
        if (sortField !== field) return <ChevronsUpDown size={13} className="sort-icon sort-icon--inactive" />;
        return sortDir === 'asc'
            ? <ChevronUp size={13} className="sort-icon" />
            : <ChevronDown size={13} className="sort-icon" />;
    };

    if (loading) return <div className="loading-container">Loading studies...</div>;
    if (error) return <div className="error-container">{error}</div>;

    return (
        <section className="studies-section">
            {/* Toolbar */}
            <div className="list-toolbar">
                <div className="list-toolbar__left">
                    <span className="list-view-label">Active Studies</span>
                    <ChevronDown size={15} className="list-view-chevron" />
                </div>
                <div className="list-toolbar__right">
                    <button className="toolbar-btn toolbar-btn--primary" onClick={() => setShowCreateModal(true)}>
                        <Plus size={15} />
                        New Study
                    </button>
                    <button className="toolbar-btn" onClick={loadStudies} title="Refresh">
                        <RefreshCw size={15} />
                        Refresh
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="list-table-wrapper">
                {studies.length === 0 ? (
                    <div className="empty-state">
                        <FlaskConical size={48} className="empty-icon" />
                        <p>No studies created yet.</p>
                    </div>
                ) : (
                    <table className="list-table">
                        <thead>
                            <tr>
                                <th className="col-check">
                                    <input
                                        type="checkbox"
                                        checked={allChecked}
                                        ref={el => { if (el) el.indeterminate = someChecked; }}
                                        onChange={toggleAll}
                                    />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('name')}>
                                    Name <SortIcon field="name" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('version')}>
                                    Version N… <SortIcon field="version" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('status')}>
                                    Status Reason <SortIcon field="status" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('category')}>
                                    Category <SortIcon field="category" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('fieldworkMarketName')}>
                                    Fieldwork Market <SortIcon field="fieldworkMarketName" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('createdOn')}>
                                    Created On <SortIcon field="createdOn" />
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedStudies.map(study => (
                                <tr
                                    key={study.studyId}
                                    className={`${selectedIds.has(study.studyId) ? 'row-selected' : ''} row-clickable`}
                                    onClick={() => navigate(`/projects/${projectId}/studies/${study.studyId}`)}
                                >
                                    <td className="col-check" onClick={e => e.stopPropagation()}>
                                        <input
                                            type="checkbox"
                                            checked={selectedIds.has(study.studyId)}
                                            onChange={() => toggleRow(study.studyId)}
                                        />
                                    </td>
                                    <td className="col-link">{study.name}</td>
                                    <td>{study.version}</td>
                                    <td>
                                        <span className={`status-pill status-pill--${study.status.toLowerCase().replace(/\s+/g, '-')}`}>
                                            {study.status}
                                        </span>
                                    </td>
                                    <td>{study.category}</td>
                                    <td>{study.fieldworkMarketName}</td>
                                    <td className="col-date">{new Date(study.createdOn).toLocaleDateString('en-GB', { day: '2-digit', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            {studies.length > 0 && (
                <div className="list-footer">
                    Rows: {studies.length}
                </div>
            )}

            {/* Create Modal */}
            {showCreateModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>Create New Study</h3>
                            <button className="close-btn" onClick={() => setShowCreateModal(false)}><X size={20} /></button>
                        </div>
                        <form onSubmit={handleCreateSubmit}>
                            <div className="modal-body">
                                {serverError && (
                                    <div className="modal-error-message">
                                        {serverError}
                                    </div>
                                )}
                                <div className="form-group">
                                    <label>Study Name <span className="required">*</span></label>
                                    <input
                                        type="text"
                                        className={`form-input ${validationErrors.Name ? 'error' : ''}`}
                                        value={createFormData.name}
                                        onChange={e => setCreateFormData({ ...createFormData, name: e.target.value })}
                                        required
                                        autoFocus
                                    />
                                    {validationErrors.Name && (
                                        <div className="field-error">{validationErrors.Name[0]}</div>
                                    )}
                                </div>
                                <div className="form-group">
                                    <label>Category <span className="required">*</span></label>
                                    <input
                                        type="text"
                                        className={`form-input ${validationErrors.Category ? 'error' : ''}`}
                                        value={createFormData.category}
                                        onChange={e => setCreateFormData({ ...createFormData, category: e.target.value })}
                                        required
                                    />
                                    {validationErrors.Category && (
                                        <div className="field-error">{validationErrors.Category[0]}</div>
                                    )}
                                </div>
                                <div className="form-group">
                                    <label>Fieldwork Market <span className="required">*</span></label>
                                    <select
                                        className={`form-input ${validationErrors.FieldworkMarketId ? 'error' : ''}`}
                                        value={createFormData.fieldworkMarketId}
                                        onChange={e => setCreateFormData({ ...createFormData, fieldworkMarketId: e.target.value })}
                                        required
                                    >
                                        <option value="">Select Market...</option>
                                        {fieldworkMarkets.map(market => (
                                            <option key={market.id} value={market.id}>
                                                {market.name} ({market.isoCode})
                                            </option>
                                        ))}
                                    </select>
                                    {validationErrors.FieldworkMarketId && (
                                        <div className="field-error">{validationErrors.FieldworkMarketId[0]}</div>
                                    )}
                                </div>
                                <div className="form-group">
                                    <label>Maconomy Job Number <span className="required">*</span></label>
                                    <input
                                        type="text"
                                        className={`form-input ${validationErrors.MaconomyJobNumber ? 'error' : ''}`}
                                        value={createFormData.maconomyJobNumber}
                                        onChange={e => setCreateFormData({ ...createFormData, maconomyJobNumber: e.target.value })}
                                        required
                                    />
                                    {validationErrors.MaconomyJobNumber && (
                                        <div className="field-error">{validationErrors.MaconomyJobNumber[0]}</div>
                                    )}
                                </div>
                                <div className="form-group">
                                    <label>Project Operations URL <span className="required">*</span></label>
                                    <input
                                        type="text"
                                        className={`form-input ${validationErrors.ProjectOperationsUrl ? 'error' : ''}`}
                                        value={createFormData.projectOperationsUrl}
                                        onChange={e => setCreateFormData({ ...createFormData, projectOperationsUrl: e.target.value })}
                                        required
                                    />
                                    {validationErrors.ProjectOperationsUrl && (
                                        <div className="field-error">{validationErrors.ProjectOperationsUrl[0]}</div>
                                    )}
                                </div>
                                <div className="form-group">
                                    <label>Scripter Notes</label>
                                    <textarea
                                        className="form-textarea"
                                        value={createFormData.scripterNotes || ''}
                                        onChange={e => setCreateFormData({ ...createFormData, scripterNotes: e.target.value })}
                                        rows={2}
                                    />
                                </div>
                            </div>
                            <div className="modal-footer">
                                <button type="button" className="cancel-btn" onClick={() => setShowCreateModal(false)}>
                                    Cancel
                                </button>
                                <button type="submit" className="save-btn" disabled={isSubmitting}>
                                    {isSubmitting ? 'Creating...' : 'Create Study'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </section>
    );
}
