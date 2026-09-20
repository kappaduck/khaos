// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// The versions the application is running on.
/// </summary>
/// <remarks>
/// Worth putting in a crash report or an "About" screen: which build of the framework is running, and
/// which native libraries it actually ended up linked against, which is not always the ones it shipped
/// with.
/// </remarks>
public static class KhaosVersion
{
    private const int MajorFactor = 1_000_000;

    private const int MinorFactor = 1_000;

    private static readonly Lazy<Version> _sdl = new(static () => Parse(SDL3.Version()));
    private static readonly Lazy<Version> _image = new(static () => Parse(SDL3_image.Version()));
    private static readonly Lazy<Version> _ttf = new(static () => Parse(SDL3_ttf.Version()));
    private static readonly Lazy<Version> _mixer = new(static () => Parse(SDL3_mixer.Version()));

    /// <summary>
    /// Gets the version of the framework.
    /// </summary>
    public static Version Framework { get; } = ResolveFramework();

    /// <summary>
    /// Gets the version of SDL image linked against.
    /// </summary>
    public static Version Image => _image.Value;

    /// <summary>
    /// Gets the version of SDL mixer linked against.
    /// </summary>
    public static Version Mixer => _mixer.Value;

    /// <summary>
    /// Gets the version of SDL linked against.
    /// </summary>
    public static Version SDL => _sdl.Value;

    /// <summary>
    /// Gets the version of SDL TTF linked against.
    /// </summary>
    public static Version Ttf => _ttf.Value;

    private static Version Parse(int version)
    {
        int major = version / MajorFactor;
        int minor = version / MinorFactor % MinorFactor;
        int patch = version % MinorFactor;

        return new Version(major, minor, patch);
    }

    private static Version ResolveFramework()
    {
        Version assembly = typeof(KhaosVersion).Assembly.GetName().Version!;
        return new Version(assembly.Major, assembly.Minor, assembly.Build);
    }
}
