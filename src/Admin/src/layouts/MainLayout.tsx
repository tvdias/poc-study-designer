import { NavLink, Outlet } from 'react-router-dom';
import './MainLayout.css';

export function MainLayout() {
    return (
        <div className="layout-container">
            <aside className="sidebar">
                <div className="brand">
                    <h2>Admin Center</h2>
                </div>
                <nav className="nav-menu">
                    <NavLink to="/" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Dashboard
                    </NavLink>
                    <NavLink to="/tags" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Tags
                    </NavLink>
                    <NavLink to="/commissioning-markets" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Commissioning Markets
                    </NavLink>
                    <NavLink to="/fieldwork-markets" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Fieldwork Markets
                    </NavLink>
                </nav>
            </aside>
            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
}
