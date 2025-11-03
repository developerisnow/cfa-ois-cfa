#!/bin/bash
set -euo pipefail

# Test restore into fresh DB container
# Usage: ./test-restore.sh <backup_file>

BACKUP_FILE="${1:-}"
if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: $0 <backup_file>"
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "Testing restore of $BACKUP_FILE"

# Export connection params
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_DB=ois_test
export POSTGRES_USER=ois
export POSTGRES_PASSWORD=ois_dev_password

export PGPASSWORD="$POSTGRES_PASSWORD"

# Create test database (if using existing container)
psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres \
    -c "DROP DATABASE IF EXISTS $POSTGRES_DB;" || true

psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres \
    -c "CREATE DATABASE $POSTGRES_DB;"

# Restore
echo "Restoring backup..."
gunzip < "$BACKUP_FILE" | \
    psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$POSTGRES_DB"

# Verify
echo "Verifying restore..."
TABLE_COUNT=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
    -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")

if [ "$TABLE_COUNT" -gt 0 ]; then
    echo "✅ Restore successful! Found $TABLE_COUNT tables."
    exit 0
else
    echo "❌ Restore failed: No tables found"
    exit 1
fi

