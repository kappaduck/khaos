// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Releases;

public sealed record PreparedRelease(Version Version, Version Native, IReadOnlyList<AnalyzerProject> Analyzers);
