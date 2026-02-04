import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import { TagsPage } from './pages/TagsPage';
import { CommissioningMarketsPage } from './pages/CommissioningMarketsPage';
import { FieldworkMarketsPage } from './pages/FieldworkMarketsPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route index element={<div style={{ padding: '2rem' }}><h2>Dashboard Coming Soon</h2></div>} />
          <Route path="tags" element={<TagsPage />} />
          <Route path="commissioning-markets" element={<CommissioningMarketsPage />} />
          <Route path="fieldwork-markets" element={<FieldworkMarketsPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
