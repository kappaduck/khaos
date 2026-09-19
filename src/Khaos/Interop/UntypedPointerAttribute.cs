// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop;

/// <summary>
/// States why a binding takes or returns <see langword="void" />* instead of a typed pointer.
/// </summary>
/// <remarks>
/// KHI004 requires it on every declaration that carries untyped memory across the boundary. The reason is
/// part of the declaration on purpose: it survives a reformat and it can be found by searching.
/// </remarks>
/// <param name="reason">Why the native side is genuinely untyped.</param>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class UntypedPointerAttribute(string reason) : Attribute
{
    /// <summary>
    /// Gets why the native side is genuinely untyped.
    /// </summary>
    internal string Reason { get; } = reason;
}
