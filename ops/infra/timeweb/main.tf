# VPC (optional, for network isolation)
resource "twc_vpc" "main" {
  count       = var.vpc_enabled ? 1 : 0
  name        = var.vpc_name
  description = "VPC for OIS-CFA Kubernetes cluster"
}

# Kubernetes Cluster
# NOTE: Some parameters (network_driver, vpc_id) may differ in actual Timeweb Cloud provider.
# Check official documentation: https://registry.terraform.io/providers/timeweb-cloud/timeweb-cloud/latest/docs/resources/k8s_cluster
resource "twc_k8s_cluster" "main" {
  name     = var.cluster_name
  location = var.cluster_location
  version  = var.cluster_version

  # VPC ID if VPC is enabled (verify parameter name in provider docs)
  # vpc_id = var.vpc_enabled ? twc_vpc.main[0].id : null

  # Network configuration (verify if this parameter exists)
  # network_driver = "flannel" # or "calico"

  # High availability (if supported)
  # ha_enabled = true

  lifecycle {
    create_before_destroy = true
  }
}

# Node Group
resource "twc_k8s_node_group" "main" {
  cluster_id   = twc_k8s_cluster.main.id
  name         = var.node_group_name
  preset_id    = var.node_group_preset_id
  node_count   = var.node_count
  disk_size    = var.node_disk_size

  # Auto-scaling (if supported by provider)
  # autoscaling {
  #   min_nodes = 3
  #   max_nodes = 10
  # }

  lifecycle {
    create_before_destroy = true
  }
}

# Firewall rules (if enabled)
resource "twc_firewall" "k8s" {
  count = var.firewall_enabled ? 1 : 0

  name        = "${var.cluster_name}-firewall"
  description = "Firewall rules for OIS-CFA Kubernetes cluster"

  # Allow Kubernetes API server (port 6443)
  rule {
    direction = "ingress"
    protocol  = "tcp"
    port      = "6443"
    cidr      = "0.0.0.0/0" # Restrict to specific IPs in production
    action    = "allow"
  }

  # Allow NodePort range (30000-32767)
  rule {
    direction = "ingress"
    protocol  = "tcp"
    port      = "30000-32767"
    cidr      = "0.0.0.0/0" # Restrict to specific IPs in production
    action    = "allow"
  }

  # Allow SSH (port 22) - restrict to admin IPs in production
  rule {
    direction = "ingress"
    protocol  = "tcp"
    port      = "22"
    cidr      = "0.0.0.0/0" # TODO: Restrict to admin IPs
    action    = "allow"
  }

  # Allow HTTP/HTTPS for ingress
  rule {
    direction = "ingress"
    protocol  = "tcp"
    port      = "80"
    cidr      = "0.0.0.0/0"
    action    = "allow"
  }

  rule {
    direction = "ingress"
    protocol  = "tcp"
    port      = "443"
    cidr      = "0.0.0.0/0"
    action    = "allow"
  }
}

