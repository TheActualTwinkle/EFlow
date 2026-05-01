#!/bin/bash

set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
ENV_FILE="$ROOT_DIR/docker/debug.env"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.debug.yml"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" down --remove-orphans
