# Timeweb Cloud Infrastructure (Terraform)

Terraform configuration for provisioning Kubernetes cluster in Timeweb Cloud for OIS-CFA project.

## Quick Start

1. **Copy example variables:**
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

2. **Edit `terraform.tfvars`** with your Timeweb Cloud API token and configuration.

3. **Initialize Terraform:**
   ```bash
   make tf:init
   ```

4. **Plan changes:**
   ```bash
   make tf:plan
   ```

5. **Apply configuration:**
   ```bash
   make tf:apply
   ```

6. **Get kubeconfig:**
   ```bash
   terraform output -raw kubeconfig > kubeconfig.yaml
   export KUBECONFIG=$(pwd)/kubeconfig.yaml
   ```

## Documentation

See [docs/ops/timeweb/terraform.md](../../../docs/ops/timeweb/terraform.md) for detailed documentation.

## GitLab State Backend

For GitLab managed Terraform state, see the documentation for setup instructions.

