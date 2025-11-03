import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-results.json' }],
  ],
  use: {
    baseURL: 'http://localhost:3001',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: 'cd ../../apps/portal-issuer && npm run dev',
      port: 3001,
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'cd ../../apps/portal-investor && npm run dev',
      port: 3002,
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'cd ../../apps/backoffice && npm run dev',
      port: 3003,
      reuseExistingServer: !process.env.CI,
    },
  ],
});

