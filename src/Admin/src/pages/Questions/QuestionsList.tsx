import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { questionsApi } from '../../services/adminApi';
import type { Question } from '../../types/entities';
import './QuestionsList.css';

function QuestionsList() {
  const [questions, setQuestions] = useState<Question[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    loadQuestions();
  }, [search, typeFilter, statusFilter]);

  const loadQuestions = async () => {
    try {
      setLoading(true);
      const response = await questionsApi.getAll(search || undefined, typeFilter || undefined, statusFilter || undefined);
      setQuestions(response.data);
    } catch (error) {
      console.error('Failed to load questions:', error);
      alert('Failed to load questions');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this question?')) return;

    try {
      await questionsApi.delete(id);
      loadQuestions();
    } catch (error) {
      console.error('Failed to delete question:', error);
      alert('Failed to delete question');
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Question Bank</h1>
          <p>Manage survey questions and answers</p>
        </div>
        <Link to="/questions/new" className="btn btn-primary">
          + New Question
        </Link>
      </div>

      <div className="filters">
        <input
          type="text"
          placeholder="Search questions..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="search-input"
        />
        <select
          value={typeFilter}
          onChange={(e) => setTypeFilter(e.target.value)}
          className="filter-select"
        >
          <option value="">All Types</option>
          <option value="Single">Single Choice</option>
          <option value="Multi">Multiple Choice</option>
          <option value="Open">Open Text</option>
          <option value="Grid">Grid</option>
        </select>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="filter-select"
        >
          <option value="">All Status</option>
          <option value="Draft">Draft</option>
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
          <option value="Archived">Archived</option>
        </select>
      </div>

      {loading ? (
        <div className="loading">Loading questions...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Variable Name</th>
                <th>Title</th>
                <th>Type</th>
                <th>Status</th>
                <th>Version</th>
                <th>Standard</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {questions.length === 0 ? (
                <tr>
                  <td colSpan={7} className="empty-state">
                    No questions found. Create your first question to get started.
                  </td>
                </tr>
              ) : (
                questions.map((question) => (
                  <tr key={question.id}>
                    <td><code>{question.variableName}</code></td>
                    <td>{question.title}</td>
                    <td><span className="badge">{question.type}</span></td>
                    <td><span className={`status-badge status-${question.status.toLowerCase()}`}>{question.status}</span></td>
                    <td>v{question.version}</td>
                    <td>{question.isStandard ? '✓' : ''}</td>
                    <td className="actions">
                      <Link to={`/questions/${question.id}`} className="btn-sm btn-secondary">
                        View
                      </Link>
                      <button 
                        onClick={() => handleDelete(question.id)}
                        className="btn-sm btn-danger"
                      >
                        Delete
                      </button>
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

export default QuestionsList;
