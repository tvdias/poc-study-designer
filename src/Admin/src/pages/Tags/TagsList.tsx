import { useEffect, useState } from 'react';
import { tagsApi } from '../../services/adminApi';
import type { Tag } from '../../types/entities';
import '../Questions/QuestionsList.css';

function TagsList() {
  const [tags, setTags] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTags();
  }, []);

  const loadTags = async () => {
    try {
      const response = await tagsApi.getAll();
      setTags(response.data);
    } catch (error) {
      console.error('Failed to load tags:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Tags</h1>
          <p>Manage question tags</p>
        </div>
        <button className="btn btn-primary">+ New Tag</button>
      </div>

      {loading ? (
        <div className="loading">Loading tags...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Description</th>
                <th>Active</th>
              </tr>
            </thead>
            <tbody>
              {tags.length === 0 ? (
                <tr><td colSpan={3} className="empty-state">No tags found.</td></tr>
              ) : (
                tags.map((tag) => (
                  <tr key={tag.id}>
                    <td>{tag.name}</td>
                    <td>{tag.description || '-'}</td>
                    <td>{tag.isActive ? '✓' : ''}</td>
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

export default TagsList;
