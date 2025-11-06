# GitLab Managed Terraform State Backend
# Configure via environment variables or terraform.tfvars
# See docs/ops/timeweb/terraform.md for setup instructions

terraform {
  backend "http" {
    # These values should be set via environment variables or passed via -backend-config
    # address        = var.tf_http_address
    # lock_address   = var.tf_http_lock_address
    # unlock_address = var.tf_http_unlock_address
    # username       = var.tf_http_username
    # password       = var.tf_http_password
  }
}

# Note: Backend configuration cannot use variables directly.
# Use one of the following approaches:
# 1. Set via environment variables (TF_HTTP_ADDRESS, etc.)
# 2. Use -backend-config flags: terraform init -backend-config="address=..." -backend-config="username=..."
# 3. Create backend.hcl file (gitignored) with sensitive values
# 4. Use GitLab CI/CD variables in pipeline

