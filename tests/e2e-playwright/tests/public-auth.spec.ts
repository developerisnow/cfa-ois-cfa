import { test, expect } from '@playwright/test';

interface PortalConfig {
  name: string;
  baseUrl: string;
  username: string;
  password: string;
  expectedPath: string;
}

const portals: PortalConfig[] = [
  {
    name: 'issuer',
    baseUrl: process.env.ISSUER_BASE_URL ?? 'https://issuer.cfa.llmneighbors.com',
    username: process.env.ISSUER_USER ?? 'issuer@test.com',
    password: process.env.ISSUER_PASS ?? 'password123',
    expectedPath: '/dashboard',
  },
  {
    name: 'investor',
    baseUrl: process.env.INVESTOR_BASE_URL ?? 'https://investor.cfa.llmneighbors.com',
    username: process.env.INVESTOR_USER ?? 'investor@test.com',
    password: process.env.INVESTOR_PASS ?? 'password123',
    expectedPath: '/portfolio',
  },
];

for (const portal of portals) {
  test(`${portal.name} portal authenticates via Keycloak`, async ({ page }) => {
    test.info().annotations.push({ type: 'portal', description: portal.name });

    await page.goto(portal.baseUrl, { waitUntil: 'networkidle' });

    const signInButton = page.getByRole('button', { name: /keycloak/i });
    await expect(signInButton).toBeVisible();
    await signInButton.first().click();

    await page.waitForSelector('#username', { timeout: 30_000 });
    await page.fill('#username', portal.username);
    await page.fill('#password', portal.password);
    await page.click('#kc-login');

    await page.waitForURL(`${portal.baseUrl}${portal.expectedPath}*`, { timeout: 60_000 });
    await expect(page).toHaveURL(new RegExp(`${portal.baseUrl}${portal.expectedPath}`));

    // Убеждаемся, что финальный URL соответствует ожидаемому разделу.
    await expect(page).toHaveURL(new RegExp(`${portal.baseUrl}${portal.expectedPath}`));
  });
}
