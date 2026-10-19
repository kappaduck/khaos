#!/usr/bin/env bash
# Cross-compiles SDL3, SDL_image, SDL_ttf and SDL_mixer for win-x64 with MinGW into
# artifacts/runtimes/win-x64/native.
set -euo pipefail

: "${SDL3:?SDL3 version not set}"
: "${SDL3_IMAGE:?SDL3_image version not set}"
: "${SDL3_TTF:?SDL3_ttf version not set}"
: "${SDL3_MIXER:?SDL3_mixer version not set}"

runtime=artifacts/runtimes/win-x64/native
licenses=artifacts/licenses
prefix="$PWD/build-windows/prefix"
toolchain="$PWD/scripts/mingw.cmake"
mkdir -p "$runtime" "$licenses"

fetch() {
    local repo=$1 version=$2
    shift 2
    git clone --branch "release-$version" --depth 1 "https://github.com/libsdl-org/$repo.git" "build-windows/$repo"
    for module in "$@"; do
        git -C "build-windows/$repo" submodule update --init --depth 1 "external/$module"
    done
}

compile() {
    local repo=$1
    shift
    cmake -S "build-windows/$repo" -B "build-windows/$repo/out" -G Ninja \
          -DCMAKE_BUILD_TYPE=Release \
          -DCMAKE_TOOLCHAIN_FILE="$toolchain" \
          -DCMAKE_PREFIX_PATH="$prefix" \
          "$@"
    cmake --build "build-windows/$repo/out"
    cmake --install "build-windows/$repo/out" --prefix "$prefix"
}

fetch SDL "$SDL3"
compile SDL -DSDL_TESTS=OFF -DSDL_TEST_LIBRARY=OFF -DSDL_EXAMPLES=OFF

fetch SDL_image "$SDL3_IMAGE" zlib libpng libwebp
compile SDL_image \
        -DSDLIMAGE_VENDORED=ON -DSDLIMAGE_DEPS_SHARED=OFF -DSDLIMAGE_STRICT=ON \
        -DSDLIMAGE_AVIF=OFF -DSDLIMAGE_JXL=OFF -DSDLIMAGE_TIF=OFF \
        -DSDLIMAGE_LBM=OFF -DSDLIMAGE_PCX=OFF -DSDLIMAGE_PNM=OFF -DSDLIMAGE_XCF=OFF -DSDLIMAGE_XPM=OFF -DSDLIMAGE_XV=OFF \
        -DSDLIMAGE_SAMPLES=OFF

fetch SDL_ttf "$SDL3_TTF" freetype harfbuzz plutosvg plutovg
compile SDL_ttf \
        -DSDLTTF_VENDORED=ON -DSDLTTF_STRICT=ON \
        -DSDLTTF_SAMPLES=OFF

fetch SDL_mixer "$SDL3_MIXER" ogg vorbis opus opusfile libxmp
compile SDL_mixer \
        -DSDLMIXER_VENDORED=ON -DSDLMIXER_DEPS_SHARED=OFF -DSDLMIXER_STRICT=ON \
        -DSDLMIXER_FLAC_LIBFLAC=OFF -DSDLMIXER_VORBIS_STB=OFF -DSDLMIXER_MP3_MPG123=OFF \
        -DSDLMIXER_GME=OFF -DSDLMIXER_WAVPACK=OFF -DSDLMIXER_MIDI=OFF -DSDLMIXER_VOC=OFF -DSDLMIXER_AU=OFF \
        -DSDLMIXER_TESTS=OFF -DSDLMIXER_EXAMPLES=OFF

for name in SDL3 SDL3_image SDL3_ttf SDL3_mixer; do
    cp "$prefix/bin/$name.dll" "$runtime/$name.dll"
    x86_64-w64-mingw32-strip --strip-unneeded "$runtime/$name.dll"
done

cp -r "$prefix/share/licenses/." "$licenses/"
mkdir -p "$licenses/SDL3_ttf/optional"
cp "build-windows/SDL_ttf/external/freetype/docs/FTL.TXT" "$licenses/SDL3_ttf/optional/LICENSE.freetype.txt"
cp "build-windows/SDL_ttf/external/harfbuzz/COPYING" "$licenses/SDL3_ttf/optional/LICENSE.harfbuzz.txt"
cp "build-windows/SDL_ttf/external/plutosvg/LICENSE" "$licenses/SDL3_ttf/optional/LICENSE.plutosvg.txt"
cp "build-windows/SDL_ttf/external/plutovg/LICENSE" "$licenses/SDL3_ttf/optional/LICENSE.plutovg.txt"

rm -rf build-windows

echo "SDL $SDL3 for win-x64: done."
