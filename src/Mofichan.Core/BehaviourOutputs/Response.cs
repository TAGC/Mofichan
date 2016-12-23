using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using PommaLabs.Thrower;

namespace Mofichan.Core.BehaviourOutputs
{
    /// <summary>
    /// Represents a response that Mofichan has created based on
    /// a received message.
    /// <para></para>
    /// Responses can consist of both outgoing messages and actions
    /// to perform.
    /// </summary>
    public class Response
    {
        private readonly SimpleOutput baseOutput;

        private Response(
            SimpleOutput baseOutput,
            MessageContext respondingTo,
            RelevanceArgument relevanceArgument)
        {
            this.baseOutput = baseOutput;
            this.RespondingTo = respondingTo;
            this.RelevanceArgument = relevanceArgument;
        }

        /// <summary>
        /// Gets the message being responding to.
        /// </summary>
        /// <value>
        /// The message being responding to.
        /// </value>
        public MessageContext RespondingTo { get; }

        /// <summary>
        /// Gets an argument about why this response is relevant to the message being responded to.
        /// </summary>
        /// <value>
        /// The relevance argument.
        /// </value>
        public RelevanceArgument RelevanceArgument { get; }

        /// <summary>
        /// Gets the outgoing message component of this response. This may be null if no outgoing
        /// message is part of this response.
        /// </summary>
        /// <value>
        /// The potential outgoing message.
        /// </value>
        public MessageContext Message
        {
            get
            {
                return this.baseOutput.Message;
            }
        }

        /// <summary>
        /// Gets the side effects that ought to be performed if this response is selected.
        /// </summary>
        /// <value>
        /// The side effects.
        /// </value>
        public IEnumerable<Action> SideEffects
        {
            get
            {
                return this.baseOutput.SideEffects;
            }
        }

        /// <summary>
        /// Derives a response from a new outgoing message.
        /// </summary>
        /// <param name="message">The new outgoing message.</param>
        /// <returns>A derived version of this response.</returns>
        public Response DeriveFromNewMessage(MessageContext message)
        {
            var newOutput = this.baseOutput.DeriveFromNewMessage(message);
            return new Response(newOutput, this.RespondingTo, this.RelevanceArgument);
        }

        /// <summary>
        /// Accepts this response.
        /// <para></para>
        /// This indicates that this response has been selected as the most appropriate
        /// candidate for responding to a particular message.
        /// </summary>
        public void Accept()
        {
            this.baseOutput.Accept();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Response '{0}' responding to '{1}' from {2}, {3} action(s), relevance: {4}",
                this.Message?.Body, this.RespondingTo.Body, this.RespondingTo.From, this.SideEffects.Count(),
                this.RelevanceArgument);
        }

        /// <summary>
        /// Builds instances of <see cref="Response"/>.
        /// </summary>
        public class Builder
        {
            private static readonly RelevanceArgument DefaultRelevanceArgument =
                new RelevanceArgument(Enumerable.Empty<string>(), false);

            private readonly SimpleOutput.Builder nestedBuilder;

            private RelevanceArgument relevanceArgument;
            private MessageContext respondingTo;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="botContext">The bot context.</param>
            /// <param name="messageBuilderFactory">A factory for producing message builders.</param>
            public Builder(BotContext botContext, Func<IResponseBodyBuilder> messageBuilderFactory)
            {
                this.nestedBuilder = new SimpleOutput.Builder(botContext, messageBuilderFactory);
                this.relevanceArgument = DefaultRelevanceArgument;
            }

            /// <summary>
            /// Configures the message that the response is based on.
            /// </summary>
            /// <param name="message">The message the response is based on.</param>
            /// <returns>This builder.</returns>
            public Builder To(MessageContext message)
            {
                this.respondingTo = message;
                return this;
            }

            /// <summary>
            /// Configures the response to use the configured message.
            /// <para></para>
            /// The message targets will be based on the message being responded to.
            /// </summary>
            /// <param name="configureBuilder">A callback to configure the message builder.</param>
            /// <returns>This builder.</returns>
            public Builder WithMessage(Action<IResponseBodyBuilder> configureBuilder)
            {
                Raise.InvalidOperationException.If(this.respondingTo == null,
                    "Message being responded to has not been specified");

                var sender = this.respondingTo.To;
                var recipient = this.respondingTo.From;

                this.nestedBuilder.WithMessage(sender, recipient, builder =>
                {
                    builder.UsingContext(this.respondingTo);
                    configureBuilder(builder);
                });

                return this;
            }

            /// <summary>
            /// Configures the response to perform the specified side effect if accepted.
            /// </summary>
            /// <param name="sideEffect">The side effect.</param>
            /// <returns>This builder.</returns>
            public Builder WithSideEffect(Action sideEffect)
            {
                this.nestedBuilder.WithSideEffect(sideEffect);
                return this;
            }

            /// <summary>
            /// Configures the response to perform the specified bot context change if accepted.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <returns>This builder.</returns>
            public Builder WithBotContextChange(Action<BotContext> action)
            {
                this.nestedBuilder.WithBotContextChange(action);
                return this;
            }

            /// <summary>
            /// Configures the argument used by the response to declare why it's relevant
            /// for the message being responded to.
            /// </summary>
            /// <param name="configureBuilder">A callback to configure the response argument.</param>
            /// <returns>This builder.</returns>
            public Builder RelevantBecause(Action<RelevanceArgument.Builder> configureBuilder)
            {
                var builder = new RelevanceArgument.Builder();
                configureBuilder(builder);
                this.relevanceArgument = builder.Build();

                return this;
            }

            /// <summary>
            /// Builds a response based on the configuration of this builder.
            /// </summary>
            /// <returns>A response.</returns>
            public Response Build()
            {
                Raise.InvalidOperationException.If(this.respondingTo == null,
                    "Message being responded to has not been specified");

                Raise.InvalidOperationException.If(this.relevanceArgument == null,
                    "A relevance argument has not been specified");

                var baseOutput = this.nestedBuilder.Build();

                return new Response(baseOutput, this.respondingTo, this.relevanceArgument);
            }
        }
    }
}
