#!/bin/bash

set -e

MIGRATION_NAME=""
ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"

cd "$ROOT_DIR"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --name)
      MIGRATION_NAME="$2"
      shift 2
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

dotnet ef migrations add $MIGRATION_NAME \
  --project EFlow.Booking/Infrastructure/EFlow.Booking.Persistence/EFlow.Booking.Persistence.csproj \
  --startup-project EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj
