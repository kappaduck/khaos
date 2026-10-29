// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos;

/// <summary>
/// The exception that is thrown when the system under the game refuses an operation: the operating system, the
/// graphics driver, the audio device or a file the game reads.
/// </summary>
/// <remarks>
/// The message names the operation that failed and gives the reason the system reported, for example a window that
/// could not be created because no display is available. These failures usually come from the machine the game runs
/// on, not from the game's code.
/// </remarks>
public sealed class PlatformException : KhaosException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformException"/> class.
    /// </summary>
    public PlatformException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformException"/> class with a message that describes the
    /// error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PlatformException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformException"/> class with a message that describes the
    /// error and the exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this one, or <see langword="null"/>.</param>
    public PlatformException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
