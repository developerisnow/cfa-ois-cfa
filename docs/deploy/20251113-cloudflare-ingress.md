created: 2025-11-13 13:45
updated: 2025-11-13 17:05
type: operations-runbook
sphere: devops
topic: uk1 cloudflare ingress
author: Alex (co-76ca)
agentID: co-76ca
partAgentID: [co-76ca]
version: 0.2.0
tags: [cloudflare, nginx, keycloak, demo, smtp]
---

# Goal
Обеспечить публичный доступ к UK1-стенду (Keycloak + порталы + API) по доменам `*.cfa.llmneighbors.com`, используя Cloudflare (DNS + TLS), системный nginx и docker-compose override для Keycloak.

# Scope
- Сервера UK1 (`185.168.192.214`, Ubuntu, `/opt/ois-cfa`).
- Cloudflare аккаунт `llmneighbors.com` (CLI/Token уже лежит в `/home/user/__Repositories/cloudflare__developerisnow/.env`).
- Keycloak + порталы (pm2) + API gateway, без модификации .NET compose.

- # Checklist
- [x] Cloudflare DNS: A-записи `auth|issuer|investor|backoffice|api.cfa.llmneighbors.com → 185.168.192.214` (DNS only).
- [x] Cloudflare SSL Mode = `Full`.
- [x] Wildcard LE-сертификат `*.cfa.llmneighbors.com` выпущен в `/etc/letsencrypt/live/cfa.llmneighbors.com/`.
- [x] `/etc/nginx/sites-available/cfa-portals.conf` развернут и nginx перезапущен.
- [x] Docker override `ops/infra/uk1/docker-compose.keycloak-proxy.yml` активирован (`KEYCLOAK_PUBLIC_URL=https://auth.cfa.llmneighbors.com`).
- [x] `.env.local` порталов обновлены, pm2 перезапущен.
- [x] Keycloak clients/realm откорректированы (redirects, webOrigins, self-registration ON, verifyEmail ON).
- [x] Playwright e2e (issuer/investor + self-registration + backoffice admin) проходит, отчёты в `tests/e2e-playwright/test-results/`.
- [x] VPN `x-ui` выключен (порт 443 свободен).
- [x] SMTP стек (Postfix + OpenDKIM) + SPF/DKIM/DMARC настроены; Keycloak использует локальный relay.
- [x] Postfix слушает только `127.0.0.1` и `172.18.0.1` (docker bridge); внешний 25 порт закрыт.

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
   cd tests/e2e-playwright && npm test
   ```

## Result
- Пользовательские порталы и Keycloak доступны по HTTPS без SSH-туннелей.
- Playwright обеспечивает «доказательство» логина (issuer/investor + self-registration).
- SMTP цепочка (Postfix + OpenDKIM) выдаёт проверочные письма; Keycloak self-registration завершает flow без ручных действий.
- Вся конфигурация задокументирована и может быть переиспользована для других VPS.

## Email / SMTP / DKIM
1. **Postfix + OpenDKIM**
   ```bash
   apt-get install -y postfix mailutils opendkim opendkim-tools
   postconf -e 'inet_interfaces = all'
   postconf -e 'mynetworks = 127.0.0.0/8 172.17.0.0/16 172.18.0.0/16'
   postconf -e 'smtpd_recipient_restrictions = permit_mynetworks, reject_unauth_destination'
   postconf -e 'smtpd_relay_restrictions = permit_mynetworks, reject_unauth_destination'
   systemctl enable --now opendkim postfix
   ```

   `/etc/opendkim.conf` (основное):
   ```conf
   UserID                  opendkim:opendkim
   Socket                  inet:8891@127.0.0.1
   KeyTable                refile:/etc/opendkim/KeyTable
   SigningTable            refile:/etc/opendkim/SigningTable
   InternalHosts           /etc/opendkim/TrustedHosts
   ```
   Ключ `mail._domainkey.cfa.llmneighbors.com` → TXT (см. Cloudflare ниже).

2. **Cloudflare DNS для почты**
   ```bash
   # A-запись
   curl -sX POST ... --data '{"type":"A","name":"mail.cfa.llmneighbors.com","content":"185.168.192.214","proxied":false}'
   # MX
   curl -sX POST ... --data '{"type":"MX","name":"cfa.llmneighbors.com","content":"mail.cfa.llmneighbors.com","priority":10}'
   # SPF
   curl -sX POST ... --data '{"type":"TXT","name":"cfa.llmneighbors.com","content":"v=spf1 ip4:185.168.192.214 ~all"}'
   # DKIM
   curl -sX POST ... --data '{"type":"TXT","name":"mail._domainkey.cfa.llmneighbors.com","content":"v=DKIM1; ..."}'
   # DMARC
   curl -sX POST ... --data '{"type":"TXT","name":"_dmarc.cfa.llmneighbors.com","content":"v=DMARC1; p=none; rua=mailto:ops@llmneighbors.com; fo=1"}'
   ```

3. **Keycloak realm SMTP**
   ```bash
   docker exec ois-keycloak /opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin123
   docker exec ois-keycloak /opt/keycloak/bin/kcadm.sh update realms/ois-dev \
     -s verifyEmail=true -s registrationAllowed=true \
     -s "smtpServer.host=172.18.0.1" \
     -s "smtpServer.port=25" \
     -s "smtpServer.from=no-reply@cfa.llmneighbors.com" \
     -s "smtpServer.replyTo=ops@llmneighbors.com" \
     -s "smtpServer.envelopeFrom=no-reply@cfa.llmneighbors.com" \
     -s "smtpServer.starttls=false" -s "smtpServer.ssl=false" -s "smtpServer.auth=false"
   ```

4. **Smoke**
   ```bash
   echo "SMTP ok" | mail -s "Test" cfa+demo@2200freefonts.com
   tail -f /var/log/mail.log  # подтверждаем delivery
   TOKEN=$(curl -s -X POST https://api.mail.tm/token ...)
   curl -H "Authorization: Bearer $TOKEN" https://api.mail.tm/messages
   ```
   Playwright self-registration (`tests/e2e-playwright/tests/self-registration.spec.ts`) и backoffice spec (`tests/e2e-playwright/tests/backoffice-auth.spec.ts`) используют те же домены/SMTP.

# Notes
- `x-ui` (VPN) был отключён из-за конфликтов порта 443. При переносе на другой порт добавьте `sudo sed -i 's/:443/:<new_port>/' /etc/systemd/system/x-ui.service` и перезапустите nginx.
- IaC артефакты: `ops/infra/uk1/nginx-cfa-portals.conf` и `ops/infra/uk1/docker-compose.keycloak-proxy.yml`.
- Инструменты: `flarectl`, `wrangler`, `certbot-dns-cloudflare`, `pm2`, `playwright`, `mail.tm`.
