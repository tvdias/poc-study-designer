import axios from 'axios';

// Default to HTTPS in production, HTTP in development
const defaultUrl = import.meta.env.PROD 
  ? 'https://localhost:7437/api' 
  : 'http://localhost:5433/api';

const API_BASE_URL = import.meta.env.VITE_API_URL || defaultUrl;

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export default apiClient;
