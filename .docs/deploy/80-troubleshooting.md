# Troubleshooting

- Port conflict: adjust `.env.deployment` and re‑up overrides
- DB migration failures: inspect service logs; check connection string and privileges
- Kafka unavailable: ensure zookeeper ready; check `KAFKA_ADVERTISED_LISTENERS`
- Keycloak fails to start: Postgres readiness and env vars
- TLS/SSL: place certs and configure reverse proxy; keep apps on localhost

Record each incident in `./_out/issue-<date>.md` with cause/remediation.
