import './App.css'

interface DataItem {
  id: number
  name: string
  status: string
  createdDate: string
}

function App() {
  const sampleData: DataItem[] = [
    { id: 1, name: "Sample Study 1", status: "Active", createdDate: "2026-01-15" },
    { id: 2, name: "Sample Study 2", status: "Draft", createdDate: "2026-01-20" },
    { id: 3, name: "Sample Study 3", status: "Active", createdDate: "2026-01-25" },
  ]

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Study Designer - Admin Portal</h1>
        <p className="subtitle">Manage and oversee all studies</p>
      </header>

      <main className="main-content">
        <div className="toolbar">
          <button className="btn-primary">+ New</button>
          <button className="btn-secondary">Delete</button>
          <input type="text" placeholder="Search..." className="search-input" />
        </div>

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th><input type="checkbox" /></th>
                <th>ID</th>
                <th>Name</th>
                <th>Status</th>
                <th>Created Date</th>
              </tr>
            </thead>
            <tbody>
              {sampleData.map((item) => (
                <tr key={item.id}>
                  <td><input type="checkbox" /></td>
                  <td>{item.id}</td>
                  <td>{item.name}</td>
                  <td>
                    <span className={`status-badge status-${item.status.toLowerCase()}`}>
                      {item.status}
                    </span>
                  </td>
                  <td>{new Date(item.createdDate).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="info-panel">
          <h3>Welcome to Admin Portal</h3>
          <p>This is a hello world page demonstrating the admin interface for managing studies.</p>
        </div>
      </main>
    </div>
  )
}

export default App
