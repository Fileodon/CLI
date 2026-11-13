#!/bin/bash
set -e

apt-get update && apt install unzip && apt install -y libicu-dev

exec /entrypoint "$@"