#!/bin/bash

set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
ENV_FILE="$ROOT_DIR/scripts/debug/docker/debug.env"
COMPOSE_FILE="$ROOT_DIR/scripts/debug/docker/docker-compose.debug.yml"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --build
