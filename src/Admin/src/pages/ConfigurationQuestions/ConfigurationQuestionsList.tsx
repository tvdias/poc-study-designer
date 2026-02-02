import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { configurationQuestionsApi } from '../../services/adminApi';
import type { ConfigurationQuestion } from '../../types/entities';
import '../Questions/QuestionsList.css';

function ConfigurationQuestionsList() {
  const [questions, setQuestions] = useState<ConfigurationQuestion[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadQuestions();
  }, []);

  const loadQuestions = async () => {
    try {
      const response = await configurationQuestionsApi.getAll();
      setQuestions(response.data);
    } catch (error) {
      console.error('Failed to load configuration questions:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Configuration Questions</h1>
          <p>Manage product configuration questions</p>
        </div>
        <Link to="/configuration-questions/new" className="btn btn-primary">+ New Question</Link>
      </div>

      {loading ? (
        <div className="loading">Loading questions...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Question</th>
                <th>Rule</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {questions.length === 0 ? (
                <tr><td colSpan={4} className="empty-state">No configuration questions found.</td></tr>
              ) : (
                questions.map((question) => (
                  <tr key={question.id}>
                    <td>{question.question}</td>
                    <td><span className="badge">{question.rule}</span></td>
                    <td><span className={`status-badge status-${question.status.toLowerCase()}`}>{question.status}</span></td>
                    <td className="actions">
                      <Link to={`/configuration-questions/${question.id}`} className="btn-sm btn-secondary">View</Link>
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

export default ConfigurationQuestionsList;
