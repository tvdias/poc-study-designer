import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import './App.css'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import QuestionsList from './pages/Questions/QuestionsList'
import QuestionDetail from './pages/Questions/QuestionDetail'
import ModulesList from './pages/Modules/ModulesList'
import ModuleDetail from './pages/Modules/ModuleDetail'
import ClientsList from './pages/Clients/ClientsList'
import ClientDetail from './pages/Clients/ClientDetail'
import TagsList from './pages/Tags/TagsList'
import MarketsPage from './pages/Markets/MarketsPage'
import ProductsList from './pages/Products/ProductsList'
import ProductDetail from './pages/Products/ProductDetail'
import ConfigurationQuestionsList from './pages/ConfigurationQuestions/ConfigurationQuestionsList'
import ConfigurationQuestionDetail from './pages/ConfigurationQuestions/ConfigurationQuestionDetail'

function App() {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/questions" element={<QuestionsList />} />
          <Route path="/questions/:id" element={<QuestionDetail />} />
          <Route path="/modules" element={<ModulesList />} />
          <Route path="/modules/:id" element={<ModuleDetail />} />
          <Route path="/clients" element={<ClientsList />} />
          <Route path="/clients/:id" element={<ClientDetail />} />
          <Route path="/tags" element={<TagsList />} />
          <Route path="/markets" element={<MarketsPage />} />
          <Route path="/products" element={<ProductsList />} />
          <Route path="/products/:id" element={<ProductDetail />} />
          <Route path="/configuration-questions" element={<ConfigurationQuestionsList />} />
          <Route path="/configuration-questions/:id" element={<ConfigurationQuestionDetail />} />
        </Routes>
      </Layout>
    </Router>
  )
}

export default App
