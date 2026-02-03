import { useState, useEffect } from 'react';
import { tagsApi } from '../services/api';
import type { Tag } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './TagsPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';

export function TagsPage() {
    const [tags, setTags] = useState<Tag[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedTag, setSelectedTag] = useState<Tag | null>(null);
    const [formData, setFormData] = useState({ name: '', isActive: true });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchTags();
    }, [search]);

    const fetchTags = async () => {
        setIsLoading(true);
        try {
            const data = await tagsApi.getAll(search);
            setTags(data);
        } catch (error) {
            console.error('Failed to fetch tags', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedTag(null);
        setFormData({ name: '', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
    };

    const openView = (tag: Tag) => {
        setSelectedTag(tag);
        setMode('view');
    };

    const openEdit = (tag?: Tag) => {
        const target = tag || selectedTag;
        if (!target) return;

        // If opening from row action, ensure selectedTag is updated
        if (tag) setSelectedTag(tag);

        setFormData({ name: target.name, isActive: target.isActive });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedTag(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedTag: Tag;
            if (mode === 'edit' && selectedTag) {
                savedTag = await tagsApi.update(selectedTag.id, { name: formData.name, isActive: formData.isActive });
            } else {
                savedTag = await tagsApi.create({ name: formData.name });
            }

            // Refresh list and optionally switch to view mode or close
            await fetchTags();
            // stay in view mode of the saved tag
            setSelectedTag(savedTag);
            setMode('view');

        } catch (err: any) {
            if (err.status === 400 && err.errors) {
                setErrors(err.errors);
            } else if (err.status === 409) {
                setServerError(err.detail || "Tag already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (tag?: Tag) => {
        const target = tag || selectedTag;
        if (!target || !confirm(`Are you sure you want to delete tag '${target.name}'?`)) return;

        try {
            await tagsApi.delete(target.id);
            closePanel();
            fetchTags();
        } catch (error) {
            console.error('Failed to delete tag', error);
        }
    };

    // --- Renders ---

    return (
        <div className="page-container">
            {/* Command Bar */}
            <div className="command-bar card">
                <button className="cmd-btn primary" onClick={openCreate}>
                    <PlusIcon /> <span className="label">New</span>
                </button>
                <button className="cmd-btn" onClick={fetchTags}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search tags..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Content Area */}
            <div className="content-area card">
                {isLoading ? (
                    <div className="loading-state">Loading...</div>
                ) : (
                    <table className="details-list">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tags.map((tag) => (
                                <tr key={tag.id} onClick={() => openView(tag)} className="clickable-row">
                                    <td>{tag.name}</td>
                                    <td>
                                        <span className={`status-text ${tag.isActive ? 'active' : 'inactive'}`}>
                                            {tag.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(tag); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(tag); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(tag); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {tags.length === 0 && (
                                <tr><td colSpan={3} className="empty-state">No tags found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Tag' : mode === 'edit' ? 'Edit Tag' : selectedTag?.name || 'Tag Details'}
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" onClick={(e) => handleSubmit(e as React.FormEvent)}>Save</button>
                            <button className="btn" onClick={mode === 'edit' ? () => setMode('view') : closePanel}>Cancel</button>
                        </>
                    ) : (
                        mode === 'view' && (
                            <>
                                <button className="btn primary" onClick={() => openEdit()}>Edit</button>
                                <button className="btn danger" onClick={() => handleDelete()}>Delete</button>
                            </>
                        )
                    )
                }
            >
                {/* View Mode */}
                {mode === 'view' && selectedTag && (
                    <div className="view-details">
                        <div className="detail-item">
                            <label>Name</label>
                            <div className="value">{selectedTag.name}</div>
                        </div>
                        <div className="detail-item">
                            <label>Status</label>
                            <div className="value">
                                {selectedTag.isActive ? 'Active' : 'Inactive'}
                            </div>
                        </div>
                        <div className="detail-item">
                            <label>ID</label>
                            <div className="value monospace">{selectedTag.id}</div>
                        </div>
                    </div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="tagName">Name</label>
                            <input
                                id="tagName"
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className={errors.Name ? 'error' : ''}
                                autoFocus
                            />
                            {errors.Name && <span className="field-error">{errors.Name[0]}</span>}
                        </div>

                        {mode === 'edit' && (
                            <div className="form-field checkbox">
                                <label>
                                    <input
                                        type="checkbox"
                                        checked={formData.isActive}
                                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                    />
                                    <span>Is Active</span>
                                </label>
                            </div>
                        )}

                        {serverError && <div className="server-error">{serverError}</div>}
                    </form>
                )}
            </SidePanel>
        </div>
    );
}
