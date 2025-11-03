// Temporary client until SDK is generated
import axios from 'axios';

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

export const apiClient: {
  getInvestorStatus: (params: { id: string }) => Promise<{ data: any }>;
  getPayoutsReport: (params: { from: string; to: string }) => Promise<{ data: any }>;
  runSettlement: (params: { date?: string }) => Promise<{ data: any }>;
} = {
  getInvestorStatus: async (params: { id: string }) => {
    const response = await axios.get(`${baseURL}/v1/compliance/investors/${params.id}/status`);
    return { data: response.data };
  },
  getPayoutsReport: async (params: { from: string; to: string }) => {
    const response = await axios.get(`${baseURL}/v1/reports/payouts`, {
      params: { from: params.from, to: params.to },
    });
    return { data: response.data };
  },
  runSettlement: async (params: { date?: string }) => {
    const response = await axios.post(`${baseURL}/v1/settlement/run`, null, {
      params: params.date ? { date: params.date } : undefined,
    });
    return { data: response.data };
  },
};
