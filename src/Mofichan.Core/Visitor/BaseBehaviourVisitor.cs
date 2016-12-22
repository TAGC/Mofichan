using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// A base implementation of <see cref="IBehaviourVisitor"/>. 
    /// </summary>
    public abstract class BaseBehaviourVisitor : IBehaviourVisitor
    {
        private readonly Func<IResponseBodyBuilder> messageBuilderFactory;
        private readonly List<Response> responses;
        private readonly BotContext botContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviourVisitor"/> class.
        /// </summary>
        /// <param name="message">The message to register responses for.</param>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">A factory for constructing instances of a message builder.</param>
        protected BaseBehaviourVisitor(MessageContext message, BotContext botContext,
            Func<IResponseBodyBuilder> messageBuilderFactory)
        {
            Raise.ArgumentNullException.IfIsNull(message, nameof(message));
            Raise.ArgumentNullException.IfIsNull(botContext, nameof(botContext));
            Raise.ArgumentNullException.IfIsNull(messageBuilderFactory, nameof(messageBuilderFactory));

            this.Message = message;
            this.botContext = botContext;
            this.messageBuilderFactory = messageBuilderFactory;
            this.responses = new List<Response>();
        }

        /// <summary>
        /// Gets the collection of responses that have been registered to this visitor so far.
        /// </summary>
        /// <value>
        /// The registered responses.
        /// </value>
        public IEnumerable<Response> Responses
        {
            get
            {
                return this.responses;
            }
        }

        /// <summary>
        /// Gets the message being responded to.
        /// </summary>
        /// <value>
        /// The message being responded to.
        /// </value>
        protected MessageContext Message { get; private set; }

        /// <summary>
        /// Modifies all currently registered responses.
        /// </summary>
        /// <param name="modification">The modification to apply to each response.</param>
        public void ModifyResponses(Func<Response, Response> modification)
        {
            var modifiedResponses = this.responses.Select(modification).ToList();

            this.responses.Clear();
            this.responses.AddRange(modifiedResponses);
        }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        public void RegisterResponse(Action<Response.Builder> configureBuilder)
        {
            var builder = new Response.Builder(this.botContext, this.messageBuilderFactory);
            builder.RespondingTo(this.Message);

            configureBuilder(builder);

            this.responses.Add(builder.Build());
        }
    }
}
