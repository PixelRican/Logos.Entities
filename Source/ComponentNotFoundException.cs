using System;

namespace Logos.Entities
{
    /// <summary>
    /// The exception that is thrown when a component of a specified type could not be found within
    /// a data structure.
    /// </summary>
    public class ComponentNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with
        /// the default error message.
        /// </summary>
        public ComponentNotFoundException()
            : base("Component of the requested type could not be found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with
        /// the specified error message.
        /// </summary>
        /// 
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
        /// 
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// 
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
