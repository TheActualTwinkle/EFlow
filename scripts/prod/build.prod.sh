#!/bin/bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker/docker-compose.prod.yml"
ENV_FILE="${ENV_FILE:-$ROOT_DIR/docker/prod.env}"
TEMPLATE_FILE="$ROOT_DIR/docker/template.prod.env"
IMAGE_TAG="${1:-}"

cd "$ROOT_DIR"

if [ -z "$IMAGE_TAG" ]; then
  echo "Usage: $0 <image-tag>" >&2
  echo "Example: $0 1.0.0" >&2
  exit 1
fi

case "$IMAGE_TAG" in
  *[!A-Za-z0-9_.-]*)
    echo "Invalid image tag '$IMAGE_TAG'. Use only letters, digits, underscore, dot, and dash." >&2
    exit 1
    ;;
esac

OUT_DIR="$ROOT_DIR/out/eflow-$IMAGE_TAG"
IMAGES_ARCHIVE="$OUT_DIR/images.tar"

if [ ! -f "$ENV_FILE" ]; then
  cp "$TEMPLATE_FILE" "$ENV_FILE"
  echo "Env file not found. Created $ENV_FILE from template $TEMPLATE_FILE" >&2
  echo "Please fill required values and re-run this script." >&2
  exit 1
fi

set -a
source "$ENV_FILE"
set +a

APP_DOMAIN_NAME="${APP_DOMAIN_NAME:?APP_DOMAIN_NAME is required}"
export IMAGE_TAG

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull --ignore-buildable
docker compose --progress=plain --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build

"$ROOT_DIR/scripts/prod/compose-artifacts.sh" "$IMAGE_TAG" "$ENV_FILE" "$OUT_DIR"

echo "Writing artifacts to $OUT_DIR"

mapfile -t IMAGES < <(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" config --images)
docker save "${IMAGES[@]}" -o "$IMAGES_ARCHIVE"

echo "Done!"
