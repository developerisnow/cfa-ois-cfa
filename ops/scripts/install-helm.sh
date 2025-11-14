#!/bin/bash
# Install Helm package manager for Kubernetes
# Usage: ./ops/scripts/install-helm.sh

set -euo pipefail

HELM_VERSION="${HELM_VERSION:-3.14.0}"

echo "=== Installing Helm ${HELM_VERSION} ==="

# Check if helm is already installed
if command -v helm &> /dev/null; then
    CURRENT_VERSION=$(helm version --template '{{.Version}}' 2>/dev/null | sed 's/v//' || echo "")
    if [ -n "${CURRENT_VERSION}" ]; then
        echo "✓ Helm already installed: v${CURRENT_VERSION}"
        echo "To reinstall, remove existing helm first: sudo rm -f $(which helm)"
        exit 0
    fi
fi

# Detect OS and architecture
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case "${ARCH}" in
    x86_64)
        ARCH="amd64"
        ;;
    aarch64|arm64)
        ARCH="arm64"
        ;;
    *)
        echo "Error: Unsupported architecture: ${ARCH}"
        exit 1
        ;;
esac

echo "Detected OS: ${OS}, Architecture: ${ARCH}"

# Download Helm
HELM_URL="https://get.helm.sh/helm-v${HELM_VERSION}-${OS}-${ARCH}.tar.gz"
TMP_DIR=$(mktemp -d)
TAR_FILE="${TMP_DIR}/helm.tar.gz"

echo "Downloading Helm from ${HELM_URL}..."
curl -L -o "${TAR_FILE}" "${HELM_URL}" || {
    echo "Error: Failed to download Helm"
    rm -rf "${TMP_DIR}"
    exit 1
}

# Extract and install
echo "Extracting Helm..."
tar -xzf "${TAR_FILE}" -C "${TMP_DIR}"

# Install to /usr/local/bin (requires sudo) or ~/.local/bin
INSTALL_DIR="${HOME}/.local/bin"
mkdir -p "${INSTALL_DIR}"

if [ -w "/usr/local/bin" ]; then
    INSTALL_DIR="/usr/local/bin"
    echo "Installing to ${INSTALL_DIR}..."
    sudo cp "${TMP_DIR}/${OS}-${ARCH}/helm" "${INSTALL_DIR}/helm" || {
        echo "Error: Failed to install helm to ${INSTALL_DIR}"
        rm -rf "${TMP_DIR}"
        exit 1
    }
else
    echo "Installing to ${INSTALL_DIR}..."
    cp "${TMP_DIR}/${OS}-${ARCH}/helm" "${INSTALL_DIR}/helm" || {
        echo "Error: Failed to install helm to ${INSTALL_DIR}"
        rm -rf "${TMP_DIR}"
        exit 1
    }
    
    # Add to PATH if not already there
    if [[ ":$PATH:" != *":${INSTALL_DIR}:"* ]]; then
        echo ""
        echo "⚠ Add ${INSTALL_DIR} to PATH:"
        echo "  export PATH=\"\${PATH}:${INSTALL_DIR}\""
        echo ""
        echo "Or add to ~/.bashrc:"
        echo "  echo 'export PATH=\"\${PATH}:${INSTALL_DIR}\"' >> ~/.bashrc"
    fi
fi

# Cleanup
rm -rf "${TMP_DIR}"

# Verify installation
if command -v helm &> /dev/null || [ -f "${INSTALL_DIR}/helm" ]; then
    if [ -f "${INSTALL_DIR}/helm" ]; then
        "${INSTALL_DIR}/helm" version --client
    else
        helm version --client
    fi
    echo ""
    echo "✓ Helm installed successfully"
    echo "Version: $("${INSTALL_DIR}/helm" version --template '{{.Version}}' 2>/dev/null || helm version --template '{{.Version}}')"
else
    echo "Error: Helm installation verification failed"
    exit 1
fi

