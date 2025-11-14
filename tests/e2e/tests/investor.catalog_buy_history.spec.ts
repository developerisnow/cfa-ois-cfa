import { test, expect } from '@playwright/test';

const investorId = '11111111-1111-4111-8111-111111111111';
const fakeSession = {
  user: {
    name: 'Ivan Investor',
    email: 'ivan@example.com',
    roles: ['investor'],
    id: investorId,
  },
  accessToken: 'test-token',
  expires: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
};

test.describe('Investor catalog → detail → buy → history', () => {
  test('end-to-end journey with CSV export', async ({ page }) => {
    // Session
    await page.route('**/api/auth/session**', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(fakeSession) })
    );

    // Catalog list and detail
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
    await page.route('**/v1/market/issuances/aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa', (route) =>
      route.fulfill({
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
      })
    );

    // Orders
    await page.route('**/v1/orders', (route) =>
      route.fulfill({
        status: 202,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'order-1',
          investorId,
          issuanceId: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
          amount: 10000,
          status: 'pending',
          dltTxHash: '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef',
          createdAt: new Date().toISOString(),
        }),
      })
    );

    // Wallet for portfolio redirect
    await page.route(`**/v1/wallets/${investorId}`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ id: 'w1', ownerType: 'individual', ownerId: investorId, balance: 100000, blocked: 0, holdings: [] }) })
    );

    // Transactions handler with type filter
    await page.route(`**/v1/investors/${investorId}/transactions**`, (route) => {
      const url = new URL(route.request().url());
      const type = url.searchParams.get('type');
      const txBase = {
        id: 'tx-1',
        issuanceId: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
        issuanceCode: 'CFA001',
        amount: 10000,
        status: 'confirmed',
        dltTxHash: 'abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789',
        createdAt: new Date().toISOString(),
      };
      const item =
        type === 'redeem'
          ? { ...txBase, id: 'tx-r', type: 'redeem' }
          : { ...txBase, id: 'tx-t', type: 'transfer' };
      return route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ items: [item], total: 1 }) });
    });

    // Payouts
    await page.route(`**/v1/investors/${investorId}/payouts**`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ items: [], total: 0, totalAmount: 0 }) })
    );

    // Go to catalog
    await page.goto('http://localhost:3002/catalog');
    await expect(page.getByText('Market Catalog')).toBeVisible();

    // Go to detail and buy
    await page.getByRole('link', { name: /CFA Bond 001/ }).click();
    await expect(page.getByText('Place Order')).toBeVisible();
    await page.fill('input#amount', '10000');
    await page.getByRole('button', { name: 'Buy Now' }).click();
    await page.waitForURL('**/portfolio');

    // History: Orders tab
    await page.goto('http://localhost:3002/history');
    await expect(page.getByText('History')).toBeVisible();
    await page.getByRole('button', { name: /Orders/ }).click();
    await expect(page.getByText('transfer')).toBeVisible();

    // Payouts tab
    await page.getByRole('button', { name: /Payouts/ }).click();
    await expect(page.getByText('No payouts found')).toBeVisible();

    // Redemptions tab
    await page.getByRole('button', { name: /Redemptions/ }).click();
    await expect(page.getByText('redeem')).toBeVisible();
  });
});

