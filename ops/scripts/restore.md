# PostgreSQL Restore Guide

## Prerequisites

- PostgreSQL client tools installed (`pg_dump`, `psql`)
- Access to backup file (`.sql.gz`)
- Database credentials

## Restore Steps

### 1. Prepare Environment

```bash
export POSTGRES_HOST=postgres
export POSTGRES_PORT=5432
export POSTGRES_DB=ois
export POSTGRES_USER=ois
export POSTGRES_PASSWORD=ois_dev_password
```

### 2. Stop Application Services (Optional)

```bash
docker-compose stop api-gateway issuance-service registry-service settlement-service compliance-service
```

### 3. Restore Backup

```bash
# Restore from compressed backup
gunzip < backups/ois_backup_YYYYMMDD_HHMMSS.sql.gz | \
    PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB

# Or restore from uncompressed
PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB < backup.sql
```

### 4. Verify Restore

```bash
# Check table count
PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB \
    -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';"

# Check sample data
PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -d $POSTGRES_DB \
    -c "SELECT COUNT(*) FROM issuances;"
```

### 5. Restart Services

```bash
docker-compose start api-gateway issuance-service registry-service settlement-service compliance-service
```

## Restore to Fresh Database Container

### 1. Create Fresh Container

```bash
docker-compose stop postgres
docker volume rm capital_postgres_data  # WARNING: Deletes existing data
docker-compose up -d postgres
```

### 2. Wait for PostgreSQL to be Ready

```bash
docker-compose exec postgres pg_isready -U ois
```

### 3. Restore Backup

```bash
./ops/scripts/restore.sh backups/ois_backup_YYYYMMDD_HHMMSS.sql.gz
```

## Automated Test Restore Script

See `ops/scripts/test-restore.sh` for automated restore testing.

