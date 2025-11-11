import { test, expect } from '@playwright/test';

const fakeSession = {
  user: {
    name: 'Ivan Investor',
    email: 'ivan@example.com',
    roles: ['investor'],
    id: '11111111-1111-4111-8111-111111111111',
  },
  accessToken: 'test-token',
  expires: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
};

test.describe('Investor: catalog → detail → buy → history', () => {
  test('browse catalog, buy, see in history', async ({ page }) => {
    // Authenticate via NextAuth session stub
    await page.route('**/api/auth/session**', (route) => {
      return route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(fakeSession) });
    });

    // Mock catalog list
    await page.route('**/v1/market/issuances**', (route) => {
      const url = new URL(route.request().url());
      if (url.pathname.endsWith('/v1/market/issuances') && !/\/v1\/market\/issuances\/[\w-]+$/.test(url.pathname)) {
        return route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
                assetCode: 'CFA001',
                assetName: 'CFA Bond 001',
                issuerName: 'Issuer LLC',
                totalAmount: 1000000,
                nominal: 1000,
                availableAmount: 900000,
                issueDate: '2025-01-01',
                maturityDate: '2026-01-01',
                yield: 12.5,
                status: 'open',
              },
            ],
            total: 1,
            limit: 20,
            offset: 0,
          }),
        });
      }
      return route.fallback();
    });

    // Mock detail
    await page.route('**/v1/market/issuances/aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa', (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
          assetCode: 'CFA001',
          assetName: 'CFA Bond 001',
          issuerName: 'Issuer LLC',
          totalAmount: 1000000,
          nominal: 1000,
          availableAmount: 900000,
          issueDate: '2025-01-01',
          maturityDate: '2026-01-01',
          yield: 12.5,
          status: 'open',
          scheduleJson: { '2025-03-01': 50000 },
        }),
      });
    });

    // Mock order placement
    await page.route('**/v1/orders', async (route) => {
      const headers = route.request().headers();
      expect(headers['idempotency-key']).toBeTruthy();
      return route.fulfill({
        status: 202,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'order-1',
          investorId: fakeSession.user.id,
          issuanceId: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
          amount: 10000,
          status: 'pending',
          dltTxHash: '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef',
          createdAt: new Date().toISOString(),
        }),
      });
    });

    // Mock wallet for portfolio page
    await page.route(`**/v1/wallets/${fakeSession.user.id}`, (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: 'w1', ownerType: 'individual', ownerId: fakeSession.user.id, balance: 100000, blocked: 0, holdings: [] }),
      });
    });

    // Mock history endpoints
    await page.route(`**/v1/investors/${fakeSession.user.id}/transactions**`, (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            {
              id: 'tx1',
              type: 'issue',
              issuanceId: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
              issuanceCode: 'CFA001',
              amount: 10000,
              status: 'confirmed',
              dltTxHash: 'abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789',
              createdAt: new Date().toISOString(),
            },
          ],
          total: 1,
        }),
      });
    });
    await page.route(`**/v1/investors/${fakeSession.user.id}/payouts**`, (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: [], total: 0, totalAmount: 0 }),
      });
    });

    // Run journey
    await page.goto('http://localhost:3002/catalog');
    await expect(page.getByText('Market Catalog')).toBeVisible();
    await page.getByRole('link', { name: /CFA Bond 001/ }).click();
    await expect(page.getByText('Place Order')).toBeVisible();
    await page.fill('input#amount', '10000');
    await page.getByRole('button', { name: 'Buy Now' }).click();
    await page.waitForURL('**/portfolio');

    // History
    await page.goto('http://localhost:3002/history');
    await expect(page.getByText('Transaction History')).toBeVisible();
    await expect(page.getByText('issue')).toBeVisible();
  });
});

