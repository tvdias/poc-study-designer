import { useParams } from 'react-router-dom';

function ModuleDetail() {
  const { id } = useParams();
  return (
    <div className="page-container">
      <h1>Module Detail - ID: {id}</h1>
      <p>Implementation pending</p>
    </div>
  );
}

export default ModuleDetail;
