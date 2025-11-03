# STRIDE — Контекст и границы доверия

> 

_Last updated: 2025-10-31 13:21  (Asia/Tokyo)_

| Flow | Threat (S/T/R/I/D/E) | Boundary | Mitigation |
|---|---|---|---|
| ESIA OIDC | Spoofing/Tampering | DMZ↔Core | mTLS, nonce, PKCE |
| УКЭП через ЭДО | Repudiation | Internet↔Core | УКЭП, OCSP, audit |
| Номинальные переводы | Tampering/DoS | Core↔Bank | idempotency, retries, signatures |
