#!/usr/bin/env bash
# Controleert de laatste geslaagde GitHub-build, migreert eerst de databases
# en activeert daarna alleen images met exact dezelfde commit-SHA.
set -Eeuo pipefail

APP_ROOT="/opt/bordspel-app"
STATE_DIR="$APP_ROOT/.deployment"
REPOSITORY="yasirkel/Bordspel-app"
BRANCH="fix/assessment-completion"
WORKFLOW="ghcr-deploy.yml"
API_IMAGE="ghcr.io/yasirkel/bordspelapp-api"
WEBAPP_IMAGE="ghcr.io/yasirkel/bordspelapp-webapp"
MIGRATOR_IMAGE="ghcr.io/yasirkel/bordspelapp-migrator"

mkdir -p "$STATE_DIR"
chmod 700 "$STATE_DIR"
exec 9>"$STATE_DIR/deploy.lock"
flock -n 9 || exit 0

cd "$APP_ROOT"

workflow_json=$(curl --fail --silent --show-error --retry 3 \
  -H 'Accept: application/vnd.github+json' \
  -H 'Cache-Control: no-cache' \
  "https://api.github.com/repos/$REPOSITORY/actions/workflows/$WORKFLOW/runs?branch=$BRANCH&status=success&per_page=20&cachebust=$(date +%s)")
commit_sha=$(jq -r --arg branch "$BRANCH" \
  '[.workflow_runs[] | select(.head_branch == $branch and .conclusion == "success")][0].head_sha // empty' \
  <<<"$workflow_json")

if [[ ! "$commit_sha" =~ ^[0-9a-f]{40}$ ]]; then
  echo "No verified successful GHCR build found for $BRANCH."
  exit 0
fi

current_sha=""
if [[ -f "$STATE_DIR/current.sha" ]]; then
  current_sha=$(tr -d '[:space:]' < "$STATE_DIR/current.sha")
fi
if [[ "$current_sha" == "$commit_sha" ]]; then
  echo "Commit $commit_sha is already deployed."
  exit 0
fi

previous_env="$STATE_DIR/env.previous"
cp --preserve=mode "$APP_ROOT/.env" "$previous_env"
chmod 600 "$previous_env"
previous_api_image=$(awk -F= '$1 == "API_IMAGE" {print substr($0, index($0, "=") + 1)}' "$APP_ROOT/.env" || true)
previous_webapp_image=$(awk -F= '$1 == "WEBAPP_IMAGE" {print substr($0, index($0, "=") + 1)}' "$APP_ROOT/.env" || true)
previous_migrator_image=$(awk -F= '$1 == "MIGRATOR_IMAGE" {print substr($0, index($0, "=") + 1)}' "$APP_ROOT/.env" || true)
activation_started=0

rollback() {
  if [[ "$activation_started" == "1" ]]; then
    echo "Deployment failed; restoring previous application images."
    cp --preserve=mode "$previous_env" "$APP_ROOT/.env"
    export API_IMAGE="$previous_api_image" WEBAPP_IMAGE="$previous_webapp_image" MIGRATOR_IMAGE="$previous_migrator_image"
    docker compose --env-file "$APP_ROOT/.env" up -d --no-deps api webapp caddy || true
    bash "$APP_ROOT/deploy/healthcheck.sh" || true
  fi
}
trap rollback ERR

export API_IMAGE="$API_IMAGE:$commit_sha"
export WEBAPP_IMAGE="$WEBAPP_IMAGE:$commit_sha"
export MIGRATOR_IMAGE="$MIGRATOR_IMAGE:$commit_sha"

docker manifest inspect "$API_IMAGE" >/dev/null
docker manifest inspect "$WEBAPP_IMAGE" >/dev/null
docker manifest inspect "$MIGRATOR_IMAGE" >/dev/null
docker compose --env-file "$APP_ROOT/.env" pull api webapp migrator

for image in "$API_IMAGE" "$WEBAPP_IMAGE" "$MIGRATOR_IMAGE"; do
  revision=$(docker image inspect "$image" --format '{{index .Config.Labels "org.opencontainers.image.revision"}}')
  [[ "$revision" == "$commit_sha" ]]
done

echo "Applying migrations for $commit_sha."
docker compose --env-file "$APP_ROOT/.env" run --rm migrator

sed -i -E "s|^API_IMAGE=.*|API_IMAGE=$API_IMAGE|; s|^WEBAPP_IMAGE=.*|WEBAPP_IMAGE=$WEBAPP_IMAGE|; s|^MIGRATOR_IMAGE=.*|MIGRATOR_IMAGE=$MIGRATOR_IMAGE|" "$APP_ROOT/.env"
activation_started=1
docker compose --env-file "$APP_ROOT/.env" up -d --no-deps api webapp caddy
bash "$APP_ROOT/deploy/healthcheck.sh"

printf '%s\n' "$commit_sha" > "$STATE_DIR/current.sha"
chmod 600 "$STATE_DIR/current.sha"
trap - ERR
echo "Deployed verified commit $commit_sha."
