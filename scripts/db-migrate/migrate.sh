#!/bin/bash

set -e

# Проверяем, что аргумент передан
if [ $# -lt 1 ]; then
    echo "Error: Migration name is required" >&2
    echo "Usage: $0 <migration-name> [-y]" >&2
    echo "Example: $0 Init" >&2
    exit 1
fi

MIGRATION_NAME="$1"
AUTO_YES=false

# Проверяем флаг -y
if [ "$2" = "-y" ]; then
    AUTO_YES=true
fi

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT_DIR"

# Формируем команду
MIGRATION_COMMAND="dotnet ef migrations add $MIGRATION_NAME \
  --project EFlow.Booking/Infrastructure/EFlow.Booking.Persistence/EFlow.Booking.Persistence.csproj \
  --startup-project EFlow.Booking/Presentation/EFlow.Booking.WebApi/EFlow.Booking.WebApi.csproj"

# Показываем превью
echo "=========================================="
echo "Migration Preview:"
echo "=========================================="
echo "Migration name: $MIGRATION_NAME"
echo "Working directory: $ROOT_DIR"
echo ""
echo "Command to execute:"
echo "$MIGRATION_COMMAND"
echo "=========================================="

# Запрашиваем подтверждение
if [ "$AUTO_YES" = false ]; then
    echo ""
    read -p "Execute this migration? [y/N]: " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Migration cancelled."
        exit 0
    fi
else
    echo "Auto-confirmed with -y flag"
fi

# Выполняем миграцию
echo ""
echo "Executing migration..."
eval $MIGRATION_COMMAND

echo ""
echo "Migration completed successfully!"