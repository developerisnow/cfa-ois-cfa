import { getSession } from 'next-auth/react';
import { OisApiClient } from '@ois/api-client';

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

// Helper to create client with token
async function getClient() {
  const session = await getSession();
  const token = (session?.accessToken as string) || undefined;
  return new OisApiClient({ baseURL, accessToken: token });
}

// SDK wrapper with NextAuth integration
export const apiClient = {
  // Issuances
  async createIssuance(data: any) {
    const client = await getClient();
    const issuance = await client.createIssuance(data);
    return { data: issuance };
  },

  async getIssuance(id: string) {
    const client = await getClient();
    const issuance = await client.getIssuance(id);
    return { data: issuance };
  },

  async publishIssuance(id: string) {
    const client = await getClient();
    const issuance = await client.publishIssuance(id);
    return { data: issuance };
  },

  async closeIssuance(id: string) {
    const client = await getClient();
    const issuance = await client.closeIssuance(id);
    return { data: issuance };
  },

  // Reports
  async getIssuerIssuancesReport(params: { issuerId: string; from: string; to: string }) {
    const client = await getClient();
    const report = await client.getIssuerIssuancesReport(params);
    return { data: report };
  },

  async getIssuerPayoutsReport(params: { issuerId: string; from: string; to: string; granularity?: 'day' | 'week' | 'month' | 'year' }) {
    const client = await getClient();
    const report = await client.getIssuerPayoutsReport(params);
    return { data: report };
  },

  // Settlement
  async runSettlement(params?: { date?: string }) {
    const client = await getClient();
    const result = await client.runSettlement(params);
    return { data: result };
  },

  // Legacy compatibility
  getPayoutsReport: async (params: { from: string; to: string }) => {
    const client = await getClient();
    const report = await client.getPayoutsReport(params);
    return { data: report };
  },
};
