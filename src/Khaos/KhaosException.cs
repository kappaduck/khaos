// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// The base of every exception Khaos throws.
/// </summary>
/// <remarks>
/// Catch <see cref="KhaosException"/> to handle any failure that comes from Khaos in one place, such as showing a
/// message before the game closes. Catch a derived exception, such as <see cref="PlatformException"/>, to handle
/// one kind of failure only.
/// </remarks>
public class KhaosException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class.
    /// </summary>
    public KhaosException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class with a message that describes the error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public KhaosException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KhaosException"/> class with a message that describes the error
    /// and the exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this one, or <see langword="null"/>.</param>
    public KhaosException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
