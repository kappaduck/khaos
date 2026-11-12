// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Interop.Browser;

[SupportedOSPlatform("browser")]
internal static partial class KhaosPage
{
#if NET12_0_OR_GREATER
    [JSImport("ended", "khaos")]
    internal static partial void Ended();

    [JSImport("log", "khaos")]
    internal static partial void Log(int level, string line);

    [JSImport("stopped", "khaos")]
    internal static partial void Stopped(string exception);
#else
    private static JSFunctionBinding? _ended;
    private static JSFunctionBinding? _log;
    private static JSFunctionBinding? _stopped;

    internal static void Ended()
    {
        _ended ??= JSFunctionBinding.BindJSFunction("ended", "khaos", [JSMarshalerType.Discard]);

        Span<JSMarshalerArgument> arguments = stackalloc JSMarshalerArgument[2];

        arguments[0].Initialize();
        arguments[1].Initialize();

        JSFunctionBinding.InvokeJS(_ended, arguments);
    }

    internal static void Log(int level, string line)
    {
        _log ??= JSFunctionBinding.BindJSFunction("log", "khaos", [JSMarshalerType.Discard, JSMarshalerType.Int32, JSMarshalerType.String]);

        Span<JSMarshalerArgument> arguments = stackalloc JSMarshalerArgument[4];
        arguments[0].Initialize();
        arguments[1].Initialize();
        arguments[2].ToJS(level);
        arguments[3].ToJS(line);

        JSFunctionBinding.InvokeJS(_log, arguments);
    }

    internal static void Stopped(string exception)
    {
        _stopped ??= JSFunctionBinding.BindJSFunction("stopped", "khaos", [JSMarshalerType.Discard, JSMarshalerType.String]);

        Span<JSMarshalerArgument> arguments = stackalloc JSMarshalerArgument[3];
        arguments[0].Initialize();
        arguments[1].Initialize();
        arguments[2].ToJS(exception);

        JSFunctionBinding.InvokeJS(_stopped, arguments);
    }
#endif
}
