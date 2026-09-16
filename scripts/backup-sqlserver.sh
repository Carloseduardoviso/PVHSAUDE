#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_dir="$(cd "$script_dir/.." && pwd)"
cd "$project_dir"

set -a
. ./.env
set +a

: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD deve estar definido em .env}"

timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
backup_file="/var/opt/mssql/backups/PVHSAUDE-${timestamp}.bak"

docker compose exec -T -e SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" sqlserver \
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b \
  -Q "BACKUP DATABASE [PVHSAUDE] TO DISK = N'${backup_file}' WITH INIT, COMPRESSION;"
