// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
internal sealed class UntypedBecauseAttribute(string reason) : Attribute
{
    internal string Reason { get; } = reason;
}
