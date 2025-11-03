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
  // Broker
  async getBrokerClients(params?: any) {
    const client = await getClient();
    const data = await client.getBrokerClients(params);
    return { data };
  },

  async createBrokerOrder(data: any, options?: { headers?: Record<string, string> }) {
    const client = await getClient();
    const order = await client.createBrokerOrder(data, options);
    return { data: order };
  },

  async getBrokerCommissions(params?: any) {
    const client = await getClient();
    const commissions = await client.getBrokerCommissions(params);
    return { data: commissions };
  },

  async getBrokerFeed(params?: any) {
    const client = await getClient();
    const feed = await client.getBrokerFeed(params);
    return { data: feed };
  },

  // Market (for broker portal)
  async getMarketIssuances(params?: any) {
    const client = await getClient();
    const data = await client.getMarketIssuances(params);
    return { data };
  },
};
