#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj
#:property AllowUnsafeBlocks=true
#:property InvariantGlobalization=true
#:property UseKhaosNativeSearchPaths=true

// Loads the SDL libraries of KappaDuck.Khaos.Native and checks every format is compiled in.
//
// Run by native.yml on Windows, and published with NativeAOT for the containers of several Linux distributions.
// It fails when:
// - a library does not load: a glibc that is too old, a DLL or .so the system does not have, or a satellite that
//   cannot find SDL3 next to it (the satellites are loaded first on purpose);
// - an image format does not round-trip, or a loader without a saver is missing;
// - SDL_ttf was built without HarfBuzz;
// - an SDL_mixer decoder is missing.

using System.Runtime.InteropServices;
using KappaDuck.Khaos.Tools.Actions;

if (args is not [string folder])
    return Script.Usage("dotnet tools/native/smoke-test.cs <folder with the libraries>");

if (!Libraries.TryLoad(Path.GetFullPath(folder)))
    return Script.Failed;

try
{
    return SmokeTest.Run();
}
catch (EntryPointNotFoundException ex)
{
    GitHubActions.Error(ex.Message);
    return Script.Failed;
}

internal static class SmokeTest
{
    private const uint InitAudio = 0x10;
    private const uint InitVideo = 0x20;
    private const uint PixelFormatRgba32 = 0x16762004; // ABGR8888 on little-endian

    private static readonly string[] SavedImages = ["png", "jpg", "webp", "gif", "tga", "bmp", "ico", "cur"];

    private static readonly string[] Decoders = ["AIFF", "DRFLAC", "DRMP3", "OPUS", "RAW", "SINEWAVE", "VORBIS", "WAV", "XMP"];

    internal static int Run()
    {
        List<string> errors = [];

        SDL.SetHint("SDL_VIDEO_DRIVER", "dummy");
        SDL.SetHint("SDL_AUDIO_DRIVER", "dummy");

        if (!SDL.Init(InitVideo | InitAudio))
        {
            GitHubActions.Error($"SDL_Init failed: {SDL.Error}");
            return Script.Failed;
        }

        Console.WriteLine($"SDL {Version(SDL.GetVersion())}, SDL_image {Version(Image.Version())}, SDL_ttf {Version(Ttf.Version())}, SDL_mixer {Version(Mixer.Version())}");

        CheckImages(errors);
        CheckTtf(errors);
        CheckMixer(errors);

        Mixer.Quit();
        Ttf.Quit();
        SDL.Quit();

        foreach (string message in errors)
            GitHubActions.Error(message);

        return errors.Count == 0 ? 0 : Script.Failed;
    }

    private static void CheckImages(List<string> errors)
    {
        nint surface = SDL.CreateSurface(16, 16, PixelFormatRgba32);

        if (surface == 0)
        {
            errors.Add($"SDL_CreateSurface failed: {SDL.Error}");
            return;
        }

        SDL.FillSurfaceRect(surface, 0, 0xFF3366CC);
        DirectoryInfo temp = Directory.CreateTempSubdirectory("khaos-smoke-test-");

        try
        {
            foreach (string extension in SavedImages)
            {
                string path = Path.Combine(temp.FullName, $"probe.{extension}");

                if (!Image.Save(surface, path))
                {
                    errors.Add($"SDL_image cannot save .{extension}: {SDL.Error}");
                    continue;
                }

                nint loaded = Image.Load(path);

                if (loaded == 0)
                {
                    errors.Add($"SDL_image cannot load .{extension}: {SDL.Error}");
                    continue;
                }

                SDL.DestroySurface(loaded);
            }
        }
        finally
        {
            SDL.DestroySurface(surface);
            temp.Delete(recursive: true);
        }

        if (!Detects(Image.IsQoi, [.. "qoif"u8, .. new byte[12]]))
            errors.Add("SDL_image was built without QOI");

        if (!Detects(Image.IsSvg, """<svg xmlns="http://www.w3.org/2000/svg"></svg>"""u8))
            errors.Add("SDL_image was built without SVG");
    }

    private static unsafe bool Detects(Func<nint, bool> detect, ReadOnlySpan<byte> header)
    {
        fixed (byte* data = header)
        {
            nint stream = SDL.IOFromConstMem(data, (nuint)header.Length);
            bool detected = stream != 0 && detect(stream);

            SDL.CloseIO(stream);
            return detected;
        }
    }

    private static void CheckTtf(List<string> errors)
    {
        if (!Ttf.Init())
            errors.Add($"TTF_Init failed: {SDL.Error}");

        Ttf.GetHarfBuzzVersion(out int major, out int minor, out int patch);

        if (major == 0)
            errors.Add("SDL_ttf was built without HarfBuzz");
        else
            Console.WriteLine($"HarfBuzz {major}.{minor}.{patch}");
    }

    private static void CheckMixer(List<string> errors)
    {
        if (!Mixer.Init())
            errors.Add($"MIX_Init failed: {SDL.Error}");

        SortedSet<string> decoders = [with(StringComparer.Ordinal)];

        for (int i = 0; i < Mixer.GetNumAudioDecoders(); i++)
        {
            string? decoder = Marshal.PtrToStringUTF8(Mixer.GetAudioDecoder(i));

            if (decoder is not null)
                decoders.Add(decoder);
        }

        Console.WriteLine($"SDL_mixer decoders: {string.Join(", ", decoders)}");

        foreach (string missing in Decoders)
        {
            if (!decoders.Contains(missing))
                errors.Add($"SDL_mixer was built without the {missing} decoder");
        }
    }

    private static string Version(int number) => $"{number / 1000000}.{number / 1000 % 1000}.{number % 1000}";
}

internal static class Libraries
{
    public const string Sdl = "SDL3";
    public const string Image = "SDL3_image";
    public const string Ttf = "SDL3_ttf";
    public const string Mixer = "SDL3_mixer";

    private static readonly Dictionary<string, nint> _handles = [];

    public static bool TryLoad(string folder)
    {
        ReadOnlySpan<string> libraries = [Image, Ttf, Mixer, Sdl];
        foreach (string name in libraries)
        {
            string file = Path.Combine(folder, OperatingSystem.IsWindows() ? $"{name}.dll" : $"lib{name}.so");

            try
            {
                _handles[name] = NativeLibrary.Load(file);
            }
            catch (DllNotFoundException ex)
            {
                GitHubActions.Error(ex.Message);
                return false;
            }
        }

        NativeLibrary.SetDllImportResolver(typeof(Libraries).Assembly, static (name, _, _) => _handles.GetValueOrDefault(name));
        return true;
    }
}

internal static partial class SDL
{
    public static string Error => Marshal.PtrToStringUTF8(GetError()) ?? "";

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_GetError")]
    private static partial nint GetError();

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_SetHint", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool SetHint(string name, string value);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Init(uint flags);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_GetVersion")]
    public static partial int GetVersion();

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_CreateSurface")]
    public static partial nint CreateSurface(int width, int height, uint format);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_FillSurfaceRect")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool FillSurfaceRect(nint surface, nint rect, uint color);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_DestroySurface")]
    public static partial void DestroySurface(nint surface);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_IOFromConstMem")]
    public static unsafe partial nint IOFromConstMem(void* memory, nuint size);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_CloseIO")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool CloseIO(nint stream);

    [LibraryImport(Libraries.Sdl, EntryPoint = "SDL_Quit")]
    public static partial void Quit();
}

internal static partial class Image
{
    [LibraryImport(Libraries.Image, EntryPoint = "IMG_Version")]
    public static partial int Version();

    [LibraryImport(Libraries.Image, EntryPoint = "IMG_Save", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Save(nint surface, string file);

    [LibraryImport(Libraries.Image, EntryPoint = "IMG_Load", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint Load(string file);

    [LibraryImport(Libraries.Image, EntryPoint = "IMG_isQOI")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool IsQoi(nint stream);

    [LibraryImport(Libraries.Image, EntryPoint = "IMG_isSVG")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool IsSvg(nint stream);
}

internal static partial class Ttf
{
    [LibraryImport(Libraries.Ttf, EntryPoint = "TTF_Version")]
    public static partial int Version();

    [LibraryImport(Libraries.Ttf, EntryPoint = "TTF_Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Init();

    [LibraryImport(Libraries.Ttf, EntryPoint = "TTF_GetHarfBuzzVersion")]
    public static partial void GetHarfBuzzVersion(out int major, out int minor, out int patch);

    [LibraryImport(Libraries.Ttf, EntryPoint = "TTF_Quit")]
    public static partial void Quit();
}

internal static partial class Mixer
{
    [LibraryImport(Libraries.Mixer, EntryPoint = "MIX_Version")]
    public static partial int Version();

    [LibraryImport(Libraries.Mixer, EntryPoint = "MIX_Init")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Init();

    [LibraryImport(Libraries.Mixer, EntryPoint = "MIX_GetNumAudioDecoders")]
    public static partial int GetNumAudioDecoders();

    [LibraryImport(Libraries.Mixer, EntryPoint = "MIX_GetAudioDecoder")]
    public static partial nint GetAudioDecoder(int index);

    [LibraryImport(Libraries.Mixer, EntryPoint = "MIX_Quit")]
    public static partial void Quit();
}
