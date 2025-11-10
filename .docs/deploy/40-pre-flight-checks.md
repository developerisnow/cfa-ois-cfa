# Pre‑flight Checks

1) Hardware & OS
- Run `bin/check_resources.sh` → review CPU/RAM/disk and Docker/Compose versions
2) Ports conflicts
- Run `bin/check_ports.sh` → review conflicts; adjust `.env.deployment` accordingly
3) Network
- Ensure outbound network for image pulls; open firewall to needed ports only
4) Storage
- Ensure volumes directories exist with correct permissions

Outputs are saved under `./_out/` for audit.
