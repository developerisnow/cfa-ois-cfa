import { Pact } from '@pact-foundation/pact';
import path from 'path';

const provider = new Pact({
  consumer: 'api-gateway',
  provider: 'registry-service',
  port: 1235,
  log: path.resolve(process.cwd(), 'pacts', 'logs', 'gateway-registry.log'),
  dir: path.resolve(process.cwd(), 'pacts'),
  logLevel: 'INFO',
  spec: 2,
});

describe('API Gateway -> Registry Service', () => {
  beforeAll(() => provider.setup());
  afterEach(() => provider.verify());
  afterAll(() => provider.finalize());

  describe('POST /v1/orders', () => {
    it('places an order successfully', async () => {
      const idemKey = '550e8400-e29b-41d4-a716-446655440000';
      const request = {
        investorId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        issuanceId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        amount: 50000,
      };

      await provider.addInteraction({
        state: 'investor has passed KYC and qualification check',
        uponReceiving: 'a request to place an order',
        withRequest: {
          method: 'POST',
          path: '/v1/orders',
          headers: {
            'Content-Type': 'application/json',
            'Idempotency-Key': idemKey,
          },
          body: request,
        },
        willRespondWith: {
          status: 202,
          headers: { 'Content-Type': 'application/json' },
          body: {
            id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            investorId: request.investorId,
            issuanceId: request.issuanceId,
            amount: request.amount,
            status: 'pending',
            createdAt: '2025-01-01T00:00:00Z',
          },
        },
      });
    });

    it('handles idempotent request', async () => {
      const idemKey = '550e8400-e29b-41d4-a716-446655440001';
      
      await provider.addInteraction({
        state: 'order with idempotency key already exists',
        uponReceiving: 'a duplicate request to place an order',
        withRequest: {
          method: 'POST',
          path: '/v1/orders',
          headers: {
            'Content-Type': 'application/json',
            'Idempotency-Key': idemKey,
          },
          body: {
            investorId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            issuanceId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            amount: 50000,
          },
        },
        willRespondWith: {
          status: 202,
          headers: { 'Content-Type': 'application/json' },
          body: {
            id: 'existing-order-id',
            status: 'pending',
          },
        },
      });
    });
  });
});

