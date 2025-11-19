#!/bin/bash
set -euo pipefail

# OIS-CFA · Provision Node Script
# Usage:
#   ./ops/scripts/deploy/provision-node.sh <host>
#
# Responsibilities (idempotent as much as possible):
#   - Ensure sudo user "user" exists on target host
#   - Install base packages (curl, git, tmux, jq, docker, docker-compose plugin, nginx, postfix)
#   - Install Node.js 20 for "user" (via nvm or system package)
#   - Create /srv/cfa and set ownership to user:user
#   - Configure tmux history-limit (1_000_000) for user
#   - Create tmux session "p-cfa" with working dir /srv/cfa

usage() {
  echo "Usage: $0 <host>" >&2
  echo "Environment (optional):" >&2
  echo "  SSH_USER         - SSH user for initial connection (default: root)" >&2
  echo "  REMOTE_USER      - App user to create/use (default: user)" >&2
  echo "  REMOTE_PROJECT_ROOT - Project root on target (default: /srv/cfa)" >&2
  exit 1
}

if [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
  usage
fi

TARGET_HOST="${1:-}"
if [ -z "$TARGET_HOST" ]; then
  usage
fi

SSH_USER="${SSH_USER:-root}"
REMOTE_USER="${REMOTE_USER:-user}"
REMOTE_PROJECT_ROOT="${REMOTE_PROJECT_ROOT:-/srv/cfa}"

SSH_OPTS="-o StrictHostKeyChecking=accept-new"

echo ">>> Provisioning host '${TARGET_HOST}' via ${SSH_USER}"

ssh $SSH_OPTS "${SSH_USER}@${TARGET_HOST}" \
  REMOTE_USER="$REMOTE_USER" REMOTE_PROJECT_ROOT="$REMOTE_PROJECT_ROOT" bash -s << 'EOF'
set -euo pipefail

echo ">>> [$(hostname)] Phase 2 · Provision Node"
echo "    REMOTE_USER=${REMOTE_USER}"
echo "    REMOTE_PROJECT_ROOT=${REMOTE_PROJECT_ROOT}"

if ! id "${REMOTE_USER}" >/dev/null 2>&1; then
  echo ">>> Creating user '${REMOTE_USER}' with sudo"
  useradd -m -s /bin/bash "${REMOTE_USER}"
  usermod -aG sudo "${REMOTE_USER}"
else
  echo ">>> User '${REMOTE_USER}' already exists, skipping creation"
fi

echo ">>> Installing base packages (curl, git, tmux, jq, ufw, docker, nginx, postfix)"
export DEBIAN_FRONTEND=noninteractive
apt-get update -y
apt-get install -y \
  ca-certificates curl git tmux jq ufw \
  nginx postfix

if ! command -v docker >/dev/null 2>&1; then
  echo ">>> Installing Docker Engine + compose plugin"
  install -m 0755 -d /etc/apt/keyrings
  if [ ! -f /etc/apt/keyrings/docker.gpg ]; then
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    chmod a+r /etc/apt/keyrings/docker.gpg
  fi
  . /etc/os-release
  echo "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  "$VERSION_CODENAME" stable" >/etc/apt/sources.list.d/docker.list
  apt-get update -y
  apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
  systemctl enable --now docker
else
  echo ">>> Docker already installed, skipping"
fi

echo ">>> Ensuring ${REMOTE_PROJECT_ROOT} exists"
mkdir -p "${REMOTE_PROJECT_ROOT}"
chown -R "${REMOTE_USER}:${REMOTE_USER}" "${REMOTE_PROJECT_ROOT}"

echo ">>> Installing Node.js 20 for ${REMOTE_USER} (via nvm if not present)"
su - "${REMOTE_USER}" -c 'bash -s' << 'EONVM'
set -euo pipefail
if [ ! -d "$HOME/.nvm" ]; then
  echo ">>> Installing nvm for ${USER}"
  curl -fsSL https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
fi

export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && . "$NVM_DIR/nvm.sh"

if ! nvm ls 20 >/dev/null 2>&1; then
  echo ">>> Installing Node.js 20 via nvm"
  nvm install 20
fi
nvm alias default 20
EONVM

TMUX_CONF="/home/${REMOTE_USER}/.tmux.conf"
if [ -f "\$TMUX_CONF" ]; then
  if ! grep -q 'history-limit' "\$TMUX_CONF"; then
    echo "set-option -g history-limit 1000000" >> "\$TMUX_CONF"
  fi
else
  echo "set-option -g history-limit 1000000" > "\$TMUX_CONF"
fi
chown "${REMOTE_USER}:${REMOTE_USER}" "\$TMUX_CONF"

echo ">>> Creating tmux session 'p-cfa' (if not exists)"
su - "${REMOTE_USER}" -c "tmux has-session -t p-cfa 2>/dev/null || tmux new-session -d -s p-cfa -c '${REMOTE_PROJECT_ROOT}'"

echo ">>> Provisioning done on \$(hostname)"
EOF

echo ">>> Host '${TARGET_HOST}' provisioned successfully"

