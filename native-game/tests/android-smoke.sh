#!/usr/bin/env bash
set -euo pipefail
package=org.example.coastalcity
apk=native-game/build/CoastalCity-debug.apk
adb install -r "$apk"
adb logcat -c
activity=$(adb shell cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER "$package" | tr -d '\r' | tail -n 1)
if [[ "$activity" != "$package/"* ]]; then echo "::error::Cannot resolve launcher activity: $activity"; exit 1; fi
echo "Starting $activity"
adb shell am start -W -n "$activity"
ready=0
for i in $(seq 1 40); do
  adb logcat -d -s godot:I '*:S' > /tmp/game-log.txt || true
  if grep -q 'COASTAL_CITY_READY' /tmp/game-log.txt; then ready=1; break; fi
  sleep 1
done
if [ "$ready" != 1 ]; then
  adb logcat -d > /tmp/android-log.txt
  echo "::error::Game did not reach its main scene. Relevant log: $(grep -Ei 'godot|AndroidRuntime|Fatal signal|hostfxr|mono|coastalcity' /tmp/android-log.txt | grep -Ev 'AppsFilter|Access denied|Blockstore|Wallpaper|PackageSetting|ProcessStats|BroadcastQueue|AppOps|DeviceConfig|ActivityManager:.*Force stopping|CompatChangeReporter' | head -n 70 | tr '\n' ' ' | cut -c1-3500)"
  exit 1
fi
# Check that the process remains alive beyond the splash screen, then relaunch
# once to catch first/second launch failures seen on some Android devices.
sleep 15
if ! adb shell pidof "$package" | grep -q '[0-9]'; then
  echo "::error::Game exited after reaching the main scene"
  exit 1
fi
adb shell am force-stop "$package"
adb logcat -c
adb shell am start -W -n "$activity"
ready=0
for i in $(seq 1 40); do
  adb logcat -d -s godot:I '*:S' > /tmp/game-log.txt || true
  if grep -q 'COASTAL_CITY_READY' /tmp/game-log.txt; then ready=1; break; fi
  sleep 1
done
if [ "$ready" != 1 ]; then
  adb logcat -d > /tmp/android-log.txt
  echo "::error::Second launch failed: $(grep -Ei 'godot|AndroidRuntime|Fatal signal|hostfxr|mono|coastalcity' /tmp/android-log.txt | grep -Ev 'AppsFilter|Access denied|Blockstore|Wallpaper|PackageSetting|ProcessStats|BroadcastQueue|AppOps|DeviceConfig|ActivityManager:.*Force stopping|CompatChangeReporter' | head -n 70 | tr '\n' ' ' | cut -c1-3500)"
  exit 1
fi
sleep 10
adb shell pidof "$package" | grep -q '[0-9]' || { echo '::error::Game exited on second launch'; exit 1; }
echo 'Android startup smoke test passed twice.'
