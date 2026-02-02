import { Link } from 'react-router-dom';
import './Layout.css';

interface LayoutProps {
  children: React.ReactNode;
}

function Layout({ children }: LayoutProps) {
  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="sidebar-header">
          <h1>Admin Portal</h1>
        </div>
        <nav className="sidebar-nav">
          <Link to="/" className="nav-link">
            <span>📊</span> Dashboard
          </Link>
          <div className="nav-section">
            <div className="nav-section-title">Core Data</div>
            <Link to="/questions" className="nav-link">
              <span>❓</span> Question Bank
            </Link>
            <Link to="/modules" className="nav-link">
              <span>📦</span> Modules
            </Link>
            <Link to="/products" className="nav-link">
              <span>🎯</span> Products
            </Link>
            <Link to="/configuration-questions" className="nav-link">
              <span>⚙️</span> Config Questions
            </Link>
          </div>
          <div className="nav-section">
            <div className="nav-section-title">Reference Data</div>
            <Link to="/clients" className="nav-link">
              <span>🏢</span> Clients
            </Link>
            <Link to="/markets" className="nav-link">
              <span>🌍</span> Markets
            </Link>
            <Link to="/tags" className="nav-link">
              <span>🏷️</span> Tags
            </Link>
          </div>
        </nav>
      </aside>
      <main className="main-content">
        {children}
      </main>
    </div>
  );
}

export default Layout;
