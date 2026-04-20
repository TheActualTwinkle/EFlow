#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
TESTS_COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.api-tests.yml"
BASE_COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.prod.yml"
ENV_FILE="$ROOT_DIR/docker/api-tests.env"

cd "$ROOT_DIR"

if [ -f "$ENV_FILE" ]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

docker compose --env-file "$ENV_FILE" -f "$BASE_COMPOSE_FILE" -f "$TESTS_COMPOSE_FILE" up -d --build
