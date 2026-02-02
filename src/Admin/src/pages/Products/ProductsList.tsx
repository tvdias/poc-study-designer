import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { productsApi } from '../../services/adminApi';
import type { Product } from '../../types/entities';
import '../Questions/QuestionsList.css';

function ProductsList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      const response = await productsApi.getAll();
      setProducts(response.data);
    } catch (error) {
      console.error('Failed to load products:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Products</h1>
          <p>Manage survey products and templates</p>
        </div>
        <Link to="/products/new" className="btn btn-primary">+ New Product</Link>
      </div>

      {loading ? (
        <div className="loading">Loading products...</div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Version</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr><td colSpan={4} className="empty-state">No products found.</td></tr>
              ) : (
                products.map((product) => (
                  <tr key={product.id}>
                    <td>{product.name}</td>
                    <td><span className={`status-badge status-${product.status.toLowerCase()}`}>{product.status}</span></td>
                    <td>v{product.version}</td>
                    <td className="actions">
                      <Link to={`/products/${product.id}`} className="btn-sm btn-secondary">View</Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default ProductsList;
