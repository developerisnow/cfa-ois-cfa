import { test, expect, APIRequestContext } from '@playwright/test';

const MAIL_TM_BASE = 'https://api.mail.tm';
const INVESTOR_BASE_URL =
  process.env.INVESTOR_BASE_URL ?? 'https://investor.cfa.llmneighbors.com';

interface DisposableMailbox {
  address: string;
  password: string;
  token: string;
}

const randomSuffix = () => Math.floor(Math.random() * 10_000);

async function resolveMailDomain(request: APIRequestContext): Promise<string> {
  const response = await request.get(`${MAIL_TM_BASE}/domains`);
  expect(response.ok()).toBeTruthy();
  const payload = await response.json();
  const domains: Array<{ domain: string; isActive: boolean }> =
    payload['hydra:member'] ?? [];
  const active = domains.find((domain) => domain.isActive);
  if (!active) {
    throw new Error('mail.tm does not expose any active domains right now');
  }
  return active.domain;
}

async function createMailbox(request: APIRequestContext): Promise<DisposableMailbox> {
  const domain = await resolveMailDomain(request);
  const address = `cfa-${Date.now()}-${randomSuffix()}@${domain}`;
  const password = `Passw0rd!${randomSuffix()}`;

  const createResp = await request.post(`${MAIL_TM_BASE}/accounts`, {
    data: { address, password },
    headers: { 'Content-Type': 'application/json' },
  });
  expect(createResp.ok()).toBeTruthy();

  const tokenResp = await request.post(`${MAIL_TM_BASE}/token`, {
    data: { address, password },
    headers: { 'Content-Type': 'application/json' },
  });
  expect(tokenResp.ok()).toBeTruthy();
  const tokenJson = await tokenResp.json();

  return {
    address,
    password,
    token: tokenJson.token as string,
  };
}

async function waitForVerificationLink(
  mailbox: DisposableMailbox,
  request: APIRequestContext,
): Promise<string> {
  const headers = {
    Authorization: `Bearer ${mailbox.token}`,
    'Content-Type': 'application/json',
  };

  for (let attempt = 0; attempt < 24; attempt += 1) {
    const messagesResp = await request.get(`${MAIL_TM_BASE}/messages`, { headers });
    expect(messagesResp.ok()).toBeTruthy();
    const data = await messagesResp.json();
    const messages = (data['hydra:member'] ?? []) as Array<Record<string, string>>;
    const candidate = messages.find((message) =>
      /verify/i.test(message.subject ?? ''),
    );
    if (candidate) {
      const detailResp = await request.get(`${MAIL_TM_BASE}${candidate['@id']}`, {
        headers,
      });
      expect(detailResp.ok()).toBeTruthy();
      const detail = await detailResp.json();
      const chunks: string[] = [];
      if (detail.text) {
        if (Array.isArray(detail.text)) {
          chunks.push(...detail.text);
        } else {
          chunks.push(detail.text as string);
        }
      }
      if (detail.html) {
        if (Array.isArray(detail.html)) {
          chunks.push(...detail.html);
        } else {
          chunks.push(detail.html as string);
        }
      }
      const normalized = chunks.join('\n');
      const match = normalized.match(
        /https:\/\/auth\.cfa\.llmneighbors\.com[^\s">]+/i,
      );
      if (!match) {
        throw new Error('Verification email received but activation link missing');
      }
      return match[0].replace(/&amp;/g, '&');
    }

    await new Promise((resolve) => setTimeout(resolve, 5000));
  }

  throw new Error('Verification email not received within 120s');
}

test('investor self-registration verifies email and logs in', async ({
  page,
  request,
}) => {
  test.setTimeout(240_000);
  const mailbox = await createMailbox(request);
  const password = `Passw0rd!${randomSuffix()}`;

  await page.goto(INVESTOR_BASE_URL, { waitUntil: 'domcontentloaded' });
  await page.getByRole('button', { name: /keycloak/i }).click();
  if (!/auth\.cfa\.llmneighbors\.com/.test(page.url())) {
    await page.waitForURL(/\/auth\/signin/, { timeout: 30_000 });
    await page.getByRole('button', { name: /keycloak/i }).click();
  }
  await page.waitForURL(/auth\.cfa\.llmneighbors\.com/, { timeout: 30_000 });

  const registerLink = page
    .locator('#kc-registration a, a#kc-registration, a[href*=\"login-actions/registration\"]')
    .first();
  await expect(registerLink).toBeVisible();
  await registerLink.click();
  await page.fill('#username', mailbox.address);
  await page.fill('#email', mailbox.address);
  await page.fill('#firstName', 'Playwright');
  await page.fill('#lastName', 'Investor');
  await page.fill('#password', password);
  await page.fill('#password-confirm', password);
  await page.getByRole('button', { name: /register/i }).click();

  await expect(page.locator('body')).toContainText(/verify/i);

  const verificationLink = await waitForVerificationLink(mailbox, request);
  await page.goto(verificationLink, { waitUntil: 'networkidle' });
  const verificationBody = await page.locator('body').innerText();
  if (/email/i.test(verificationBody) && /verified/i.test(verificationBody)) {
    await expect(page.locator('body')).toContainText(/email/i);
    await expect(page.locator('body')).toContainText(/verified/i);
  } else {
    await expect(page).toHaveURL(new RegExp(`${INVESTOR_BASE_URL}|auth\\.cfa`));
  }

  await page.goto(INVESTOR_BASE_URL, { waitUntil: 'domcontentloaded' });
  const loginButton = page.getByRole('button', { name: /keycloak/i });
  if ((await loginButton.count()) > 0) {
    await expect(loginButton).toBeVisible();
    await loginButton.click();
    await page.fill('#username', mailbox.address);
    await page.fill('#password', password);
    await page.click('#kc-login');
    await page.waitForURL(
      new RegExp(`${INVESTOR_BASE_URL}/(portfolio|catalog).*`),
      { timeout: 60_000 },
    );
  } else {
    await page.waitForURL(
      new RegExp(`${INVESTOR_BASE_URL}/(portfolio|catalog).*`),
      { timeout: 60_000 },
    );
  }
  await expect(page).toHaveURL(new RegExp(`${INVESTOR_BASE_URL}/(portfolio|catalog)`));
});
