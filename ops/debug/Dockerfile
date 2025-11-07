# Debug toolbox image with all necessary tools
FROM bitnami/kubectl:1.30

# Install additional tools
USER root

# Install jq, curl, and other utilities
RUN apt-get update && \
    apt-get install -y \
    jq \
    curl \
    wget \
    vim \
    nano \
    less \
    tar \
    gzip \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install GitLab CLI (glab)
RUN GLAB_VERSION="1.33.0" && \
    curl -L "https://github.com/profclems/glab/releases/download/v${GLAB_VERSION}/glab_${GLAB_VERSION}_Linux_x86_64.tar.gz" -o /tmp/glab.tar.gz && \
    tar -xzf /tmp/glab.tar.gz -C /tmp && \
    mv /tmp/bin/glab /usr/local/bin/glab && \
    chmod +x /usr/local/bin/glab && \
    rm -rf /tmp/glab.tar.gz /tmp/bin

# Install yq
RUN YQ_VERSION="v4.40.5" && \
    curl -L "https://github.com/mikefarah/yq/releases/download/${YQ_VERSION}/yq_linux_amd64" -o /usr/local/bin/yq && \
    chmod +x /usr/local/bin/yq

# Install helm (optional)
RUN curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash || true

# Install ArgoCD CLI (optional, can be installed at runtime)
# RUN ARGOCD_VERSION="2.10.0" && \
#     curl -L "https://github.com/argoproj/argo-cd/releases/download/v${ARGOCD_VERSION}/argocd-linux-amd64" -o /usr/local/bin/argocd && \
#     chmod +x /usr/local/bin/argocd

# Create scripts directory
RUN mkdir -p /scripts

# Set working directory
WORKDIR /scripts

# Default command
CMD ["/bin/bash"]

