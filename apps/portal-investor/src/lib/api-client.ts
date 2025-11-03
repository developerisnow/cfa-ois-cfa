// Temporary client until SDK is generated
import axios from 'axios';

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

// Create axios instance with auth
const axiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth interceptor if needed
// axiosInstance.interceptors.request.use((config) => {
//   const token = getToken(); // Implement getToken
//   if (token) {
//     config.headers.Authorization = `Bearer ${token}`;
//   }
//   return config;
// });

export const apiClient = {
  get: axiosInstance.get.bind(axiosInstance),
  post: axiosInstance.post.bind(axiosInstance),
  put: axiosInstance.put.bind(axiosInstance),
  delete: axiosInstance.delete.bind(axiosInstance),

  // Legacy methods for compatibility
  getWallet: async (params: { investorId: string }) => {
    const response = await axiosInstance.get(`/v1/wallets/${params.investorId}`);
    return { data: response.data };
  },
  placeOrder: async (data: any, options?: { headers?: Record<string, string> }) => {
    const response = await axiosInstance.post(
      `/v1/orders`,
      data.createOrderRequest,
      { headers: options?.headers }
    );
    return { data: response.data };
  },
};
