#!/usr/bin/env bash
set -euo pipefail

KC_URL=${KC_URL:-http://localhost:8080}
KC_USER=${KC_USER:-admin}
KC_PASS=${KC_PASS:-admin123}
# Use local-dev realm name per docs
REALM=${REALM:-ois-dev}

# Public app URLs (dev): local ports
ISSUER_URL=${ISSUER_URL:-http://localhost:3001}
INVESTOR_URL=${INVESTOR_URL:-http://localhost:3002}
BACKOFFICE_URL=${BACKOFFICE_URL:-http://localhost:3003}
# Optional tunnel addresses for mac SSH tunnels
ISSUER_TUNNEL_URL=${ISSUER_TUNNEL_URL:-http://localhost:15301}
INVESTOR_TUNNEL_URL=${INVESTOR_TUNNEL_URL:-http://localhost:15302}
BACKOFFICE_TUNNEL_URL=${BACKOFFICE_TUNNEL_URL:-http://localhost:15303}

KCADM=/opt/keycloak/bin/kcadm.sh

${KCADM} config credentials --server ${KC_URL} --realm master --user ${KC_USER} --password ${KC_PASS}

# Create realm if not exists
if ! ${KCADM} get realms/${REALM} >/dev/null 2>&1; then
  ${KCADM} create realms -s realm=${REALM} -s enabled=true
fi

ensure_client_public() {
  local cid=$1
  local url=$2
  local tunnel_url=$3
  local rid
  rid=$(${KCADM} get clients -r ${REALM} -q clientId=${cid} | sed -n 's/.*"id"\s*:\s*"\([^"]*\)".*/\1/p' | head -1 || true)
  if [ -z "${rid:-}" ]; then
    rid=$(${KCADM} create clients -r ${REALM} -s clientId=${cid} -s enabled=true -s protocol=openid-connect -i)
  fi
  ${KCADM} update clients/${rid} -r ${REALM} \
    -s publicClient=true \
    -s directAccessGrantsEnabled=true \
    -s standardFlowEnabled=true \
    -s 'redirectUris=["'${url}'/*","'${tunnel_url}'/*"]' \
    -s 'webOrigins=["'${url}'","'${tunnel_url}'"]'
}

ensure_client_public portal-issuer ${ISSUER_URL} ${ISSUER_TUNNEL_URL}
ensure_client_public portal-investor ${INVESTOR_URL} ${INVESTOR_TUNNEL_URL}
ensure_client_public backoffice ${BACKOFFICE_URL} ${BACKOFFICE_TUNNEL_URL}

# Create demo users
if ! ${KCADM} get users -r ${REALM} -q username=investor >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=investor@test.com -s email=investor@test.com -s emailVerified=true -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi
if ! ${KCADM} get users -r ${REALM} -q username=issuer >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=issuer@test.com -s email=issuer@test.com -s emailVerified=true -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi
if ! ${KCADM} get users -r ${REALM} -q username=backoffice >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=admin@test.com -s email=admin@test.com -s emailVerified=true -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi

echo "Realm '${REALM}' bootstrapped with clients and demo users."
