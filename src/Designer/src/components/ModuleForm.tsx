import { useState, useEffect } from 'react'
import type { Module, Question, ModuleQuestion } from '../types/module'
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

  const [availableQuestions, setAvailableQuestions] = useState<Question[]>([])
  const [moduleQuestions, setModuleQuestions] = useState<ModuleQuestion[]>([])
  const [selectedQuestionId, setSelectedQuestionId] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [activeTab, setActiveTab] = useState<'general' | 'related'>('general')

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
      setModuleQuestions(module.questions || [])
    }
    fetchQuestions()
  }, [module])

  const fetchQuestions = async () => {
    try {
      const response = await fetch('/api/questions')
      if (response.ok) {
        const data = await response.json()
        setAvailableQuestions(data)
      }
    } catch (err) {
      console.error('Error fetching questions:', err)
    }
  }

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

  const handleAddQuestion = async () => {
    if (!selectedQuestionId || !module) return

    try {
      const response = await fetch(`/api/modules/${module.id}/questions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          questionId: selectedQuestionId,
          displayOrder: moduleQuestions.length + 1
        })
      })

      if (!response.ok) {
        throw new Error('Failed to add question')
      }

      // Refresh the module data
      onSave()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add question')
    }
  }

  const handleRemoveQuestion = async (questionId: string) => {
    if (!module) return

    try {
      const response = await fetch(`/api/modules/${module.id}/questions/${questionId}`, {
        method: 'DELETE'
      })

      if (!response.ok) {
        throw new Error('Failed to remove question')
      }

      setModuleQuestions(moduleQuestions.filter(q => q.questionId !== questionId))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove question')
    }
  }

  const handleReorderQuestion = async (questionId: string, direction: 'up' | 'down') => {
    if (!module) return

    const currentIndex = moduleQuestions.findIndex(q => q.questionId === questionId)
    if (currentIndex === -1) return
    
    const newIndex = direction === 'up' ? currentIndex - 1 : currentIndex + 1
    if (newIndex < 0 || newIndex >= moduleQuestions.length) return

    const newOrder = [...moduleQuestions]
    const [movedQuestion] = newOrder.splice(currentIndex, 1)
    newOrder.splice(newIndex, 0, movedQuestion)

    // Update display orders
    const updatedQuestions = newOrder.map((q, index) => ({
      ...q,
      displayOrder: index + 1
    }))

    try {
      const response = await fetch(`/api/modules/${module.id}/questions/reorder`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          questions: updatedQuestions.map(q => ({
            questionId: q.questionId,
            displayOrder: q.displayOrder
          }))
        })
      })

      if (!response.ok) {
        throw new Error('Failed to reorder questions')
      }

      setModuleQuestions(updatedQuestions)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reorder questions')
    }
  }

  return (
    <div className="module-form-container">
      <div className="module-form-header">
        <h2>{module ? 'Edit Module' : 'Create Module'}</h2>
        <button onClick={onCancel} className="btn-close" aria-label="Close form">×</button>
      </div>

      <div className="tabs">
        <button 
          className={`tab ${activeTab === 'general' ? 'active' : ''}`}
          onClick={() => setActiveTab('general')}
        >
          General
        </button>
        {module && (
          <button 
            className={`tab ${activeTab === 'related' ? 'active' : ''}`}
            onClick={() => setActiveTab('related')}
          >
            Related
          </button>
        )}
      </div>

      {error && (
        <div className="error-message" role="alert">
          {error}
        </div>
      )}

      {activeTab === 'general' && (
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
      )}

      {activeTab === 'related' && module && (
        <div className="questions-section">
          <h3>Questions in Module</h3>

          <div className="add-question-section">
            <select
              value={selectedQuestionId}
              onChange={(e) => setSelectedQuestionId(e.target.value)}
              className="question-select"
            >
              <option value="">Select a question...</option>
              {availableQuestions
                .filter(q => !moduleQuestions.some(mq => mq.questionId === q.id))
                .map(q => (
                  <option key={q.id} value={q.id}>
                    {q.variableName} - {q.questionText}
                  </option>
                ))}
            </select>
            <button 
              onClick={handleAddQuestion} 
              className="btn-primary"
              disabled={!selectedQuestionId}
            >
              + Add Question
            </button>
          </div>

          <div className="questions-table">
            <table>
              <thead>
                <tr>
                  <th>Question Variable Name</th>
                  <th>Question Type</th>
                  <th>Question Text</th>
                  <th>Standard or Custom</th>
                  <th>Created By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {moduleQuestions.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="no-data">
                      No questions assigned to this module yet.
                    </td>
                  </tr>
                ) : (
                  moduleQuestions
                    .sort((a, b) => a.displayOrder - b.displayOrder)
                    .map((mq, index) => (
                      <tr key={mq.questionId}>
                        <td>{mq.variableName}</td>
                        <td>{mq.questionType}</td>
                        <td>{mq.questionText}</td>
                        <td>{mq.questionSource}</td>
                        <td>{mq.createdBy}</td>
                        <td className="actions-cell">
                          <button
                            onClick={() => handleReorderQuestion(mq.questionId, 'up')}
                            disabled={index === 0}
                            className="btn-icon"
                            aria-label="Move up"
                          >
                            ↑
                          </button>
                          <button
                            onClick={() => handleReorderQuestion(mq.questionId, 'down')}
                            disabled={index === moduleQuestions.length - 1}
                            className="btn-icon"
                            aria-label="Move down"
                          >
                            ↓
                          </button>
                          <button
                            onClick={() => handleRemoveQuestion(mq.questionId)}
                            className="btn-icon btn-danger"
                            aria-label="Remove question"
                          >
                            ×
                          </button>
                        </td>
                      </tr>
                    ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
