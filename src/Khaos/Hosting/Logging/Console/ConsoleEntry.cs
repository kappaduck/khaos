// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

internal readonly record struct ConsoleEntry(DateTimeOffset? Time, LogLevel Level, string Message, Exception? Exception);
