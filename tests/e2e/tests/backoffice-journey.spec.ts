import { test, expect } from '@playwright/test';

const fakeSession = {
  user: {
    name: 'Boris Backoffice',
    email: 'backoffice@example.com',
    roles: ['backoffice'],
    id: '33333333-3333-4333-8333-333333333333',
  },
  accessToken: 'backoffice-token',
  expires: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
};

test.describe('Backoffice: KYC approve/reject → audit appears', () => {
  test('approve KYC and see audit entry', async ({ page }) => {
    await page.route('**/api/auth/session**', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(fakeSession) })
    );

    const investorId = '123e4567-e89b-12d3-a456-426614174000';

    // KYC decision endpoint
    await page.route(`**/v1/kyc/${investorId}/decision`, (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'dec1',
          investorId,
          status: 'approved',
          comment: 'OK',
          decisionBy: fakeSession.user.id,
          decisionAt: new Date().toISOString(),
        }),
      })
    );

    // KYC documents list
    await page.route(`**/v1/kyc/${investorId}/documents`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ items: [], total: 0 }) })
    );

    // Investor status
    await page.route(`**/v1/compliance/investors/${investorId}/status`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ kyc: 'pending' }) })
    );

    // Audit endpoint
    await page.route('**/v1/audit**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            {
              id: 'aud1',
              actor: fakeSession.user.id,
              actorName: fakeSession.user.name,
              action: 'kyc_decision',
              entity: 'kyc',
              entityId: investorId,
              timestamp: new Date().toISOString(),
              result: 'success',
            },
          ],
          total: 1,
          limit: 20,
          offset: 0,
        }),
      })
    );

    await page.goto('http://localhost:3003/kyc');
    await expect(page.getByText('KYC Management')).toBeVisible();

    // Open the single mocked application row
    await page.getByRole('button', { name: 'View' }).click();
    await expect(page.getByText('Compliance Status')).toBeVisible();

    // Fill comment and approve
    await page.locator('textarea').first().fill('OK');
    await page.getByRole('button', { name: 'Approve' }).click();

    // Go to audit
    await page.goto('http://localhost:3003/audit');
    await expect(page.getByText('Audit Log')).toBeVisible();
    await expect(page.getByText('kyc_decision')).toBeVisible();
  });
});

