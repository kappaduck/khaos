// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Delegate, Inherited = false)]
internal sealed class UntypedBecauseAttribute(string reason) : Attribute
{
    internal string Reason { get; } = reason;
}
