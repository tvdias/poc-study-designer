import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Trash2, X, List, ChevronRight, ChevronDown } from 'lucide-react';
import {
    managedListsApi,
    type ManagedList,
    type CreateManagedListRequest,
    type ManagedListItemRequest
} from '../services/api';
import './ManagedListsSection.css';

interface ManagedListsSectionProps {
    projectId: string;
}

export function ManagedListsSection({ projectId }: ManagedListsSectionProps) {
    const [managedLists, setManagedLists] = useState<ManagedList[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [expandedListId, setExpandedListId] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Create Form State
    const [createFormData, setCreateFormData] = useState<CreateManagedListRequest>({
        projectId,
        name: '',
        description: ''
    });

    // Item Management State
    const [newItemData, setNewItemData] = useState<ManagedListItemRequest>({
        code: '',
        label: '',
        sortOrder: 0,
        isActive: true
    });
    const [itemSubmitting, setItemSubmitting] = useState(false);

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

    useEffect(() => {
        // Reset sort order when switching lists
        if (expandedListId) {
            const list = managedLists.find(l => l.id === expandedListId);
            if (list) {
                const maxSort = list.items?.length > 0
                    ? Math.max(...list.items.map(i => i.sortOrder)) + 10
                    : 10;
                setNewItemData(prev => ({ ...prev, sortOrder: maxSort }));
            }
        }
    }, [expandedListId, managedLists]);

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            await managedListsApi.create(createFormData);
            await loadManagedLists();
            setShowCreateModal(false);
            setCreateFormData({ projectId, name: '', description: '' });
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
            if (expandedListId === id) setExpandedListId(null);
        } catch {
            alert('Failed to delete managed list');
        }
    };

    const handleAddItem = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!expandedListId) return;

        setItemSubmitting(true);
        try {
            const newItem = await managedListsApi.addItem(expandedListId, newItemData);

            // Update local state to avoid full reload
            setManagedLists(prev => prev.map(list => {
                if (list.id === expandedListId) {
                    return {
                        ...list,
                        items: [...(list.items || []), newItem].sort((a, b) => a.sortOrder - b.sortOrder)
                    };
                }
                return list;
            }));

            // Reset form for next item
            setNewItemData(prev => ({
                code: '',
                label: '',
                sortOrder: prev.sortOrder + 10,
                isActive: true
            }));

            // Focus code input
            const codeInput = document.getElementById('item-code-input');
            if (codeInput) codeInput.focus();

        } catch (err: unknown) {
            if (err instanceof Error) {
                alert(err.message || 'Failed to add item');
            } else {
                alert('Failed to add item');
            }
        } finally {
            setItemSubmitting(false);
        }
    };

    const handleDeleteItem = async (itemId: string) => {
        if (!expandedListId) return;
        if (!confirm('Remove this item?')) return;

        try {
            await managedListsApi.deleteItem(expandedListId, itemId);
            setManagedLists(prev => prev.map(list => {
                if (list.id === expandedListId) {
                    return {
                        ...list,
                        items: list.items.filter(i => i.id !== itemId)
                    };
                }
                return list;
            }));
        } catch {
            alert('Failed to delete item');
        }
    };

    const toggleListExpansion = (id: string) => {
        if (expandedListId === id) {
            setExpandedListId(null);
        } else {
            setExpandedListId(id);
        }
    };

    if (loading) return <div className="loading-container">Loading managed lists...</div>;
    if (error) return <div className="error-container">{error}</div>;

    return (
        <section className="managed-lists-section">
            <div className="section-header">
                <h2 className="section-title">Managed Lists</h2>
                <button className="add-btn" onClick={() => setShowCreateModal(true)}>
                    <Plus size={16} />
                    <span>Create List</span>
                </button>
            </div>

            <div className="lists-container">
                {managedLists.length === 0 ? (
                    <div className="empty-state">
                        <List size={48} className="empty-icon" />
                        <p>No managed lists created yet.</p>
                    </div>
                ) : (
                    <div className="lists-grid">
                        {managedLists.map(list => (
                            <div key={list.id} className={`list-card ${expandedListId === list.id ? 'expanded' : ''}`}>
                                <div className="list-card-header" onClick={() => toggleListExpansion(list.id)}>
                                    <div className="list-info">
                                        <h3 className="list-name">{list.name}</h3>
                                        <div className="list-meta">
                                            <span className={`status-badge ${list.status.toLowerCase()}`}>
                                                {list.status}
                                            </span>
                                            <span className="item-count">
                                                {list.items?.length || 0} items
                                            </span>
                                        </div>
                                    </div>
                                    <div className="list-actions">
                                        <button
                                            className="icon-btn delete-btn"
                                            onClick={(e) => handleDeleteList(list.id, e)}
                                            title="Delete List"
                                        >
                                            <Trash2 size={16} />
                                        </button>
                                        <div className="expand-icon">
                                            {expandedListId === list.id ? <ChevronDown size={20} /> : <ChevronRight size={20} />}
                                        </div>
                                    </div>
                                </div>

                                {expandedListId === list.id && (
                                    <div className="list-items-panel">
                                        <div className="items-header">
                                            <h4>List Items</h4>
                                        </div>

                                        {/* Add Item Form */}
                                        <form className="add-item-form" onSubmit={handleAddItem}>
                                            <div className="form-row">
                                                <input
                                                    id="item-code-input"
                                                    type="text"
                                                    placeholder="Code (e.g. 1)"
                                                    className="form-input code-input"
                                                    value={newItemData.code}
                                                    onChange={e => setNewItemData({ ...newItemData, code: e.target.value })}
                                                    required
                                                />
                                                <input
                                                    type="text"
                                                    placeholder="Label (e.g. Yes)"
                                                    className="form-input label-input"
                                                    value={newItemData.label}
                                                    onChange={e => setNewItemData({ ...newItemData, label: e.target.value })}
                                                    required
                                                />
                                                <input
                                                    type="number"
                                                    placeholder="Order"
                                                    className="form-input order-input"
                                                    value={newItemData.sortOrder}
                                                    onChange={e => setNewItemData({ ...newItemData, sortOrder: parseInt(e.target.value) })}
                                                />
                                                <button type="submit" className="add-item-btn" disabled={itemSubmitting}>
                                                    <Plus size={16} />
                                                </button>
                                            </div>
                                        </form>

                                        {/* Items List */}
                                        <div className="items-list">
                                            {(list.items || []).length === 0 ? (
                                                <p className="no-items">No items added.</p>
                                            ) : (
                                                <table>
                                                    <thead>
                                                        <tr>
                                                            <th>Order</th>
                                                            <th>Code</th>
                                                            <th>Label</th>
                                                            <th>Actions</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        {list.items.map(item => (
                                                            <tr key={item.id}>
                                                                <td>{item.sortOrder}</td>
                                                                <td className="code-cell">{item.code}</td>
                                                                <td>{item.label}</td>
                                                                <td>
                                                                    <button
                                                                        className="icon-btn delete-btn"
                                                                        onClick={() => handleDeleteItem(item.id)}
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
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </div>

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
