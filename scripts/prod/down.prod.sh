#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.prod.yml"
ENV_FILE="${ENV_FILE:-$ROOT_DIR/docker/prod.env}"

cd "$ROOT_DIR"

if [ -f "$ENV_FILE" ]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

JWT_KEY="${JWT_KEY:-down-script-placeholder-key}"
docker compose -f "$COMPOSE_FILE" down --remove-orphans
