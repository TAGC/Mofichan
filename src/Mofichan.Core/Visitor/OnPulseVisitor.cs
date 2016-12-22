using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// Represents a type of <see cref="IBehaviourVisitor"/> that visits behaviours when a pulse event occurs.
    /// </summary>
    public class OnPulseVisitor : BaseBehaviourVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnPulseVisitor"/> class.
        /// </summary>
        /// <param name="lastReceivedMessage">The last message that was received.</param>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">The message builder factory.</param>
        public OnPulseVisitor(MessageContext lastReceivedMessage, BotContext botContext,
            Func<IResponseBodyBuilder> messageBuilderFactory)
            : base(lastReceivedMessage, botContext, messageBuilderFactory)
        {
        }
    }
}
