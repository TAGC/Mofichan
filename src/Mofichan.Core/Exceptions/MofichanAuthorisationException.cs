using System;

namespace Mofichan.Core.Exceptions
{
    /// <summary>
    /// An type of exception raised when a user makes an unauthorised request
    /// to Mofichan.
    /// </summary>
    public class MofichanAuthorisationException : MofichanException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanAuthorisationException" /> class.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        public MofichanAuthorisationException(MessageContext messageContext) : this(null, messageContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanAuthorisationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="messageContext">The message context.</param>
        public MofichanAuthorisationException(string message, MessageContext messageContext)
            : this(message, null, messageContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MofichanAuthorisationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception,
        /// or a null reference (Nothing in Visual Basic) if no inner exception is specified.
        /// </param>
        /// <param name="messageContext">The message context.</param>
        public MofichanAuthorisationException(string message, Exception innerException, MessageContext messageContext)
        {
            this.MessageContext = messageContext;
        }

        /// <summary>
        /// Gets the message context associated with the authorisation failure.
        /// </summary>
        /// <value>
        /// The message context.
        /// </value>
        public MessageContext MessageContext { get; }
    }
}
