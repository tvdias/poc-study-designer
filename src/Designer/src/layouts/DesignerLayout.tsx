import { Outlet, useLocation, useNavigate, matchPath } from 'react-router-dom';
import { Zap, ChevronDown, Bell, Plus, ArrowLeft } from 'lucide-react';
import { useState, useEffect } from 'react';
import { projectsApi, type Project } from '../services/api';
import './DesignerLayout.css';

export function DesignerLayout() {
    const location = useLocation();
    const navigate = useNavigate();
    const isProjectDetail = location.pathname.includes('/projects/') && location.pathname.split('/').length > 2;

    const matchUrl = matchPath({ path: '/projects/:id/*' }, location.pathname)
        || matchPath({ path: '/projects/:id' }, location.pathname);
    const projectId = matchUrl?.params?.id;

    const [scrolled, setScrolled] = useState(false);
    const [project, setProject] = useState<Project | null>(null);

    useEffect(() => {
        if (projectId && projectId !== 'new') {
            projectsApi.getById(projectId).then(setProject).catch(console.error);
        } else {
            setProject(null);
        }
    }, [projectId]);

    const handleScroll = (e: React.UIEvent<HTMLElement>) => {
        setScrolled(e.currentTarget.scrollTop > 20);
    };

    const handleCreateProject = () => {
        navigate('/projects/new');
    };

    return (
        <div className={`designer-layout ${scrolled ? 'scrolled' : ''}`}>
            {/* Header */}
            <header className={`designer-header ${scrolled ? 'header-collapsed' : 'header-expanded'}`} id="main-header">
                <div className="header-left hover-cursor-pointer" onClick={() => navigate('/')} style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }} className="flex-shrink-0">
                        <img src="/logo.svg" alt="Study Designer" style={{ height: '24px' }} className="header-logo" />
                        <span className="service-name">Study Designer</span>
                    </div>

                    {project && (
                        <div id="project-info" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', paddingLeft: scrolled ? '0' : '0.5rem', transition: 'opacity 0.2s ease, transform 0.2s ease' }}>
                            <h1 className="project-title-header" style={{ fontSize: scrolled ? '0.875rem' : '1.25rem', fontWeight: scrolled ? 500 : 700, lineHeight: 1.25, margin: 0, color: 'white', transition: 'all 0.2s' }}>
                                {project.name}
                            </h1>
                            <span className="client-name-header" style={{ display: scrolled ? 'none' : 'block', fontSize: '0.75rem', color: '#60a5fa', fontWeight: 500, textTransform: 'uppercase', letterSpacing: '0.05em', marginTop: '0.125rem' }}>
                                {project.clientName}
                            </span>
                        </div>
                    )}
                </div>

                <div className="header-right">
                    {/* Create Project Button */}
                    {!isProjectDetail && (
                        <button className="create-btn" onClick={handleCreateProject}>
                            <Plus size={16} />
                            <span className="hidden sm:inline">Create Project</span>
                        </button>
                    )}

                    {location.pathname !== '/' && (
                        <button className="header-action-btn" onClick={() => navigate('/')} style={{ marginRight: '8px' }}>
                            <ArrowLeft size={16} className="text-slate-400" />
                            <span className="hidden sm:inline">Back to Projects</span>
                        </button>
                    )}

                    {/* Quick Actions Dropdown */}
                    <div className="dropdown-wrapper">
                        <button className="header-action-btn group-hover">
                            <Zap size={16} className="text-yellow-400 zap-icon" />
                            <span className="hidden sm:inline action-text">Quick Actions</span>
                            <ChevronDown size={12} className="text-slate-400 ml-1 chevron-icon" />
                        </button>
                        <div className="dropdown-menu">
                            <div className="dropdown-header">Recent Projects</div>
                            <a href="#" className="dropdown-item">Adidas Q3 Survey</a>
                            <a href="#" className="dropdown-item">Diageo Brand Lift</a>
                            <div className="dropdown-divider"></div>
                            <a href="#" className="dropdown-item">
                                <span>📌 Pinned Items</span>
                            </a>
                        </div>
                    </div>

                    {/* Notifications */}
                    <button className="notification-btn">
                        <Bell size={20} className="bell-icon" />
                        <span className="notification-badge"></span>
                    </button>

                    {/* User Avatar */}
                    <img
                        src="https://api.dicebear.com/7.x/avataaars/svg?seed=Felix"
                        className="user-avatar"
                        alt="User Profile"
                    />
                </div>
            </header>

            {/* Main Content */}
            <main className="main-layout" onScroll={handleScroll}>
                <Outlet context={{ handleScroll }} />
            </main>
        </div>
    );
}
