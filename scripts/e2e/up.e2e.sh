#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.e2e.yml"
ENV_FILE="$ROOT_DIR/docker/e2e.env"

cd "$ROOT_DIR"

if [ -f "$ENV_FILE" ]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

docker compose -f "$COMPOSE_FILE" up -d --build
