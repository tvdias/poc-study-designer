import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Zap, ChevronDown, Bell, Plus } from 'lucide-react';
import './DesignerLayout.css';

export function DesignerLayout() {
    const location = useLocation();
    const navigate = useNavigate();
    const isProjectDetail = location.pathname.includes('/projects/') && location.pathname.split('/').length > 2;

    const handleCreateProject = () => {
        navigate('/projects/new');
    };

    return (
        <div className="designer-layout">
            {/* Header */}
            <header className="designer-header" id="main-header">
                <div className="header-left">
                    <span className="service-name">Study Designer</span>
                </div>

                <div className="header-right">
                    {/* Quick Actions Dropdown */}
                    <div className="dropdown-wrapper">
                        <button className="header-action-btn">
                            <Zap size={16} className="text-yellow-400" />
                            <span className="hidden sm:inline">Quick Actions</span>
                            <ChevronDown size={12} className="text-slate-400 ml-1" />
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

                    {/* Create Project Button */}
                    {!isProjectDetail && (
                        <button className="create-btn" onClick={handleCreateProject}>
                            <Plus size={16} />
                            <span className="hidden sm:inline">Create Project</span>
                        </button>
                    )}

                    {/* Notifications */}
                    <button className="notification-btn">
                        <Bell size={20} />
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
            <main className="main-layout">
                <Outlet />
            </main>
        </div>
    );
}
