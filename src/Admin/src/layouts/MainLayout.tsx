import { NavLink, Outlet } from 'react-router-dom';
import './MainLayout.css';

export function MainLayout() {
    return (
        <div className="layout-container">
            <aside className="sidebar">
                <div className="brand">
                    <img src="/logo.svg" alt="Logo" style={{ height: '28px' }} />
                    <h2>Study Designer Administration</h2>
                </div>
                <nav className="nav-menu">

                    <NavLink to="/question-bank" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Question Bank
                    </NavLink>
                    <NavLink to="/modules" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Modules
                    </NavLink>
                    <NavLink to="/products" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Products
                    </NavLink>
                    <NavLink to="/product-templates" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Product Templates
                    </NavLink>
                    <NavLink to="/configuration-questions" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Configuration Questions
                    </NavLink>
                    <NavLink to="/clients" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Clients
                    </NavLink>
                    <NavLink to="/commissioning-markets" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Commissioning Markets
                    </NavLink>
                    <NavLink to="/fieldwork-markets" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Fieldwork Markets
                    </NavLink>
                    <NavLink to="/tags" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
                        Tags
                    </NavLink>
                </nav>
            </aside>
            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
}
