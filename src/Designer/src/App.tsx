import { useState } from 'react'
import { ModuleList } from './components/ModuleList'
import { ModuleForm } from './components/ModuleForm'
import type { Module } from './types/module'
import './App.css'

function App() {
  const [selectedModule, setSelectedModule] = useState<Module | null>(null)
  const [showForm, setShowForm] = useState(false)

  const handleSelectModule = async (module: Module) => {
    try {
      // Fetch full module details including questions
      const response = await fetch(`/api/modules/${module.id}`)
      if (response.ok) {
        const fullModule = await response.json()
        setSelectedModule(fullModule)
        setShowForm(true)
      }
    } catch (err) {
      console.error('Error fetching module details:', err)
    }
  }

  const handleNewModule = () => {
    setSelectedModule(null)
    setShowForm(true)
  }

  const handleSave = () => {
    setShowForm(false)
    setSelectedModule(null)
    // Refresh the list
    window.location.reload()
  }

  const handleCancel = () => {
    setShowForm(false)
    setSelectedModule(null)
  }

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>POC Study Designer</h1>
        <p>Module Management</p>
      </header>

      <main className="main-content">
        {!showForm ? (
          <ModuleList 
            onSelectModule={handleSelectModule}
            onNewModule={handleNewModule}
          />
        ) : (
          <ModuleForm
            module={selectedModule}
            onSave={handleSave}
            onCancel={handleCancel}
          />
        )}
      </main>
    </div>
  )
}

export default App
