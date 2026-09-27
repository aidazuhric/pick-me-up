#!/usr/bin/env bash
set -euo pipefail

PORT="${EMULATOR_PORT:-5554}"
TOKEN_FILE="$HOME/.emulator_console_auth_token"

[ -f "$TOKEN_FILE" ] || { echo "No console auth token at $TOKEN_FILE." >&2; exit 1; }

nc -z localhost "$PORT" 2>/dev/null \
  || { echo "No emulator console on port $PORT. Is the emulator running?" >&2; exit 1; }

printf 'auth %s\nrotate\nrotate\nrotate\nrotate\n' "$(cat "$TOKEN_FILE")" \
  | nc -w 5 localhost "$PORT" >/dev/null

echo "Rotated a full circle. The window should have repainted, still in portrait."
