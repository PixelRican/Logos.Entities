// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities
{
    /// <summary>
    /// The exception that is thrown when an attempt to access a entity that does not exist in a
    /// data structure fails.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with the
        /// default error message.
        /// </summary>
        public EntityNotFoundException()
            : base("Unable to find the specified entity.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with the
        /// specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public EntityNotFoundException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class with the
        /// specified error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the <see cref="EntityNotFoundException"/>, or
        /// <see langword="null"/> if no inner exception is specified.
        /// </param>
        public EntityNotFoundException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
