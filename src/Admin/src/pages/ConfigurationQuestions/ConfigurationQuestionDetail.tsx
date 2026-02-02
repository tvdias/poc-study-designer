import { useParams } from 'react-router-dom';

function ConfigurationQuestionDetail() {
  const { id } = useParams();
  return (
    <div className="page-container">
      <h1>Configuration Question Detail - ID: {id}</h1>
      <p>Implementation pending</p>
    </div>
  );
}

export default ConfigurationQuestionDetail;
