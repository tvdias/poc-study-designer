import { useParams } from 'react-router-dom';

function QuestionDetail() {
  const { id } = useParams();

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>Question Detail</h1>
        <p>View and edit question ID: {id}</p>
      </div>
      <div style={{ background: 'white', padding: '30px', borderRadius: '8px' }}>
        <p>Question detail view - Implementation pending</p>
      </div>
    </div>
  );
}

export default QuestionDetail;
