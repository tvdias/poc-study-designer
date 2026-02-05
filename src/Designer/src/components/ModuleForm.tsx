import { useState, useEffect } from 'react'
import type { Module } from '../types/module'
import './ModuleForm.css'

interface ModuleFormProps {
  module?: Module | null;
  onSave: () => void;
  onCancel: () => void;
}

export const ModuleForm = ({ module, onSave, onCancel }: ModuleFormProps) => {
  const [formData, setFormData] = useState({
    variableName: '',
    label: '',
    description: '',
    parentModuleId: '',
    instructions: '',
    status: 'Active',
    statusReason: '',
    isActive: true
  })

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (module) {
      setFormData({
        variableName: module.variableName,
        label: module.label,
        description: module.description || '',
        parentModuleId: module.parentModuleId || '',
        instructions: module.instructions || '',
        status: module.status,
        statusReason: module.statusReason || '',
        isActive: module.isActive
      })
    }
  }, [module])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)

    try {
      const url = module ? `/api/modules/${module.id}` : '/api/modules'
      const method = module ? 'PUT' : 'POST'

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      })

      if (!response.ok) {
        const errorData = await response.json()
        throw new Error(errorData.title || 'Failed to save module')
      }

      onSave()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save module')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="module-form-container">
      <div className="module-form-header">
        <h2>{module ? 'Edit Module' : 'Create Module'}</h2>
        <button onClick={onCancel} className="btn-close" aria-label="Close form">×</button>
      </div>

      {error && (
        <div className="error-message" role="alert">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="module-form">
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="variableName">Module Variable Name *</label>
            <input
              id="variableName"
              type="text"
              value={formData.variableName}
              onChange={(e) => setFormData({ ...formData, variableName: e.target.value })}
              required
              maxLength={100}
              aria-required="true"
            />
          </div>
          <div className="form-group">
            <label htmlFor="label">Module Label *</label>
            <input
              id="label"
              type="text"
              value={formData.label}
              onChange={(e) => setFormData({ ...formData, label: e.target.value })}
              required
              maxLength={100}
              aria-required="true"
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="description">Module Description</label>
          <textarea
            id="description"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            maxLength={500}
            rows={3}
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="versionNumber">Module Version Number</label>
            <input
              id="versionNumber"
              type="number"
              value={module?.versionNumber || 1}
              disabled
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="parentModuleId">Parent Module</label>
            <input
              id="parentModuleId"
              type="text"
              value={formData.parentModuleId}
              onChange={(e) => setFormData({ ...formData, parentModuleId: e.target.value })}
              placeholder="Leave empty for no parent"
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="instructions">Module Instructions</label>
          <textarea
            id="instructions"
            value={formData.instructions}
            onChange={(e) => setFormData({ ...formData, instructions: e.target.value })}
            maxLength={2000}
            rows={4}
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="status">Status *</label>
            <select
              id="status"
              value={formData.status}
              onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              required
              aria-required="true"
            >
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
              <option value="Draft">Draft</option>
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="statusReason">Status Reason</label>
            <input
              id="statusReason"
              type="text"
              value={formData.statusReason}
              onChange={(e) => setFormData({ ...formData, statusReason: e.target.value })}
              maxLength={200}
            />
          </div>
        </div>

        <div className="form-actions">
          <button type="button" onClick={onCancel} className="btn-secondary">
            Cancel
          </button>
          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? 'Saving...' : 'Save Module'}
          </button>
        </div>
      </form>
    </div>
  )
}
