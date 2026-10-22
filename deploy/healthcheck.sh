#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
for attempt in $(seq 1 30); do
    if docker compose --env-file .env exec -T api curl --fail --silent http://localhost:8080/health >/dev/null \
        && docker compose --env-file .env exec -T webapp curl --fail --silent http://localhost:8080/health >/dev/null; then
        echo "API and WebApp health checks passed."
        exit 0
    fi
    sleep 2
done

echo "API and WebApp health checks failed after retries." >&2
exit 1
