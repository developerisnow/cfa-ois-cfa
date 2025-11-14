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
  // Market
  async getMarketIssuances(params?: any) {
    const client = await getClient();
    const data = await client.getMarketIssuances(params);
    return { data };
  },

  async getMarketIssuance(id: string) {
    const client = await getClient();
    const data = await client.getMarketIssuance(id);
    return { data };
  },

  // Orders
  async createOrder(
    data: any,
    options?: { headers?: Record<string, string> }
  ) {
    const client = await getClient();
    const orderData = await client.createOrder(data, {
      headers: options?.headers,
    });
    return { data: orderData };
  },

  // Wallet
  async getWallet(investorId: string) {
    const client = await getClient();
    const wallet = await client.getWallet(investorId);
    return { data: wallet };
  },

  // Investor
  async getInvestorTransactions(investorId: string, params?: any) {
    const client = await getClient();
    const transactions = await client.getInvestorTransactions(investorId, params);
    return { data: transactions };
  },

  async getInvestorPayouts(investorId: string, params?: any) {
    const client = await getClient();
    const payouts = await client.getInvestorPayouts(investorId, params);
    return { data: payouts };
  },

  // Legacy compatibility methods
  placeOrder: async (data: any, options?: { headers?: Record<string, string> }) => {
    return apiClient.createOrder(data, options);
  },
};
