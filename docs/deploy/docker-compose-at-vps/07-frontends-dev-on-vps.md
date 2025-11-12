# Frontends on VPS (Dev mode)

## ✅ TL;DR
- Запускаем фронтенды в dev-режиме на VPS (без Docker)
- Порты: 3001 (issuer), 3002 (investor), 3003 (backoffice)
- Для доступа с Mac используйте SSH-туннели (см. ниже)

## 1) Предусловия (host)
- [x] Docker/Compose для бэкендов уже подняты (gateway 5000, keycloak 8080)
- [x] Node.js 20 LTS установлен
  ```bash
  curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
  apt-get update -y && apt-get install -y nodejs build-essential
  node -v && npm -v
  npm i -g pm2@latest
  ```

## 2) Keycloak (realm и клиенты)
- [x] Убедитесь, что Keycloak запущен и доступен
  ```bash
  docker compose -f docker-compose.yml -f docker-compose.override.yml up -d keycloak
  curl -I http://localhost:8080/admin   # 302
  ```
- [x] Бутстрап реалма и клиентов (PUBLIC)
  ```bash
  docker cp ops/keycloak/bootstrap-realm.sh ois-keycloak:/tmp/bootstrap.sh
  docker exec ois-keycloak bash -lc \
    'KC_USER=admin KC_PASS=admin123 REALM=ois-dev \
     ISSUER_URL=http://localhost:3001 INVESTOR_URL=http://localhost:3002 BACKOFFICE_URL=http://localhost:3003 \
     ISSUER_TUNNEL_URL=http://localhost:15301 INVESTOR_TUNNEL_URL=http://localhost:15302 BACKOFFICE_TUNNEL_URL=http://localhost:15303 \
     bash /tmp/bootstrap.sh'
  ```

## 3) Env для фронтов
- [x] Создайте `.env.local` в каждой из папок:
  - `apps/portal-issuer/.env.local`
  - `apps/portal-investor/.env.local`
  - `apps/backoffice/.env.local`

  Содержимое:
  ```dotenv
  NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
  NEXT_PUBLIC_KEYCLOAK_URL=http://localhost:8080
  NEXT_PUBLIC_KEYCLOAK_REALM=ois-dev
  NEXT_PUBLIC_KEYCLOAK_CLIENT_ID=<portal-issuer|portal-investor|backoffice>
  NEXTAUTH_URL=http://localhost:<3001|3002|3003>
  ```

## 4) Установка зависимостей
- [x] Сначала SDK и shared-ui:
  ```bash
  cd /opt/ois-cfa/packages/sdks/ts && npm install --no-audit --no-fund --include=dev && npm run build
  cd /opt/ois-cfa/apps/shared-ui && npm install --no-audit --no-fund --include=dev
  ```
- [x] Затем каждый фронт:
  ```bash
  cd /opt/ois-cfa/apps/portal-issuer && npm install --no-audit --no-fund --include=dev
  cd /opt/ois-cfa/apps/portal-investor && npm install --no-audit --no-fund --include=dev
  cd /opt/ois-cfa/apps/backoffice && npm install --no-audit --no-fund --include=dev
  ```

## 5) Старт в dev-режиме (pm2)
- [x] Запуск и автосохранение:
  ```bash
  pm2 start npm --name portal-issuer --cwd /opt/ois-cfa/apps/portal-issuer -- run dev
  pm2 start npm --name portal-investor --cwd /opt/ois-cfa/apps/portal-investor -- run dev
  pm2 start npm --name backoffice    --cwd /opt/ois-cfa/apps/backoffice    -- run dev
  pm2 save
  pm2 ls
  ```

## 6) Проверки (обязательные)
- [x] Слушатели:
  ```bash
  ss -ltnp | egrep ":5000|:8080|:3001|:3002|:3003"
  ```
- [x] Коды ответов:
  ```bash
  curl -s -o /dev/null -w "GATEWAY:%{http_code}\n" http://localhost:5000/health
  curl -s -o /dev/null -w "KC:/admin %{http_code}\n" http://localhost:8080/admin
  for p in 3001 3002 3003; do curl -s -o /dev/null -w ":$p => %{http_code}\n" http://localhost:$p/; done
  ```

## 7) SSH‑туннели (Mac)
```bash
ssh -N \
  -L 15500:localhost:5000 \
  -L 15808:localhost:8080 \
  -L 15301:localhost:3001 \
  -L 15302:localhost:3002 \
  -L 15303:localhost:3003 \
  cfa1-mux
```
- Gateway: http://localhost:15500/health
- Keycloak: http://localhost:15808/admin (admin/admin123)
- Issuer: http://localhost:15301
- Investor: http://localhost:15302
- Backoffice: http://localhost:15303

## 8) Примечания
- Для временной разработки модульные зависимости `shared-ui` резолвятся через `next.config.js` (modules: `../shared-ui/node_modules`).
- В перспективе лучше перейти на workspaces (hoist deps на корень).

