#!/bin/bash
set -euo pipefail

# OIS-CFA · Deploy Node Script
# Usage:
#   ./ops/scripts/deploy/deploy-node.sh <host>
#
# Responsibilities:
#   - Ensure tmux session "p-cfa" exists on target host
#   - Clone or update ois-cfa repo under /srv/cfa (or REMOTE_PROJECT_ROOT)
#   - Checkout desired branch (infra.defis.deploy by default)
#   - Run docker compose to start backend stack
#   - Prepare basic commands for frontends (Next.js + PM2) inside tmux

usage() {
  echo "Usage: $0 <host>" >&2
  echo "Environment (optional):" >&2
  echo "  SSH_USER             - SSH user for connection (default: user)" >&2
  echo "  REMOTE_PROJECT_ROOT  - Project root on target (default: /srv/cfa)" >&2
  echo "  OIS_CFA_GIT_URL      - Git URL for ois-cfa (default: git@git.telex.global:npk/ois-cfa.git)" >&2
  echo "  OIS_CFA_BRANCH       - Branch to checkout (default: infra.defis.deploy)" >&2
  exit 1
}

if [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
  usage
fi

TARGET_HOST="${1:-}"
if [ -z "$TARGET_HOST" ]; then
  usage
fi

SSH_USER="${SSH_USER:-user}"
REMOTE_PROJECT_ROOT="${REMOTE_PROJECT_ROOT:-/srv/cfa}"
OIS_CFA_GIT_URL="${OIS_CFA_GIT_URL:-git@git.telex.global:npk/ois-cfa.git}"
OIS_CFA_BRANCH="${OIS_CFA_BRANCH:-infra.defis.deploy}"
SSH_OPTS="-o StrictHostKeyChecking=accept-new"

echo ">>> Deploying OIS-CFA to host '${TARGET_HOST}' as ${SSH_USER}"
echo "    REMOTE_PROJECT_ROOT=${REMOTE_PROJECT_ROOT}"
echo "    OIS_CFA_GIT_URL=${OIS_CFA_GIT_URL}"
echo "    OIS_CFA_BRANCH=${OIS_CFA_BRANCH}"

SSH_CMD="ssh ${SSH_OPTS} ${SSH_USER}@${TARGET_HOST}"

$SSH_CMD bash -s << EOF
set -euo pipefail

echo ">>> [\$(hostname)] Phase 3 · Deploy OIS-CFA Stack"

if ! command -v tmux >/dev/null 2>&1; then
  echo "ERROR: tmux is not installed. Run provision-node.sh first." >&2
  exit 1
fi

mkdir -p "${REMOTE_PROJECT_ROOT}"
cd "${REMOTE_PROJECT_ROOT}"

if ! tmux has-session -t p-cfa 2>/dev/null; then
  echo ">>> Creating tmux session 'p-cfa'"
  tmux new-session -d -s p-cfa -c "${REMOTE_PROJECT_ROOT}"
else
  echo ">>> Reusing existing tmux session 'p-cfa'"
fi

tmux send-keys -t p-cfa "set -euo pipefail" C-m
tmux send-keys -t p-cfa "cd '${REMOTE_PROJECT_ROOT}'" C-m

if [ ! -d "${REMOTE_PROJECT_ROOT}/ois-cfa/.git" ]; then
  echo ">>> Cloning ois-cfa repo"
  tmux send-keys -t p-cfa "git clone '${OIS_CFA_GIT_URL}' ois-cfa" C-m
  tmux send-keys -t p-cfa "cd ois-cfa" C-m
else
  echo ">>> Updating existing ois-cfa repo"
  tmux send-keys -t p-cfa "cd ois-cfa" C-m
fi

tmux send-keys -t p-cfa "git fetch origin" C-m
tmux send-keys -t p-cfa "git checkout '${OIS_CFA_BRANCH}'" C-m
tmux send-keys -t p-cfa "git pull --ff-only origin '${OIS_CFA_BRANCH}'" C-m

tmux send-keys -t p-cfa "echo '>>> NOTE: ensure .env files are prepared as per docs/deploy/docker-compose-at-vps/02-env-and-compose.md'" C-m

tmux send-keys -t p-cfa "docker compose -f docker-compose.yml -f docker-compose.override.yml up -d" C-m

tmux send-keys -t p-cfa "echo '>>> Backend stack started. Next steps: configure Keycloak, nginx, and PM2 frontends as per docs/deploy/docker-compose-at-vps/* and 20251113-cloudflare-ingress.md'" C-m

echo ">>> Commands sent to tmux session 'p-cfa'. Attach with: tmux attach -t p-cfa"
EOF

echo ">>> Deploy commands dispatched to host '${TARGET_HOST}'"

