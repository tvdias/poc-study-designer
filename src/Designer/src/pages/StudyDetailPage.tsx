import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    RefreshCw,
    Save,
    ArrowLeft,
    ExternalLink,
    CheckCircle2,
    X,
    Clock,
    History,
    Layers,
    Cpu,
    Plus,
    Search
} from 'lucide-react';
import {
    studiesApi,
    fieldworkMarketsApi,
    type GetStudyDetailsResponse,
    type UpdateStudyRequest,
    type FieldworkMarket,
    type StudyQuestionnaireLine
} from '../services/api';
import './StudyDetailPage.css';

type TabType = 'general' | 'questions' | 'snapshots' | 'change_log' | 'scripter_view' | 'versions';

export function StudyDetailPage() {
    const { id: routeId, projectId: routeProjectId, studyId } = useParams<{ id?: string; projectId?: string; studyId: string }>();
    const projectId = routeProjectId || routeId;
    const navigate = useNavigate();

    const [study, setStudy] = useState<GetStudyDetailsResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [fieldworkMarkets, setFieldworkMarkets] = useState<FieldworkMarket[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<TabType>('general');
    const [questions, setQuestions] = useState<StudyQuestionnaireLine[]>([]);

    // Form state
    const [formData, setFormData] = useState<UpdateStudyRequest>({
        name: '',
        status: 'Draft',
        category: '',
        fieldworkMarketId: '',
        maconomyJobNumber: '',
        projectOperationsUrl: '',
        scripterNotes: ''
    });

    const loadStudy = useCallback(async () => {
        if (!studyId) return;
        try {
            setLoading(true);
            setError(null);
            const [studyData, marketsData, questionsData] = await Promise.all([
                studiesApi.getById(studyId),
                fieldworkMarketsApi.getAll(),
                studiesApi.getQuestions(studyId)
            ]);

            setStudy(studyData);
            setFieldworkMarkets(marketsData);
            setQuestions(questionsData);
            setFormData({
                name: studyData.name,
                status: studyData.status,
                category: studyData.category || '',
                fieldworkMarketId: studyData.fieldworkMarketId || '',
                maconomyJobNumber: studyData.maconomyJobNumber || '',
                projectOperationsUrl: studyData.projectOperationsUrl || '',
                scripterNotes: studyData.scripterNotes || ''
            });
        } catch (err) {
            setError('Failed to load study details');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [studyId]);

    useEffect(() => {
        loadStudy();
    }, [loadStudy]);

    const handleInputChange = (field: keyof UpdateStudyRequest, value: any) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        setSuccessMessage(null);
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!studyId) return;

        try {
            setIsSaving(true);
            await studiesApi.update(studyId, formData);
            setSuccessMessage('Study updated successfully');
            setTimeout(() => setSuccessMessage(null), 5000);

            // Reload to get updated metadata
            const updatedStudy = await studiesApi.getById(studyId);
            setStudy(updatedStudy);
        } catch (err) {
            alert('Failed to save study details');
            console.error(err);
        } finally {
            setIsSaving(false);
        }
    };

    if (loading) return <div className="std-detail-loading">Loading study details...</div>;
    if (error || !study) return <div className="std-detail-error">{error || 'Study not found'}</div>;

    const renderComingSoon = (title: string, icon: React.ReactNode) => (
        <div className="std-coming-soon">
            <div className="std-coming-soon-icon">{icon}</div>
            <h2>{title}</h2>
            <p>This feature is coming soon...</p>
        </div>
    );

    return (
        <div className="std-detail-page">
            {/* Page Header */}
            <div className="std-detail-header">
                <div className="std-header-left">
                    <div className="std-title-block">
                        <h1 className="std-title">
                            {study.name}
                            <span className="std-version-badge">v{study.version}</span>
                        </h1>
                    </div>
                    <span className={`std-status-pill std-status-pill--${study.status.toLowerCase()}`}>
                        {study.status}
                    </span>
                </div>

                <div className="std-header-actions">
                    <button className="std-action-btn" onClick={loadStudy}>
                        <RefreshCw size={14} className={loading ? 'spinning' : ''} />
                        Refresh
                    </button>
                    <button
                        className="std-action-btn std-action-btn--primary"
                        onClick={handleSave}
                        disabled={isSaving || activeTab !== 'general'}
                    >
                        <Save size={14} />
                        {isSaving ? 'Saving...' : 'Save Changes'}
                    </button>
                </div>
            </div>

            {/* Tabs */}
            <div className="std-tabs">
                <button
                    className={`std-tab ${activeTab === 'general' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('general')}
                >
                    General
                </button>
                <button
                    className={`std-tab ${activeTab === 'questions' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('questions')}
                >
                    Questions
                </button>
                <button
                    className={`std-tab ${activeTab === 'snapshots' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('snapshots')}
                >
                    Snapshots
                </button>
                <button
                    className={`std-tab ${activeTab === 'change_log' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('change_log')}
                >
                    Change Log URLs
                </button>
                <button
                    className={`std-tab ${activeTab === 'scripter_view' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('scripter_view')}
                >
                    Scripter View
                </button>
                <button
                    className={`std-tab ${activeTab === 'versions' ? 'std-tab--active' : ''}`}
                    onClick={() => setActiveTab('versions')}
                >
                    Study Versions
                </button>
            </div>

            {/* Tab Content */}
            <div className="std-tab-content">
                {successMessage && (
                    <div className="std-success-banner">
                        <CheckCircle2 size={16} />
                        {successMessage}
                        <button className="std-banner-close" onClick={() => setSuccessMessage(null)}>
                            <X size={14} />
                        </button>
                    </div>
                )}

                {activeTab === 'general' && (
                    <div className="std-general-tab">
                        <form onSubmit={handleSave}>
                            <div className="std-fields-grid">
                                <div className="std-fields-col">
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">
                                            Study Name <span className="std-required">*</span>
                                        </label>
                                        <input
                                            type="text"
                                            className="std-field-input"
                                            value={formData.name}
                                            onChange={(e) => handleInputChange('name', e.target.value)}
                                            required
                                        />
                                    </div>
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">
                                            Category <span className="std-required">*</span>
                                        </label>
                                        <input
                                            type="text"
                                            className="std-field-input"
                                            value={formData.category}
                                            onChange={(e) => handleInputChange('category', e.target.value)}
                                            required
                                        />
                                    </div>
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">Status</label>
                                        <select
                                            className="std-field-select"
                                            value={formData.status}
                                            onChange={(e) => handleInputChange('status', e.target.value)}
                                        >
                                            <option value="Draft">Draft</option>
                                            <option value="Final">Final</option>
                                            <option value="Archived">Archived</option>
                                            <option value="Completed">Completed</option>
                                            <option value="Abandoned">Abandoned</option>
                                        </select>
                                    </div>
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">
                                            Fieldwork Market <span className="std-required">*</span>
                                        </label>
                                        <select
                                            className="std-field-select"
                                            value={formData.fieldworkMarketId}
                                            onChange={(e) => handleInputChange('fieldworkMarketId', e.target.value)}
                                            required
                                        >
                                            <option value="">Select Market...</option>
                                            {fieldworkMarkets.map(m => (
                                                <option key={m.id} value={m.id}>{m.name} ({m.isoCode})</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">
                                            Maconomy Job # <span className="std-required">*</span>
                                        </label>
                                        <input
                                            type="text"
                                            className="std-field-input"
                                            value={formData.maconomyJobNumber}
                                            onChange={(e) => handleInputChange('maconomyJobNumber', e.target.value)}
                                            required
                                        />
                                    </div>
                                    <div className="std-field std-field-horizontal">
                                        <label className="std-field-label">
                                            Project Ops URL <span className="std-required">*</span>
                                        </label>
                                        <div className="std-input-with-icon">
                                            <input
                                                type="url"
                                                className="std-field-input"
                                                value={formData.projectOperationsUrl}
                                                onChange={(e) => handleInputChange('projectOperationsUrl', e.target.value)}
                                                required
                                            />
                                            {formData.projectOperationsUrl && (
                                                <a href={formData.projectOperationsUrl} target="_blank" rel="noopener noreferrer" className="std-input-icon-link">
                                                    <ExternalLink size={14} />
                                                </a>
                                            )}
                                        </div>
                                    </div>
                                    <div className="std-field">
                                        <label className="std-field-label">Scripter Notes</label>
                                        <textarea
                                            className="std-field-textarea"
                                            value={formData.scripterNotes || ''}
                                            onChange={(e) => handleInputChange('scripterNotes', e.target.value)}
                                            rows={8}
                                        />
                                    </div>
                                </div>

                                <div className="std-fields-col">
                                    <div className="std-market-languages">
                                        <div className="std-market-languages-header">
                                            <h3>Fieldwork Market Languages</h3>
                                            <div className="std-market-languages-actions">
                                                <button type="button" className="std-icon-btn" title="Refresh">
                                                    <RefreshCw size={14} />
                                                    <span>Refresh</span>
                                                </button>
                                                <button type="button" className="std-icon-btn" title="Flow">
                                                    <Layers size={14} />
                                                    <span>Flow</span>
                                                </button>
                                            </div>
                                        </div>
                                        <div className="std-market-languages-table-container">
                                            <table className="std-market-languages-table">
                                                <thead>
                                                    <tr>
                                                        <th>Language</th>
                                                        <th>Direction</th>
                                                        <th>Locale Code (Language)</th>
                                                        <th>Language Name (Language)</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <tr>
                                                        <td colSpan={4} className="std-table-empty">
                                                            <div className="std-empty-message">
                                                                We didn't find anything to show here
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                            <div className="std-table-footer">
                                                Rows: 0
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>



                        </form>
                    </div>
                )}

                {activeTab === 'questions' && (
                    <div className="std-questions-tab">
                        <div className="std-questions-toolbar">
                            <div className="std-toolbar-left">
                                <h2 className="std-section-title">Questions</h2>
                            </div>
                            <div className="std-toolbar-right">
                                <button className="std-toolbar-btn">
                                    <Plus size={14} />
                                    New Study - Question...
                                </button>
                                <button className="std-toolbar-btn">
                                    <Plus size={14} />
                                    Add Existing Study - Q...
                                </button>
                                <button className="std-toolbar-btn" onClick={loadStudy}>
                                    <RefreshCw size={14} />
                                </button>
                                <button className="std-toolbar-btn">
                                    <Search size={14} />
                                </button>
                            </div>
                        </div>

                        <div className="std-questions-table-container">
                            <table className="std-questions-table">
                                <thead>
                                    <tr>
                                        <th className="col-check">
                                            <input type="checkbox" />
                                        </th>
                                        <th>Question Variable Name (Questionnaire Line)</th>
                                        <th>Question Type (Questionnaire Line)</th>
                                        <th>Question Text (Questionnaire Line)</th>
                                        <th>Standard or Custom (Questionnaire Line)</th>
                                        <th>Sort Order</th>
                                        <th className="col-actions"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {questions.length === 0 ? (
                                        <tr>
                                            <td colSpan={7} className="std-table-empty">
                                                <div className="std-empty-message">No questions found for this study.</div>
                                            </td>
                                        </tr>
                                    ) : (
                                        questions.map(q => (
                                            <tr key={q.id}>
                                                <td className="col-check">
                                                    <input type="checkbox" />
                                                </td>
                                                <td className="col-bold">{q.variableName}</td>
                                                <td>{q.questionType || '-'}</td>
                                                <td className="col-truncate" title={q.questionText}>{q.questionText || '-'}</td>
                                                <td>{q.classification || 'Standard'}</td>
                                                <td>{q.sortOrder}</td>
                                                <td className="col-actions">
                                                    <button className="std-icon-link">
                                                        <ExternalLink size={14} />
                                                    </button>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                            <div className="std-table-footer">
                                Rows: {questions.length}
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'snapshots' && renderComingSoon('Snapshots', <Clock size={48} />)}
                {activeTab === 'change_log' && renderComingSoon('Change Log URLs', <History size={48} />)}
                {activeTab === 'scripter_view' && renderComingSoon('Scripter View', <Cpu size={48} />)}
                {activeTab === 'versions' && renderComingSoon('Study Versions', <Layers size={48} />)}
            </div>
        </div>
    );
}
