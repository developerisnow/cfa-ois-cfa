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
  // Audit
  async getAuditEvents(params?: any) {
    const client = await getClient();
    const events = await client.getAuditEvents(params);
    return { data: events };
  },

  async getAuditEvent(id: string) {
    const client = await getClient();
    const event = await client.getAuditEvent(id);
    return { data: event };
  },

  // KYC
  async getKycDocuments(investorId: string) {
    const client = await getClient();
    const documents = await client.getKycDocuments(investorId);
    return { data: documents };
  },

  async makeKycDecision(investorId: string, data: any) {
    const client = await getClient();
    const result = await client.makeKycDecision(investorId, data);
    return { data: result };
  },

  async uploadKycDocuments(investorId: string, formData: FormData) {
    const client = await getClient();
    const result = await client.uploadKycDocuments(investorId, formData);
    return { data: result };
  },

  // Compliance
  async getInvestorStatus(investorId: string) {
    const client = await getClient();
    const status = await client.getInvestorStatus(investorId);
    return { data: status };
  },

  // Reports
  async getPayoutsReport(params: { from: string; to: string }) {
    const client = await getClient();
    const report = await client.getPayoutsReport(params);
    return { data: report };
  },

  // Settlement
  async runSettlement(params?: { date?: string }) {
    const client = await getClient();
    const result = await client.runSettlement(params);
    return { data: result };
  },
};

// Legacy methods for backward compatibility
export const legacyApiClient = {
  getInvestorStatus: async (params: { id: string }) => {
    const client = await getClient();
    const status = await client.getInvestorStatus(params.id);
    return { data: status };
  },
  getPayoutsReport: async (params: { from: string; to: string }) => {
    const client = await getClient();
    const report = await client.getPayoutsReport(params);
    return { data: report };
  },
  runSettlement: async (params?: { date?: string }) => {
    const client = await getClient();
    const result = await client.runSettlement(params);
    return { data: result };
  },
};
