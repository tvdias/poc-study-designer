import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, useLocation, useOutletContext } from 'react-router-dom';
import { LayoutDashboard, FileQuestion, FlaskConical, List, Users, ChevronDown, FileDown, History, Save } from 'lucide-react';
import { projectsApi, clientsApi, commissioningMarketsApi, studiesApi, managedListsApi, type Project, type Client, type CommissioningMarket, type CreateProjectRequest, type StudySummary, type ManagedList } from '../services/api';
import { QuestionnaireSection } from './QuestionnaireSection';
import { ManagedListsSection } from './ManagedListsSection';
import { StudiesSection } from './StudiesSection';
import { ManagedListDetailPage } from './ManagedListDetailPage';
import './ProjectDetailPage.css';

export function ProjectDetailPage() {
    const { handleScroll } = useOutletContext<{ handleScroll: (e: React.UIEvent<HTMLElement>) => void }>();
    const { id: routeId, projectId, listId } = useParams<{ id?: string, projectId?: string, listId?: string }>();
    const id = routeId || projectId;
    const navigate = useNavigate();
    const location = useLocation();
    const [project, setProject] = useState<Project | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeSection, setActiveSection] = useState(() => {
        if (listId) return 'lists';
        const params = new URLSearchParams(location.search);
        return params.get('section') || 'details';
    });

    useEffect(() => {
        if (listId) setActiveSection('lists');
    }, [listId]);

    const changeSection = (section: string) => {
        setActiveSection(section);
        if (listId && id !== 'new') {
            navigate(`/projects/${id}?section=${section}`);
        } else if (!listId && id !== 'new') {
            navigate(`/projects/${id}?section=${section}`, { replace: true });
        }
    };

    // Sidebar items state
    const [studiesExpanded, setStudiesExpanded] = useState(true);
    const [listsExpanded, setListsExpanded] = useState(true);
    const [recentStudies, setRecentStudies] = useState<StudySummary[]>([]);
    const [recentLists, setRecentLists] = useState<ManagedList[]>([]);

    // Creation mode state
    const isCreateMode = id === 'new';
    const [clients, setClients] = useState<Client[]>([]);
    const [commissioningMarkets, setCommissioningMarkets] = useState<CommissioningMarket[]>([]);
    const [formData, setFormData] = useState<CreateProjectRequest>({
        name: '',
        description: '',
        clientId: undefined,
        commissioningMarketId: undefined,
        methodology: undefined
    });
    const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const loadSidebarItems = useCallback(async (pid: string) => {
        try {
            const [studiesData, listsData] = await Promise.all([
                studiesApi.getAll(pid).catch(() => [] as StudySummary[]),
                managedListsApi.getAll(pid).catch(() => [] as ManagedList[])
            ]);

            const sortedStudies = [...studiesData]
                .sort((a, b) => new Date(b.createdOn).getTime() - new Date(a.createdOn).getTime())
                .slice(0, 6);

            const sortedLists = [...listsData]
                .sort((a, b) => {
                    const tA = new Date(a.modifiedOn || a.firstSnapshotDate || 0).getTime();
                    const tB = new Date(b.modifiedOn || b.firstSnapshotDate || 0).getTime();
                    return tB - tA;
                })
                .slice(0, 6);

            setRecentStudies(sortedStudies);
            setRecentLists(sortedLists);
        } catch (e) {
            console.error('Failed to load sidebar items', e);
        }
    }, []);

    useEffect(() => {
        if (isCreateMode) {
            // In create mode, load clients and commissioning markets for the dropdowns
            loadClients();
            loadCommissioningMarkets();
            setLoading(false);
        } else if (id) {
            loadProject(id);
        }
    }, [id, isCreateMode]);

    useEffect(() => {
        if (!isCreateMode && project?.id) {
            loadSidebarItems(project.id);
        }
    }, [isCreateMode, project?.id, loadSidebarItems]);

    const loadClients = async () => {
        try {
            const data = await clientsApi.getAll();
            setClients(data);
        } catch (err) {
            console.error('Failed to load clients', err);
        }
    };

    const loadCommissioningMarkets = async () => {
        try {
            const data = await commissioningMarketsApi.getAll();
            setCommissioningMarkets(data);
        } catch (err) {
            console.error('Failed to load commissioning markets', err);
        }
    };

    const loadProject = async (projectId: string) => {
        try {
            setLoading(true);
            setError(null);
            const data = await projectsApi.getById(projectId);
            setProject(data);
        } catch (err) {
            setError('Failed to load project');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleInputChange = (field: keyof CreateProjectRequest, value: string | undefined) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Clear validation error for this field
        if (validationErrors[field]) {
            setValidationErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[field];
                return newErrors;
            });
        }
    };

    const handleCreateProject = async (e: React.FormEvent) => {
        e.preventDefault();
        setValidationErrors({});
        setServerError('');
        setIsSubmitting(true);

        try {
            const createdProject = await projectsApi.create(formData);
            // Navigate to the newly created project
            navigate(`/projects/${createdProject.id}`, { replace: true });
        } catch (err) {
            const error = err as { status?: number; errors?: Record<string, string[]> };
            if (error.status === 400 && error.errors) {
                // Validation errors
                setValidationErrors(error.errors);
            } else if (error.status === 409) {
                // Conflict error (duplicate name)
                setServerError(typeof err === 'string' ? err : 'A project with this name already exists');
            } else {
                setServerError('Failed to create project. Please try again.');
            }
            console.error('Failed to create project', err);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (loading) {
        return <div className="loading-container">Loading project...</div>;
    }

    if (error || (!project && !isCreateMode)) {
        return <div className="error-container">{error || 'Project not found'}</div>;
    }

    return (
        <div className="project-detail-page">
            {/* Project Header Info */}
            <div className="project-header-info">
                <h1 className="project-title">{isCreateMode ? 'New Project' : project?.name}</h1>
                {!isCreateMode && project?.clientName && (
                    <span className="client-badge">{project.clientName}</span>
                )}
            </div>

            <div className="detail-layout">
                {/* Sidebar */}
                <aside className="detail-sidebar">
                    <nav className="sidebar-nav">
                        <div className="nav-header">Project Sections</div>
                        <ul className="nav-list">
                            <li>
                                <button
                                    className={`nav-item ${activeSection === 'details' ? 'active' : ''}`}
                                    onClick={() => changeSection('details')}
                                >
                                    <LayoutDashboard size={16} />
                                    <span>Details</span>
                                </button>
                            </li>
                            {!isCreateMode && (
                                <>
                                    <li>
                                        <button
                                            className={`nav-item ${activeSection === 'questionnaire' ? 'active' : ''}`}
                                            onClick={() => changeSection('questionnaire')}
                                        >
                                            <FileQuestion size={16} />
                                            <span>Questionnaire</span>
                                        </button>
                                    </li>
                                    <li>
                                        <button
                                            className={`nav-item-accordion ${studiesExpanded ? 'expanded' : ''}`}
                                            onClick={() => {
                                                if (!studiesExpanded) setStudiesExpanded(true);
                                                changeSection('studies');
                                            }}
                                        >
                                            <div className="nav-item-content">
                                                <FlaskConical size={16} />
                                                <span>Studies</span>
                                            </div>
                                            <ChevronDown
                                                size={14}
                                                className={`chevron ${studiesExpanded ? 'rotated' : ''}`}
                                                onClick={(e) => { e.stopPropagation(); setStudiesExpanded(!studiesExpanded); }}
                                            />
                                        </button>
                                        {studiesExpanded && (
                                            <ul className="sub-nav-list">
                                                {recentStudies.map(study => (
                                                    <li key={study.studyId}>
                                                        <button
                                                            className="sub-nav-item recent-item"
                                                            title={study.name}
                                                            onClick={() => changeSection('studies')}
                                                        >
                                                            <span className={`status-dot status-${study.status?.toLowerCase() || 'draft'}`}></span>
                                                            <span className="truncate">{study.name}</span>
                                                        </button>
                                                    </li>
                                                ))}
                                            </ul>
                                        )}
                                    </li>
                                    <li>
                                        <button
                                            className={`nav-item-accordion ${listsExpanded ? 'expanded' : ''}`}
                                            onClick={() => {
                                                if (!listsExpanded) setListsExpanded(true);
                                                changeSection('lists');
                                            }}
                                        >
                                            <div className="nav-item-content">
                                                <List size={16} />
                                                <span>Managed Lists</span>
                                            </div>
                                            <ChevronDown
                                                size={14}
                                                className={`chevron ${listsExpanded ? 'rotated' : ''}`}
                                                onClick={(e) => { e.stopPropagation(); setListsExpanded(!listsExpanded); }}
                                            />
                                        </button>
                                        {listsExpanded && (
                                            <ul className="sub-nav-list">
                                                {recentLists.map(list => (
                                                    <li key={list.id}>
                                                        <button
                                                            className="sub-nav-item recent-item"
                                                            title={list.name}
                                                            onClick={() => navigate(`/projects/${project!.id}/managed-lists/${list.id}`)}
                                                        >
                                                            <span className={`status-dot status-${list.status?.toLowerCase() || 'active'}`}></span>
                                                            <span className="truncate">{list.name}</span>
                                                        </button>
                                                    </li>
                                                ))}
                                            </ul>
                                        )}
                                    </li>
                                    <li>
                                        <button
                                            className={`nav-item ${activeSection === 'users' ? 'active' : ''}`}
                                            onClick={() => changeSection('users')}
                                        >
                                            <Users size={16} />
                                            <span>Access Team</span>
                                        </button>
                                    </li>
                                </>
                            )}
                        </ul>
                    </nav>

                    {/* Utilities */}
                    {!isCreateMode && (
                        <div className="utilities-section">
                            <div className="utilities-header">Utilities</div>
                            <ul className="utilities-list">
                                <li>
                                    <button className="utility-btn">
                                        <FileDown size={16} />
                                        <span>Generate Document</span>
                                    </button>
                                </li>
                                <li>
                                    <button className="utility-btn">
                                        <History size={16} />
                                        <span>Audit Log</span>
                                    </button>
                                </li>
                            </ul>
                        </div>
                    )}
                </aside>

                {/* Main Content */}
                <main className="detail-main" onScroll={handleScroll}>
                    {activeSection === 'details' && !listId && (
                        <section className="detail-section">
                            <div className="section-header">
                                <h2 className="section-title">{isCreateMode ? 'Create Project' : 'Project Details'}</h2>
                                {!isCreateMode && project && (
                                    <span className="project-id">ID: {project.id.substring(0, 13)}</span>
                                )}
                            </div>

                            {isCreateMode ? (
                                /* Creation Form */
                                <form onSubmit={handleCreateProject}>
                                    {serverError && (
                                        <div className="error-message">
                                            {serverError}
                                        </div>
                                    )}

                                    <div className="section-content">
                                        <div className="form-grid">
                                            <div className="form-group col-span-2">
                                                <label className="form-label">
                                                    Project Name <span className="required-asterisk">*</span>
                                                </label>
                                                <input
                                                    type="text"
                                                    value={formData.name}
                                                    onChange={(e) => handleInputChange('name', e.target.value)}
                                                    className={`form-input ${validationErrors.Name ? 'error' : ''}`}
                                                    required
                                                    maxLength={200}
                                                    autoFocus
                                                />
                                                {validationErrors.Name && (
                                                    <div className="field-error">{validationErrors.Name[0]}</div>
                                                )}
                                            </div>

                                            <div className="form-group col-span-2">
                                                <label className="form-label">Client Account</label>
                                                <select
                                                    value={formData.clientId || ''}
                                                    onChange={(e) => handleInputChange('clientId', e.target.value || undefined)}
                                                    className={`form-select ${validationErrors.ClientId ? 'error' : ''}`}
                                                >
                                                    <option value="">Select a client (optional)</option>
                                                    {clients.map(client => (
                                                        <option key={client.id} value={client.id}>
                                                            {client.accountName}
                                                        </option>
                                                    ))}
                                                </select>
                                                {validationErrors.ClientId && (
                                                    <div className="field-error">{validationErrors.ClientId[0]}</div>
                                                )}
                                            </div>

                                            <div className="form-group col-span-2">
                                                <label className="form-label">Commissioning Market</label>
                                                <select
                                                    value={formData.commissioningMarketId || ''}
                                                    onChange={(e) => handleInputChange('commissioningMarketId', e.target.value || undefined)}
                                                    className={`form-select ${validationErrors.CommissioningMarketId ? 'error' : ''}`}
                                                >
                                                    <option value="">Select a market (optional)</option>
                                                    {commissioningMarkets.map(market => (
                                                        <option key={market.id} value={market.id}>
                                                            {market.name} ({market.isoCode})
                                                        </option>
                                                    ))}
                                                </select>
                                                {validationErrors.CommissioningMarketId && (
                                                    <div className="field-error">{validationErrors.CommissioningMarketId[0]}</div>
                                                )}
                                            </div>

                                            <div className="form-group col-span-2">
                                                <label className="form-label">Methodology</label>
                                                <select
                                                    value={formData.methodology || ''}
                                                    onChange={(e) => handleInputChange('methodology', e.target.value || undefined)}
                                                    className={`form-select ${validationErrors.Methodology ? 'error' : ''}`}
                                                >
                                                    <option value="">Select methodology (optional)</option>
                                                    <option value="CATI">CATI</option>
                                                    <option value="CAPI">CAPI</option>
                                                    <option value="CAWI">CAWI</option>
                                                    <option value="Online">Online</option>
                                                    <option value="Mixed">Mixed</option>
                                                </select>
                                                {validationErrors.Methodology && (
                                                    <div className="field-error">{validationErrors.Methodology[0]}</div>
                                                )}
                                            </div>

                                            <div className="form-group col-span-2">
                                                <label className="form-label">Description</label>
                                                <textarea
                                                    rows={3}
                                                    value={formData.description || ''}
                                                    onChange={(e) => handleInputChange('description', e.target.value)}
                                                    className={`form-textarea ${validationErrors.Description ? 'error' : ''}`}
                                                    maxLength={2000}
                                                />
                                                {validationErrors.Description && (
                                                    <div className="field-error">{validationErrors.Description[0]}</div>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                    <div className="section-footer">
                                        <button
                                            type="button"
                                            onClick={() => navigate('/projects')}
                                            className="cancel-btn"
                                            disabled={isSubmitting}
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            type="submit"
                                            className="save-btn"
                                            disabled={isSubmitting}
                                        >
                                            <Save size={14} />
                                            <span>{isSubmitting ? 'Creating...' : 'Create Project'}</span>
                                        </button>
                                    </div>
                                </form>
                            ) : project ? (
                                /* View Mode */
                                <>
                                    <div className="section-content">
                                        <div className="form-grid">
                                            <div className="form-group col-span-2">
                                                <label className="form-label">Project Name</label>
                                                <input
                                                    type="text"
                                                    value={project.name}
                                                    className="form-input"
                                                    readOnly
                                                />
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Status</label>
                                                <select className="form-select" value={project.status} disabled>
                                                    <option value="Active">Active</option>
                                                    <option value="OnHold">On Hold</option>
                                                    <option value="Closed">Closed</option>
                                                </select>
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Cost Management</label>
                                                <div className="toggle-wrapper">
                                                    <input
                                                        type="checkbox"
                                                        checked={project.costManagementEnabled}
                                                        disabled
                                                        className="toggle-input"
                                                    />
                                                    <span className="toggle-label">
                                                        {project.costManagementEnabled ? 'Enabled' : 'Disabled'}
                                                    </span>
                                                </div>
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Client Account</label>
                                                <input
                                                    type="text"
                                                    value={project.clientName || '-'}
                                                    className="form-input"
                                                    readOnly
                                                />
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Commissioning Market</label>
                                                <input
                                                    type="text"
                                                    value={project.commissioningMarketName || '-'}
                                                    className="form-input"
                                                    readOnly
                                                />
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Methodology</label>
                                                <input
                                                    type="text"
                                                    value={project.methodology || '-'}
                                                    className="form-input"
                                                    readOnly
                                                />
                                            </div>
                                            <div className="form-group">
                                                <label className="form-label">Owner</label>
                                                <input
                                                    type="text"
                                                    value={project.owner || '-'}
                                                    className="form-input"
                                                    readOnly
                                                />
                                            </div>
                                            <div className="form-group col-span-2">
                                                <label className="form-label">Description</label>
                                                <textarea
                                                    rows={2}
                                                    value={project.description || ''}
                                                    className="form-textarea"
                                                    readOnly
                                                />
                                            </div>
                                        </div>
                                    </div>
                                    <div className="section-footer">
                                        <button className="save-btn">
                                            <Save size={14} />
                                            <span>Save Details</span>
                                        </button>
                                    </div>
                                </>
                            ) : null}
                        </section>
                    )}

                    {!isCreateMode && activeSection === 'questionnaire' && project && !listId && (
                        <QuestionnaireSection projectId={project.id} />
                    )}

                    {!isCreateMode && activeSection === 'studies' && project && !listId && (
                        <StudiesSection projectId={project.id} onListUpdate={() => loadSidebarItems(project.id)} />
                    )}

                    {!isCreateMode && activeSection === 'lists' && project && !listId && (
                        <ManagedListsSection projectId={project.id} onListUpdate={() => loadSidebarItems(project.id)} />
                    )}

                    {!isCreateMode && project && listId && (
                        <ManagedListDetailPage />
                    )}
                </main>
            </div>
        </div>
    );
}
