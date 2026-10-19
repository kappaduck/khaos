set(CMAKE_SYSTEM_NAME Windows)
set(CMAKE_SYSTEM_PROCESSOR x86_64)

find_program(CMAKE_C_COMPILER NAMES x86_64-w64-mingw32-gcc REQUIRED)
find_program(CMAKE_CXX_COMPILER NAMES x86_64-w64-mingw32-g++ REQUIRED)
find_program(CMAKE_RC_COMPILER NAMES x86_64-w64-mingw32-windres windres REQUIRED)

# Link the MinGW runtime into each DLL. Without it, the DLLs can import libraries that only exist on
# a machine with MinGW installed: libstdc++-6 and libgcc_s_seh-1 (harfbuzz is C++), libwinpthread-1
# (threads) and libssp-0 (stack protector). Windows' own DLLs are still imported normally.
# native.yml checks the imports.
set(CMAKE_SHARED_LINKER_FLAGS_INIT "-static -static-libgcc -static-libstdc++")
