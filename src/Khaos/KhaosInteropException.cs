// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// The exception raised when a call to the underlying platform fails.
/// </summary>
public sealed class KhaosInteropException : KhaosException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosInteropException"/> class.
    /// </summary>
    public KhaosInteropException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosInteropException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public KhaosInteropException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosInteropException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public KhaosInteropException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
