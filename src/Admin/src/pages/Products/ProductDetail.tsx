import { useParams } from 'react-router-dom';

function ProductDetail() {
  const { id } = useParams();
  return (
    <div className="page-container">
      <h1>Product Detail - ID: {id}</h1>
      <p>Implementation pending</p>
    </div>
  );
}

export default ProductDetail;
