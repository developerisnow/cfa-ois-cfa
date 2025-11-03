// Temporary client until SDK is generated
import axios from 'axios';

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

// Create axios instance
const axiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const apiClient = {
  get: axiosInstance.get.bind(axiosInstance),
  post: axiosInstance.post.bind(axiosInstance),
  put: axiosInstance.put.bind(axiosInstance),
  delete: axiosInstance.delete.bind(axiosInstance),

  // Legacy methods for compatibility
  createIssuance: async (data: any) => {
    const response = await axiosInstance.post('/issuances', data.createIssuanceRequest);
    return { data: response.data };
  },
  getIssuance: async (params: { id: string }) => {
    const response = await axiosInstance.get(`/issuances/${params.id}`);
    return { data: response.data };
  },
  publishIssuance: async (params: { id: string }) => {
    const response = await axiosInstance.post(`/issuances/${params.id}/publish`);
    return { data: response.data };
  },
  closeIssuance: async (params: { id: string }) => {
    const response = await axiosInstance.post(`/issuances/${params.id}/close`);
    return { data: response.data };
  },
  getPayoutsReport: async (params: { from: string; to: string }) => {
    const response = await axiosInstance.get('/v1/reports/payouts', { params });
    return { data: response.data };
  },
  runSettlement: async (params?: { date?: string }) => {
    const response = await axiosInstance.post('/v1/settlement/run', null, {
      params: params?.date ? { date: params.date } : undefined,
    });
    return { data: response.data };
  },
};
