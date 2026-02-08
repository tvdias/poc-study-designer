import { useState, useEffect } from 'react';
import { clientsApi } from '../services/api';
import type { Client } from '../services/api';
import { SidePanel } from '../components/ui/SidePanel';
import { EyeIcon, EditIcon, TrashIcon, RefreshIcon, PlusIcon } from '../components/ui/Icons';
import './ClientsPage.css';

type Mode = 'list' | 'view' | 'create' | 'edit';

export function ClientsPage() {
    const [clients, setClients] = useState<Client[]>([]);
    const [search, setSearch] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // Panel State
    const [mode, setMode] = useState<Mode>('list');
    const [selectedClient, setSelectedClient] = useState<Client | null>(null);
    const [formData, setFormData] = useState({
        accountName: '',
        companyNumber: '',
        customerNumber: '',
        companyCode: '',
        isActive: true
    });

    // Error State
    const [errors, setErrors] = useState<Record<string, string[]>>({});
    const [serverError, setServerError] = useState<string>('');

    useEffect(() => {
        fetchClients();
    }, [search]);

    const fetchClients = async () => {
        setIsLoading(true);
        try {
            const data = await clientsApi.getAll(search);
            setClients(data);
        } catch (error) {
            console.error('Failed to fetch clients', error);
        } finally {
            setIsLoading(false);
        }
    };

    // --- Actions ---

    const openCreate = () => {
        setSelectedClient(null);
        setFormData({ accountName: '', companyNumber: '', customerNumber: '', companyCode: '', isActive: true });
        setErrors({});
        setServerError('');
        setMode('create');
    };

    const openView = (client: Client) => {
        setSelectedClient(client);
        setMode('view');
    };

    const openEdit = (client?: Client) => {
        const target = client || selectedClient;
        if (!target) return;

        // If opening from row action, ensure selectedClient is updated
        if (client) setSelectedClient(client);

        setFormData({
            accountName: target.accountName,
            companyNumber: target.companyNumber || '',
            customerNumber: target.customerNumber || '',
            companyCode: target.companyCode || '',
            isActive: target.isActive
        });
        setErrors({});
        setServerError('');
        setMode('edit');
    };

    const closePanel = () => {
        setMode('list');
        setSelectedClient(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrors({});
        setServerError('');

        try {
            let savedClient: Client;
            const requestData = {
                accountName: formData.accountName,
                companyNumber: formData.companyNumber || null,
                customerNumber: formData.customerNumber || null,
                companyCode: formData.companyCode || null,
                isActive: formData.isActive
            };

            if (mode === 'edit' && selectedClient) {
                savedClient = await clientsApi.update(selectedClient.id, requestData);
            } else {
                savedClient = await clientsApi.create(requestData);
            }

            // Refresh list and optionally switch to view mode or close
            await fetchClients();
            // stay in view mode of the saved client
            setSelectedClient(savedClient);
            setMode('view');

        } catch (err: any) {
            if (err.status === 400 && err.errors) {
                setErrors(err.errors);
            } else if (err.status === 409) {
                setServerError(err.detail || "Client already exists");
            } else {
                setServerError("An unexpected error occurred.");
            }
        }
    };

    const handleDelete = async (client?: Client) => {
        const target = client || selectedClient;
        if (!target || !confirm(`Are you sure you want to delete client '${target.accountName}'?`)) return;

        try {
            await clientsApi.delete(target.id);
            closePanel();
            fetchClients();
        } catch (error) {
            console.error('Failed to delete client', error);
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
                <button className="cmd-btn" onClick={fetchClients}>
                    <RefreshIcon /> <span className="label">Refresh</span>
                </button>
                <div className="separator"></div>
                <div className="search-box">
                    <input
                        type="text"
                        placeholder="Search clients..."
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
                                <th>Account Name</th>
                                <th>Customer Number</th>
                                <th>Company Number</th>
                                <th>Company Code</th>
                                <th>Created On</th>
                                <th style={{ width: '100px' }}>Status</th>
                                <th style={{ width: '150px' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {clients.map((client) => (
                                <tr key={client.id} onClick={() => openView(client)} className="clickable-row">
                                    <td>{client.accountName}</td>
                                    <td>{client.customerNumber || '-'}</td>
                                    <td>{client.companyNumber || '-'}</td>
                                    <td>{client.companyCode || '-'}</td>
                                    <td>{new Date(client.createdOn).toLocaleDateString()}</td>
                                    <td>
                                        <span className={`status-text ${client.isActive ? 'active' : 'inactive'}`}>
                                            {client.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td>
                                        <div className="row-actions">
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openView(client); }} title="View">
                                                <EyeIcon />
                                            </button>
                                            <button className="action-btn" onClick={(e) => { e.stopPropagation(); openEdit(client); }} title="Edit">
                                                <EditIcon />
                                            </button>
                                            <button className="action-btn danger" onClick={(e) => { e.stopPropagation(); handleDelete(client); }} title="Delete">
                                                <TrashIcon />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {clients.length === 0 && (
                                <tr><td colSpan={7} className="empty-state">No clients found.</td></tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            {/* Side Panel for Create/Edit/View */}
            <SidePanel
                isOpen={mode !== 'list'}
                onClose={closePanel}
                title={mode === 'create' ? 'New Client' : mode === 'edit' ? 'Edit Client' : selectedClient?.accountName || 'Client Details'}
                footer={
                    (mode === 'create' || mode === 'edit') ? (
                        <>
                            <button className="btn primary" type="submit" form="clients-form">Save</button>
                            <button type="button" className="btn" onClick={mode === 'edit' ? () => setMode('view') : closePanel}>Cancel</button>
                        </>
                    ) : (
                        mode === 'view' && (
                            <>
                                <button type="button" className="btn primary" onClick={() => openEdit()}>Edit</button>
                                <button type="button" className="btn danger" onClick={() => handleDelete()}>Delete</button>
                            </>
                        )
                    )
                }
            >
                {/* View Mode */}
                {mode === 'view' && selectedClient && (
                    <div className="view-details">
                        <div className="detail-item">
                            <label>Account Name</label>
                            <div className="value">{selectedClient.accountName}</div>
                        </div>
                        <div className="detail-item">
                            <label>Company Number</label>
                            <div className="value">{selectedClient.companyNumber || 'N/A'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Customer Number</label>
                            <div className="value">{selectedClient.customerNumber || 'N/A'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Company Code</label>
                            <div className="value">{selectedClient.companyCode || 'N/A'}</div>
                        </div>
                        <div className="detail-item">
                            <label>Created On</label>
                            <div className="value">{new Date(selectedClient.createdOn).toLocaleString()}</div>
                        </div>
                        <div className="detail-item">
                            <label>Status</label>
                            <div className="value">
                                {selectedClient.isActive ? 'Active' : 'Inactive'}
                            </div>
                        </div>
                        <div className="detail-item">
                            <label>ID</label>
                            <div className="value monospace">{selectedClient.id}</div>
                        </div>
                    </div>
                )}

                {/* Form Mode */}
                {(mode === 'create' || mode === 'edit') && (
                    <form id="clients-form" className="panel-form" onSubmit={handleSubmit}>
                        <div className="form-field">
                            <label htmlFor="accountName">Account Name *</label>
                            <input
                                id="accountName"
                                type="text"
                                value={formData.accountName}
                                onChange={(e) => setFormData({ ...formData, accountName: e.target.value })}
                                className={errors.AccountName ? 'error' : ''}
                                autoFocus
                            />
                            {errors.AccountName && <span className="field-error">{errors.AccountName[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="companyNumber">Company Number</label>
                            <input
                                id="companyNumber"
                                type="text"
                                value={formData.companyNumber}
                                onChange={(e) => setFormData({ ...formData, companyNumber: e.target.value })}
                                className={errors.CompanyNumber ? 'error' : ''}
                                placeholder="Enter company number (optional)"
                            />
                            {errors.CompanyNumber && <span className="field-error">{errors.CompanyNumber[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="customerNumber">Customer Number *</label>
                            <input
                                id="customerNumber"
                                type="text"
                                value={formData.customerNumber}
                                onChange={(e) => setFormData({ ...formData, customerNumber: e.target.value })}
                                className={errors.CustomerNumber ? 'error' : ''}
                                placeholder="Enter customer number"
                            />
                            {errors.CustomerNumber && <span className="field-error">{errors.CustomerNumber[0]}</span>}
                        </div>

                        <div className="form-field">
                            <label htmlFor="companyCode">Company Code</label>
                            <input
                                id="companyCode"
                                type="text"
                                value={formData.companyCode}
                                onChange={(e) => setFormData({ ...formData, companyCode: e.target.value })}
                                className={errors.CompanyCode ? 'error' : ''}
                                placeholder="Enter company code (optional)"
                            />
                            {errors.CompanyCode && <span className="field-error">{errors.CompanyCode[0]}</span>}
                        </div>

                        {mode === 'edit' && selectedClient && (
                            <div className="form-field">
                                <label htmlFor="createdOn">Created On</label>
                                <input
                                    id="createdOn"
                                    type="text"
                                    value={new Date(selectedClient.createdOn).toLocaleString()}
                                    readOnly
                                    disabled
                                    className="readonly-field"
                                />
                            </div>
                        )}

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
