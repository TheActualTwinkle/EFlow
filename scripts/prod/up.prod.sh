#!/bin/bash
set -e

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.prod.yml"
ENV_FILE="$ROOT_DIR/docker/prod.env"

cd "$ROOT_DIR"

TEMPLATE_FILE="$ROOT_DIR/docker/template.prod.env"

if [ ! -f "$ENV_FILE" ]; then
  if [ -f "$TEMPLATE_FILE" ]; then
    cp "$TEMPLATE_FILE" "$ENV_FILE"
    echo "prod.env file not found. Created $ENV_FILE from template $TEMPLATE_FILE"
  else
    echo "Template $TEMPLATE_FILE not found. Please create it or provide $ENV_FILE" >&2
    exit 1
  fi

  echo "Please fill required values."

  read -r -p "Open editor to edit $ENV_FILE now? [Y/n]: " REPLY || true
  REPLY="${REPLY:-Y}"

  case "$REPLY" in
    [Yy]|[Yy][Ee][Ss])
      if [ -n "${EDITOR:-}" ]; then
        "$EDITOR" "$ENV_FILE" || true
      else
        if command -v nano >/dev/null 2>&1; then
          nano "$ENV_FILE" || true
        elif command -v vi >/dev/null 2>&1; then
          vi "$ENV_FILE" || true
        else
          echo "No nano/vi editor found. Please edit $ENV_FILE manually." >&2
        fi
      fi
      ;;
    *)
      echo "Skipping editor. Please edit $ENV_FILE manually.";
      ;;
  esac

  echo "After filling $ENV_FILE, re-run this script: $0"
  exit 1
fi

set -a
source "$ENV_FILE"
set +a

if [ -z "${JWT_KEY:-}" ]; then
  echo "JWT_KEY is required. Export it or create $ENV_FILE" >&2
  exit 1
fi

docker compose -f "$COMPOSE_FILE" up -d --build
