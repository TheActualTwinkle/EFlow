#!/bin/bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
OUT_DIR="$ROOT_DIR/out"
IMAGE_TAG="${1:-}"
ENV_FILE="${2:-}"

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

rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR/docker"

awk '
  /^    build:$/ { skip = 1; next }
  skip && /^    [A-Za-z0-9_-]+:/ { skip = 0 }
  !skip { print }
' "$ROOT_DIR/docker/docker-compose.prod.yml" \
  | sed \
      -e "s/\${IMAGE_TAG:?IMAGE_TAG is required}/$IMAGE_TAG/g" \
      -e 's/^name: eflow-prod$/name: eflow/' \
  > "$OUT_DIR/docker/compose.yml"
cp "$ROOT_DIR/docker/Caddyfile" "$OUT_DIR/docker/Caddyfile"
cp "$ROOT_DIR/docker/template.prod.env" "$OUT_DIR/docker/template.env"
if [ -n "$ENV_FILE" ] && [ -f "$ENV_FILE" ]; then
  cp "$ENV_FILE" "$OUT_DIR/docker/.env"
fi
sed \
  -e 's/docker-compose\.prod\.yml/compose.yml/g' \
  -e 's/template\.prod\.env/template.env/g' \
  -e 's/prod\.env/.env/g' \
  "$ROOT_DIR/scripts/prod/up.prod.sh" > "$OUT_DIR/up.sh"

sed \
  -e 's/docker-compose\.prod\.yml/compose.yml/g' \
  -e 's/prod\.env/.env/g' \
  "$ROOT_DIR/scripts/prod/down.prod.sh" > "$OUT_DIR/down.sh"

chmod +x "$OUT_DIR/"*.sh
