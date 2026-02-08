import { useState, useEffect } from 'react';
import {
    questionBankApi,
    questionAnswerApi,
    type QuestionBankItem,
    type QuestionBankItemDetail,
    type QuestionAnswer
} from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './QuestionBankPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';
type ActiveTab = 'question' | 'answers' | 'admin' | 'related';
type AnswerMode = 'none' | 'create' | 'edit';

export function QuestionBankPage() {
    const [questions, setQuestions] = useState<QuestionBankItem[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [activeTab, setActiveTab] = useState<ActiveTab>('question');
    const [selectedQuestion, setSelectedQuestion] = useState<QuestionBankItemDetail | null>(null);
    const [formData, setFormData] = useState({
        variableName: '',
        version: 1,
        questionType: '',
        questionText: '',
        classification: '',
        isDummy: false,
        questionTitle: '',
        status: '',
        methodology: '',
        dataQualityTag: '',
        rowSortOrder: null as number | null,
        columnSortOrder: null as number | null,
        answerMin: null as number | null,
        answerMax: null as number | null,
        questionFormatDetails: '',
        scraperNotes: '',
        customNotes: '',
        metricGroup: '',
        tableTitle: '',
        questionRationale: '',
        singleOrMulticode: '',
        managedListReferences: '',
        isTranslatable: false,
        isHidden: false,
        isQuestionActive: true,
        isQuestionOutOfUse: false,
        answerRestrictionMin: null as number | null,
        answerRestrictionMax: null as number | null,
        restrictionDataType: '',
        restrictedToClient: '',
        answerTypeCode: '',
        isAnswerRequired: false,
        scalePoint: '',
        scaleType: '',
        displayType: '',
        instructionText: '',
        parentQuestionId: '',
        questionFacet: ''
    });

    // Answer management
    const [answerMode, setAnswerMode] = useState<AnswerMode>('none');
    const [selectedAnswer, setSelectedAnswer] = useState<QuestionAnswer | null>(null);
    const [answerFormData, setAnswerFormData] = useState({
        answerText: '',
        answerCode: '',
        answerLocation: '',
        isOpen: false,
        isFixed: false,
        isExclusive: false,
        isActive: true,
        customProperty: '',
        facets: '',
        version: 1,
        displayOrder: null as number | null
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
            const data = await questionBankApi.getAll(search);
            setQuestions(data);
        } catch (error) {
            console.error('Failed to fetch question bank items', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Question Actions ---

    const openCreate = () => {
        setSelectedQuestion(null);
        setFormData({
            variableName: '',
            version: 1,
            questionType: '',
            questionText: '',
            classification: '',
            isDummy: false,
            questionTitle: '',
            status: '',
            methodology: '',
            dataQualityTag: '',
            rowSortOrder: null,
            columnSortOrder: null,
            answerMin: null,
            answerMax: null,
            questionFormatDetails: '',
            scraperNotes: '',
            customNotes: '',
            metricGroup: '',
            tableTitle: '',
            questionRationale: '',
            singleOrMulticode: '',
            managedListReferences: '',
            isTranslatable: false,
            isHidden: false,
            isQuestionActive: true,
            isQuestionOutOfUse: false,
            answerRestrictionMin: null,
            answerRestrictionMax: null,
            restrictionDataType: '',
            restrictedToClient: '',
            answerTypeCode: '',
            isAnswerRequired: false,
            scalePoint: '',
            scaleType: '',
            displayType: '',
            instructionText: '',
            parentQuestionId: '',
            questionFacet: ''
        });
        setErrors({});
        setServerError('');
        setActiveTab('question');
        setMode('create');
    };

    const openView = async (question: QuestionBankItem) => {
        setIsLoading(true);
        try {
            const fullQuestion = await questionBankApi.getById(question.id);
            setSelectedQuestion(fullQuestion);
            setActiveTab('question');
            setMode('view');
        } catch (error) {
            console.error('Failed to fetch question details', error);
        } finally {
            setIsLoading(false);
        }
    };

    const openEdit = (question?: QuestionBankItemDetail) => {
        const target = question || selectedQuestion;
        if (!target) return;

        if (question) setSelectedQuestion(question);

        setFormData({
            variableName: target.variableName,
            version: target.version,
            questionType: target.questionType || '',
            questionText: target.questionText || '',
            classification: target.classification || '',
            isDummy: target.isDummy,
            questionTitle: target.questionTitle || '',
            status: target.status || '',
            methodology: target.methodology || '',
            dataQualityTag: target.dataQualityTag || '',
            rowSortOrder: target.rowSortOrder,
            columnSortOrder: target.columnSortOrder,
            answerMin: target.answerMin,
            answerMax: target.answerMax,
            questionFormatDetails: target.questionFormatDetails || '',
            scraperNotes: target.scraperNotes || '',
            customNotes: target.customNotes || '',
            metricGroup: target.metricGroup || '',
            tableTitle: target.tableTitle || '',
            questionRationale: target.questionRationale || '',
            singleOrMulticode: target.singleOrMulticode || '',
            managedListReferences: target.managedListReferences || '',
            isTranslatable: target.isTranslatable,
            isHidden: target.isHidden,
            isQuestionActive: target.isQuestionActive,
            isQuestionOutOfUse: target.isQuestionOutOfUse,
            answerRestrictionMin: target.answerRestrictionMin,
            answerRestrictionMax: target.answerRestrictionMax,
            restrictionDataType: target.restrictionDataType || '',
            restrictedToClient: target.restrictedToClient || '',
            answerTypeCode: target.answerTypeCode || '',
            isAnswerRequired: target.isAnswerRequired,
            scalePoint: target.scalePoint || '',
            scaleType: target.scaleType || '',
            displayType: target.displayType || '',
            instructionText: target.instructionText || '',
            parentQuestionId: target.parentQuestionId || '',
            questionFacet: target.questionFacet || ''
        });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedQuestion(null);
        setActiveTab('question');
        setAnswerMode('none');
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            const payload = {
                variableName: formData.variableName,
                version: formData.version,
                questionType: formData.questionType || null,
                questionText: formData.questionText || null,
                classification: formData.classification || null,
                isDummy: formData.isDummy,
                questionTitle: formData.questionTitle || null,
                status: formData.status || null,
                methodology: formData.methodology || null,
                dataQualityTag: formData.dataQualityTag || null,
                rowSortOrder: formData.rowSortOrder,
                columnSortOrder: formData.columnSortOrder,
                answerMin: formData.answerMin,
                answerMax: formData.answerMax,
                questionFormatDetails: formData.questionFormatDetails || null,
                scraperNotes: formData.scraperNotes || null,
                customNotes: formData.customNotes || null,
                metricGroup: formData.metricGroup || null,
                tableTitle: formData.tableTitle || null,
                questionRationale: formData.questionRationale || null,
                singleOrMulticode: formData.singleOrMulticode || null,
                managedListReferences: formData.managedListReferences || null,
                isTranslatable: formData.isTranslatable,
                isHidden: formData.isHidden,
                isQuestionActive: formData.isQuestionActive,
                isQuestionOutOfUse: formData.isQuestionOutOfUse,
                answerRestrictionMin: formData.answerRestrictionMin,
                answerRestrictionMax: formData.answerRestrictionMax,
                restrictionDataType: formData.restrictionDataType || null,
                restrictedToClient: formData.restrictedToClient || null,
                answerTypeCode: formData.answerTypeCode || null,
                isAnswerRequired: formData.isAnswerRequired,
                scalePoint: formData.scalePoint || null,
                scaleType: formData.scaleType || null,
                displayType: formData.displayType || null,
                instructionText: formData.instructionText || null,
                parentQuestionId: formData.parentQuestionId || null,
                questionFacet: formData.questionFacet || null
            };

            if (mode === 'create') {
                await questionBankApi.create(payload);
                await fetchQuestions();
                closePanel();
            } else if (mode === 'edit' && selectedQuestion) {
                await questionBankApi.update(selectedQuestion.id, payload);
                const updatedQuestion = await questionBankApi.getById(selectedQuestion.id);
                setSelectedQuestion(updatedQuestion);
                setMode('view');
            }
        } catch (err: unknown) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                setErrors(error.errors);
            } else if (error.status === 409) {
                setServerError("A question with this variable name and version already exists.");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (question?: QuestionBankItem) => {
        const target = question || selectedQuestion;
        if (!target) return;

        if (!confirm(`Are you sure you want to delete question '${target.variableName} v${target.version}'?`)) return;

        try {
            await questionBankApi.delete(target.id);
            await fetchQuestions();
            if (!question) closePanel();
        } catch (error) {
            console.error('Failed to delete question', error);
            setServerError("Failed to delete question.");
        }
    };

    // --- Answer Management ---

    const openAnswerCreate = () => {
        setAnswerFormData({
            answerText: '',
            answerCode: '',
            answerLocation: '',
            isOpen: false,
            isFixed: false,
            isExclusive: false,
            isActive: true,
            customProperty: '',
            facets: '',
            version: 1,
            displayOrder: null
        });
        setSelectedAnswer(null);
        setAnswerMode('create');
        setErrors({});
    };

    const openAnswerEdit = (answer: QuestionAnswer) => {
        setAnswerFormData({
            answerText: answer.answerText,
            answerCode: answer.answerCode || '',
            answerLocation: answer.answerLocation || '',
            isOpen: answer.isOpen,
            isFixed: answer.isFixed,
            isExclusive: answer.isExclusive,
            isActive: answer.isActive,
            customProperty: answer.customProperty || '',
            facets: answer.facets || '',
            version: answer.version,
            displayOrder: answer.displayOrder
        });
        setSelectedAnswer(answer);
        setAnswerMode('edit');
        setErrors({});
    };

    const handleAnswerSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedQuestion) return;

        setErrors({});
        setServerError('');

        try {
            const payload = {
                answerText: answerFormData.answerText,
                answerCode: answerFormData.answerCode || null,
                answerLocation: answerFormData.answerLocation || null,
                isOpen: answerFormData.isOpen,
                isFixed: answerFormData.isFixed,
                isExclusive: answerFormData.isExclusive,
                isActive: answerFormData.isActive,
                customProperty: answerFormData.customProperty || null,
                facets: answerFormData.facets || null,
                version: answerFormData.version,
                displayOrder: answerFormData.displayOrder
            };

            if (answerMode === 'edit' && selectedAnswer) {
                await questionAnswerApi.update(selectedQuestion.id, selectedAnswer.id, payload);
            } else {
                await questionAnswerApi.create(selectedQuestion.id, payload);
            }

            const updatedQuestion = await questionBankApi.getById(selectedQuestion.id);
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

    const handleAnswerDelete = async (answer: QuestionAnswer) => {
        if (!selectedQuestion) return;
        if (!confirm(`Are you sure you want to delete answer '${answer.answerText}'?`)) return;

        try {
            await questionAnswerApi.delete(selectedQuestion.id, answer.id);
            const updatedQuestion = await questionBankApi.getById(selectedQuestion.id);
            setSelectedQuestion(updatedQuestion);
        } catch (error) {
            console.error('Failed to delete answer', error);
        }
    };

    // --- Renders ---

    const renderQuestionTab = () => {
        if (mode === 'view' && selectedQuestion) {
            return (
                <div className="view-details">
                    <div className="detail-section">
                        <h3>Basic Information</h3>
                        <div className="detail-item">
                            <label>Variable Name</label>
                            <div className="value">{selectedQuestion.variableName}</div>
                        </div>
                        <div className="detail-item">
                            <label>Version</label>
                            <div className="value">{selectedQuestion.version}</div>
                        </div>
                        <div className="detail-item">
                            <label>Status</label>
                            <div className="value">{selectedQuestion.status || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Question Type</label>
                            <div className="value">{selectedQuestion.questionType || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Classification</label>
                            <div className="value">{selectedQuestion.classification || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Question Title</label>
                            <div className="value">{selectedQuestion.questionTitle || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Is Dummy Question</label>
                            <div className="value">{selectedQuestion.isDummy ? 'Yes' : 'No'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Question Content</h3>
                        <div className="detail-item">
                            <label>Question Text</label>
                            <div className="value text-content">{selectedQuestion.questionText || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Data Quality Tag</label>
                            <div className="value">{selectedQuestion.dataQualityTag || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Methodology</label>
                            <div className="value">{selectedQuestion.methodology || '-'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Sort Orders & Ranges</h3>
                        <div className="detail-item">
                            <label>Row Sort Order</label>
                            <div className="value">{selectedQuestion.rowSortOrder ?? '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Column Sort Order</label>
                            <div className="value">{selectedQuestion.columnSortOrder ?? '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Answer Min</label>
                            <div className="value">{selectedQuestion.answerMin ?? '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Answer Max</label>
                            <div className="value">{selectedQuestion.answerMax ?? '-'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Additional Details</h3>
                        <div className="detail-item">
                            <label>Format Details</label>
                            <div className="value text-content">{selectedQuestion.questionFormatDetails || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Scraper Notes</label>
                            <div className="value text-content">{selectedQuestion.scraperNotes || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Custom Notes</label>
                            <div className="value text-content">{selectedQuestion.customNotes || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Metric Group</label>
                            <div className="value">{selectedQuestion.metricGroup || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Table Title</label>
                            <div className="value">{selectedQuestion.tableTitle || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Question Rationale</label>
                            <div className="value text-content">{selectedQuestion.questionRationale || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Single or Multicode</label>
                            <div className="value">{selectedQuestion.singleOrMulticode || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Managed List References</label>
                            <div className="value">{selectedQuestion.managedListReferences || '-'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Metadata</h3>
                        <div className="detail-item">
                            <label>Created On</label>
                            <div className="value">{new Date(selectedQuestion.createdOn).toLocaleString()}</div>
                        </div>
                        <div className="detail-item">
                            <label>Created By</label>
                            <div className="value">{selectedQuestion.createdBy || '-'}</div>
                        </div>
                        {selectedQuestion.modifiedOn && (
                            <>
                                <div className="detail-item">
                                    <label>Modified On</label>
                                    <div className="value">{new Date(selectedQuestion.modifiedOn).toLocaleString()}</div>
                                </div>
                                <div className="detail-item">
                                    <label>Modified By</label>
                                    <div className="value">{selectedQuestion.modifiedBy || '-'}</div>
                                </div>
                            </>
                        )}
                    </div>
                </div>
            );
        }

        if (mode === 'create' || mode === 'edit') {
            return (
                <form id="question-bank-form" className="panel-form" onSubmit={handleSubmit}>
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
                        <label htmlFor="version">Version *</label>
                        <input
                            id="version"
                            type="number"
                            value={formData.version}
                            onChange={(e) => setFormData({ ...formData, version: parseInt(e.target.value) || 1 })}
                            className={errors.Version ? 'error' : ''}
                            min="1"
                        />
                        {errors.Version && <span className="field-error">{errors.Version[0]}</span>}
                    </div>

                    <div className="form-field">
                        <label htmlFor="status">Status</label>
                        <select
                            id="status"
                            value={formData.status}
                            onChange={(e) => setFormData({ ...formData, status: e.target.value })}
                        >
                            <option value="">-- Select Status --</option>
                            <option value="Draft">Draft</option>
                            <option value="Active">Active</option>
                            <option value="Inactive">Inactive</option>
                            <option value="Archived">Archived</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionType">Question Type</label>
                        <select
                            id="questionType"
                            value={formData.questionType}
                            onChange={(e) => setFormData({ ...formData, questionType: e.target.value })}
                        >
                            <option value="">-- Select Type --</option>
                            <option value="SingleChoice">Single Choice</option>
                            <option value="MultipleChoice">Multiple Choice</option>
                            <option value="OpenEnded">Open Ended</option>
                            <option value="Scale">Scale</option>
                            <option value="Grid">Grid</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="classification">Classification</label>
                        <select
                            id="classification"
                            value={formData.classification}
                            onChange={(e) => setFormData({ ...formData, classification: e.target.value })}
                        >
                            <option value="">-- Select Classification --</option>
                            <option value="Standard">Standard</option>
                            <option value="Custom">Custom</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionTitle">Question Title</label>
                        <input
                            id="questionTitle"
                            type="text"
                            value={formData.questionTitle}
                            onChange={(e) => setFormData({ ...formData, questionTitle: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionText">Question Text</label>
                        <textarea
                            id="questionText"
                            value={formData.questionText}
                            onChange={(e) => setFormData({ ...formData, questionText: e.target.value })}
                            rows={4}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="dataQualityTag">Data Quality Tag</label>
                        <input
                            id="dataQualityTag"
                            type="text"
                            value={formData.dataQualityTag}
                            onChange={(e) => setFormData({ ...formData, dataQualityTag: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="methodology">Methodology</label>
                        <select
                            id="methodology"
                            value={formData.methodology}
                            onChange={(e) => setFormData({ ...formData, methodology: e.target.value })}
                        >
                            <option value="">-- Select Methodology --</option>
                            <option value="Quantitative">Quantitative</option>
                            <option value="Qualitative">Qualitative</option>
                            <option value="Mixed">Mixed</option>
                        </select>
                    </div>

                    <div className="form-row">
                        <div className="form-field">
                            <label htmlFor="rowSortOrder">Row Sort Order</label>
                            <input
                                id="rowSortOrder"
                                type="number"
                                value={formData.rowSortOrder ?? ''}
                                onChange={(e) => setFormData({ ...formData, rowSortOrder: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="columnSortOrder">Column Sort Order</label>
                            <input
                                id="columnSortOrder"
                                type="number"
                                value={formData.columnSortOrder ?? ''}
                                onChange={(e) => setFormData({ ...formData, columnSortOrder: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>
                    </div>

                    <div className="form-row">
                        <div className="form-field">
                            <label htmlFor="answerMin">Answer Min</label>
                            <input
                                id="answerMin"
                                type="number"
                                value={formData.answerMin ?? ''}
                                onChange={(e) => setFormData({ ...formData, answerMin: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="answerMax">Answer Max</label>
                            <input
                                id="answerMax"
                                type="number"
                                value={formData.answerMax ?? ''}
                                onChange={(e) => setFormData({ ...formData, answerMax: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionFormatDetails">Format Details</label>
                        <textarea
                            id="questionFormatDetails"
                            value={formData.questionFormatDetails}
                            onChange={(e) => setFormData({ ...formData, questionFormatDetails: e.target.value })}
                            rows={3}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="scraperNotes">Scraper Notes</label>
                        <textarea
                            id="scraperNotes"
                            value={formData.scraperNotes}
                            onChange={(e) => setFormData({ ...formData, scraperNotes: e.target.value })}
                            rows={3}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="customNotes">Custom Notes</label>
                        <textarea
                            id="customNotes"
                            value={formData.customNotes}
                            onChange={(e) => setFormData({ ...formData, customNotes: e.target.value })}
                            rows={3}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="metricGroup">Metric Group</label>
                        <input
                            id="metricGroup"
                            type="text"
                            value={formData.metricGroup}
                            onChange={(e) => setFormData({ ...formData, metricGroup: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="tableTitle">Table Title</label>
                        <input
                            id="tableTitle"
                            type="text"
                            value={formData.tableTitle}
                            onChange={(e) => setFormData({ ...formData, tableTitle: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionRationale">Rationale</label>
                        <textarea
                            id="questionRationale"
                            value={formData.questionRationale}
                            onChange={(e) => setFormData({ ...formData, questionRationale: e.target.value })}
                            rows={3}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="singleOrMulticode">Single or Multicode</label>
                        <select
                            id="singleOrMulticode"
                            value={formData.singleOrMulticode}
                            onChange={(e) => setFormData({ ...formData, singleOrMulticode: e.target.value })}
                        >
                            <option value="">-- Select --</option>
                            <option value="Single">Single</option>
                            <option value="Multi">Multi</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="managedListReferences">Managed List References</label>
                        <input
                            id="managedListReferences"
                            type="text"
                            value={formData.managedListReferences}
                            onChange={(e) => setFormData({ ...formData, managedListReferences: e.target.value })}
                        />
                    </div>

                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isDummy}
                                onChange={(e) => setFormData({ ...formData, isDummy: e.target.checked })}
                            />
                            <span>Is Dummy Question</span>
                        </label>
                    </div>
                </form>
            );
        }

        return null;
    };

    const renderAnswersTab = () => {
        if (mode !== 'view' || !selectedQuestion) {
            return <div className="tab-empty-state">Please save the question first before adding answers.</div>;
        }

        return (
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
                                <th>Answer Text</th>
                                <th>Code</th>
                                <th>Location</th>
                                <th>Version</th>
                                <th style={{ width: '80px' }}>Display Order</th>
                                <th style={{ width: '60px' }}>Open</th>
                                <th style={{ width: '60px' }}>Fixed</th>
                                <th style={{ width: '80px' }}>Exclusive</th>
                                <th style={{ width: '80px' }}>Status</th>
                                <th style={{ width: '100px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {selectedQuestion.answers && selectedQuestion.answers.length > 0 ? (
                                selectedQuestion.answers.map((answer) => (
                                    <tr key={answer.id}>
                                        <td>{answer.answerText}</td>
                                        <td>{answer.answerCode || '-'}</td>
                                        <td>{answer.answerLocation || '-'}</td>
                                        <td>{answer.version}</td>
                                        <td>{answer.displayOrder ?? '-'}</td>
                                        <td>{answer.isOpen ? 'Yes' : 'No'}</td>
                                        <td>{answer.isFixed ? 'Yes' : 'No'}</td>
                                        <td>{answer.isExclusive ? 'Yes' : 'No'}</td>
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
                                <tr><td colSpan={10} className="empty-state">No answers defined.</td></tr>
                            )}
                        </tbody>
                    </table>
                ) : (
                    <form className="sub-form" onSubmit={handleAnswerSubmit}>
                        <div className="form-field">
                            <label htmlFor="answerText">Answer Text *</label>
                            <input
                                id="answerText"
                                type="text"
                                value={answerFormData.answerText}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, answerText: e.target.value })}
                                className={errors.AnswerText ? 'error' : ''}
                                autoFocus
                            />
                            {errors.AnswerText && <span className="field-error">{errors.AnswerText[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="answerCode">Answer Code</label>
                            <input
                                id="answerCode"
                                type="text"
                                value={answerFormData.answerCode}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, answerCode: e.target.value })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="answerLocation">Answer Location</label>
                            <input
                                id="answerLocation"
                                type="text"
                                value={answerFormData.answerLocation}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, answerLocation: e.target.value })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="answerVersion">Version *</label>
                            <input
                                id="answerVersion"
                                type="number"
                                value={answerFormData.version}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, version: parseInt(e.target.value) || 1 })}
                                min="1"
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="displayOrder">Display Order</label>
                            <input
                                id="displayOrder"
                                type="number"
                                value={answerFormData.displayOrder ?? ''}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, displayOrder: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="customProperty">Custom Property</label>
                            <input
                                id="customProperty"
                                type="text"
                                value={answerFormData.customProperty}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, customProperty: e.target.value })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="facets">Facets</label>
                            <input
                                id="facets"
                                type="text"
                                value={answerFormData.facets}
                                onChange={(e) => setAnswerFormData({ ...answerFormData, facets: e.target.value })}
                            />
                        </div>

                        <div className="form-field checkbox">
                            <label>
                                <input
                                    type="checkbox"
                                    checked={answerFormData.isOpen}
                                    onChange={(e) => setAnswerFormData({ ...answerFormData, isOpen: e.target.checked })}
                                />
                                <span>Is Open</span>
                            </label>
                        </div>

                        <div className="form-field checkbox">
                            <label>
                                <input
                                    type="checkbox"
                                    checked={answerFormData.isFixed}
                                    onChange={(e) => setAnswerFormData({ ...answerFormData, isFixed: e.target.checked })}
                                />
                                <span>Is Fixed</span>
                            </label>
                        </div>

                        <div className="form-field checkbox">
                            <label>
                                <input
                                    type="checkbox"
                                    checked={answerFormData.isExclusive}
                                    onChange={(e) => setAnswerFormData({ ...answerFormData, isExclusive: e.target.checked })}
                                />
                                <span>Is Exclusive</span>
                            </label>
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
        );
    };

    const renderAdminTab = () => {
        if (mode === 'view' && selectedQuestion) {
            return (
                <div className="view-details">
                    <div className="detail-section">
                        <h3>Question Settings</h3>
                        <div className="detail-item">
                            <label>Is Translatable</label>
                            <div className="value">{selectedQuestion.isTranslatable ? 'Yes' : 'No'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Is Hidden</label>
                            <div className="value">{selectedQuestion.isHidden ? 'Yes' : 'No'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Is Question Active</label>
                            <div className="value">{selectedQuestion.isQuestionActive ? 'Yes' : 'No'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Is Question Out of Use</label>
                            <div className="value">{selectedQuestion.isQuestionOutOfUse ? 'Yes' : 'No'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Answer Restrictions</h3>
                        <div className="detail-item">
                            <label>Answer Restriction Min</label>
                            <div className="value">{selectedQuestion.answerRestrictionMin ?? '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Answer Restriction Max</label>
                            <div className="value">{selectedQuestion.answerRestrictionMax ?? '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Restriction Data Type</label>
                            <div className="value">{selectedQuestion.restrictionDataType || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Restricted to Client</label>
                            <div className="value">{selectedQuestion.restrictedToClient || '-'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Answer Configuration</h3>
                        <div className="detail-item">
                            <label>Answer Type Code</label>
                            <div className="value">{selectedQuestion.answerTypeCode || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Is Answer Required</label>
                            <div className="value">{selectedQuestion.isAnswerRequired ? 'Yes' : 'No'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Display Configuration</h3>
                        <div className="detail-item">
                            <label>Scale Point</label>
                            <div className="value">{selectedQuestion.scalePoint || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Scale Type</label>
                            <div className="value">{selectedQuestion.scaleType || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Display Type</label>
                            <div className="value">{selectedQuestion.displayType || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Instruction Text</label>
                            <div className="value text-content">{selectedQuestion.instructionText || '-'}</div>
                        </div>
                    </div>

                    <div className="detail-section">
                        <h3>Relationships</h3>
                        <div className="detail-item">
                            <label>Parent Question ID</label>
                            <div className="value">{selectedQuestion.parentQuestionId || '-'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Question Facet</label>
                            <div className="value">{selectedQuestion.questionFacet || '-'}</div>
                        </div>
                    </div>
                </div>
            );
        }

        if (mode === 'create' || mode === 'edit') {
            return (
                <form id="question-bank-admin-form" className="panel-form" onSubmit={handleSubmit}>
                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isTranslatable}
                                onChange={(e) => setFormData({ ...formData, isTranslatable: e.target.checked })}
                            />
                            <span>Is Translatable</span>
                        </label>
                    </div>

                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isHidden}
                                onChange={(e) => setFormData({ ...formData, isHidden: e.target.checked })}
                            />
                            <span>Is Hidden</span>
                        </label>
                    </div>

                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isQuestionActive}
                                onChange={(e) => setFormData({ ...formData, isQuestionActive: e.target.checked })}
                            />
                            <span>Is Question Active</span>
                        </label>
                    </div>

                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isQuestionOutOfUse}
                                onChange={(e) => setFormData({ ...formData, isQuestionOutOfUse: e.target.checked })}
                            />
                            <span>Is Question Out of Use</span>
                        </label>
                    </div>

                    <div className="form-row">
                        <div className="form-field">
                            <label htmlFor="answerRestrictionMin">Answer Restriction Min</label>
                            <input
                                id="answerRestrictionMin"
                                type="number"
                                value={formData.answerRestrictionMin ?? ''}
                                onChange={(e) => setFormData({ ...formData, answerRestrictionMin: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>

                        <div className="form-field">
                            <label htmlFor="answerRestrictionMax">Answer Restriction Max</label>
                            <input
                                id="answerRestrictionMax"
                                type="number"
                                value={formData.answerRestrictionMax ?? ''}
                                onChange={(e) => setFormData({ ...formData, answerRestrictionMax: e.target.value ? parseInt(e.target.value) : null })}
                            />
                        </div>
                    </div>

                    <div className="form-field">
                        <label htmlFor="restrictionDataType">Restriction Data Type</label>
                        <select
                            id="restrictionDataType"
                            value={formData.restrictionDataType}
                            onChange={(e) => setFormData({ ...formData, restrictionDataType: e.target.value })}
                        >
                            <option value="">-- Select Data Type --</option>
                            <option value="String">String</option>
                            <option value="Number">Number</option>
                            <option value="Date">Date</option>
                            <option value="Boolean">Boolean</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="restrictedToClient">Restricted to Client</label>
                        <input
                            id="restrictedToClient"
                            type="text"
                            value={formData.restrictedToClient}
                            onChange={(e) => setFormData({ ...formData, restrictedToClient: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="answerTypeCode">Answer Type Code</label>
                        <input
                            id="answerTypeCode"
                            type="text"
                            value={formData.answerTypeCode}
                            onChange={(e) => setFormData({ ...formData, answerTypeCode: e.target.value })}
                        />
                    </div>

                    <div className="form-field checkbox">
                        <label>
                            <input
                                type="checkbox"
                                checked={formData.isAnswerRequired}
                                onChange={(e) => setFormData({ ...formData, isAnswerRequired: e.target.checked })}
                            />
                            <span>Is Answer Required</span>
                        </label>
                    </div>

                    <div className="form-field">
                        <label htmlFor="scalePoint">Scale Point</label>
                        <input
                            id="scalePoint"
                            type="text"
                            value={formData.scalePoint}
                            onChange={(e) => setFormData({ ...formData, scalePoint: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="scaleType">Scale Type</label>
                        <select
                            id="scaleType"
                            value={formData.scaleType}
                            onChange={(e) => setFormData({ ...formData, scaleType: e.target.value })}
                        >
                            <option value="">-- Select Scale Type --</option>
                            <option value="Likert">Likert</option>
                            <option value="Numeric">Numeric</option>
                            <option value="Semantic">Semantic</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="displayType">Display Type</label>
                        <select
                            id="displayType"
                            value={formData.displayType}
                            onChange={(e) => setFormData({ ...formData, displayType: e.target.value })}
                        >
                            <option value="">-- Select Display Type --</option>
                            <option value="Radio">Radio</option>
                            <option value="Checkbox">Checkbox</option>
                            <option value="Dropdown">Dropdown</option>
                            <option value="Slider">Slider</option>
                            <option value="Text">Text</option>
                        </select>
                    </div>

                    <div className="form-field">
                        <label htmlFor="instructionText">Instruction Text</label>
                        <textarea
                            id="instructionText"
                            value={formData.instructionText}
                            onChange={(e) => setFormData({ ...formData, instructionText: e.target.value })}
                            rows={3}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="parentQuestionId">Parent Question ID</label>
                        <input
                            id="parentQuestionId"
                            type="text"
                            value={formData.parentQuestionId}
                            onChange={(e) => setFormData({ ...formData, parentQuestionId: e.target.value })}
                        />
                    </div>

                    <div className="form-field">
                        <label htmlFor="questionFacet">Question Facet</label>
                        <input
                            id="questionFacet"
                            type="text"
                            value={formData.questionFacet}
                            onChange={(e) => setFormData({ ...formData, questionFacet: e.target.value })}
                        />
                    </div>
                </form>
            );
        }

        return null;
    };

    const renderRelatedTab = () => {
        return (
            <div className="tab-empty-state">
                <p>Related functionality will be available in a future release.</p>
            </div>
        );
    };

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
                        placeholder="Search question bank..."
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
                                <th style={{ width: '80px' }}>Version</th>
                                <th>Question Type</th>
                                <th>Question Text</th>
                                <th style={{ width: '120px' }}>Classification</th>
                                <th style={{ width: '80px' }}>Dummy</th>
                                <th>Title</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th>Created By</th>
                                <th style={{ width: '150px' }}>Created On</th>
                                <th>Methodology</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {questions.map((question) => (
                                <tr key={question.id} onClick={() => openView(question)} className="clickable-row">
                                    <td>{question.variableName}</td>
                                    <td>{question.version}</td>
                                    <td>{question.questionType || '-'}</td>
                                    <td className="text-truncate">{question.questionText || '-'}</td>
                                    <td>{question.classification || '-'}</td>
                                    <td>{question.isDummy ? 'Yes' : 'No'}</td>
                                    <td className="text-truncate">{question.questionTitle || '-'}</td>
                                    <td>
                                        <span className={`status-text ${question.status === 'Active' ? 'active' : 'inactive'}`}>
                                            {question.status || '-'}
                                        </span>
                                    </td>
                                    <td>{question.createdBy || '-'}</td>
                                    <td>{new Date(question.createdOn).toLocaleString()}</td>
                                    <td>{question.methodology || '-'}</td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(question); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(); }} title="Edit" disabled>
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
                                <tr><td colSpan={12} className="empty-state">No questions found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel with Tabs */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Question' : mode === 'edit' ? 'Edit Question' : selectedQuestion ? `${selectedQuestion.variableName} v${selectedQuestion.version}` : 'Question Details'}
                width="600px"
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" type="submit" form={activeTab === 'admin' ? 'question-bank-admin-form' : 'question-bank-form'}>Save</button>
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

                {/* Tabs */}
                <div className="tabs">
                    <button
                        className={`tab ${activeTab === 'question' ? 'active' : ''}`}
                        onClick={() => setActiveTab('question')}
                    >
                        Question
                    </button>
                    <button
                        className={`tab ${activeTab === 'answers' ? 'active' : ''}`}
                        onClick={() => setActiveTab('answers')}
                        disabled={mode === 'create'}
                    >
                        Answers
                    </button>
                    <button
                        className={`tab ${activeTab === 'admin' ? 'active' : ''}`}
                        onClick={() => setActiveTab('admin')}
                    >
                        Admin
                    </button>
                    <button
                        className={`tab ${activeTab === 'related' ? 'active' : ''}`}
                        onClick={() => setActiveTab('related')}
                        disabled={mode === 'create'}
                    >
                        Related
                    </button>
                </div>

                {/* Tab Content */}
                <div className="tab-content">
                    {activeTab === 'question' && renderQuestionTab()}
                    {activeTab === 'answers' && renderAnswersTab()}
                    {activeTab === 'admin' && renderAdminTab()}
                    {activeTab === 'related' && renderRelatedTab()}
                </div>
            </SidePanel>
        </div>
    );
}
