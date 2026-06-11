#!/usr/bin/env bash
# Build PantryToPlate for Android + run xUnit tests.
# Does NOT deploy or run the emulator — build only.
set -euo pipefail

HDD="/Volumes/APPLE HDD ST2000DM001 Media"
export DOTNET_ROOT="$HDD/dotnet-sdk"
export PATH="$DOTNET_ROOT:$PATH"
export NUGET_PACKAGES="$HDD/nuget-cache"
export ANDROID_SDK_ROOT="$HDD/android-sdk"
export TMPDIR="$HDD/tmp"
mkdir -p "$TMPDIR"

REPO="$(cd "$(dirname "$0")/.." && pwd)"

echo ">>> xUnit tests (Core)"
dotnet test "$REPO/PantryToPlate.Tests/" \
    --logger "console;verbosity=normal" \
    --no-build 2>/dev/null || \
dotnet test "$REPO/PantryToPlate.Tests/" \
    --logger "console;verbosity=normal"

echo ""
echo ">>> Restore MAUI (android target first)"
dotnet restore "$REPO/PantryToPlate/" -p:TargetFramework=net10.0-android

echo ">>> Re-restore Core standalone"
dotnet restore "$REPO/PantryToPlate.Core/"

echo ">>> Build Android APK (Debug, no deploy)"
dotnet build "$REPO/PantryToPlate/" \
    -f net10.0-android \
    --no-restore \
    -c Debug \
    -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
    -p:EmbedAssembliesIntoApk=true \
    -maxCpuCount:1 \
    -p:UseSharedCompilation=false \
    /nodeReuse:false

echo ""
echo ">>> APK location:"
find "$REPO/PantryToPlate/bin/Debug/net10.0-android/" -name "*.apk" 2>/dev/null \
    || echo "  (no APK found — check build output above)"
