import React, { useState, useEffect } from 'react';
import { Plus, Trash2, GripVertical, ChevronDown, ChevronUp, Info, X } from 'lucide-react';
import { 
    projectQuestionnairesApi, 
    questionBankApi, 
    type ProjectQuestionnaire, 
    type QuestionBankItem 
} from '../services/api';
import './QuestionnaireSection.css';

interface QuestionnaireSectionProps {
    projectId: string;
}

export function QuestionnaireSection({ projectId }: QuestionnaireSectionProps) {
    const [questionnaires, setQuestionnaires] = useState<ProjectQuestionnaire[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
    const [draggedIndex, setDraggedIndex] = useState<number | null>(null);
    const [showImportPanel, setShowImportPanel] = useState(false);
    const [questionBank, setQuestionBank] = useState<QuestionBankItem[]>([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [importLoading, setImportLoading] = useState(false);
    const [selectedQuestion, setSelectedQuestion] = useState<string | null>(null);

    useEffect(() => {
        loadQuestionnaires();
    }, [projectId]);

    const loadQuestionnaires = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await projectQuestionnairesApi.getAll(projectId);
            setQuestionnaires(data);
        } catch (err) {
            setError('Failed to load questionnaires');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const loadQuestionBank = async () => {
        try {
            setImportLoading(true);
            const data = await questionBankApi.getAll();
            setQuestionBank(data);
        } catch (err) {
            console.error('Failed to load question bank', err);
            setQuestionBank([]);
        } finally {
            setImportLoading(false);
        }
    };

    const handleImportQuestion = async () => {
        if (!selectedQuestion) return;

        try {
            const newQuestionnaire = await projectQuestionnairesApi.add(projectId, {
                questionBankItemId: selectedQuestion
            });
            setQuestionnaires([...questionnaires, newQuestionnaire]);
            setSelectedQuestion(null);
            setShowImportPanel(false);
        } catch (err: any) {
            if (err.status === 409) {
                alert('This question has already been added to the project questionnaire.');
            } else {
                alert('Failed to add question. Please try again.');
            }
            console.error('Failed to add question', err);
        }
    };

    const handleDeleteQuestionnaire = async (id: string) => {
        if (!confirm('Are you sure you want to remove this question from the questionnaire?')) {
            return;
        }

        try {
            await projectQuestionnairesApi.delete(projectId, id);
            setQuestionnaires(questionnaires.filter(q => q.id !== id));
        } catch (err) {
            alert('Failed to delete questionnaire item');
            console.error(err);
        }
    };

    const toggleRowExpanded = (id: string) => {
        const newExpanded = new Set(expandedRows);
        if (newExpanded.has(id)) {
            newExpanded.delete(id);
        } else {
            newExpanded.add(id);
        }
        setExpandedRows(newExpanded);
    };

    const handleDragStart = (index: number) => {
        setDraggedIndex(index);
    };

    const handleDragOver = (e: React.DragEvent, index: number) => {
        e.preventDefault();
        if (draggedIndex === null || draggedIndex === index) return;

        const newQuestionnaires = [...questionnaires];
        const draggedItem = newQuestionnaires[draggedIndex];
        newQuestionnaires.splice(draggedIndex, 1);
        newQuestionnaires.splice(index, 0, draggedItem);
        
        setQuestionnaires(newQuestionnaires);
        setDraggedIndex(index);
    };

    const handleDragEnd = async () => {
        if (draggedIndex === null) return;

        // Update sort orders
        const updatedItems = questionnaires.map((q, index) => ({
            id: q.id,
            sortOrder: index
        }));

        try {
            await projectQuestionnairesApi.updateSortOrder(projectId, { items: updatedItems });
            // Reload to get the updated data from server
            await loadQuestionnaires();
        } catch (err) {
            alert('Failed to update sort order');
            console.error(err);
            // Reload to revert changes
            await loadQuestionnaires();
        } finally {
            setDraggedIndex(null);
        }
    };

    const openImportPanel = () => {
        setShowImportPanel(true);
        if (questionBank.length === 0) {
            loadQuestionBank();
        }
    };

    const filteredQuestions = questionBank.filter(q => 
        !searchQuery || 
        q.variableName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        q.questionText?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        q.questionTitle?.toLowerCase().includes(searchQuery.toLowerCase())
    );

    if (loading) {
        return (
            <section className="detail-section">
                <div className="section-content">
                    <p className="loading-text">Loading questionnaires...</p>
                </div>
            </section>
        );
    }

    return (
        <>
            <section className="detail-section">
                <div className="section-header">
                    <h2 className="section-title">Questionnaire Structure</h2>
                    <button className="add-btn" onClick={openImportPanel}>
                        <Plus size={16} />
                        <span>Import from Library</span>
                    </button>
                </div>

                {error && <div className="error-message">{error}</div>}

                <div className="section-content">
                    {questionnaires.length === 0 ? (
                        <div className="empty-state">
                            <p>No questions added yet.</p>
                            <p className="empty-state-hint">Click "Import from Library" to add questions.</p>
                        </div>
                    ) : (
                        <div className="questionnaire-table">
                            <table>
                                <thead>
                                    <tr>
                                        <th className="drag-col"></th>
                                        <th>Variable Name</th>
                                        <th>Version</th>
                                        <th>Question Text</th>
                                        <th>Type</th>
                                        <th>Classification</th>
                                        <th className="actions-col">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {questionnaires.map((q, index) => (
                                        <React.Fragment key={q.id}>
                                            <tr
                                                draggable
                                                onDragStart={() => handleDragStart(index)}
                                                onDragOver={(e) => handleDragOver(e, index)}
                                                onDragEnd={handleDragEnd}
                                                className={draggedIndex === index ? 'dragging' : ''}
                                            >
                                                <td className="drag-col">
                                                    <GripVertical size={16} className="drag-handle" />
                                                </td>
                                                <td>
                                                    <div className="variable-name-cell">
                                                        {q.variableName}
                                                        {q.questionRationale && (
                                                            <div className="info-icon-wrapper">
                                                                <Info size={14} className="info-icon" />
                                                                <div className="info-tooltip">
                                                                    <div className="tooltip-row">
                                                                        <strong>Version:</strong> {q.version}
                                                                    </div>
                                                                    <div className="tooltip-row">
                                                                        <strong>Rationale:</strong> {q.questionRationale}
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        )}
                                                    </div>
                                                </td>
                                                <td>v{q.version}</td>
                                                <td className="question-text-col">
                                                    {q.questionText || '-'}
                                                </td>
                                                <td>{q.questionType || '-'}</td>
                                                <td>
                                                    {q.classification && (
                                                        <span className="classification-badge">
                                                            {q.classification}
                                                        </span>
                                                    )}
                                                </td>
                                                <td className="actions-col">
                                                    <button
                                                        className="icon-btn"
                                                        onClick={() => toggleRowExpanded(q.id)}
                                                        title={expandedRows.has(q.id) ? 'Collapse' : 'Expand'}
                                                    >
                                                        {expandedRows.has(q.id) ? (
                                                            <ChevronUp size={16} />
                                                        ) : (
                                                            <ChevronDown size={16} />
                                                        )}
                                                    </button>
                                                    <button
                                                        className="icon-btn delete-btn"
                                                        onClick={() => handleDeleteQuestionnaire(q.id)}
                                                        title="Remove"
                                                    >
                                                        <Trash2 size={16} />
                                                    </button>
                                                </td>
                                            </tr>
                                            {expandedRows.has(q.id) && (
                                                <tr className="expanded-row">
                                                    <td colSpan={7}>
                                                        <div className="expanded-content">
                                                            <div className="detail-grid">
                                                                <div className="detail-item">
                                                                    <span className="detail-label">Variable Name:</span>
                                                                    <span className="detail-value">{q.variableName}</span>
                                                                </div>
                                                                <div className="detail-item">
                                                                    <span className="detail-label">Version:</span>
                                                                    <span className="detail-value">v{q.version}</span>
                                                                </div>
                                                                <div className="detail-item">
                                                                    <span className="detail-label">Question Type:</span>
                                                                    <span className="detail-value">{q.questionType || '-'}</span>
                                                                </div>
                                                                <div className="detail-item">
                                                                    <span className="detail-label">Classification:</span>
                                                                    <span className="detail-value">{q.classification || '-'}</span>
                                                                </div>
                                                                <div className="detail-item full-width">
                                                                    <span className="detail-label">Question Text:</span>
                                                                    <span className="detail-value">{q.questionText || '-'}</span>
                                                                </div>
                                                                {q.questionTitle && (
                                                                    <div className="detail-item full-width">
                                                                        <span className="detail-label">Question Title:</span>
                                                                        <span className="detail-value">{q.questionTitle}</span>
                                                                    </div>
                                                                )}
                                                                {q.questionRationale && (
                                                                    <div className="detail-item full-width">
                                                                        <span className="detail-label">Rationale:</span>
                                                                        <span className="detail-value">{q.questionRationale}</span>
                                                                    </div>
                                                                )}
                                                            </div>
                                                        </div>
                                                    </td>
                                                </tr>
                                            )}
                                        </React.Fragment>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </section>

            {/* Import Panel */}
            {showImportPanel && (
                <div className="side-panel-overlay" onClick={() => setShowImportPanel(false)}>
                    <div className="side-panel" onClick={(e) => e.stopPropagation()}>
                        <div className="side-panel-header">
                            <h2>Import from Library</h2>
                            <button className="close-btn" onClick={() => setShowImportPanel(false)}>
                                <X size={20} />
                            </button>
                        </div>
                        <div className="side-panel-content">
                            <div className="search-box">
                                <input
                                    type="text"
                                    placeholder="Search questions..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="search-input"
                                />
                            </div>
                            {importLoading ? (
                                <p className="loading-text">Loading questions...</p>
                            ) : (
                                <div className="question-list">
                                    {filteredQuestions.length === 0 ? (
                                        <p className="empty-state-text">No questions found</p>
                                    ) : (
                                        filteredQuestions.map((q) => (
                                            <div
                                                key={q.id}
                                                className={`question-item ${selectedQuestion === q.id ? 'selected' : ''}`}
                                                onClick={() => setSelectedQuestion(q.id)}
                                            >
                                                <div className="question-item-header">
                                                    <span className="variable-name">{q.variableName}</span>
                                                    <span className="version-badge">v{q.version}</span>
                                                </div>
                                                {q.questionText && (
                                                    <div className="question-text">{q.questionText}</div>
                                                )}
                                                <div className="question-meta">
                                                    {q.questionType && <span className="meta-tag">{q.questionType}</span>}
                                                    {q.classification && <span className="meta-tag">{q.classification}</span>}
                                                </div>
                                            </div>
                                        ))
                                    )}
                                </div>
                            )}
                        </div>
                        <div className="side-panel-footer">
                            <button 
                                className="cancel-btn" 
                                onClick={() => setShowImportPanel(false)}
                            >
                                Cancel
                            </button>
                            <button
                                className="save-btn"
                                onClick={handleImportQuestion}
                                disabled={!selectedQuestion}
                            >
                                <Plus size={16} />
                                <span>Add Question</span>
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
