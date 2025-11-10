# Inventory · WDS

Use `bin/check_resources.sh` and `bin/check_ports.sh` on the target server.

Commands
- `./.ui/docs/deploy/bin/check_resources.sh > ./.ui/docs/deploy/_out/resources.md`
- `./.ui/docs/deploy/bin/check_ports.sh > ./.ui/docs/deploy/_out/ports.md`

Captures
- CPU, memory, disk
- OS, kernel, Docker/Compose versions
- Listening TCP/UDP ports and owning processes
