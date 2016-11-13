using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core
{
    /// <summary>
    /// Represents a message and associated contextual details.
    /// </summary>
    public struct MessageContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContext"/> struct.
        /// </summary>
        /// <param name="from">The sender of the message.</param>
        /// <param name="to">The recipient of the message.</param>
        /// <param name="body">The message body.</param>
        /// <param name="delay">An optional delay that should be waited before sending the message.</param>
        public MessageContext(IMessageTarget from, IMessageTarget to, string body, TimeSpan? delay = null)
        {
            this.From = from;
            this.To = to;
            this.Body = body;
            this.Delay = delay ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the message sender.
        /// </summary>
        /// <value>
        /// The message sender.
        /// </value>
        public IMessageTarget From { get; }

        /// <summary>
        /// Gets the message recipient.
        /// </summary>
        /// <value>
        /// The message recipient.
        /// </value>
        public IMessageTarget To { get; }

        /// <summary>
        /// Gets the message body.
        /// </summary>
        /// <value>
        /// The message body.
        /// </value>
        public string Body { get; }

        /// <summary>
        /// Gets the amount of time that should be waited before sending this message (if applicable).
        /// </summary>
        /// <value>
        /// The message send delay.
        /// </value>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Message (from={0}, to={1}, body={2} delay={3})",
                this.From, this.To, this.Body, this.Delay);
        }
    }

    /// <summary>
    /// Represents a message being received by Mofichan.
    /// </summary>
    public struct IncomingMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncomingMessage"/> struct.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="potentialReply">A potential reply by Mofichan to the message.</param>
        public IncomingMessage(MessageContext context, MessageContext? potentialReply = null)
        {
            this.Context = context;
            this.PotentialReply = potentialReply;
        }

        /// <summary>
        /// Gets the message context.
        /// </summary>
        /// <value>
        /// The message context.
        /// </value>
        public MessageContext Context { get; }

        /// <summary>
        /// Gets the potential reply to the message.
        /// </summary>
        /// <value>
        /// The potential reply.
        /// </value>
        public MessageContext? PotentialReply { get; }

        /// <summary>
        /// Creates an instance of <code>IncomingMessage</code> that uses the specified reply
        /// but preserves all other details.
        /// </summary>
        /// <param name="reply">The reply context.</param>
        /// <returns>A derived <code>IncomingMessage</code> instance.</returns>
        public IncomingMessage WithReply(MessageContext reply)
        {
            return new IncomingMessage(this.Context, reply);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Incoming message (context={0}, potential reply={1})",
                this.Context, this.PotentialReply);
        }
    }

    /// <summary>
    /// Represents a message being sent by Mofichan into the wider world.
    /// </summary>
    public struct OutgoingMessage
    {
        /// <summary>
        /// Gets or sets the message context.
        /// </summary>
        /// <value>
        /// The message context.
        /// </value>
        public MessageContext Context { set;  get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Outgoing message (context={0}, delay={1})", this.Context);
        }
    }
}
