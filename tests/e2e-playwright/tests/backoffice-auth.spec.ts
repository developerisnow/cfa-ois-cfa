import { test, expect } from '@playwright/test';

test('backoffice admin authenticates via Keycloak', async ({ page }) => {
  const baseUrl = process.env.BACKOFFICE_BASE_URL ?? 'https://backoffice.cfa.llmneighbors.com';
  const username = process.env.BACKOFFICE_USER ?? 'admin@test.com';
  const password = process.env.BACKOFFICE_PASS ?? 'password123';

  await page.goto(baseUrl, { waitUntil: 'networkidle' });
  const signInButton = page.getByRole('button', { name: /keycloak/i });
  await expect(signInButton).toBeVisible();
  await signInButton.click();

  await page.waitForSelector('#username', { timeout: 30_000 });
  await page.fill('#username', username);
  await page.fill('#password', password);
  await page.click('#kc-login');

  await page.waitForURL(/backoffice/, { timeout: 60_000 });
  await expect(page).toHaveURL(new RegExp(`${baseUrl}`));
});
