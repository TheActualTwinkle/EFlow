#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.prod.yml"
ENV_FILE="$ROOT_DIR/docker/prod.env"

cd "$ROOT_DIR"

if [ -f "$ENV_FILE" ]; then
  set -a
  # shellcheck disable=SC1090
  source "$ENV_FILE"
  set +a
fi

if [ -z "${JWT_KEY:-}" ]; then
  echo "JWT_KEY is required. Export it or create $ENV_FILE" >&2
  exit 1
fi

docker compose -f "$COMPOSE_FILE" up -d --build
