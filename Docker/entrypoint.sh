#!/bin/bash
set -e

apt-get update && apt install -y unzip libicu-dev

exec /entrypoint "$@"