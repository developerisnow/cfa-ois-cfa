#!/bin/bash
# Install Timeweb Cloud CLI (twc)
# Usage: ./tools/timeweb/install.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

echo "Installing Timeweb Cloud CLI (twc)..."

# Check if Python 3 is available
if ! command -v python3 &> /dev/null; then
    echo "Error: python3 is required but not installed."
    echo "Please install Python 3.8+ and try again."
    exit 1
fi

# Check if pip is available
if ! command -v pip3 &> /dev/null && ! command -v pip &> /dev/null; then
    echo "Error: pip is required but not installed."
    echo "Please install pip and try again."
    exit 1
fi

# Use pip3 if available, otherwise pip
PIP_CMD="pip3"
if ! command -v pip3 &> /dev/null; then
    PIP_CMD="pip"
fi

# Install twc-cli
echo "Installing twc-cli via pip..."
${PIP_CMD} install --user twc-cli || ${PIP_CMD} install twc-cli

# Verify installation
if command -v twc &> /dev/null; then
    echo "✓ twc CLI installed successfully"
    twc --version
else
    # Check if it's in user's local bin
    if [ -f "${HOME}/.local/bin/twc" ]; then
        echo "✓ twc CLI installed to ${HOME}/.local/bin/twc"
        echo "Add to PATH: export PATH=\"\${HOME}/.local/bin:\${PATH}\""
        "${HOME}/.local/bin/twc" --version
    else
        echo "Error: twc CLI installation failed or not found in PATH"
        echo "Please check installation and ensure ~/.local/bin is in your PATH"
        exit 1
    fi
fi

echo ""
echo "Next steps:"
echo "1. Configure twc with your API token:"
echo "   export TWC_TOKEN='your-token-here'"
echo "   twc config set token \$TWC_TOKEN"
echo ""
echo "2. Or use environment variable:"
echo "   export TWC_TOKEN='your-token-here'"
echo ""
echo "3. Verify configuration:"
echo "   twc k8s cluster list"

