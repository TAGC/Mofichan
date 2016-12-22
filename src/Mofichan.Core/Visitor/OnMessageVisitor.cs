using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// A type of <see cref="IBehaviourVisitor"/> that visits behaviours when a message is received.
    /// <para></para>
    /// Responses to these received messages should be registered with this visitor.
    /// </summary>
    public class OnMessageVisitor : BaseBehaviourVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnMessageVisitor"/> class.
        /// </summary>
        /// <param name="message">The message to register responses for.</param>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">A factory for constructing instances of a message builder.</param>
        public OnMessageVisitor(MessageContext message, BotContext botContext,
            Func<IResponseBodyBuilder> messageBuilderFactory)
            : base(message, botContext, messageBuilderFactory)
        {
        }

        /// <summary>
        /// Gets the message being responded to.
        /// </summary>
        /// <value>
        /// The message being responded to.
        /// </value>
        public new MessageContext Message
        {
            get
            {
                return base.Message;
            }
        }
    }
}
