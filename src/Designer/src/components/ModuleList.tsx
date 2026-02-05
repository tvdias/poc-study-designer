import { useState, useEffect } from 'react'
import type { Module } from '../types/module'
import './ModuleList.css'

interface ModuleListProps {
  onSelectModule: (module: Module) => void;
  onNewModule: () => void;
}

export const ModuleList = ({ onSelectModule, onNewModule }: ModuleListProps) => {
  const [modules, setModules] = useState<Module[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')

  const fetchModules = async (query?: string) => {
    setLoading(true)
    setError(null)
    
    try {
      const url = query 
        ? `/api/modules?query=${encodeURIComponent(query)}`
        : '/api/modules'
      const response = await fetch(url)
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      
      const data: Module[] = await response.json()
      setModules(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch modules')
      console.error('Error fetching modules:', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchModules()
  }, [])

  const handleSearch = () => {
    fetchModules(searchQuery)
  }

  return (
    <div className="module-list-container">
      <div className="module-list-header">
        <h2>Module Management</h2>
        <button 
          className="btn-primary"
          onClick={onNewModule}
          aria-label="Create new module"
        >
          + New Module
        </button>
      </div>

      <div className="search-bar">
        <input
          type="text"
          placeholder="Search modules..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          aria-label="Search modules"
        />
        <button onClick={handleSearch}>Search</button>
      </div>

      {error && (
        <div className="error-message" role="alert">
          {error}
        </div>
      )}

      {loading && modules.length === 0 ? (
        <div className="loading" role="status">
          Loading modules...
        </div>
      ) : (
        <div className="module-grid">
          {modules.length === 0 ? (
            <div className="no-modules">
              No modules found. Create your first module to get started.
            </div>
          ) : (
            modules.map((module) => (
              <article 
                key={module.id} 
                className="module-card"
                onClick={() => onSelectModule(module)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => e.key === 'Enter' && onSelectModule(module)}
                aria-label={`Module: ${module.label}`}
              >
                <div className="module-card-header">
                  <h3>{module.variableName}</h3>
                  <span className={`status-badge ${module.status.toLowerCase()}`}>
                    {module.status}
                  </span>
                </div>
                <div className="module-card-body">
                  <p className="module-label">{module.label}</p>
                  {module.description && (
                    <p className="module-description">{module.description}</p>
                  )}
                  <div className="module-meta">
                    <span>Version: {module.versionNumber}</span>
                  </div>
                </div>
              </article>
            ))
          )}
        </div>
      )}
    </div>
  )
}
