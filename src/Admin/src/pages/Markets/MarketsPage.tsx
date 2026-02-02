import { useEffect, useState } from 'react';
import { marketsApi } from '../../services/adminApi';
import type { CommissioningMarket, FieldworkMarket } from '../../types/entities';
import '../Questions/QuestionsList.css';

function MarketsPage() {
  const [commissioningMarkets, setCommissioningMarkets] = useState<CommissioningMarket[]>([]);
  const [fieldworkMarkets, setFieldworkMarkets] = useState<FieldworkMarket[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadMarkets();
  }, []);

  const loadMarkets = async () => {
    try {
      const [commissioning, fieldwork] = await Promise.all([
        marketsApi.getCommissioningMarkets(),
        marketsApi.getFieldworkMarkets(),
      ]);
      setCommissioningMarkets(commissioning.data);
      setFieldworkMarkets(fieldwork.data);
    } catch (error) {
      console.error('Failed to load markets:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1>Markets</h1>
          <p>Manage commissioning and fieldwork markets</p>
        </div>
      </div>

      {loading ? (
        <div className="loading">Loading markets...</div>
      ) : (
        <div style={{ display: 'grid', gap: '30px' }}>
          <div>
            <h2 style={{ marginBottom: '15px' }}>Commissioning Markets</h2>
            <div className="table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>ISO Code</th>
                  </tr>
                </thead>
                <tbody>
                  {commissioningMarkets.map((market) => (
                    <tr key={market.id}>
                      <td>{market.name}</td>
                      <td><code>{market.isoCode}</code></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          <div>
            <h2 style={{ marginBottom: '15px' }}>Fieldwork Markets</h2>
            <div className="table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>ISO Code</th>
                  </tr>
                </thead>
                <tbody>
                  {fieldworkMarkets.map((market) => (
                    <tr key={market.id}>
                      <td>{market.name}</td>
                      <td><code>{market.isoCode}</code></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default MarketsPage;
