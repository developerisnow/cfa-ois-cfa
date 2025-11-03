#!/bin/bash
set -euo pipefail

# PostgreSQL Backup Script for OIS
# Usage: ./backup.sh [output_dir]

BACKUP_DIR="${1:-./backups}"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/ois_backup_${TIMESTAMP}.sql.gz"
RETENTION_DAYS=7

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Database connection
DB_HOST="${POSTGRES_HOST:-postgres}"
DB_PORT="${POSTGRES_PORT:-5432}"
DB_NAME="${POSTGRES_DB:-ois}"
DB_USER="${POSTGRES_USER:-ois}"
DB_PASSWORD="${POSTGRES_PASSWORD:-ois_dev_password}"

export PGPASSWORD="$DB_PASSWORD"

echo "Starting backup at $(date)"
echo "Database: $DB_NAME@$DB_HOST:$DB_PORT"

# Perform backup
pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
    --no-owner --no-acl \
    | gzip > "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    echo "✅ Backup completed: $BACKUP_FILE ($BACKUP_SIZE)"
    echo "$BACKUP_FILE" > "${BACKUP_DIR}/latest.txt"
else
    echo "❌ Backup failed!"
    exit 1
fi

# Cleanup old backups
find "$BACKUP_DIR" -name "ois_backup_*.sql.gz" -type f -mtime +$RETENTION_DAYS -delete
echo "Cleaned up backups older than $RETENTION_DAYS days"

unset PGPASSWORD
echo "Backup finished at $(date)"

