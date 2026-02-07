import { useState, useEffect } from 'react';
import {
    configurationQuestionsApi,
    configurationAnswersApi,
    dependencyRulesApi,
    type ConfigurationQuestionDetail,
    type ConfigurationAnswer,
    type DependencyRule
} from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ConfigurationQuestionsPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';
type AnswerMode = 'none' | 'create' | 'edit';
type RuleMode = 'none' | 'create' | 'edit';

export function ConfigurationQuestionsPage() {
    const [questions, setQuestions] = useState<ConfigurationQuestionDetail[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedQuestion, setSelectedQuestion] = useState<ConfigurationQuestionDetail | null>(null);
    const [formData, setFormData] = useState({
        question: '',
        aiPrompt: '',
        ruleType: 'SingleCoded' as 'SingleCoded' | 'MultiCoded',
        isActive: true
    });

    // Answer management
    const [answerMode, setAnswerMode] = useState<AnswerMode>('none');
    const [selectedAnswer, setSelectedAnswer] = useState<ConfigurationAnswer | null>(null);
    const [answerFormData, setAnswerFormData] = useState({
        name: '',
        isActive: true
    });

    // Dependency Rule management
    const [ruleMode, setRuleMode] = useState<RuleMode>('none');
    const [selectedRule, setSelectedRule] = useState<DependencyRule | null>(null);
    const [ruleFormData, setRuleFormData] = useState({
        name: '',
        triggeringAnswerId: '',
        classification: '',
        type: '',
        contentType: '',
        module: '',
        questionBank: '',
        tag: '',
        statusReason: '',
        isActive: true
    });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchQuestions();
    }, [search]);

    const fetchQuestions = async () => {
        setIsLoading(true);
        try {
            const data = await configurationQuestionsApi.getAll(search);
            setQuestions(data);
        } catch (error) {
            console.error('Failed to fetch configuration questions', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Configuration Question Actions ---

    const openCreate = () => {
        setSelectedQuestion(null);
        setFormData({ question: '', aiPrompt: '', ruleType: 'SingleCoded', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
    };

    const openView = async (question: ConfigurationQuestionDetail) => {
        setIsLoading(true);
        try {
            // Fetch full details including answers and rules
            const fullQuestion = await configurationQuestionsApi.getById(question.id);
            setSelectedQuestion(fullQuestion);
            setMode('view');
        } catch (error) {
            console.error('Failed to fetch question details', error);
        } finally {
            setIsLoading(false);
        }
    };

    const openEdit = (question?: ConfigurationQuestionDetail) => {
        const target = question || selectedQuestion;
        if (!target) return;

        if (question) setSelectedQuestion(question);

        setFormData({
            question: target.question,
            aiPrompt: target.aiPrompt || '',
            ruleType: target.ruleType,
            isActive: target.isActive
        });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedQuestion(null);
        setAnswerMode('none');
        setRuleMode('none');
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedQuestion: ConfigurationQuestionDetail;
            if (mode === 'edit' && selectedQuestion) {
                savedQuestion = await configurationQuestionsApi.update(selectedQuestion.id, {
                    question: formData.question,
                    aiPrompt: formData.aiPrompt || undefined,
                    ruleType: formData.ruleType,
                    isActive: formData.isActive
                });
            } else {
                savedQuestion = await configurationQuestionsApi.create({
                    question: formData.question,
                    aiPrompt: formData.aiPrompt || undefined,
                    ruleType: formData.ruleType
                });
            }

            await fetchQuestions();
            const fullQuestion = await configurationQuestionsApi.getById(savedQuestion.id);
            setSelectedQuestion(fullQuestion);
            setMode('view');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]>; detail?: string };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else if (error.status === 409) {
                setServerError(error.detail || "Configuration question already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (question?: ConfigurationQuestionDetail) => {
        const target = question || selectedQuestion;
        if (!target || !confirm(`Are you sure you want to delete configuration question '${target.question}'?`)) return;

        try {
            await configurationQuestionsApi.delete(target.id);
            closePanel();
            fetchQuestions();
        } catch (error) {
            console.error('Failed to delete configuration question', error);
        }
    };

    // --- Answer Management ---

    const openAnswerCreate = () => {
        setAnswerFormData({ name: '', isActive: true });
        setSelectedAnswer(null);
        setAnswerMode('create');
    };

    const openAnswerEdit = (answer: ConfigurationAnswer) => {
        setAnswerFormData({
            name: answer.name,
            isActive: answer.isActive
        });
        setSelectedAnswer(answer);
        setAnswerMode('edit');
    };

    const handleAnswerSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedQuestion) return;

        setErrors({});
        setServerError('');

        try {
            if (answerMode === 'edit' && selectedAnswer) {
                await configurationAnswersApi.update(selectedAnswer.id, {
                    name: answerFormData.name,
                    isActive: answerFormData.isActive
                });
            } else {
                await configurationAnswersApi.create({
                    name: answerFormData.name,
                    configurationQuestionId: selectedQuestion.id
                });
            }

            // Refresh question details
            const updatedQuestion = await configurationQuestionsApi.getById(selectedQuestion.id);
            setSelectedQuestion(updatedQuestion);
            setAnswerMode('none');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleAnswerDelete = async (answer: ConfigurationAnswer) => {
        if (!confirm(`Are you sure you want to delete answer '${answer.name}'?`)) return;

        try {
            await configurationAnswersApi.delete(answer.id);
            if (selectedQuestion) {
                const updatedQuestion = await configurationQuestionsApi.getById(selectedQuestion.id);
                setSelectedQuestion(updatedQuestion);
            }
        } catch (error) {
            console.error('Failed to delete answer', error);
        }
    };

    // --- Dependency Rule Management ---

    const openRuleCreate = () => {
        setRuleFormData({
            name: '',
            triggeringAnswerId: '',
            classification: '',
            type: '',
            contentType: '',
            module: '',
            questionBank: '',
            tag: '',
            statusReason: '',
            isActive: true
        });
        setSelectedRule(null);
        setRuleMode('create');
    };

    const openRuleEdit = (rule: DependencyRule) => {
        setRuleFormData({
            name: rule.name,
            triggeringAnswerId: rule.triggeringAnswerId || '',
            classification: rule.classification || '',
            type: rule.type || '',
            contentType: rule.contentType || '',
            module: rule.module || '',
            questionBank: rule.questionBank || '',
            tag: rule.tag || '',
            statusReason: rule.statusReason || '',
            isActive: rule.isActive
        });
        setSelectedRule(rule);
        setRuleMode('edit');
    };

    const handleRuleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedQuestion) return;

        setErrors({});
        setServerError('');

        try {
            if (ruleMode === 'edit' && selectedRule) {
                await dependencyRulesApi.update(selectedRule.id, {
                    name: ruleFormData.name,
                    triggeringAnswerId: ruleFormData.triggeringAnswerId || undefined,
                    classification: ruleFormData.classification || undefined,
                    type: ruleFormData.type || undefined,
                    contentType: ruleFormData.contentType || undefined,
                    module: ruleFormData.module || undefined,
                    questionBank: ruleFormData.questionBank || undefined,
                    tag: ruleFormData.tag || undefined,
                    statusReason: ruleFormData.statusReason || undefined,
                    isActive: ruleFormData.isActive
                });
            } else {
                await dependencyRulesApi.create({
                    name: ruleFormData.name,
                    configurationQuestionId: selectedQuestion.id,
                    triggeringAnswerId: ruleFormData.triggeringAnswerId || undefined,
                    classification: ruleFormData.classification || undefined,
                    type: ruleFormData.type || undefined,
                    contentType: ruleFormData.contentType || undefined,
                    module: ruleFormData.module || undefined,
                    questionBank: ruleFormData.questionBank || undefined,
                    tag: ruleFormData.tag || undefined
                });
            }

            // Refresh question details
            const updatedQuestion = await configurationQuestionsApi.getById(selectedQuestion.id);
            setSelectedQuestion(updatedQuestion);
            setRuleMode('none');

        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleRuleDelete = async (rule: DependencyRule) => {
        if (!confirm(`Are you sure you want to delete rule '${rule.name}'?`)) return;

        try {
            await dependencyRulesApi.delete(rule.id);
            if (selectedQuestion) {
                const updatedQuestion = await configurationQuestionsApi.getById(selectedQuestion.id);
                setSelectedQuestion(updatedQuestion);
            }
        } catch (error) {
            console.error('Failed to delete rule', error);
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
                <button className="cmd-btn" onClick={fetchQuestions}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search configuration questions..."
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
                                <th>Question</th>
                                <th style={{ width: '150px' }}>Rule Type</th>
                                <th style={{ width: '100px' }}>Answers</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {questions.map((question) => (
                                <tr key={question.id} onClick={() => openView(question)} className="clickable-row">
                                    <td>{question.question}</td>
                                    <td>{question.ruleType === 'SingleCoded' ? 'Single Coded' : 'Multi Coded'}</td>
                                    <td>{question.answersCount || 0}</td>
                                    <td>
                                        <span className={`status-text ${question.isActive ? 'active' : 'inactive'}`}>
                                            {question.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(question); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(question); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(question); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {questions.length === 0 && (
                                <tr><td colSpan={5} className="empty-state">No configuration questions found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Configuration Question' : mode === 'edit' ? 'Edit Configuration Question' : selectedQuestion?.question || 'Configuration Question Details'}
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" type="submit" form="configuration-questions-form">Save</button>
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
                {serverError && <div className="error-message">{serverError}</div>}

                {/* View Mode */}
                {mode === 'view' && selectedQuestion && (
                    <div className="view-details">
                        <div className="detail-section">
                            <h3>General</h3>
                            <div className="detail-item">
                                <label>Question</label>
                                <div className="value">{selectedQuestion.question}</div>
                            </div>
                            <div className="detail-item">
                                <label>Rule Type</label>
                                <div className="value">{selectedQuestion.ruleType === 'SingleCoded' ? 'Single Coded' : 'Multi Coded'}</div>
                            </div>
                            {selectedQuestion.aiPrompt && (
                                <div className="detail-item">
                                    <label>AI Prompt</label>
                                    <div className="value">{selectedQuestion.aiPrompt}</div>
                                </div>
                            )}
                            <div className="detail-item">
                                <label>Status</label>
                                <div className="value">{selectedQuestion.isActive ? 'Active' : 'Inactive'}</div>
                            </div>
                            <div className="detail-item">
                                <label>Version</label>
                                <div className="value">{selectedQuestion.version}</div>
                            </div>
                        </div>

                        {/* Answers Section */}
                        <div className="detail-section">
                            <div className="section-header">
                                <h3>Answers</h3>
                                <button className="btn-link" onClick={openAnswerCreate}>
                                    <PlusIcon /> Add Answer
                                </button>
                            </div>

                            {answerMode === 'none' ? (
                                <table className="sub-details-list">
                                    <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th style={{ width: '100px' }}>Status</th>
                                            <th style={{ width: '100px' }}>Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {selectedQuestion.answers && selectedQuestion.answers.length > 0 ? (
                                            selectedQuestion.answers.map((answer) => (
                                                <tr key={answer.id}>
                                                    <td>{answer.name}</td>
                                                    <td>
                                                        <span className={`status-text ${answer.isActive ? 'active' : 'inactive'}`}>
                                                            {answer.isActive ? 'Active' : 'Inactive'}
                                                        </span>
                                                    </td>
                                                    <td>
                                                        <div className="row-actions">
                                                            <button className="action-btn" onClick={() => openAnswerEdit(answer)} title="Edit">
                                                                <EditIcon />
                                                            </button>
                                                            <button className="action-btn danger" onClick={() => handleAnswerDelete(answer)} title="Delete">
                                                                <TrashIcon />
                                                            </button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            ))
                                        ) : (
                                            <tr><td colSpan={3} className="empty-state">No answers defined.</td></tr>
                                        )}
                                    </tbody>
                                </table>
                            ) : (
                                <form className="sub-form" onSubmit={handleAnswerSubmit}>
                                    <div className="form-field">
                                        <label htmlFor="answerName">Name</label>
                                        <input
                                            id="answerName"
                                            type="text"
                                            value={answerFormData.name}
                                            onChange={(e) => setAnswerFormData({ ...answerFormData, name: e.target.value })}
                                            className={errors.Name ? 'error' : ''}
                                            autoFocus
                                        />
                                        {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                                    </div>

                                    {answerMode === 'edit' && (
                                        <div className="form-field checkbox">
                                            <label>
                                                <input
                                                    type="checkbox"
                                                    checked={answerFormData.isActive}
                                                    onChange={(e) => setAnswerFormData({ ...answerFormData, isActive: e.target.checked })}
                                                />
                                                <span>Is Active</span>
                                            </label>
                                        </div>
                                    )}

                                    <div className="form-actions">
                                        <button type="submit" className="btn primary">Save Answer</button>
                                        <button type="button" className="btn" onClick={() => setAnswerMode('none')}>Cancel</button>
                                    </div>
                                </form>
                            )}
                        </div>

                        {/* Dependency Rules Section */}
                        <div className="detail-section">
                            <div className="section-header">
                                <h3>Dependency Rules</h3>
                                <button className="btn-link" onClick={openRuleCreate}>
                                    <PlusIcon /> Add Rule
                                </button>
                            </div>

                            {ruleMode === 'none' ? (
                                <table className="sub-details-list">
                                    <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th>Triggering Answer</th>
                                            <th>Classification</th>
                                            <th style={{ width: '100px' }}>Status</th>
                                            <th style={{ width: '100px' }}>Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {selectedQuestion.dependencyRules && selectedQuestion.dependencyRules.length > 0 ? (
                                            selectedQuestion.dependencyRules.map((rule) => (
                                                <tr key={rule.id}>
                                                    <td>{rule.name}</td>
                                                    <td>{rule.triggeringAnswerName || '-'}</td>
                                                    <td>{rule.classification || '-'}</td>
                                                    <td>
                                                        <span className={`status-text ${rule.isActive ? 'active' : 'inactive'}`}>
                                                            {rule.isActive ? 'Active' : 'Inactive'}
                                                        </span>
                                                    </td>
                                                    <td>
                                                        <div className="row-actions">
                                                            <button className="action-btn" onClick={() => openRuleEdit(rule)} title="Edit">
                                                                <EditIcon />
                                                            </button>
                                                            <button className="action-btn danger" onClick={() => handleRuleDelete(rule)} title="Delete">
                                                                <TrashIcon />
                                                            </button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            ))
                                        ) : (
                                            <tr><td colSpan={5} className="empty-state">No dependency rules defined.</td></tr>
                                        )}
                                    </tbody>
                                </table>
                            ) : (
                                <form className="sub-form" onSubmit={handleRuleSubmit}>
                                    <div className="form-field">
                                        <label htmlFor="ruleName">Name</label>
                                        <input
                                            id="ruleName"
                                            type="text"
                                            value={ruleFormData.name}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, name: e.target.value })}
                                            className={errors.Name ? 'error' : ''}
                                            autoFocus
                                        />
                                        {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="triggeringAnswer">Triggering Answer</label>
                                        <select
                                            id="triggeringAnswer"
                                            value={ruleFormData.triggeringAnswerId}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, triggeringAnswerId: e.target.value })}
                                        >
                                            <option value="">-- Select Answer --</option>
                                            {selectedQuestion.answers?.map((answer) => (
                                                <option key={answer.id} value={answer.id}>{answer.name}</option>
                                            ))}
                                        </select>
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="classification">Classification</label>
                                        <input
                                            id="classification"
                                            type="text"
                                            value={ruleFormData.classification}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, classification: e.target.value })}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="ruleType">Type</label>
                                        <input
                                            id="ruleType"
                                            type="text"
                                            value={ruleFormData.type}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, type: e.target.value })}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="contentType">Content Type</label>
                                        <input
                                            id="contentType"
                                            type="text"
                                            value={ruleFormData.contentType}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, contentType: e.target.value })}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="module">Module</label>
                                        <input
                                            id="module"
                                            type="text"
                                            value={ruleFormData.module}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, module: e.target.value })}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="questionBank">Question Bank</label>
                                        <input
                                            id="questionBank"
                                            type="text"
                                            value={ruleFormData.questionBank}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, questionBank: e.target.value })}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <label htmlFor="ruleTag">Tag</label>
                                        <input
                                            id="ruleTag"
                                            type="text"
                                            value={ruleFormData.tag}
                                            onChange={(e) => setRuleFormData({ ...ruleFormData, tag: e.target.value })}
                                        />
                                    </div>

                                    {ruleMode === 'edit' && (
                                        <>
                                            <div className="form-field">
                                                <label htmlFor="ruleStatusReason">Status Reason</label>
                                                <input
                                                    id="ruleStatusReason"
                                                    type="text"
                                                    value={ruleFormData.statusReason}
                                                    onChange={(e) => setRuleFormData({ ...ruleFormData, statusReason: e.target.value })}
                                                />
                                            </div>
                                            <div className="form-field checkbox">
                                                <label>
                                                    <input
                                                        type="checkbox"
                                                        checked={ruleFormData.isActive}
                                                        onChange={(e) => setRuleFormData({ ...ruleFormData, isActive: e.target.checked })}
                                                    />
                                                    <span>Is Active</span>
                                                </label>
                                            </div>
                                        </>
                                    )}

                                    <div className="form-actions">
                                        <button type="submit" className="btn primary">Save Rule</button>
                                        <button type="button" className="btn" onClick={() => setRuleMode('none')}>Cancel</button>
                                    </div>
                                </form>
                            )}
                        </div>
                    </div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form id="configuration-questions-form" className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="question">Question</label>
                            <input
                                id="question"
                                type="text"
                                value={formData.question}
                                onChange={(e) => setFormData({ ...formData, question: e.target.value })}
                                className={errors.Question ? 'error' : ''}
                                autoFocus
                            />
                            {errors.Question && <span className="field-error">{errors.Question[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="ruleType">Rule Type</label>
                            <select
                                id="ruleType"
                                value={formData.ruleType}
                                onChange={(e) => setFormData({ ...formData, ruleType: e.target.value as 'SingleCoded' | 'MultiCoded' })}
                            >
                                <option value="SingleCoded">Single Coded</option>
                                <option value="MultiCoded">Multi Coded</option>
                            </select>
                        </div>

                        <div className="form-field">
                            <label htmlFor="aiPrompt">AI Prompt (Optional)</label>
                            <textarea
                                id="aiPrompt"
                                value={formData.aiPrompt}
                                onChange={(e) => setFormData({ ...formData, aiPrompt: e.target.value })}
                                rows={4}
                                placeholder="Provide an optional AI prompt to assist users with their selection..."
                            />
                        </div>

                        {mode === 'edit' && (
                            <>
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
                            </>
                        )}
                    </form>
                )}
            </SidePanel>
        </div>
    );
}
