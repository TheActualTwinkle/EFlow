#!/usr/bin/env bash
set -euo pipefail

UI_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ROOT_DIR="$(cd "${UI_DIR}/.." && pwd)"
API_PROJECT="${EFLOW_API_PROJECT:-${ROOT_DIR}/EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj}"
OPENAPI_HOST="${EFLOW_OPENAPI_HOST:-127.0.0.1}"
OPENAPI_PORT="${EFLOW_OPENAPI_PORT:-5117}"
OPENAPI_URL="${EFLOW_OPENAPI_URL:-http://${OPENAPI_HOST}:${OPENAPI_PORT}/openapi/v1.json}"
OPENAPI_FILE="${UI_DIR}/src/app/api/openapi.json"
CONTRACTS_FILE="${UI_DIR}/src/app/api/contracts.ts"
LOG_FILE="${UI_DIR}/.openapi-generator.log"

mkdir -p "${UI_DIR}/src/app/api"

ASPNETCORE_ENVIRONMENT=OpenApiGenerator \
  dotnet run --no-launch-profile --project "${API_PROJECT}" --urls "http://${OPENAPI_HOST}:${OPENAPI_PORT}" \
  >"${LOG_FILE}" 2>&1 &

API_PID=$!

cleanup() {
  if kill -0 "${API_PID}" >/dev/null 2>&1; then
    kill "${API_PID}" >/dev/null 2>&1 || true
  fi
}
trap cleanup EXIT

for _ in {1..60}; do
  if curl -fsS "${OPENAPI_URL}" -o "${OPENAPI_FILE}"; then
    npx openapi-typescript "${OPENAPI_FILE}" -o "${CONTRACTS_FILE}"
    echo "Generated ${CONTRACTS_FILE}"
    exit 0
  fi

  sleep 1
done

echo "OpenAPI endpoint did not become ready: ${OPENAPI_URL}" >&2
echo "Backend log: ${LOG_FILE}" >&2
exit 1
