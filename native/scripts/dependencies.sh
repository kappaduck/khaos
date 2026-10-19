#!/usr/bin/env bash
# Installs what build-linux.sh or build-windows.sh needs:
#
#     scripts/dependencies.sh linux     # in the Steam Runtime SDK container (native.yml)
#     scripts/dependencies.sh windows   # on ubuntu-latest, MinGW cross-compilation
set -euo pipefail

target=${1:?usage: dependencies.sh linux|windows}

# The Steam Runtime container runs as root and may not have sudo; the GitHub runner is not root.
sudo=""
if [ "$(id -u)" -ne 0 ]; then
    sudo="sudo"
fi

common=(build-essential cmake git ninja-build nasm perl pkg-config)

linux=(
    patchelf
    libasound2-dev libpulse-dev libaudio-dev libjack-dev libsndio-dev libpipewire-0.3-dev
    libx11-dev libxext-dev libxrandr-dev libxcursor-dev libxfixes-dev libxi-dev libxss-dev libxtst-dev
    libxkbcommon-dev libwayland-dev libdecor-0-dev
    libdrm-dev libgbm-dev libgl1-mesa-dev libgles2-mesa-dev libegl1-mesa-dev
    libdbus-1-dev libibus-1.0-dev libudev-dev liburing-dev libusb-1.0-0-dev
    libfribidi-dev libthai-dev
)

windows=(mingw-w64)

case "$target" in
    linux) packages=("${common[@]}" "${linux[@]}") ;;
    windows) packages=("${common[@]}" "${windows[@]}") ;;
    *) echo "Unknown target: $target (linux or windows)" >&2; exit 1 ;;
esac

echo "Installing dependencies for $target..."

export DEBIAN_FRONTEND=noninteractive
$sudo apt-get update
$sudo apt-get install -y --no-install-recommends "${packages[@]}"
$sudo rm -rf /var/lib/apt/lists/*
