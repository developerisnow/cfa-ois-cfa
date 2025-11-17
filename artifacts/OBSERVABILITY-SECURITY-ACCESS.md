# Observability & Security - Access Information

## 📊 Grafana

**URL:** http://localhost:3000  
**Username:** `admin`  
**Password:** `admin`

**Note:** При первом входе Grafana попросит изменить пароль.

**Dashboards:**
- OIS - API Latency & RPS
- OIS - Payouts Throughput

## 📈 Prometheus

**URL:** http://localhost:9090  
**No authentication** (dev mode)

**Scrape targets:**
- api-gateway:8080/metrics
- issuance-service:8080/metrics
- registry-service:8080/metrics
- settlement-service:8080/metrics
- compliance-service:8080/metrics
- identity-service:8080/metrics
- bank-nominal:8080/metrics

## 🔒 Security Features

### Rate Limiting
- **Token Bucket:** 100 tokens, replenish 10/sec
- **Response:** 429 Too Many Requests
- **Logging:** Rejected requests logged with IP and reason

### Request Size Limits
- **Max size:** 10 MB
- **Rejection:** 413 Payload Too Large

### Security Headers
Все ответы от API Gateway включают:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Content-Security-Policy: default-src 'self'`

### CORS
**Allowed Origins:**
- http://localhost:3001 (portal-issuer)
- http://localhost:3002 (portal-investor)
- http://localhost:3003 (backoffice)

## 🔍 Monitoring Queries

### Request Rate
```promql
rate(http_server_request_duration_seconds_count[5m])
```

### P95 Latency
```promql
histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))
```

### Error Rate
```promql
rate(http_server_request_duration_seconds_count{status_code=~"5.."}[5m])
```

## 💾 Backups

**Location:** `./backups/ois_backup_YYYYMMDD_HHMMSS.sql.gz`

**Scheduled:** Ежедневно (через docker-compose postgres-backup service)

**Retention:** 7 дней

**Manual backup:**
```bash
./ops/scripts/backup.sh
```

**Restore:**
```bash
# See ops/scripts/restore.md for details
gunzip < backups/ois_backup_*.sql.gz | \
  PGPASSWORD=ois_dev_password psql -h localhost -U ois -d ois
```

