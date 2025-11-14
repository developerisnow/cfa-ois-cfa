variable "twc_token" {
  description = "Timeweb Cloud API token"
  type        = string
  sensitive   = true
}

variable "cluster_name" {
  description = "Kubernetes cluster name"
  type        = string
  default     = "ois-cfa-k8s"
}

variable "cluster_location" {
  description = "Kubernetes cluster location (region). Available: ru-1 (Moscow), ru-2 (St. Petersburg), ru-3 (Kazan)"
  type        = string
  default     = "ru-1"
}

variable "cluster_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "node_group_name" {
  description = "Node group name"
  type        = string
  default     = "ois-cfa-nodes"
}

variable "node_group_preset_id" {
  description = "Node group preset ID (see Timeweb Cloud docs for available presets)"
  type        = number
  default     = 1 # Default preset, adjust based on requirements
}

variable "node_count" {
  description = "Number of nodes in the node group"
  type        = number
  default     = 3
}

variable "node_disk_size" {
  description = "Disk size for each node in GB"
  type        = number
  default     = 50
}

variable "vpc_enabled" {
  description = "Enable VPC for cluster isolation"
  type        = bool
  default     = true
}

variable "vpc_name" {
  description = "VPC name (if enabled)"
  type        = string
  default     = "ois-cfa-vpc"
}

variable "firewall_enabled" {
  description = "Enable firewall rules"
  type        = bool
  default     = true
}

# GitLab Terraform State Backend variables
variable "tf_http_address" {
  description = "GitLab Terraform state HTTP backend address"
  type        = string
  default     = ""
}

variable "tf_http_lock_address" {
  description = "GitLab Terraform state HTTP backend lock address"
  type        = string
  default     = ""
}

variable "tf_http_unlock_address" {
  description = "GitLab Terraform state HTTP backend unlock address"
  type        = string
  default     = ""
}

variable "tf_http_username" {
  description = "GitLab Terraform state HTTP backend username"
  type        = string
  default     = ""
  sensitive   = true
}

variable "tf_http_password" {
  description = "GitLab Terraform state HTTP backend password (token)"
  type        = string
  default     = ""
  sensitive   = true
}

