#!/usr/bin/env bash
# Builds SDL3, SDL_image, SDL_mixer and SDL_ttf for WebAssembly
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
work="$root/artifacts/sdl-wasm"
props="$root/build/emscripten.props"

step() { printf '\n==> %s\n' "$*"; }
fail() { printf 'error: %s\n' "$*" >&2; exit 1; }

for tool in git python3 cmake ninja dotnet; do
    command -v "$tool" >/dev/null || fail "'$tool' was not found on PATH."
done

# SDL3=3.4.18, SDL3_IMAGE=..., shared with the desktop runtimes so both run the same SDL.
set -a
# shellcheck source=../sdl.env
. "$root/native/sdl.env"
set +a

# <KhaosEmscripten Include="net11.0" Version="6.0.3" /> -> "net11.0 6.0.3"
rows() {
    sed -n 's/.*<KhaosEmscripten[[:space:]]\{1,\}Include="\([^"]*\)"[[:space:]]\{1,\}Version="\([^"]*\)".*/\1 \2/p' "$props"
}

pinned_version() {
    rows | awk -v tfm="$1" '$1 == tfm { print $2 }'
}

workload_version() {
    local major="$1" dotnet_root file
    local roots=("${DOTNET_ROOT:-}" "$(dirname "$(readlink -f "$(command -v dotnet)")")" "$HOME/.dotnet")

    for dotnet_root in "${roots[@]}"; do
        [[ -n "$dotnet_root" && -d "$dotnet_root/packs" ]] || continue

        file="$(ls -d "$dotnet_root"/packs/Microsoft.NET.Runtime.Emscripten.*.Sdk.*/"$major".*/tools/emscripten/emscripten-version.txt 2>/dev/null | sort -V | tail -n 1 || true)"

        if [[ -n "$file" ]]; then
            tr -d '"[:space:]' < "$file"
            return
        fi
    done
}

accepts() {
    local mode="$1"; shift
    local probe="$work/probe"
    mkdir -p "$probe"
    printf '#include <setjmp.h>\nstatic jmp_buf b;\nint main(void) { return setjmp(b); }\n' > "$probe/probe.c"

    if [[ "$mode" == link ]]; then
        emcc "$probe/probe.c" -o "$probe/probe.js" "$@" >/dev/null 2>&1
    else
        emcc -c "$probe/probe.c" -o "$probe/probe.o" "$@" >/dev/null 2>&1
    fi
}

source_of() {
    local repo="$1" version="$2" dir="$work/src/$1-$2"
    shift 2

    if [[ ! -d "$dir" ]]; then
        git clone --quiet --depth 1 --branch "release-$version" "https://github.com/libsdl-org/$repo.git" "$dir" >&2

        for module in "$@"; do
            git -C "$dir" submodule update --init --depth 1 "$module" >&2
        done
    fi

    printf '%s' "$dir"
}

build_library() {
    local name="$1" source="$2" build="$out/build/$1"
    shift 2

    [[ "${KHAOS_CLEAN:-}" == 1 ]] && rm -rf "$build"

    emcmake cmake -S "$source" -B "$build" -G Ninja \
        -DCMAKE_BUILD_TYPE=Release \
        -DCMAKE_INSTALL_PREFIX="$prefix" \
        -DSDL3_DIR="$prefix/lib/cmake/SDL3" \
        -DBUILD_SHARED_LIBS=OFF \
        "$@"

    cmake --build "$build"
    cmake --install "$build"
}

build_tfm() {
    local tfm="$1" emscripten major installed target
    emscripten="$(pinned_version "$tfm")"
    [[ -n "$emscripten" ]] || fail "$tfm has no row in build/emscripten.props."

    major="${tfm#net}"
    major="${major%%.*}"

    out="$work/$tfm"
    prefix="$out/install"
    target="$root/native/packages/wasm/$tfm"

    step "$tfm: Emscripten $emscripten"

    installed="$(workload_version "$major")"

    if [[ -z "$installed" ]]; then
        [[ "${KHAOS_SKIP_WORKLOAD_CHECK:-}" == 1 ]] || fail "No wasm-tools workload for .NET $major. Install it: dotnet workload install wasm-tools (or wasm-tools-net$major when the SDK is newer than .NET $major)."
        echo "  no workload for .NET $major, check skipped"
    elif [[ "$installed" != "$emscripten" ]]; then
        fail "The .NET $major workload links with Emscripten $installed, build/emscripten.props pins $emscripten for $tfm. Update the row (and rebuild), or update the workload."
    else
        echo "  matches the .NET $major workload"
    fi

    step "Installing Emscripten $emscripten through emsdk"

    local emsdk="$work/emsdk"

    if [[ -d "$emsdk" ]]; then
        # A version newer than the clone knows about needs a fresher tag list.
        git -C "$emsdk" pull --quiet --ff-only || true
    else
        git clone --quiet --depth 1 https://github.com/emscripten-core/emsdk.git "$emsdk"
    fi

    "$emsdk/emsdk" install "$emscripten"
    "$emsdk/emsdk" activate "$emscripten"
    EMSDK_QUIET=1 . "$emsdk/emsdk_env.sh"
    emcc --version | head -n 1

    step "Matching the flags .NET builds with"

    # .NET compiles its runtime with WebAssembly exception handling on (WasmEnableExceptionHandling,
    # true by default). Code using setjmp/longjmp (freetype does) must agree, or the final link fails.
    local compile_flags=() flag
    for flag in -fwasm-exceptions -sWASM_LEGACY_EXCEPTIONS=0 -sSUPPORT_LONGJMP=wasm; do
        if accepts compile "${compile_flags[@]}" "$flag"; then
            compile_flags+=("$flag")
        fi
    done

    # Newer Emscripten only hands out GL function pointers when asked to, and SDL's WebGL
    # renderer loads every GL function through one. A link flag: it goes to the game's link.
    local link_flags=()
    if accepts link -sGL_ENABLE_GET_PROC_ADDRESS=1; then
        link_flags+=(-sGL_ENABLE_GET_PROC_ADDRESS=1)
    fi

    echo "  compile: ${compile_flags[*]}"
    echo "  link:    ${link_flags[*]}"

    export CFLAGS="${compile_flags[*]}"
    export CXXFLAGS="${compile_flags[*]}"

    [[ "${KHAOS_CLEAN:-}" == 1 ]] && rm -rf "$prefix"

    step "SDL $SDL3"
    build_library SDL "$(source_of SDL "$SDL3")" \
        -DSDL_SHARED=OFF -DSDL_STATIC=ON -DSDL_TESTS=OFF -DSDL_EXAMPLES=OFF -DSDL_TEST_LIBRARY=OFF

    step "SDL_image $SDL3_IMAGE"
    build_library SDL_image "$(source_of SDL_image "$SDL3_IMAGE")" \
        -DSDLIMAGE_VENDORED=OFF -DSDLIMAGE_DEPS_SHARED=OFF -DSDLIMAGE_BACKEND_STB=ON \
        -DSDLIMAGE_PNG_LIBPNG=OFF -DSDLIMAGE_AVIF=OFF -DSDLIMAGE_JXL=OFF -DSDLIMAGE_TIF=OFF \
        -DSDLIMAGE_WEBP=OFF -DSDLIMAGE_LBM=OFF -DSDLIMAGE_PCX=OFF -DSDLIMAGE_PNM=OFF -DSDLIMAGE_XCF=OFF \
        -DSDLIMAGE_XPM=OFF -DSDLIMAGE_XV=OFF -DSDLIMAGE_SAMPLES=OFF -DSDLIMAGE_TESTS=OFF

    step "SDL_mixer $SDL3_MIXER"
    build_library SDL_mixer "$(source_of SDL_mixer "$SDL3_MIXER")" \
        -DSDLMIXER_VENDORED=OFF -DSDLMIXER_DEPS_SHARED=OFF -DSDLMIXER_FLAC_LIBFLAC=OFF \
        -DSDLMIXER_GME=OFF -DSDLMIXER_MOD=OFF -DSDLMIXER_MP3_MPG123=OFF -DSDLMIXER_MIDI_FLUIDSYNTH=OFF \
        -DSDLMIXER_OPUS=OFF -DSDLMIXER_VORBIS_VORBISFILE=OFF -DSDLMIXER_WAVPACK=OFF \
        -DSDLMIXER_MIDI=OFF -DSDLMIXER_VOC=OFF -DSDLMIXER_AU=OFF \
        -DSDLMIXER_TESTS=OFF -DSDLMIXER_EXAMPLES=OFF

    step "SDL_ttf $SDL3_TTF"
    build_library SDL_ttf "$(source_of SDL_ttf "$SDL3_TTF" external/freetype)" \
        -DSDLTTF_VENDORED=ON -DSDLTTF_HARFBUZZ=OFF -DSDLTTF_PLUTOSVG=OFF -DSDLTTF_SAMPLES=OFF

    step "Copying to native/packages/wasm/$tfm"

    rm -rf "$target"
    mkdir -p "$target/deps"

    local lib
    for lib in SDL3 SDL3_image SDL3_mixer SDL3_ttf; do
        [[ -f "$prefix/lib/lib$lib.a" ]] || fail "Expected $prefix/lib/lib$lib.a after the build."
        cp "$prefix/lib/lib$lib.a" "$target/$lib.a"
        echo "  $lib.a"
    done

    while IFS= read -r -d '' dep; do
        cp "$dep" "$target/deps/"
        echo "  deps/$(basename "$dep")"
    done < <(find "$out/build/SDL_ttf" -name '*.a' ! -name '*SDL3_ttf*' -print0)

    cat > "$target/emscripten.props" <<PROPS
<Project>
  <!-- Generated by native/scripts/build-wasm.sh. Do not edit: run the script again. -->
  <PropertyGroup>
    <KhaosWasmEmscriptenVersion>$emscripten</KhaosWasmEmscriptenVersion>
    <EmccExtraLDFlags>\$(EmccExtraLDFlags) ${link_flags[*]}</EmccExtraLDFlags>
  </PropertyGroup>
</Project>
PROPS

    printf '%s\n' "$emscripten" > "$target/emscripten-version.txt"

    echo "  emscripten.props, emscripten-version.txt"
}

if [[ $# -gt 0 ]]; then
    tfms=("$@")
else
    mapfile -t tfms < <(rows | awk '{ print $1 }')
fi

[[ ${#tfms[@]} -gt 0 ]] || fail "build/emscripten.props has no KhaosEmscripten row."

for tfm in "${tfms[@]}"; do
    build_tfm "$tfm"
done

step "Done"
