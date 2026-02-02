import { useParams } from 'react-router-dom';

function ClientDetail() {
  const { id } = useParams();
  return (
    <div className="page-container">
      <h1>Client Detail - ID: {id}</h1>
      <p>Implementation pending</p>
    </div>
  );
}

export default ClientDetail;
