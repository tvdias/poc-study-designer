import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { modulesApi } from '../../services/adminApi';
import type { Module } from '../../types/entities';
import '../Questions/QuestionsList.css';

function ModulesList() {
  const [modules, setModules] = useState<Module[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    loadModules();
  }, [search]);

  const loadModules = async () => {
    try {
      setLoading(true);
      const response = await modulesApi.getAll(search || undefined);
      setModules(response.data);
    } catch (error) {
      console.error('Failed to load modules:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Modules</h1>
          <p>Manage reusable question modules</p>
        </div>
        <Link to="/modules/new" className="btn btn-primary">+ New Module</Link>
      </div>

      <div className="filters">
        <input
          type="text"
          placeholder="Search modules..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="search-input"
        />
      </div>

      {loading ? (
        <div className="loading">Loading modules...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Variable Name</th>
                <th>Label</th>
                <th>Status</th>
                <th>Version</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {modules.length === 0 ? (
                <tr><td colSpan={5} className="empty-state">No modules found.</td></tr>
              ) : (
                modules.map((module) => (
                  <tr key={module.id}>
                    <td><code>{module.variableName}</code></td>
                    <td>{module.label}</td>
                    <td><span className={`status-badge status-${module.status.toLowerCase()}`}>{module.status}</span></td>
                    <td>v{module.version}</td>
                    <td className="actions">
                      <Link to={`/modules/${module.id}`} className="btn-sm btn-secondary">View</Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default ModulesList;
