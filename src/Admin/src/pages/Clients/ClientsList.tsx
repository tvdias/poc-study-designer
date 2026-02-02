import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { clientsApi } from '../../services/adminApi';
import type { Client } from '../../types/entities';
import '../Questions/QuestionsList.css';

function ClientsList() {
  const [clients, setClients] = useState<Client[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadClients();
  }, []);

  const loadClients = async () => {
    try {
      const response = await clientsApi.getAll();
      setClients(response.data);
    } catch (error) {
      console.error('Failed to load clients:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Clients</h1>
          <p>Manage client information</p>
        </div>
        <Link to="/clients/new" className="btn btn-primary">+ New Client</Link>
      </div>

      {loading ? (
        <div className="loading">Loading clients...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Active</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {clients.length === 0 ? (
                <tr><td colSpan={3} className="empty-state">No clients found.</td></tr>
              ) : (
                clients.map((client) => (
                  <tr key={client.id}>
                    <td>{client.name}</td>
                    <td>{client.isActive ? '✓' : ''}</td>
                    <td className="actions">
                      <Link to={`/clients/${client.id}`} className="btn-sm btn-secondary">View</Link>
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

export default ClientsList;
