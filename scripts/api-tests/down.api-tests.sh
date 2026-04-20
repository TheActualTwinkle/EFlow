#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.api-tests.yml"
ENV_FILE="$ROOT_DIR/docker/api-tests.env"

cd "$ROOT_DIR"

if [ -f "$ENV_FILE" ]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

docker compose -f "$COMPOSE_FILE" down -v
