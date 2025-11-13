---
created: 2025-11-13 13:45
updated: 2025-11-13 13:45
type: operations-runbook
sphere: devops
topic: uk1 cloudflare ingress
author: Alex (co-76ca)
agentID: co-76ca
partAgentID: [co-76ca]
version: 0.1.0
tags: [cloudflare, nginx, keycloak, demo]
---

# Goal
Обеспечить публичный доступ к UK1-стенду (Keycloak + порталы + API) по доменам `*.cfa.llmneighbors.com`, используя Cloudflare (DNS + TLS), системный nginx и docker-compose override для Keycloak.

# Scope
- Сервера UK1 (`185.168.192.214`, Ubuntu, `/opt/ois-cfa`).
- Cloudflare аккаунт `llmneighbors.com` (CLI/Token уже лежит в `/home/user/__Repositories/cloudflare__developerisnow/.env`).
- Keycloak + порталы (pm2) + API gateway, без модификации .NET compose.

# Checklist
- [ ] Cloudflare DNS: A-записи `auth|issuer|investor|backoffice|api.cfa.llmneighbors.com → 185.168.192.214` (DNS only).
- [ ] Cloudflare SSL Mode = `Full` (после первичной настройки можно краткосрочно переключать на Flexible для отладки).
- [ ] Wildcard LE-сертификат `*.cfa.llmneighbors.com` выпущен через `certbot --dns-cloudflare` и хранится в `/etc/letsencrypt/live/cfa.llmneighbors.com/`.
- [ ] `/etc/nginx/sites-available/cfa-portals.conf` развёрнут из шаблона `ops/infra/uk1/nginx-cfa-portals.conf`, включён и перезапущен nginx.
- [ ] Docker override `ops/infra/uk1/docker-compose.keycloak-proxy.yml` прокатили (KEYCLOAK_PUBLIC_URL указывает на `https://auth.cfa.llmneighbors.com`).
- [ ] `.env.local` порталов указывает на публичные URL, pm2 перезапущен.
- [ ] Keycloak clients обновлены (redirect/webOrigins) + отключены `requiredActions`.
- [ ] Playwright e2e (issuer/investor) прошёл и скриншоты сохранены.
- [ ] VPN `x-ui` пересажен на свободный порт, если нужен (по умолчанию сервис выключен, чтобы освободить 443).

# Why → What → How → Result

## Why
- Демки должны открываться из браузера без SSH-туннелей.
- Клиенты хотят использовать собственный домен (`*.cfa.llmneighbors.com`).
- Надо иметь повторяемый чеклист для второго DevOps (Саша О.).

## What
- Cloudflare управляет DNS и выпускает wildcard сертификат (через DNS challenge).
- Nginx на UK1 выполняет offload TLS + проксирует на локальные порты (Keycloak 8081, pm2 порталы 300x, API 5000).
- docker-compose override поднимает Keycloak с правильным `KC_HOSTNAME_URL`.
- `.env.local` порталов и Keycloak clients должны ссылаться на публичные URL.

## How
1. **DNS + SSL (Cloudflare CLI):**
   ```bash
   cd /home/user/__Repositories/cloudflare__developerisnow
   source .env  # экспортирует CLOUDFLARE_API_TOKEN
   CF_API_TOKEN="$CLOUDFLARE_API_TOKEN" flarectl dns create-or-update --zone llmneighbors.com --name auth.cfa --type A --content 185.168.192.214 --ttl 1
   # повторить для issuer / investor / backoffice / api (DNS only)
   # SSL mode
   curl -sX PATCH "https://api.cloudflare.com/client/v4/zones/2f4591aa91796b09311095cfee03d817/settings/ssl" \
     -H "Authorization: Bearer $CLOUDFLARE_API_TOKEN" -H "Content-Type: application/json" \
     --data '{"value":"full"}'
   ```
2. **Wildcard сертификат:**
   ```bash
   ssh -p 51821 root@185.168.192.214
   mkdir -p /root/.secrets && chmod 700 /root/.secrets
   cat > /root/.secrets/cloudflare.ini <<'EOF'
   dns_cloudflare_api_token = ${CLOUDFLARE_API_TOKEN}
   EOF
   chmod 600 /root/.secrets/cloudflare.ini
   certbot certonly --dns-cloudflare --dns-cloudflare-credentials /root/.secrets/cloudflare.ini \
     --dns-cloudflare-propagation-seconds 45 \
     -d "*.cfa.llmneighbors.com" -d "cfa.llmneighbors.com" \
     --agree-tos --email <ops@developerisnow.com> --non-interactive
   ```
3. **nginx:**
   ```bash
   apt-get install -y nginx
   systemctl stop x-ui && systemctl disable x-ui   # освободить 443 (пересадим позже)
   cd /opt/ois-cfa
   env CFA_BASE_DOMAIN=cfa.llmneighbors.com \
       AUTH_HOST=auth.cfa.llmneighbors.com \
       ISSUER_HOST=issuer.cfa.llmneighbors.com \
       INVESTOR_HOST=investor.cfa.llmneighbors.com \
       BACKOFFICE_HOST=backoffice.cfa.llmneighbors.com \
       API_HOST=api.cfa.llmneighbors.com \
       envsubst < ops/infra/uk1/nginx-cfa-portals.conf > /etc/nginx/sites-available/cfa-portals.conf
   ln -sf /etc/nginx/sites-available/cfa-portals.conf /etc/nginx/sites-enabled/cfa-portals.conf
   rm -f /etc/nginx/sites-enabled/default
   nginx -t && systemctl reload nginx
   ```
4. **docker-compose override:**
   ```bash
   cd /opt/ois-cfa
   KEYCLOAK_PUBLIC_URL=https://auth.cfa.llmneighbors.com \
   docker compose -f docker-compose.yml -f docker-compose.override.yml \
     -f ops/infra/uk1/docker-compose.keycloak-proxy.yml up -d keycloak keycloak-proxy
   ```
5. **Клиенты + пользователи:**
   ```bash
   docker exec ois-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin123
   docker exec ois-keycloak /opt/keycloak/bin/kcadm.sh update clients/<id> -r ois-dev -s "redirectUris=[\"https://issuer.cfa.llmneighbors.com/*\"]" -s "webOrigins=[\"https://issuer.cfa.llmneighbors.com\"]"
   # повторить для investor/backoffice
   docker exec ois-keycloak /opt/keycloak/bin/kcadm.sh update realms/ois-dev/authentication/required-actions/VERIFY_PROFILE -s enabled=false -s defaultAction=false
   ```
6. **Порталы:**
   ```bash
   cat > /opt/ois-cfa/apps/portal-issuer/.env.local <<'EOF'
   NEXT_PUBLIC_API_BASE_URL=https://api.cfa.llmneighbors.com
   NEXT_PUBLIC_KEYCLOAK_URL=https://auth.cfa.llmneighbors.com
   NEXT_PUBLIC_KEYCLOAK_REALM=ois-dev
   NEXT_PUBLIC_KEYCLOAK_CLIENT_ID=portal-issuer
   NEXTAUTH_URL=https://issuer.cfa.llmneighbors.com
   NEXTAUTH_SECRET=...
   KEYCLOAK_CLIENT_SECRET=...
   EOF
   # аналогично для investor/backoffice
   source /root/.nvm/nvm.sh && pm2 restart portal-issuer portal-investor portal-backoffice --update-env
   ```
7. **Проверка:**
   ```bash
   curl -I https://auth.cfa.llmneighbors.com
   curl https://api.cfa.llmneighbors.com/health
   cd /tmp/playwright-run && node index.js  # сценарий из /tmp/uk1-login-check.js
   ```

## Result
- Пользовательские порталы и Keycloak доступны по HTTPS без SSH-туннелей.
- Playwright обеспечивает «доказательство» логина (скриншоты приложены в memory-bank).
- Вся конфигурация задокументирована и может быть переиспользована для других VPS.

# Notes
- `x-ui` (VPN) был отключён из-за конфликтов порта 443. При переносе на другой порт добавьте `sudo sed -i 's/:443/:<new_port>/' /etc/systemd/system/x-ui.service` и перезапустите nginx.
- IaC артефакты: `ops/infra/uk1/nginx-cfa-portals.conf` и `ops/infra/uk1/docker-compose.keycloak-proxy.yml`.
- Инструменты: `flarectl`, `wrangler`, `certbot-dns-cloudflare`, `pm2`, `playwright`.
