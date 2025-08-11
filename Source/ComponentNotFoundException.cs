// Copyright (c) 2025 Roberto I. Mercado
// Released under the MIT License. See LICENSE for details.

using System;

namespace Logos.Entities
{
    /// <summary>
    /// The exception that is thrown when an attempt to access a component that does not exist in a
    /// data source fails.
    /// </summary>
    public class ComponentNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with
        /// the default error message.
        /// </summary>
        public ComponentNotFoundException()
            : base("Unable to find the specified component.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with
        /// the specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public ComponentNotFoundException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with
        /// the specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the <see cref="ComponentNotFoundException"/>, or
        /// <see langword="null"/> if no inner exception is specified.
        /// </param>
        public ComponentNotFoundException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
