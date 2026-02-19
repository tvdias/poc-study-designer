import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import { TagsPage } from './pages/TagsPage';
import { CommissioningMarketsPage } from './pages/CommissioningMarketsPage';
import { FieldworkMarketsPage } from './pages/FieldworkMarketsPage';
import { ModulesPage } from './pages/ModulesPage';
import { ClientsPage } from './pages/ClientsPage';
import { ConfigurationQuestionsPage } from './pages/ConfigurationQuestionsPage';
import { ProductsPage } from './pages/ProductsPage';
import { ProductTemplatesPage } from './pages/ProductTemplatesPage';
import { QuestionBankPage } from './pages/QuestionBankPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Navigate to="question-bank" replace />} />
          <Route path="tags" element={<TagsPage />} />
          <Route path="commissioning-markets" element={<CommissioningMarketsPage />} />
          <Route path="fieldwork-markets" element={<FieldworkMarketsPage />} />
          <Route path="modules" element={<ModulesPage />} />
          <Route path="clients" element={<ClientsPage />} />
          <Route path="configuration-questions" element={<ConfigurationQuestionsPage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="product-templates" element={<ProductTemplatesPage />} />
          <Route path="question-bank" element={<QuestionBankPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
