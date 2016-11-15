using System;

namespace Mofichan.Core.Exceptions
{
    /// <summary>
    /// A base class for all custom exceptions related to Mofichan.
    /// </summary>
    public class MofichanException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanException"/> class.
        /// </summary>
        public MofichanException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MofichanException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception,
        /// or a null reference (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        public MofichanException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
