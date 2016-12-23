using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// An implementation of <see cref="IBehaviourVisitorFactory"/>. 
    /// </summary>
    public class BehaviourVisitorFactory : IBehaviourVisitorFactory
    {
        private readonly BotContext botContext;
        private readonly Func<IResponseBodyBuilder> messageBuilderFactory;
        private readonly IList<IBehaviourVisitor> visitors;

        private MessageContext lastMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourVisitorFactory"/> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">The message builder factory.</param>
        public BehaviourVisitorFactory(BotContext botContext, Func<IResponseBodyBuilder> messageBuilderFactory)
        {
            this.botContext = botContext;
            this.messageBuilderFactory = messageBuilderFactory;
            this.visitors = new List<IBehaviourVisitor>();
            this.lastMessage = new MessageContext();
        }

        /// <summary>
        /// Gets the responses.
        /// </summary>
        /// <value>
        /// The responses.
        /// </value>
        public IEnumerable<Response> Responses
        {
            get
            {
                return this.visitors.SelectMany(it => it.Responses);
            }
        }

        /// <summary>
        /// Gets the autonomous outputs.
        /// </summary>
        /// <value>
        /// The autonomous outputs.
        /// </value>
        public IEnumerable<SimpleOutput> AutononousOutputs
        {
            get
            {
                return this.visitors.SelectMany(it => it.AutonomousOutputs);
            }
        }

        /// <summary>
        /// Creates an <see cref="OnMessageVisitor" /> that carries the message that responses should be
        /// collected for.
        /// </summary>
        /// <param name="message">The received message, which responses should be registered for.</param>
        /// <returns>
        /// A new <c>OnMessageVisitor</c>.
        /// </returns>
        public OnMessageVisitor CreateMessageVisitor(MessageContext message)
        {
            var visitor = new OnMessageVisitor(message, this.botContext, this.messageBuilderFactory);
            this.visitors.Add(visitor);
            this.lastMessage = message;

            return visitor;
        }

        /// <summary>
        /// Creates an <see cref="OnPulseVisitor" />.
        /// </summary>
        /// <returns>
        /// A new <c>OnPulseVisitor</c>.
        /// </returns>
        public OnPulseVisitor CreatePulseVisitor()
        {
            var visitor = new OnPulseVisitor(new[] { this.lastMessage }, this.botContext, this.messageBuilderFactory);
            this.visitors.Add(visitor);

            return visitor;
        }
    }
}
