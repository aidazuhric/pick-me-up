#!/usr/bin/env bash
set -euo pipefail

AVD="${AVD:-flutter_emulator}"
DNS_SERVERS="${DNS_SERVERS:-1.1.1.1,8.8.8.8}"
EMBED_FLAGS="${EMBED_FLAGS:--qt-hide-window -grpc-use-token}"
PACKAGE="ba.fit.pickmeup_mobile"
ACTIVITY="$PACKAGE/.MainActivity"
SDK="${ANDROID_HOME:-$HOME/Library/Android/sdk}"
EMULATOR="$SDK/emulator/emulator"
ADB="$SDK/platform-tools/adb"
APP_DIR="$(cd "$(dirname "$0")/../UI/pickmeup_mobile" && pwd)"
APK="$APP_DIR/build/app/outputs/flutter-apk/app-debug.apk"
BOOT_TIMEOUT="${BOOT_TIMEOUT:-1500}"

say() { printf '\n==> %s\n' "$1"; }

[ -x "$EMULATOR" ] || { echo "No emulator at $EMULATOR. Set ANDROID_HOME." >&2; exit 1; }

if ! "$EMULATOR" -list-avds | grep -qx "$AVD"; then
  echo "AVD '$AVD' not found. Available:" >&2
  "$EMULATOR" -list-avds >&2
  exit 1
fi

curl -sf -o /dev/null --max-time 5 http://localhost:5121/swagger/index.html \
  || say "WARNING: API not answering on 5121. Run 'docker compose up -d' in PickMeUp/."

boot_emulator() {
  local extra="${1:-}"
  pkill -f qemu-system >/dev/null 2>&1 || true
  "$ADB" kill-server >/dev/null 2>&1 || true
  sleep 5
  # shellcheck disable=SC2086
  nohup "$EMULATOR" -avd "$AVD" -no-snapshot-load -memory 4096 -port 5554 \
    -dns-server "$DNS_SERVERS" $EMBED_FLAGS $extra \
    >/tmp/pickmeup-emulator.log 2>&1 &

  local waited=0
  until [ "$("$ADB" -s emulator-5554 shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" = "1" ]; do
    sleep 10
    waited=$((waited + 10))
    [ $((waited % 60)) -eq 0 ] && echo "    ${waited}s..."
    [ "$waited" -ge "$BOOT_TIMEOUT" ] && return 1
  done
  echo "    booted after ${waited}s"
  return 0
}

prepare_device() {
  "$ADB" -s emulator-5554 shell input keyevent KEYCODE_WAKEUP >/dev/null 2>&1 || true
  "$ADB" -s emulator-5554 shell input keyevent 82 >/dev/null 2>&1 || true
  "$ADB" -s emulator-5554 shell svc power stayon true >/dev/null 2>&1 || true
  "$ADB" -s emulator-5554 shell settings put system screen_off_timeout 1800000 >/dev/null 2>&1 || true
  "$ADB" -s emulator-5554 shell settings put secure stylus_handwriting_enabled 0 >/dev/null 2>&1 || true
  for s in window_animation_scale transition_animation_scale animator_duration_scale; do
    "$ADB" -s emulator-5554 shell settings put global "$s" 0 >/dev/null 2>&1 || true
  done
}

if [ "$("$ADB" -s emulator-5554 shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" = "1" ]; then
  say "Reusing the running emulator"
else
  say "Cold-booting $AVD (this takes several minutes)"
  if ! boot_emulator; then
    say "Boot stalled. Retrying once with fresh userdata (-wipe-data)"
    boot_emulator "-wipe-data" || {
      say "Still no boot. Retrying once more on the plain profile"
      boot_emulator || {
        echo "Emulator would not boot. See /tmp/pickmeup-emulator.log" >&2
        exit 1
      }
    }
  fi
fi

prepare_device

if ! "$ADB" -s emulator-5554 shell pm list packages 2>/dev/null | grep -q "$PACKAGE"; then
  if [ -f "$APK" ]; then
    say "Installing the existing APK (no Gradle rebuild)"
    "$ADB" -s emulator-5554 install -r "$APK" >/dev/null
  else
    say "No APK yet; building with flutter run (slow, then hot reload is attached)"
    cd "$APP_DIR" && exec flutter run -d emulator-5554
  fi
fi

say "Launching PickMeUp"
"$ADB" -s emulator-5554 shell am start -n "$ACTIVITY" >/dev/null
prepare_device
"$(dirname "$0")/unstick-emulator.sh" || true

cat <<INFO

PickMeUp is starting. Sign in with passenger1 or driver1, password Test123.

If the window is black but a screenshot shows the app, the host window has
stopped repainting. Force it to redraw:
    ./scripts/unstick-emulator.sh

If the window is black and a screenshot is black too, the display fell asleep:
    adb shell input keyevent KEYCODE_WAKEUP

To edit Dart code with hot reload instead:
    cd $APP_DIR && flutter run
INFO
