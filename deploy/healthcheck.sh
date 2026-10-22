#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
docker compose --env-file .env exec -T api curl --fail --silent http://localhost:8080/health >/dev/null
docker compose --env-file .env exec -T webapp curl --fail --silent http://localhost:8080/health >/dev/null
echo "API and WebApp health checks passed."
