import { useState, useEffect } from 'react';
import { fieldworkMarketsApi } from '../services/api';
import type { FieldworkMarket } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './TagsPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';

export function FieldworkMarketsPage() {
    const [markets, setMarkets] = useState<FieldworkMarket[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedMarket, setSelectedMarket] = useState<FieldworkMarket | null>(null);
    const [formData, setFormData] = useState({ isoCode: '', name: '', isActive: true });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchMarkets();
    }, [search]);

    const fetchMarkets = async () => {
        setIsLoading(true);
        try {
            const data = await fieldworkMarketsApi.getAll(search);
            setMarkets(data);
        } catch (error) {
            console.error('Failed to fetch fieldwork markets', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedMarket(null);
        setFormData({ isoCode: '', name: '', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
    };

    const openView = (market: FieldworkMarket) => {
        setSelectedMarket(market);
        setMode('view');
    };

    const openEdit = (market?: FieldworkMarket) => {
        const target = market || selectedMarket;
        if (!target) return;

        // If opening from row action, ensure selectedMarket is updated
        if (market) setSelectedMarket(market);

        setFormData({ isoCode: target.isoCode, name: target.name, isActive: target.isActive });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedMarket(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedMarket: FieldworkMarket;
            if (mode === 'edit' && selectedMarket) {
                savedMarket = await fieldworkMarketsApi.update(selectedMarket.id, { isoCode: formData.isoCode, name: formData.name, isActive: formData.isActive });
            } else {
                savedMarket = await fieldworkMarketsApi.create({ isoCode: formData.isoCode, name: formData.name });
            }

            // Refresh list and optionally switch to view mode or close
            await fetchMarkets();
            // stay in view mode of the saved market
            setSelectedMarket(savedMarket);
            setMode('view');

        } catch (err: any) {
            if (err.status === 400 && err.errors) {
                setErrors(err.errors);
            } else if (err.status === 409) {
                setServerError(err.detail || "Fieldwork Market already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (market?: FieldworkMarket) => {
        const target = market || selectedMarket;
        if (!target || !confirm(`Are you sure you want to delete fieldwork market '${target.name}'?`)) return;

        try {
            await fieldworkMarketsApi.delete(target.id);
            closePanel();
            fetchMarkets();
        } catch (error) {
            console.error('Failed to delete fieldwork market', error);
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
                <button className="cmd-btn" onClick={fetchMarkets}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search fieldwork markets..."
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
                                <th style={{ width: '120px' }}>ISO Code</th>
                                <th>Name</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {markets.map((market) => (
                                <tr key={market.id} onClick={() => openView(market)} className="clickable-row">
                                    <td>{market.isoCode}</td>
                                    <td>{market.name}</td>
                                    <td>
                                        <span className={`status-text ${market.isActive ? 'active' : 'inactive'}`}>
                                            {market.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(market); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(market); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(market); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {markets.length === 0 && (
                                <tr><td colSpan={4} className="empty-state">No fieldwork markets found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Fieldwork Market' : mode === 'edit' ? 'Edit Fieldwork Market' : selectedMarket?.name || 'Fieldwork Market Details'}
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" type="submit" form="fieldwork-markets-form">Save</button>
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
                {mode === 'view' && selectedMarket && (
                    <div className="view-details">
                        <div className="detail-item">
                            <label>ISO Code</label>
                            <div className="value">{selectedMarket.isoCode}</div>
                        </div>
                        <div className="detail-item">
                            <label>Name</label>
                            <div className="value">{selectedMarket.name}</div>
                        </div>
                        <div className="detail-item">
                            <label>Status</label>
                            <div className="value">
                                {selectedMarket.isActive ? 'Active' : 'Inactive'}
                            </div>
                        </div>
                        <div className="detail-item">
                            <label>ID</label>
                            <div className="value monospace">{selectedMarket.id}</div>
                        </div>
                    </div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form id="fieldwork-markets-form" className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="marketIsoCode">ISO Code</label>
                            <input
                                id="marketIsoCode"
                                type="text"
                                value={formData.isoCode}
                                onChange={(e) => setFormData({ ...formData, isoCode: e.target.value })}
                                className={errors.IsoCode ? 'error' : ''}
                                autoFocus
                            />
                            {errors.IsoCode && <span className="field-error">{errors.IsoCode[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="marketName">Name</label>
                            <input
                                id="marketName"
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                className={errors.Name ? 'error' : ''}
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
