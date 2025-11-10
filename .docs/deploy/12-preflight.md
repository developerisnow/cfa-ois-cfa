# 12 · Preflight Checks

Run this before any deployment action. It validates capacity, tools, and port availability based on `.env.ports`.

Usage
```bash
scp .docs/deploy/13-preflight.sh <user>@<host>:/opt/ois-cfa/
scp .docs/deploy/22-env.ports.example <user>@<host>:/opt/ois-cfa/.env.ports
ssh <user>@<host> 'cd /opt/ois-cfa && bash 13-preflight.sh | tee preflight-$(date +%F-%H%M).md'
```

Checks
- CPU/RAM/Disk summary
- Docker + Compose presence
- Port availability for configured host ports
- Basic network reachability

Output
- Markdown report file: `preflight-YYYY-MM-DD-HHMM.md`
- Exit non‑zero on failure
