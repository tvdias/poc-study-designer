import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { LayoutDashboard, FileQuestion, FlaskConical, List, Users, ChevronDown, FileDown, History, Save } from 'lucide-react';
import { projectsApi, type Project } from '../services/api';
import './ProjectDetailPage.css';

export function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const [project, setProject] = useState<Project | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeSection, setActiveSection] = useState('details');
    const [studiesExpanded, setStudiesExpanded] = useState(false);

    useEffect(() => {
        if (id) {
            loadProject(id);
        }
    }, [id]);

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

    if (loading) {
        return <div className="loading-container">Loading project...</div>;
    }

    if (error || !project) {
        return <div className="error-container">{error || 'Project not found'}</div>;
    }

    return (
        <div className="project-detail-page">
            {/* Project Header Info */}
            <div className="project-header-info">
                <h1 className="project-title">{project.name}</h1>
                {project.clientName && (
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
                                    onClick={() => setActiveSection('details')}
                                >
                                    <LayoutDashboard size={16} />
                                    <span>Details</span>
                                </button>
                            </li>
                            <li>
                                <button
                                    className={`nav-item ${activeSection === 'questionnaire' ? 'active' : ''}`}
                                    onClick={() => setActiveSection('questionnaire')}
                                >
                                    <FileQuestion size={16} />
                                    <span>Questionnaire</span>
                                </button>
                            </li>
                            <li>
                                <button
                                    className={`nav-item-accordion ${studiesExpanded ? 'expanded' : ''}`}
                                    onClick={() => setStudiesExpanded(!studiesExpanded)}
                                >
                                    <div className="nav-item-content">
                                        <FlaskConical size={16} />
                                        <span>Studies</span>
                                    </div>
                                    <ChevronDown size={14} className={`chevron ${studiesExpanded ? 'rotated' : ''}`} />
                                </button>
                                {studiesExpanded && (
                                    <ul className="sub-nav-list">
                                        <li>
                                            <button
                                                className={`sub-nav-item ${activeSection === 'studies' ? 'active' : ''}`}
                                                onClick={() => setActiveSection('studies')}
                                            >
                                                All Studies Overview
                                            </button>
                                        </li>
                                    </ul>
                                )}
                            </li>
                            <li>
                                <button
                                    className={`nav-item ${activeSection === 'lists' ? 'active' : ''}`}
                                    onClick={() => setActiveSection('lists')}
                                >
                                    <List size={16} />
                                    <span>Managed Lists</span>
                                </button>
                            </li>
                            <li>
                                <button
                                    className={`nav-item ${activeSection === 'users' ? 'active' : ''}`}
                                    onClick={() => setActiveSection('users')}
                                >
                                    <Users size={16} />
                                    <span>Access Team</span>
                                </button>
                            </li>
                        </ul>
                    </nav>

                    {/* Utilities */}
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
                </aside>

                {/* Main Content */}
                <main className="detail-main">
                    {activeSection === 'details' && (
                        <section className="detail-section">
                            <div className="section-header">
                                <h2 className="section-title">Project Details</h2>
                                <span className="project-id">ID: {project.id.substring(0, 13)}</span>
                            </div>
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
                        </section>
                    )}

                    {activeSection === 'questionnaire' && (
                        <section className="detail-section">
                            <div className="section-header">
                                <h2 className="section-title">Questionnaire Structure</h2>
                            </div>
                            <div className="section-content">
                                <p className="placeholder-text">Questionnaire section coming soon...</p>
                            </div>
                        </section>
                    )}

                    {activeSection === 'studies' && (
                        <section className="detail-section">
                            <div className="section-header">
                                <h2 className="section-title">Studies</h2>
                            </div>
                            <div className="section-content">
                                <p className="placeholder-text">Studies section coming soon...</p>
                            </div>
                        </section>
                    )}
                </main>
            </div>
        </div>
    );
}
