# Public ingress smoke tests

Набор Playwright-тестов, проверяющих, что публичные домены (`issuer`, `investor`) действительно отдают Next.js портал и завершают Keycloak OAuth-флоу.

## Требования

- Node.js ≥ 18
- `npm install` в текущей директории (устанавливает `@playwright/test`).
- Chromium браузер для Playwright (`npx playwright install chromium`).

## Запуск

```bash
cd tests/e2e-playwright
npm install
npx playwright install chromium  # первый запуск
npm test
```

### Переменные окружения

| Env                     | Default                                 | Назначение |
|-------------------------|-----------------------------------------|-----------|
| `ISSUER_BASE_URL`       | `https://issuer.cfa.llmneighbors.com`   | URL портала эмитента |
| `INVESTOR_BASE_URL`     | `https://investor.cfa.llmneighbors.com` | URL портала инвестора |
| `ISSUER_USER`           | `issuer@test.com`                       | Пользователь для входа |
| `ISSUER_PASS`           | `password123`                           | Пароль |
| `INVESTOR_USER`         | `investor@test.com`                     | Пользователь инвестора |
| `INVESTOR_PASS`         | `password123`                           | Пароль |

Переменные нужны только если вы хотите использовать иные креды/домены.

## Структура

- `playwright.config.ts` – настройки раннера.
- `tests/public-auth.spec.ts` – главный сценарий (Keycloak OAuth → ожидаемая страница).

Результаты тестов по умолчанию доступны в `playwright-report/` и `test-results/` после завершения.
