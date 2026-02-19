import React, { useState, useEffect, useCallback } from 'react';
import { Plus, FlaskConical, Calendar, FileText } from 'lucide-react';
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
}

export function StudiesSection({ projectId }: StudiesSectionProps) {
    const [studies, setStudies] = useState<StudySummary[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [serverError, setServerError] = useState<string | null>(null);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [fieldworkMarkets, setFieldworkMarkets] = useState<FieldworkMarket[]>([]);

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

    if (loading) return <div className="loading-container">Loading studies...</div>;
    if (error) return <div className="error-container">{error}</div>;

    return (
        <section className="studies-section">
            <div className="section-header">
                <h2 className="section-title">Studies</h2>
                <button
                    className="add-btn"
                    onClick={() => setShowCreateModal(true)}
                >
                    <Plus size={16} />
                    <span>New Study</span>
                </button>
            </div>

            <div className="studies-container">
                {studies.length === 0 ? (
                    <div className="empty-state">
                        <FlaskConical size={48} className="empty-icon" />
                        <p>No studies created yet.</p>
                    </div>
                ) : (
                    <div className="studies-grid">
                        {studies.map(study => (
                            <div key={study.studyId} className="study-card">
                                <div className="study-header">
                                    <div className="study-title-wrapper">
                                        <h3 className="study-title">{study.name}</h3>
                                        <span className={`status-badge ${study.status.toLowerCase()}`}>
                                            {study.status}
                                        </span>
                                    </div>
                                    <span className="version-badge">v{study.version}</span>
                                </div>

                                <div className="study-meta">
                                    <div className="meta-item">
                                        <FileText size={14} />
                                        <span>{study.questionCount} Questions</span>
                                    </div>
                                    <div className="meta-item">
                                        <Calendar size={14} />
                                        <span>{new Date(study.createdOn).toLocaleDateString()}</span>
                                    </div>
                                </div>

                                <div className="study-footer">
                                    <button className="view-btn">
                                        View Details
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Create Modal */}
            {showCreateModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>Create New Study</h3>
                            <button className="close-btn" onClick={() => setShowCreateModal(false)}>×</button>
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
