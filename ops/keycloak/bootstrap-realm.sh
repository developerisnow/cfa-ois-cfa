#!/usr/bin/env bash
set -euo pipefail

KC_URL=${KC_URL:-http://localhost:8080}
KC_USER=${KC_USER:-admin}
KC_PASS=${KC_PASS:-admin123}
REALM=${REALM:-ois}

ISSUER_URL=${ISSUER_URL:-http://87.249.49.56:53001}
INVESTOR_URL=${INVESTOR_URL:-http://87.249.49.56:53002}
BACKOFFICE_URL=${BACKOFFICE_URL:-http://87.249.49.56:53003}
# Опционально добавить локальные туннельные адреса для редиректов
ISSUER_TUNNEL_URL=${ISSUER_TUNNEL_URL:-http://localhost:155101}
INVESTOR_TUNNEL_URL=${INVESTOR_TUNNEL_URL:-http://localhost:155102}
BACKOFFICE_TUNNEL_URL=${BACKOFFICE_TUNNEL_URL:-http://localhost:155103}
CLIENT_SECRET=${CLIENT_SECRET:-secret}

KCADM=/opt/keycloak/bin/kcadm.sh

${KCADM} config credentials --server ${KC_URL} --realm master --user ${KC_USER} --password ${KC_PASS}

# Create realm if not exists
if ! ${KCADM} get realms/${REALM} >/dev/null 2>&1; then
  ${KCADM} create realms -s realm=${REALM} -s enabled=true
fi

create_client() {
  local cid=$1
  local url=$2
  local tunnel_url=$3
  local id=$(${KCADM} create clients -r ${REALM} -s clientId=${cid} -s enabled=true -s protocol=openid-connect -s publicClient=false -s 'redirectUris=["'${url}'/*","'${tunnel_url}'/*"]' -s 'webOrigins=["*"]' -i)
  ${KCADM} update clients/${id}/client-secret -r ${REALM} -s value=${CLIENT_SECRET}
}

create_client portal-issuer ${ISSUER_URL} ${ISSUER_TUNNEL_URL}
create_client portal-investor ${INVESTOR_URL} ${INVESTOR_TUNNEL_URL}
create_client backoffice ${BACKOFFICE_URL} ${BACKOFFICE_TUNNEL_URL}

# Create demo users
if ! ${KCADM} get users -r ${REALM} -q username=investor >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=investor -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi
if ! ${KCADM} get users -r ${REALM} -q username=issuer >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=issuer -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi
if ! ${KCADM} get users -r ${REALM} -q username=backoffice >/dev/null 2>&1; then
  uid=$(${KCADM} create users -r ${REALM} -s username=backoffice -s enabled=true -i)
  ${KCADM} set-password -r ${REALM} --userid ${uid} --new-password Passw0rd!
fi

echo "Realm '${REALM}' bootstrapped with clients and demo users."
