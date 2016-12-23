using System;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

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
            : base(botContext, messageBuilderFactory)
        {
            Raise.ArgumentNullException.IfIsNull(message, nameof(message));
            this.Message = message;
        }

        /// <summary>
        /// Gets the message being responded to.
        /// </summary>
        /// <value>
        /// The message being responded to.
        /// </value>
        public MessageContext Message { get; }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        public override void RegisterResponse(Action<Response.Builder> configureBuilder)
        {
            var builder = new Response.Builder(this.BotContext, this.MessageBuilderFactory);
            builder.To(this.Message);

            configureBuilder(builder);

            this.AddResponse(builder.Build());
        }
    }
}
