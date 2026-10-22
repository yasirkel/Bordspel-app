#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
test -f .env || { echo ".env is missing" >&2; exit 1; }

public_ip="$(curl --fail --silent --show-error --ipv4 https://checkip.amazonaws.com | tr -d '[:space:]')"
[[ "$public_ip" =~ ^[0-9]+(\.[0-9]+){3}$ ]] || { echo "Could not determine a valid public IPv4 address" >&2; exit 1; }

sed -i -E "s/^WEB_HOSTNAME=.*/WEB_HOSTNAME=web.${public_ip}.sslip.io/" .env
sed -i -E "s/^API_HOSTNAME=.*/API_HOSTNAME=api.${public_ip}.sslip.io/" .env

docker compose --env-file .env up -d caddy
/bin/bash "$(dirname "$0")/healthcheck.sh"
echo "WebApp: https://web.${public_ip}.sslip.io"
echo "API:    https://api.${public_ip}.sslip.io"
