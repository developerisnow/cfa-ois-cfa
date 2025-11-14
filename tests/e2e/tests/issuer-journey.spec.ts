import { test, expect } from '@playwright/test';

const fakeSession = {
  user: {
    name: 'Irina Issuer',
    email: 'issuer@example.com',
    roles: ['issuer'],
    id: '22222222-2222-4222-8222-222222222222',
    issuerId: '22222222-2222-4222-8222-222222222222',
  },
  accessToken: 'issuer-token',
  expires: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
};

test.describe('Issuer: reports export', () => {
  test('view reports and export', async ({ page }) => {
    await page.route('**/api/auth/session**', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(fakeSession) })
    );

    await page.route('**/v1/reports/issuances**', (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          issuerId: fakeSession.user.issuerId,
          period: { from: '2025-01-01', to: '2025-03-31' },
          items: [
            {
              issuanceId: 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa',
              assetCode: 'CFA001',
              assetName: 'CFA Bond 001',
              totalAmount: 1000000,
              soldAmount: 250000,
              investorsCount: 12,
              status: 'published',
              issueDate: '2025-01-01',
              maturityDate: '2026-01-01',
              publishedAt: '2025-01-02T00:00:00Z',
            },
          ],
          summary: { totalIssuances: 1, totalAmount: 1000000, totalSold: 250000, totalInvestors: 12 },
        }),
      });
    });

    await page.route('**/v1/reports/payouts**', (route) => {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          issuerId: fakeSession.user.issuerId,
          period: { from: '2025-01-01', to: '2025-03-31' },
          granularity: 'month',
          items: [
            { period: '2025-02', totalAmount: 50000, payoutCount: 3, investorsCount: 10 },
            { period: '2025-03', totalAmount: 70000, payoutCount: 4, investorsCount: 12 },
          ],
          summary: { totalAmount: 120000, totalPayouts: 7, totalInvestors: 22 },
        }),
      });
    });

    await page.goto('http://localhost:3001/reports');
    await expect(page.getByText('Reports')).toBeVisible();
    // Export buttons enabled
    await expect(page.getByRole('button', { name: 'Export CSV' })).toBeEnabled();
    await expect(page.getByRole('button', { name: 'Export XLSX' })).toBeEnabled();

    // Switch to payouts and ensure rendered
    await page.getByRole('button', { name: 'Payouts' }).click();
    await expect(page.getByText('Payouts Over Time')).toBeVisible();
  });
});

