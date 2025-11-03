import { Pact } from '@pact-foundation/pact';
import path from 'path';

const provider = new Pact({
  consumer: 'api-gateway',
  provider: 'issuance-service',
  port: 1234,
  log: path.resolve(process.cwd(), 'pacts', 'logs', 'gateway-issuance.log'),
  dir: path.resolve(process.cwd(), 'pacts'),
  logLevel: 'INFO',
  spec: 2,
});

describe('API Gateway -> Issuance Service', () => {
  beforeAll(() => provider.setup());
  afterEach(() => provider.verify());
  afterAll(() => provider.finalize());

  describe('POST /v1/issuances', () => {
    const createIssuanceRequest = {
      assetId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      issuerId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      totalAmount: 1000000,
      nominal: 1000,
      issueDate: '2025-01-01',
      maturityDate: '2026-01-01',
    };

    const createIssuanceResponse = {
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      assetId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      issuerId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      totalAmount: 1000000,
      nominal: 1000,
      issueDate: '2025-01-01',
      maturityDate: '2026-01-01',
      status: 'draft',
      createdAt: '2025-01-01T00:00:00Z',
    };

    it('creates an issuance', async () => {
      await provider.addInteraction({
        state: 'no existing issuance',
        uponReceiving: 'a request to create an issuance',
        withRequest: {
          method: 'POST',
          path: '/v1/issuances',
          headers: { 'Content-Type': 'application/json' },
          body: createIssuanceRequest,
        },
        willRespondWith: {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
          body: createIssuanceResponse,
        },
      });

      // Consumer test would call gateway API here
      // const response = await fetch('http://localhost:5000/v1/issuances', { ... });
    });
  });
});

