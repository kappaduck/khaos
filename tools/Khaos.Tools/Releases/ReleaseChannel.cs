// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Releases;

/// <summary>
/// The channel of a release.
/// </summary>
public enum ReleaseChannel
{
    /// <summary>
    /// A release, tagged with its version.
    /// </summary>
    Release,

    /// <summary>
    /// A pre-release, tagged with a beta of its version.
    /// </summary>
    Beta,
}
