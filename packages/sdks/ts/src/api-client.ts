/**
 * Generated TypeScript API Client for OIS Gateway
 * Based on OpenAPI Gateway specification
 */

import axios, { AxiosInstance, AxiosRequestConfig, AxiosError } from 'axios';

export interface ApiClientConfig {
  baseURL?: string;
  accessToken?: string;
  timeout?: number;
}

// Utilities
function uuid(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return (crypto as any).randomUUID();
  }
  // Fallback UUID v4 generator
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

function toHex(bytes: Uint8Array): string {
  return Array.from(bytes)
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}

function generateTraceparent(): string {
  try {
    const traceIdBytes = new Uint8Array(16);
    const spanIdBytes = new Uint8Array(8);
    if (typeof crypto !== 'undefined' && (crypto as any).getRandomValues) {
      (crypto as any).getRandomValues(traceIdBytes);
      (crypto as any).getRandomValues(spanIdBytes);
    } else {
      for (let i = 0; i < 16; i++) traceIdBytes[i] = (Math.random() * 256) | 0;
      for (let i = 0; i < 8; i++) spanIdBytes[i] = (Math.random() * 256) | 0;
    }
    const traceId = toHex(traceIdBytes);
    const spanId = toHex(spanIdBytes);
    return `00-${traceId}-${spanId}-01`;
  } catch {
    // Safe fallback
    return `00-${uuid().replace(/-/g, '').padEnd(32, '0').slice(0, 32)}-${uuid()
      .replace(/-/g, '')
      .padEnd(16, '0')
      .slice(0, 16)}-01`;
  }
}

const sleep = (ms: number) => new Promise((res) => setTimeout(res, ms));

// Types from OpenAPI schemas
export interface HealthStatus {
  status: 'healthy' | 'unhealthy';
  timestamp?: string;
}

export interface CreateIssuanceRequest {
  assetId: string;
  issuerId: string;
  totalAmount: number;
  nominal: number;
  issueDate: string;
  maturityDate: string;
  scheduleJson?: Record<string, any>;
}

export interface IssuanceResponse {
  id: string;
  assetId: string;
  issuerId: string;
  totalAmount: number;
  nominal: number;
  issueDate: string;
  maturityDate: string;
  status: 'draft' | 'published' | 'closed' | 'redeemed';
  scheduleJson?: Record<string, any>;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateOrderRequest {
  investorId: string;
  issuanceId: string;
  amount: number;
}

export interface OrderResponse {
  id: string;
  investorId: string;
  issuanceId: string;
  amount: number;
  status: 'pending' | 'confirmed' | 'failed' | 'cancelled';
  createdAt?: string;
  updatedAt?: string;
}

export interface WalletResponse {
  id: string;
  ownerType: 'individual' | 'legal_entity';
  ownerId: string;
  balance: number;
  blocked: number;
  holdings?: Array<{
    assetId: string;
    assetCode: string;
    assetName: string;
    amount: number;
  }>;
}

export interface RedeemRequest {
  amount: number;
}

export interface RedeemResponse {
  id: string;
  status: 'redeemed' | 'partial';
  redeemedAmount: number;
  remainingAmount: number;
}

export interface MarketIssuanceCard {
  id: string;
  assetCode: string;
  assetName: string;
  issuerName: string;
  totalAmount: number;
  nominal: number;
  availableAmount: number;
  issueDate: string;
  maturityDate: string;
  yield: number;
  status: 'open' | 'closed';
  publishedAt?: string;
  scheduleJson?: Record<string, any>;
}

export interface MarketIssuancesResponse {
  items: MarketIssuanceCard[];
  total: number;
  limit: number;
  offset: number;
}

export interface TxHistoryItem {
  id: string;
  type: 'transfer' | 'redeem' | 'issue';
  issuanceId: string;
  issuanceCode: string;
  amount: number;
  status: 'pending' | 'confirmed' | 'failed';
  dltTxHash?: string;
  createdAt: string;
  confirmedAt?: string;
}

export interface TransactionHistoryResponse {
  items: TxHistoryItem[];
  total: number;
}

export interface PayoutItem {
  id: string;
  batchId: string;
  issuanceId: string;
  investorId: string;
  amount: number;
  status: 'pending' | 'executed' | 'failed';
  executedAt?: string;
}

export interface PayoutHistoryResponse {
  items: PayoutItem[];
  total: number;
  totalAmount: number;
}

export interface IssuerReportRow {
  issuanceId: string;
  assetCode: string;
  assetName: string;
  totalAmount: number;
  soldAmount: number;
  investorsCount: number;
  status: 'draft' | 'published' | 'closed' | 'redeemed';
  issueDate: string;
  maturityDate: string;
  publishedAt?: string;
}

export interface IssuerIssuancesReportResponse {
  issuerId: string;
  period: {
    from: string;
    to: string;
  };
  items: IssuerReportRow[];
  summary: {
    totalIssuances: number;
    totalAmount: number;
    totalSold: number;
    totalInvestors: number;
  };
}

export interface IssuerPayoutsReportResponse {
  issuerId: string;
  period: {
    from: string;
    to: string;
  };
  granularity: 'day' | 'week' | 'month' | 'year';
  items: Array<{
    period: string;
    totalAmount: number;
    payoutCount: number;
    investorsCount: number;
  }>;
  summary: {
    totalAmount: number;
    totalPayouts: number;
    totalInvestors: number;
  };
}

export interface BrokerClient {
  id: string;
  name: string;
  email: string;
  inn?: string;
  type: 'individual' | 'legal_entity';
  kycStatus: 'pending' | 'approved' | 'rejected';
  qualificationStatus: 'none' | 'qualified' | 'unqualified';
  createdAt: string;
  lastActivityAt?: string;
}

export interface BrokerClientsResponse {
  items: BrokerClient[];
  total: number;
  limit: number;
  offset: number;
}

export interface CreateBrokerOrderRequest {
  clientId: string;
  issuanceId: string;
  amount: number;
}

export interface BrokerOrderResponse {
  id: string;
  clientId: string;
  issuanceId: string;
  amount: number;
  status: 'pending' | 'confirmed' | 'failed' | 'cancelled';
  commission?: number;
  createdAt: string;
}

export interface CommissionRow {
  period: string;
  totalAmount: number;
  commissionAmount: number;
  ordersCount: number;
  clientsCount: number;
}

export interface CommissionsResponse {
  items: CommissionRow[];
  total: number;
  totalAmount: number;
  totalCommission: number;
}

export interface FeedItem {
  id: string;
  type: 'order' | 'transfer' | 'payout' | 'kyc' | 'qualification';
  title?: string;
  description?: string;
  clientId?: string;
  clientName?: string;
  issuanceId?: string;
  amount?: number;
  status?: 'pending' | 'completed' | 'failed';
  timestamp: string;
  metadata?: Record<string, any>;
}

export interface FeedResponse {
  items: FeedItem[];
  total: number;
  hasMore: boolean;
}

export interface AuditEvent {
  id: string;
  actor: string;
  actorName?: string;
  action: string;
  entity: string;
  entityId?: string;
  payload?: Record<string, any>;
  ip?: string;
  userAgent?: string;
  timestamp: string;
  result?: 'success' | 'failure' | 'pending';
}

export interface AuditEventsResponse {
  items: AuditEvent[];
  total: number;
  limit: number;
  offset: number;
}

export interface KycDecisionRequest {
  status: 'approved' | 'rejected';
  comment: string;
}

export interface KycDecisionResponse {
  id: string;
  investorId: string;
  status: 'approved' | 'rejected';
  comment: string;
  decisionBy: string;
  decisionAt: string;
}

export interface KycDocument {
  id: string;
  investorId: string;
  documentType: 'passport' | 'inn' | 'snils' | 'address_proof' | 'income_proof' | 'other';
  fileName: string;
  fileSize?: number;
  mimeType?: string;
  storageUrl: string;
  uploadedAt: string;
  uploadedBy?: string;
  comment?: string;
}

export interface KycDocumentsResponse {
  items: KycDocument[];
  total: number;
}

export interface InvestorStatusResponse {
  kyc: string;
  qualificationTier?: string;
  qualificationLimit?: number;
  qualificationUsed?: number;
}

export interface PayoutsReportResponse {
  period: {
    from: string;
    to: string;
  };
  items: PayoutItem[];
  totalAmount: number;
}

export interface SettlementResponse {
  batchId: string;
  status: string;
}

export class OisApiClient {
  private client: AxiosInstance;

  constructor(config: ApiClientConfig = {}) {
    this.client = axios.create({
      baseURL:
        config.baseURL ||
        (typeof process !== 'undefined' && (process as any).env?.NEXT_PUBLIC_API_BASE_URL) ||
        'http://localhost:5000',
      timeout: config.timeout || 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth + observability interceptors
    this.client.interceptors.request.use((cfg) => {
      // Auth
      if (config.accessToken) {
        (cfg.headers as any).Authorization = `Bearer ${config.accessToken}`;
      }

      // Correlation/observability headers
      const headers = (cfg.headers = cfg.headers || {} as any);
      if (!(headers as any)['x-request-id']) {
        (headers as any)['x-request-id'] = uuid();
      }
      if (!(headers as any)['traceparent']) {
        (headers as any)['traceparent'] = generateTraceparent();
      }
      (headers as any)['x-client-app'] = (headers as any)['x-client-app'] || 'ois-web';
      (headers as any)['Accept'] = (headers as any)['Accept'] || 'application/json';

      // Start time for basic latency metric
      (cfg as any).__start = Date.now();
      return cfg;
    });

    // Basic retry with exp backoff + jitter
    this.client.interceptors.response.use(
      (resp) => resp,
      async (error: AxiosError) => {
        const cfg: any = error.config || {};
        const status = (error.response && (error.response as any).status) || 0;
        const isRetryable = !cfg.__noRetry && (status === 429 || (status >= 500 && status < 600) || !status);
        cfg.__retryCount = cfg.__retryCount || 0;
        const maxRetries = cfg.__maxRetries ?? 3;

        if (isRetryable && cfg.__retryCount < maxRetries) {
          cfg.__retryCount += 1;
          const base = 300 * Math.pow(2, cfg.__retryCount - 1);
          const jitter = Math.floor(Math.random() * 100);
          await sleep(base + jitter);
          return this.client.request(cfg);
        }

        // Attach simple duration metric for logging
        const start = cfg.__start || 0;
        const duration = start ? Date.now() - start : undefined;
        if (typeof window !== 'undefined' && (window as any).console) {
          (console as any).warn?.('API request failed', {
            url: cfg.url,
            method: cfg.method,
            status,
            duration,
          });
        }
        return Promise.reject(error);
      }
    );
  }

  // Set access token dynamically
  setAccessToken(token: string) {
    this.client.interceptors.request.use((config) => {
      config.headers.Authorization = `Bearer ${token}`;
      return config;
    });
  }

  // Health
  async healthCheck(): Promise<HealthStatus> {
    const response = await this.client.get<HealthStatus>('/health');
    return response.data;
  }

  // Issuances
  async createIssuance(data: CreateIssuanceRequest, config?: AxiosRequestConfig): Promise<IssuanceResponse> {
    const response = await this.client.post<IssuanceResponse>('/issuances', data, config);
    return response.data;
  }

  async getIssuance(id: string, config?: AxiosRequestConfig): Promise<IssuanceResponse> {
    const response = await this.client.get<IssuanceResponse>(`/issuances/${id}`, config);
    return response.data;
  }

  async publishIssuance(id: string, config?: AxiosRequestConfig): Promise<IssuanceResponse> {
    const response = await this.client.post<IssuanceResponse>(`/issuances/${id}/publish`, undefined, config);
    return response.data;
  }

  async closeIssuance(id: string, config?: AxiosRequestConfig): Promise<IssuanceResponse> {
    const response = await this.client.post<IssuanceResponse>(`/issuances/${id}/close`, undefined, config);
    return response.data;
  }

  async redeemIssuance(id: string, data: RedeemRequest, config?: AxiosRequestConfig): Promise<RedeemResponse> {
    const response = await this.client.post<RedeemResponse>(`/v1/issuances/${id}/redeem`, data, config);
    return response.data;
  }

  // Orders
  async createOrder(data: CreateOrderRequest, config?: AxiosRequestConfig): Promise<OrderResponse> {
    const response = await this.client.post<OrderResponse>('/v1/orders', data, config);
    return response.data;
  }

  async getOrder(id: string, config?: AxiosRequestConfig): Promise<OrderResponse> {
    const response = await this.client.get<OrderResponse>(`/orders/${id}`, config);
    return response.data;
  }

  // Wallets
  async getWallet(investorId: string, config?: AxiosRequestConfig): Promise<WalletResponse> {
    const response = await this.client.get<WalletResponse>(`/v1/wallets/${investorId}`, config);
    return response.data;
  }

  // Market
  async getMarketIssuances(params?: {
    status?: 'open' | 'closed' | 'all';
    sort?: string;
    limit?: number;
    offset?: number;
  }, config?: AxiosRequestConfig): Promise<MarketIssuancesResponse> {
    const response = await this.client.get<MarketIssuancesResponse>('/v1/market/issuances', {
      ...config,
      params,
    });
    return response.data;
  }

  async getMarketIssuance(id: string, config?: AxiosRequestConfig): Promise<MarketIssuanceCard> {
    const response = await this.client.get<MarketIssuanceCard>(`/v1/market/issuances/${id}`, config);
    return response.data;
  }

  // Investor
  async getInvestorTransactions(investorId: string, params?: {
    from?: string;
    to?: string;
    type?: string;
  }, config?: AxiosRequestConfig): Promise<TransactionHistoryResponse> {
    const response = await this.client.get<TransactionHistoryResponse>(`/v1/investors/${investorId}/transactions`, {
      ...config,
      params,
    });
    return response.data;
  }

  async getInvestorPayouts(investorId: string, params?: {
    from?: string;
    to?: string;
  }, config?: AxiosRequestConfig): Promise<PayoutHistoryResponse> {
    const response = await this.client.get<PayoutHistoryResponse>(`/v1/investors/${investorId}/payouts`, {
      ...config,
      params,
    });
    return response.data;
  }

  // Reports
  async getIssuerIssuancesReport(params: {
    issuerId: string;
    from: string;
    to: string;
  }, config?: AxiosRequestConfig): Promise<IssuerIssuancesReportResponse> {
    const response = await this.client.get<IssuerIssuancesReportResponse>('/v1/reports/issuances', {
      ...config,
      params,
    });
    return response.data;
  }

  async getIssuerPayoutsReport(params: {
    issuerId: string;
    from: string;
    to: string;
    granularity?: 'day' | 'week' | 'month' | 'year';
  }, config?: AxiosRequestConfig): Promise<IssuerPayoutsReportResponse> {
    const response = await this.client.get<IssuerPayoutsReportResponse>('/v1/reports/payouts', {
      ...config,
      params,
    });
    return response.data;
  }

  async getPayoutsReport(params: {
    from: string;
    to: string;
  }, config?: AxiosRequestConfig): Promise<PayoutsReportResponse> {
    const response = await this.client.get<PayoutsReportResponse>('/v1/reports/payouts', {
      ...config,
      params,
    });
    return response.data;
  }

  // Broker
  async getBrokerClients(params?: {
    limit?: number;
    offset?: number;
    query?: string;
  }, config?: AxiosRequestConfig): Promise<BrokerClientsResponse> {
    const response = await this.client.get<BrokerClientsResponse>('/v1/broker/clients', {
      ...config,
      params,
    });
    return response.data;
  }

  async createBrokerOrder(data: CreateBrokerOrderRequest, config?: AxiosRequestConfig): Promise<BrokerOrderResponse> {
    const response = await this.client.post<BrokerOrderResponse>('/v1/broker/orders', data, config);
    return response.data;
  }

  async getBrokerCommissions(params?: {
    from?: string;
    to?: string;
  }, config?: AxiosRequestConfig): Promise<CommissionsResponse> {
    const response = await this.client.get<CommissionsResponse>('/v1/broker/commissions', {
      ...config,
      params,
    });
    return response.data;
  }

  async getBrokerFeed(params?: {
    from?: string;
    to?: string;
    types?: string;
  }, config?: AxiosRequestConfig): Promise<FeedResponse> {
    const response = await this.client.get<FeedResponse>('/v1/broker/feed', {
      ...config,
      params,
    });
    return response.data;
  }

  // Audit
  async getAuditEvents(params?: {
    actor?: string;
    action?: string;
    entity?: string;
    from?: string;
    to?: string;
    limit?: number;
    offset?: number;
  }, config?: AxiosRequestConfig): Promise<AuditEventsResponse> {
    const response = await this.client.get<AuditEventsResponse>('/v1/audit', {
      ...config,
      params,
    });
    return response.data;
  }

  async getAuditEvent(id: string, config?: AxiosRequestConfig): Promise<AuditEvent> {
    const response = await this.client.get<AuditEvent>(`/v1/audit/${id}`, config);
    return response.data;
  }

  // KYC
  async makeKycDecision(investorId: string, data: KycDecisionRequest, config?: AxiosRequestConfig): Promise<KycDecisionResponse> {
    const response = await this.client.post<KycDecisionResponse>(`/v1/kyc/${investorId}/decision`, data, config);
    return response.data;
  }

  async uploadKycDocuments(
    investorId: string,
    formData: FormData,
    config?: AxiosRequestConfig
  ): Promise<KycDocumentsResponse> {
    const response = await this.client.post<KycDocumentsResponse>(`/v1/kyc/${investorId}/documents`, formData, {
      ...config,
      headers: {
        ...config?.headers,
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  async getKycDocuments(investorId: string, config?: AxiosRequestConfig): Promise<KycDocumentsResponse> {
    const response = await this.client.get<KycDocumentsResponse>(`/v1/kyc/${investorId}/documents`, config);
    return response.data;
  }

  // Compliance
  async getInvestorStatus(investorId: string, config?: AxiosRequestConfig): Promise<InvestorStatusResponse> {
    const response = await this.client.get<InvestorStatusResponse>(`/v1/compliance/investors/${investorId}/status`, config);
    return response.data;
  }

  // Settlement
  async runSettlement(params?: { date?: string }, config?: AxiosRequestConfig): Promise<SettlementResponse> {
    const response = await this.client.post<SettlementResponse>('/v1/settlement/run', null, {
      ...config,
      params,
    });
    return response.data;
  }
}

// Export singleton instance factory
export function createApiClient(config?: ApiClientConfig): OisApiClient {
  return new OisApiClient(config);
}

// Export default instance
export const apiClient = createApiClient();

