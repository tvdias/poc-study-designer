import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Search, Filter, ArrowUpDown, MoreHorizontal } from 'lucide-react';
import { projectsApi, type Project, type CreateProjectRequest } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import './ProjectsListPage.css';

export function ProjectsListPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const [projects, setProjects] = useState<Project[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState('');
    const [error, setError] = useState<string | null>(null);
    
    // Side panel state
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [formData, setFormData] = useState<CreateProjectRequest>({
        name: '',
        description: '',
        owner: ''
    });
    const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        loadProjects();
    }, []);

    // Open create panel when URL param is set
    useEffect(() => {
        if (searchParams.get('create') === 'true') {
            setFormData({
                name: '',
                description: '',
                owner: ''
            });
            setValidationErrors({});
            setServerError('');
            setIsCreateOpen(true);
        }
    }, [searchParams]);

    const loadProjects = async (query?: string) => {
        try {
            setLoading(true);
            setError(null);
            const data = await projectsApi.getAll(query);
            setProjects(data);
        } catch (err) {
            setError('Failed to load projects');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        loadProjects(searchQuery);
    };

    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    };

    const getInitials = (name?: string) => {
        if (!name) return '?';
        return name.charAt(0).toUpperCase();
    };

    const closeCreatePanel = () => {
        setIsCreateOpen(false);
        // Remove the create param from URL
        searchParams.delete('create');
        setSearchParams(searchParams, { replace: true });
    };

    const handleInputChange = (field: keyof CreateProjectRequest, value: string) => {
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
            await loadProjects(); // Refresh the list
            closeCreatePanel();
            navigate(`/projects/${createdProject.id}`); // Navigate to the newly created project
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

    return (
        <div className="projects-list-page">
            <div className="projects-container">
                <div className="projects-card">
                    {/* Toolbar */}
                    <div className="toolbar">
                        <form onSubmit={handleSearch} className="search-form">
                            <div className="search-wrapper">
                                <Search className="search-icon" size={16} />
                                <input
                                    type="text"
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    placeholder="Search projects..."
                                    className="search-input"
                                />
                            </div>
                        </form>
                        <div className="toolbar-actions">
                            <button className="toolbar-btn">
                                <Filter size={14} />
                                <span>Filters</span>
                            </button>
                            <button className="toolbar-btn">
                                <ArrowUpDown size={14} />
                                <span>Sort</span>
                            </button>
                        </div>
                    </div>

                    {/* Table */}
                    <div className="table-container">
                        {loading ? (
                            <div className="loading-state">Loading projects...</div>
                        ) : error ? (
                            <div className="error-state">{error}</div>
                        ) : projects.length === 0 ? (
                            <div className="empty-state">No projects found</div>
                        ) : (
                            <table className="projects-table">
                                <thead>
                                    <tr>
                                        <th className="th-name">Project Name</th>
                                        <th className="th-client">Client</th>
                                        <th className="th-description">Description</th>
                                        <th className="th-product">Product</th>
                                        <th className="th-owner">Owner</th>
                                        <th className="th-modified">Last Modified</th>
                                        <th className="th-actions">
                                            <span className="sr-only">Actions</span>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {projects.map((project) => (
                                        <tr
                                            key={project.id}
                                            className="table-row"
                                            onClick={() => navigate(`/projects/${project.id}`)}
                                        >
                                            <td className="td-name">
                                                <span className="project-name">{project.name}</span>
                                            </td>
                                            <td className="td-client">{project.clientName || '-'}</td>
                                            <td className="td-description">
                                                {project.description || '-'}
                                            </td>
                                            <td className="td-product">{project.productName || '-'}</td>
                                            <td className="td-owner">
                                                {project.owner ? (
                                                    <div className="owner-cell">
                                                        <div className="owner-avatar">
                                                            {getInitials(project.owner)}
                                                        </div>
                                                        <span className="owner-name">{project.owner}</span>
                                                    </div>
                                                ) : (
                                                    '-'
                                                )}
                                            </td>
                                            <td className="td-modified">
                                                {formatDate(project.modifiedOn || project.createdOn)}
                                            </td>
                                            <td className="td-actions">
                                                <button
                                                    className="action-btn"
                                                    onClick={(e) => e.stopPropagation()}
                                                >
                                                    <MoreHorizontal size={16} />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>

                    {/* Pagination */}
                    {!loading && !error && projects.length > 0 && (
                        <div className="pagination">
                            <div className="pagination-info">
                                <p>
                                    Showing <span className="font-medium">1</span> to{' '}
                                    <span className="font-medium">{projects.length}</span> of{' '}
                                    <span className="font-medium">{projects.length}</span> results
                                </p>
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* Create Project Side Panel */}
            <SidePanel
                isOpen={isCreateOpen}
                onClose={closeCreatePanel}
                title="Create New Project"
                footer={
                    <>
                        <button
                            type="button"
                            onClick={closeCreatePanel}
                            className="btn-secondary"
                            disabled={isSubmitting}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            form="create-project-form"
                            className="btn-primary"
                            disabled={isSubmitting}
                        >
                            {isSubmitting ? 'Creating...' : 'Create Project'}
                        </button>
                    </>
                }
            >
                <form id="create-project-form" onSubmit={handleCreateProject}>
                    {serverError && (
                        <div className="error-message">
                            {serverError}
                        </div>
                    )}

                    <div className="form-group">
                        <label htmlFor="name" className="form-label">
                            Project Name <span className="required-asterisk">*</span>
                        </label>
                        <input
                            type="text"
                            id="name"
                            className={`form-input ${validationErrors.Name ? 'error' : ''}`}
                            value={formData.name}
                            onChange={(e) => handleInputChange('name', e.target.value)}
                            required
                            maxLength={200}
                        />
                        {validationErrors.Name && (
                            <div className="field-error">{validationErrors.Name[0]}</div>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="description" className="form-label">
                            Description
                        </label>
                        <textarea
                            id="description"
                            className={`form-input ${validationErrors.Description ? 'error' : ''}`}
                            value={formData.description || ''}
                            onChange={(e) => handleInputChange('description', e.target.value)}
                            rows={4}
                            maxLength={2000}
                        />
                        {validationErrors.Description && (
                            <div className="field-error">{validationErrors.Description[0]}</div>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="owner" className="form-label">
                            Owner
                        </label>
                        <input
                            type="text"
                            id="owner"
                            className={`form-input ${validationErrors.Owner ? 'error' : ''}`}
                            value={formData.owner || ''}
                            onChange={(e) => handleInputChange('owner', e.target.value)}
                            maxLength={100}
                        />
                        {validationErrors.Owner && (
                            <div className="field-error">{validationErrors.Owner[0]}</div>
                        )}
                    </div>
                </form>
            </SidePanel>
        </div>
    );
}
