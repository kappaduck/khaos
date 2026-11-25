#!/usr/bin/env bash
# Checks the WebAssembly libraries of KappaDuck.Khaos.Native before they are packed.
#
#     native/scripts/check-wasm.sh native/packages/wasm/net11.0
#
# Run by native.yml after build-wasm.sh. The desktop libraries are loaded by tools/native/smoke-test.cs;
# static WebAssembly libraries cannot be, so this reads their symbols instead. It fails when:
# - one of the four SDL libraries is missing, or deps/ is empty;
# - an SDL_mixer decoder is missing (the list of tools/native/smoke-test.cs: change both together);
# - SDL_image was built without libpng or libwebp, SDL_ttf without HarfBuzz or plutosvg,
#   SDL_mixer without opusfile, libvorbis or libxmp;
# - a library needs a symbol of SDL or of a vendored library that no .a defines: a game's link
#   would fail on it, after Native is published.
#
# The formats SDL_image compiles from its own sources (BMP, GIF, JPG, QOI, SVG, TGA...) leave no
# symbol to tell them apart; they come from the same options as the desktop (options.sh), which
# tools/native/smoke-test.cs checks.
#
# NM is the llvm-nm to use; by default the one of the emsdk build-wasm.sh installed.
set -euo pipefail

folder="${1:?usage: check-wasm.sh <folder with the .a files>}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
nm="${NM:-$root/artifacts/sdl-wasm/emsdk/upstream/bin/llvm-nm}"

decoders=(WAV AIFF DRFLAC DRMP3 VORBIS OPUS XMP RAW SINEWAVE)
vendored='^(SDL_|IMG_|TTF_|MIX_|WebP|SharpYuv|png_|inflate|deflate|crc32|adler32|hb_|FT_|plutosvg_|plutovg_|ogg_|ov_|vorbis_|opus_|op_|xmp_)'

[[ -x "$nm" ]] || command -v "$nm" >/dev/null || { echo "error: '$nm' was not found. Set NM to an llvm-nm." >&2; exit 2; }

errors=()

for lib in SDL3 SDL3_image SDL3_mixer SDL3_ttf; do
    [[ -f "$folder/$lib.a" ]] || errors+=("$lib.a is missing")
done

shopt -s nullglob
deps=("$folder"/deps/*.a)
(( ${#deps[@]} > 0 )) || errors+=("deps/ has no .a")

if (( ${#errors[@]} == 0 )); then
    symbols() { "$nm" "$1" -j "${@:2}" 2>/dev/null | grep -v -e ':$' -e '^$' | sort -u; }

    defined="$(symbols --defined-only "$folder"/*.a "${deps[@]}")"
    needs() { symbols --undefined-only "$folder/$1.a" | grep -Eq "$2"; }

    for decoder in "${decoders[@]}"; do
        grep -qx "MIX_Decoder_$decoder" <<< "$defined" || errors+=("SDL_mixer was built without the $decoder decoder")
    done

    needs SDL3_image '^png_' || errors+=("SDL_image was built without libpng")
    needs SDL3_image '^WebP' || errors+=("SDL_image was built without libwebp")
    needs SDL3_ttf '^hb_' || errors+=("SDL_ttf was built without HarfBuzz")
    needs SDL3_ttf '^plutosvg_' || errors+=("SDL_ttf was built without plutosvg")
    needs SDL3_mixer '^op_' || errors+=("SDL_mixer was built without opusfile")
    needs SDL3_mixer '^ov_' || errors+=("SDL_mixer was built without libvorbis")
    needs SDL3_mixer '^xmp_' || errors+=("SDL_mixer was built without libxmp")

    unresolved="$(comm -23 <(symbols --undefined-only "$folder"/SDL3_image.a "$folder"/SDL3_mixer.a "$folder"/SDL3_ttf.a "${deps[@]}") <(printf '%s\n' "$defined") | grep -E "$vendored" || true)"

    while IFS= read -r symbol; do
        [[ -n "$symbol" ]] && errors+=("$symbol is needed but no .a defines it")
    done <<< "$unresolved"
fi

for message in "${errors[@]}"; do
    echo "::error::$message"
done

if (( ${#errors[@]} > 0 )); then
    exit 1
fi

echo "The WebAssembly libraries of $folder have their decoders, their codecs, and every symbol they need."
