terraform {
  required_version = ">= 1.5.0"

  required_providers {
    twc = {
      source  = "timeweb-cloud/timeweb-cloud"
      version = "~> 1.6"
    }
  }
}

provider "twc" {
  token = var.twc_token
}

