#!/usr/bin/env bash
# The CMake options and the vendored libraries the three build scripts share, sourced by build-linux.sh,
# build-windows.sh and build-wasm.sh, so a format turned on or off here changes the desktop and the
# browser together. Each script adds what only its platform needs.
#
# The list of formats is also in tools/native/smoke-test.cs, check-wasm.sh and native/readme.md: change them together.

sdl_options=(-DSDL_TESTS=OFF -DSDL_TEST_LIBRARY=OFF -DSDL_EXAMPLES=OFF)

sdl_image_externals=(zlib libpng libwebp)
sdl_image_options=(
    -DSDLIMAGE_VENDORED=ON -DSDLIMAGE_DEPS_SHARED=OFF -DSDLIMAGE_STRICT=ON
    -DSDLIMAGE_AVIF=OFF -DSDLIMAGE_JXL=OFF -DSDLIMAGE_TIF=OFF
    -DSDLIMAGE_LBM=OFF -DSDLIMAGE_PCX=OFF -DSDLIMAGE_PNM=OFF -DSDLIMAGE_XCF=OFF -DSDLIMAGE_XPM=OFF -DSDLIMAGE_XV=OFF
    -DSDLIMAGE_SAMPLES=OFF
)

sdl_ttf_externals=(freetype harfbuzz plutosvg plutovg)
sdl_ttf_options=(-DSDLTTF_VENDORED=ON -DSDLTTF_STRICT=ON -DSDLTTF_SAMPLES=OFF)

sdl_mixer_externals=(ogg vorbis opus opusfile libxmp)
sdl_mixer_options=(
    -DSDLMIXER_VENDORED=ON -DSDLMIXER_DEPS_SHARED=OFF -DSDLMIXER_STRICT=ON
    -DSDLMIXER_FLAC_LIBFLAC=OFF -DSDLMIXER_VORBIS_STB=OFF -DSDLMIXER_MP3_MPG123=OFF
    -DSDLMIXER_GME=OFF -DSDLMIXER_WAVPACK=OFF -DSDLMIXER_MIDI=OFF -DSDLMIXER_VOC=OFF -DSDLMIXER_AU=OFF
    -DSDLMIXER_TESTS=OFF -DSDLMIXER_EXAMPLES=OFF
)
