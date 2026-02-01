import './App.css'

interface DataItem {
  id: number
  name: string
  type: string
  lastModified: string
}

function App() {
  const sampleData: DataItem[] = [
    { id: 1, name: "Study Design 1", type: "Clinical Trial", lastModified: "2026-02-01" },
    { id: 2, name: "Study Design 2", type: "Survey", lastModified: "2026-01-30" },
    { id: 3, name: "Study Design 3", type: "Observational", lastModified: "2026-01-28" },
  ]

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Study Designer</h1>
        <p className="subtitle">Create and design studies</p>
      </header>

      <main className="main-content">
        <div className="toolbar">
          <button className="btn-primary">+ New Study</button>
          <button className="btn-secondary">Import</button>
          <input type="text" placeholder="Search studies..." className="search-input" />
        </div>

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th><input type="checkbox" /></th>
                <th>ID</th>
                <th>Study Name</th>
                <th>Type</th>
                <th>Last Modified</th>
              </tr>
            </thead>
            <tbody>
              {sampleData.map((item) => (
                <tr key={item.id}>
                  <td><input type="checkbox" /></td>
                  <td>{item.id}</td>
                  <td>{item.name}</td>
                  <td>{item.type}</td>
                  <td>{new Date(item.lastModified).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="info-panel">
          <h3>Welcome to Study Designer</h3>
          <p>This is a hello world page for designing and creating studies.</p>
        </div>
      </main>
    </div>
  )
}

export default App
