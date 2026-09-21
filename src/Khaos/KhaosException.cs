// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// The base exception for every error raised by the framework.
/// </summary>
public class KhaosException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class.
    /// </summary>
    public KhaosException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public KhaosException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public KhaosException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
