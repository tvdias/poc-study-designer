import './Dashboard.css';
import { Link } from 'react-router-dom';

function Dashboard() {
  const cards = [
    { title: 'Question Bank', icon: '❓', path: '/questions', description: 'Manage survey questions and answers' },
    { title: 'Modules', icon: '📦', path: '/modules', description: 'Organize questions into reusable modules' },
    { title: 'Products', icon: '🎯', path: '/products', description: 'Configure survey products and templates' },
    { title: 'Clients', icon: '🏢', path: '/clients', description: 'Manage client information' },
    { title: 'Markets', icon: '🌍', path: '/markets', description: 'Commissioning and fieldwork markets' },
    { title: 'Tags', icon: '🏷️', path: '/tags', description: 'Categorize and organize questions' },
    { title: 'Config Questions', icon: '⚙️', path: '/configuration-questions', description: 'Product configuration questions' },
  ];

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Admin Portal Dashboard</h1>
        <p>Manage core reference and metadata for study design</p>
      </div>
      <div className="dashboard-grid">
        {cards.map((card) => (
          <Link to={card.path} key={card.path} className="dashboard-card">
            <div className="card-icon">{card.icon}</div>
            <h3>{card.title}</h3>
            <p>{card.description}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}

export default Dashboard;
