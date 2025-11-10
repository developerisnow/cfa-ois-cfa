# 10 · WDS Environment Inventory

Fill with actual server data before rollout.

Host
- OS / kernel: <Ubuntu 22.04, 5.x>
- CPU: <vCPU count>
- RAM: <GiB>
- Disk: <device, mount, free>
- DNS / domain: <fqdn>
- SSH: <user@host>

Network
- Egress to Docker registries: yes/no
- Firewall rules: <list>
- Reverse proxy / ingress: <none|nginx|haproxy>

Accounts & Access
- sudo enabled: yes/no
- Docker group membership: yes/no
- Secrets storage: <path/provider>
