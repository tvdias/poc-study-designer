import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, RefreshCw, X, List, ChevronDown, ChevronUp, ChevronsUpDown } from 'lucide-react';
import {
    managedListsApi,
    type ManagedList,
    type CreateManagedListRequest
} from '../services/api';
import './ManagedListsSection.css';

interface ManagedListsSectionProps {
    projectId: string;
}

type SortField = 'name' | 'status' | 'itemCount' | 'description';
type SortDirection = 'asc' | 'desc';

export function ManagedListsSection({ projectId }: ManagedListsSectionProps) {
    const navigate = useNavigate();
    const [managedLists, setManagedLists] = useState<ManagedList[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
    const [sortField, setSortField] = useState<SortField>('name');
    const [sortDir, setSortDir] = useState<SortDirection>('asc');

    // Create Form State
    const [createFormData, setCreateFormData] = useState<CreateManagedListRequest>({
        projectId,
        name: '',
        description: ''
    });

    const loadManagedLists = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await managedListsApi.getAll(projectId);
            setManagedLists(data);
        } catch (err) {
            setError('Failed to load managed lists');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [projectId]);

    useEffect(() => {
        loadManagedLists();
    }, [loadManagedLists]);

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            const created = await managedListsApi.create(createFormData);
            await loadManagedLists();
            setShowCreateModal(false);
            setCreateFormData({ projectId, name: '', description: '' });
            // Navigate directly to the new list
            navigate(`/projects/${projectId}/managed-lists/${created.id}`);
        } catch (err: unknown) {
            if (err instanceof Error) {
                alert(err.message || 'Failed to create managed list');
            } else {
                alert('Failed to create managed list');
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDeleteList = async (id: string, e: React.MouseEvent) => {
        e.stopPropagation();
        if (!confirm('Are you sure you want to delete this managed list? This cannot be undone.')) return;
        try {
            await managedListsApi.delete(id);
            setManagedLists(prev => prev.filter(l => l.id !== id));
        } catch {
            alert('Failed to delete managed list');
        }
    };

    const toggleSort = (field: SortField) => {
        if (sortField === field) {
            setSortDir(d => d === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDir('asc');
        }
    };

    const sortedLists = [...managedLists].sort((a, b) => {
        let aVal: string | number = '';
        let bVal: string | number = '';
        switch (sortField) {
            case 'name': aVal = a.name; bVal = b.name; break;
            case 'status': aVal = a.status || ''; bVal = b.status || ''; break;
            case 'itemCount': aVal = a.items?.length || 0; bVal = b.items?.length || 0; break;
            case 'description': aVal = a.description || ''; bVal = b.description || ''; break;
        }
        if (aVal < bVal) return sortDir === 'asc' ? -1 : 1;
        if (aVal > bVal) return sortDir === 'asc' ? 1 : -1;
        return 0;
    });

    const allChecked = sortedLists.length > 0 && selectedIds.size === sortedLists.length;
    const someChecked = selectedIds.size > 0 && !allChecked;

    const toggleAll = () => {
        if (allChecked || someChecked) {
            setSelectedIds(new Set());
        } else {
            setSelectedIds(new Set(sortedLists.map(l => l.id)));
        }
    };

    const toggleRow = (id: string) => {
        setSelectedIds(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    };

    const SortIcon = ({ field }: { field: SortField }) => {
        if (sortField !== field) return <ChevronsUpDown size={13} className="sort-icon sort-icon--inactive" />;
        return sortDir === 'asc'
            ? <ChevronUp size={13} className="sort-icon" />
            : <ChevronDown size={13} className="sort-icon" />;
    };

    if (loading) return <div className="loading-container">Loading managed lists...</div>;
    if (error) return <div className="error-container">{error}</div>;

    return (
        <section className="managed-lists-section">
            {/* Toolbar */}
            <div className="list-toolbar">
                <div className="list-toolbar__left">
                    <span className="list-view-label">Active Managed Lists</span>
                    <ChevronDown size={15} className="list-view-chevron" />
                </div>
                <div className="list-toolbar__right">
                    <button className="toolbar-btn toolbar-btn--primary" onClick={() => setShowCreateModal(true)}>
                        <Plus size={15} />
                        New Managed List
                    </button>
                    <button className="toolbar-btn" onClick={loadManagedLists} title="Refresh">
                        <RefreshCw size={15} />
                        Refresh
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="list-table-wrapper">
                {managedLists.length === 0 ? (
                    <div className="empty-state">
                        <List size={48} className="empty-icon" />
                        <p>No managed lists created yet.</p>
                    </div>
                ) : (
                    <table className="list-table">
                        <thead>
                            <tr>
                                <th className="col-check">
                                    <input
                                        type="checkbox"
                                        checked={allChecked}
                                        ref={el => { if (el) el.indeterminate = someChecked; }}
                                        onChange={toggleAll}
                                    />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('name')}>
                                    Name <SortIcon field="name" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('status')}>
                                    Status <SortIcon field="status" />
                                </th>
                                <th className="col-sortable col-num" onClick={() => toggleSort('itemCount')}>
                                    Question Count <SortIcon field="itemCount" />
                                </th>
                                <th className="col-sortable" onClick={() => toggleSort('description')}>
                                    Description <SortIcon field="description" />
                                </th>
                                <th className="col-actions"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedLists.map(list => (
                                <tr
                                    key={list.id}
                                    className={`${selectedIds.has(list.id) ? 'row-selected' : ''} row-clickable`}
                                    onClick={() => navigate(`/projects/${projectId}/managed-lists/${list.id}`)}
                                >
                                    <td className="col-check" onClick={e => e.stopPropagation()}>
                                        <input
                                            type="checkbox"
                                            checked={selectedIds.has(list.id)}
                                            onChange={() => toggleRow(list.id)}
                                        />
                                    </td>
                                    <td className="col-link">{list.name}</td>
                                    <td>
                                        <span className={`status-pill status-pill--${list.status.toLowerCase()}`}>
                                            {list.status}
                                        </span>
                                    </td>
                                    <td className="col-num">{list.items?.length || 0}</td>
                                    <td>{list.description || '—'}</td>
                                    <td className="col-actions" onClick={e => e.stopPropagation()}>
                                        <button
                                            className="icon-btn delete-btn"
                                            onClick={(e) => handleDeleteList(list.id, e)}
                                            title="Delete List"
                                        >
                                            <X size={14} />
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            {managedLists.length > 0 && (
                <div className="list-footer">
                    Rows: {managedLists.length}
                </div>
            )}

            {/* Create Modal */}
            {showCreateModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h3>Create Managed List</h3>
                            <button className="close-btn" onClick={() => setShowCreateModal(false)}>
                                <X size={20} />
                            </button>
                        </div>
                        <form onSubmit={handleCreateSubmit}>
                            <div className="modal-body">
                                <div className="form-group">
                                    <label>List Name <span className="required">*</span></label>
                                    <input
                                        type="text"
                                        className="form-input"
                                        value={createFormData.name}
                                        onChange={e => setCreateFormData({ ...createFormData, name: e.target.value })}
                                        required
                                        autoFocus
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Description</label>
                                    <textarea
                                        className="form-textarea"
                                        value={createFormData.description || ''}
                                        onChange={e => setCreateFormData({ ...createFormData, description: e.target.value })}
                                        rows={3}
                                    />
                                </div>
                            </div>
                            <div className="modal-footer">
                                <button type="button" className="cancel-btn" onClick={() => setShowCreateModal(false)}>
                                    Cancel
                                </button>
                                <button type="submit" className="save-btn" disabled={isSubmitting}>
                                    {isSubmitting ? 'Creating...' : 'Create List'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </section>
    );
}
