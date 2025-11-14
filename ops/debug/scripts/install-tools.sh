#!/bin/bash
# Install additional tools in debug pod
# This script runs inside the debug pod

set -euo pipefail

echo "=== Installing debug tools ==="

# Update package list
apt-get update || apk update || yum update -y || true

# Install jq
if ! command -v jq &> /dev/null; then
  echo "Installing jq..."
  apt-get install -y jq || apk add jq || yum install -y jq || {
    # Fallback: download binary
    curl -L https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-amd64 -o /usr/local/bin/jq
    chmod +x /usr/local/bin/jq
  }
fi

# Install curl (usually already present)
if ! command -v curl &> /dev/null; then
  echo "Installing curl..."
  apt-get install -y curl || apk add curl || yum install -y curl || true
fi

# Install GitLab CLI (glab)
if ! command -v glab &> /dev/null; then
  echo "Installing GitLab CLI (glab)..."
  curl -s https://raw.githubusercontent.com/profclems/glab/main/scripts/install.sh | bash || {
    # Fallback: download binary
    GLAB_VERSION="1.33.0"
    curl -L "https://github.com/profclems/glab/releases/download/v${GLAB_VERSION}/glab_${GLAB_VERSION}_Linux_x86_64.tar.gz" -o /tmp/glab.tar.gz
    tar -xzf /tmp/glab.tar.gz -C /tmp
    mv /tmp/bin/glab /usr/local/bin/glab
    chmod +x /usr/local/bin/glab
    rm -rf /tmp/glab.tar.gz /tmp/bin
  }
fi

# Configure glab if token is available
if [ -n "${GITLAB_TOKEN:-}" ] && [ -n "${GITLAB_HOST:-}" ]; then
  echo "Configuring glab..."
  glab auth login --token "${GITLAB_TOKEN}" --hostname "${GITLAB_HOST}" || {
    echo "Warning: Failed to configure glab"
  }
fi

# Install k9s (optional)
if [ "${INSTALL_K9S:-false}" = "true" ]; then
  if ! command -v k9s &> /dev/null; then
    echo "Installing k9s..."
    K9S_VERSION="0.28.2"
    curl -L "https://github.com/derailed/k9s/releases/download/v${K9S_VERSION}/k9s_Linux_amd64.tar.gz" -o /tmp/k9s.tar.gz
    tar -xzf /tmp/k9s.tar.gz -C /tmp
    mv /tmp/k9s /usr/local/bin/k9s
    chmod +x /usr/local/bin/k9s
    rm -rf /tmp/k9s.tar.gz /tmp/LICENSE /tmp/README.md
  fi
fi

# Install additional useful tools
echo "Installing additional tools..."

# Install yq (YAML processor)
if ! command -v yq &> /dev/null; then
  echo "Installing yq..."
  YQ_VERSION="v4.40.5"
  curl -L "https://github.com/mikefarah/yq/releases/download/${YQ_VERSION}/yq_linux_amd64" -o /usr/local/bin/yq
  chmod +x /usr/local/bin/yq
fi

# Install helm (if not present)
if ! command -v helm &> /dev/null; then
  echo "Installing helm..."
  curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash || {
    echo "Warning: Failed to install helm"
  }
fi

# Install argocd CLI (if ArgoCD is present)
if kubectl get namespace argocd &>/dev/null 2>&1; then
  if ! command -v argocd &> /dev/null; then
    echo "Installing ArgoCD CLI..."
    ARGOCD_VERSION="2.10.0"
    curl -L "https://github.com/argoproj/argo-cd/releases/download/v${ARGOCD_VERSION}/argocd-linux-amd64" -o /usr/local/bin/argocd
    chmod +x /usr/local/bin/argocd
  fi
fi

echo ""
echo "=== Installed Tools ==="
echo "kubectl: $(kubectl version --client --short 2>/dev/null || echo 'not available')"
echo "jq: $(jq --version 2>/dev/null || echo 'not available')"
echo "curl: $(curl --version 2>/dev/null | head -1 || echo 'not available')"
echo "glab: $(glab --version 2>/dev/null || echo 'not available')"
echo "yq: $(yq --version 2>/dev/null || echo 'not available')"
echo "helm: $(helm version --short 2>/dev/null || echo 'not available')"
[ "${INSTALL_K9S:-false}" = "true" ] && echo "k9s: $(k9s version --short 2>/dev/null || echo 'not available')"
command -v argocd &>/dev/null && echo "argocd: $(argocd version --client --short 2>/dev/null || echo 'not available')"

echo ""
echo "Tools installation completed"

