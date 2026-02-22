import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Filter, ArrowUpDown, MoreHorizontal } from 'lucide-react';
import { projectsApi, type Project } from '../services/api';
import './ProjectsListPage.css';

export function ProjectsListPage() {
    const navigate = useNavigate();
    const [projects, setProjects] = useState<Project[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState('');
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadProjects();
    }, []);

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

    return (
        <div className="projects-list-page">
            <div className="projects-container">
                <div className="projects-card">
                    {/* Toolbar */}
                    <div className="toolbar">
                        <form onSubmit={handleSearch} className="search-form">
                            <div className="search-wrapper" style={{ display: 'flex', alignItems: 'center', position: 'relative' }}>
                                <Search
                                    size={16}
                                    style={{
                                        position: 'absolute',
                                        left: '12px',
                                        color: '#94a3b8',
                                        pointerEvents: 'none',
                                        zIndex: 1
                                    }}
                                />
                                <input
                                    type="text"
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    placeholder="Search projects..."
                                    className="search-input"
                                    style={{ paddingLeft: '36px' }}
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
                                        <th className="th-methodology">Methodology</th>
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
                                            <td className="td-methodology">{project.methodology || '-'}</td>
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
        </div>
    );
}
