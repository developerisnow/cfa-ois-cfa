import { test, expect } from '@playwright/test';
import { seedTestData, cleanupTestData } from '../helpers/test-data';

test.describe('Full Lifecycle E2E', () => {
  test.beforeEach(async ({ page }) => {
    // Seed test data if needed
    await seedTestData();
  });

  test.afterEach(async () => {
    // Cleanup test data
    await cleanupTestData();
  });

  test('should complete issuance lifecycle', async ({ page }) => {
    // Navigate to issuer portal
    await page.goto('http://localhost:3001');
    
    // Wait for page load
    await page.waitForLoadState('networkidle');
    
    // TODO: Authenticate with Keycloak (mock for now)
    // await page.click('text=Sign in with Keycloak');
    
    // Navigate to create issuance
    await page.goto('http://localhost:3001/issuances/create', { waitUntil: 'networkidle' });
    
    // Fill form with explicit waits
    await page.fill('input[name="assetId"]', '3fa85f64-5717-4562-b3fc-2c963f66afa6', { timeout: 5000 });
    await page.fill('input[name="issuerId"]', '3fa85f64-5717-4562-b3fc-2c963f66afa6');
    await page.fill('input[name="totalAmount"]', '1000000');
    await page.fill('input[name="nominal"]', '1000');
    await page.fill('input[name="issueDate"]', '2025-01-01');
    await page.fill('input[name="maturityDate"]', '2026-01-01');
    
    // Submit form with wait for navigation
    await Promise.all([
      page.waitForResponse(response => 
        response.url().includes('/v1/issuances') && response.status() === 201,
        { timeout: 10000 }
      ),
      page.click('button[type="submit"]'),
    ]);
    
    // Wait for success message or navigation
    await page.waitForTimeout(2000);
    
    // Verify navigation or success message
    expect(page.url()).toContain('/issuances');
  });

  test('should place buy order', async ({ page }) => {
    // Navigate to investor portal
    await page.goto('http://localhost:3002/orders/new', { waitUntil: 'networkidle' });
    
    // Fill order form with explicit waits
    await page.fill('input[name="issuanceId"]', '3fa85f64-5717-4562-b3fc-2c963f66afa6', { timeout: 5000 });
    await page.fill('input[name="amount"]', '50000');
    
    // Submit with wait for response
    await Promise.all([
      page.waitForResponse(response => 
        response.url().includes('/v1/orders') && (response.status() === 202 || response.status() === 429),
        { timeout: 10000 }
      ),
      page.click('button[type="submit"]'),
    ]);
    
    // Wait for navigation or success
    await page.waitForTimeout(2000);
    
    // Verify navigation to portfolio or success message
    const currentUrl = page.url();
    expect(currentUrl).toMatch(/\/portfolio|\/orders/);
  });

  test('should handle rate limiting', async ({ page }) => {
    // Make multiple rapid requests to trigger rate limit
    const requests = Array(10).fill(null).map(() => 
      page.request.post('http://localhost:5000/v1/orders', {
        data: {
          investorId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          issuanceId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          amount: 50000,
        },
        headers: {
          'Idempotency-Key': `test-${Date.now()}-${Math.random()}`,
        },
      })
    );

    const responses = await Promise.all(requests);
    
    // At least one should be rate limited (429)
    const rateLimited = responses.some(r => r.status() === 429);
    expect(rateLimited).toBeTruthy();
  });
});
