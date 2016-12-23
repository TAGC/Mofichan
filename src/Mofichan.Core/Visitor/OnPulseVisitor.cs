using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// Represents a type of <see cref="IBehaviourVisitor"/> that visits behaviours when a pulse event occurs.
    /// </summary>
    public class OnPulseVisitor : BaseBehaviourVisitor
    {
        private readonly IEnumerable<MessageContext> validResponseContexts;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnPulseVisitor" /> class.
        /// </summary>
        /// <param name="validResponseContexts">The valid response contexts.</param>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">The message builder factory.</param>
        public OnPulseVisitor(IEnumerable<MessageContext> validResponseContexts, BotContext botContext,
            Func<IResponseBodyBuilder> messageBuilderFactory)
            : base(botContext, messageBuilderFactory)
        {
            this.validResponseContexts = validResponseContexts;
        }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if responding to an invalid response context.
        /// </exception>
        public override void RegisterResponse(Action<Response.Builder> configureBuilder)
        {
            var builder = new Response.Builder(this.BotContext, this.MessageBuilderFactory);

            configureBuilder(builder);

            var response = builder.Build();

            if (!this.validResponseContexts.Contains(response.RespondingTo))
            {
                throw new InvalidOperationException("Registered invalid response: " + response);
            }

            this.AddResponse(response);
        }
    }
}
