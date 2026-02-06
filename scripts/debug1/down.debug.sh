#!/bin/bash

set -e

cd "$(dirname "$0")/docker" || exit

docker compose --env-file debug.env -f docker-compose.debug.yml down