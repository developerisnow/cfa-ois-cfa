output "cluster_id" {
  description = "Kubernetes cluster ID"
  value       = twc_k8s_cluster.main.id
}

output "cluster_name" {
  description = "Kubernetes cluster name"
  value       = twc_k8s_cluster.main.name
}

output "api_server_url" {
  description = "Kubernetes API server URL"
  value       = twc_k8s_cluster.main.api_server_url
}

output "kubeconfig" {
  description = "Kubernetes cluster kubeconfig (sensitive)"
  value       = twc_k8s_cluster.main.kubeconfig
  sensitive   = true
}

output "node_group_id" {
  description = "Node group ID"
  value       = twc_k8s_node_group.main.id
}

output "vpc_id" {
  description = "VPC ID (if enabled)"
  value       = var.vpc_enabled ? twc_vpc.main[0].id : null
}

output "firewall_id" {
  description = "Firewall ID (if enabled)"
  value       = var.firewall_enabled ? twc_firewall.k8s[0].id : null
}

