using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using PommaLabs.Thrower;

namespace Mofichan.Core
{
    public class Response
    {
        private Response(
            MessageContext respondingTo,
            MessageContext message,
            IEnumerable<Action> sideEffects,
            RelevanceArgument relevanceArgument)
        {
            this.RespondingTo = respondingTo;
            this.Message = message;
            this.SideEffects = sideEffects;
            this.RelevanceArgument = relevanceArgument;
        }

        public MessageContext RespondingTo { get; }

        public MessageContext Message { get; }

        public IEnumerable<Action> SideEffects { get; }

        public RelevanceArgument RelevanceArgument { get; }

        public Response DeriveFromNewMessage(MessageContext message)
        {
            return new Response(this.RespondingTo, message, this.SideEffects, this.RelevanceArgument);
        }

        public void Accept()
        {
            foreach (var action in this.SideEffects)
            {
                action();
            }
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

        public class Builder
        {
            private static readonly RelevanceArgument DefaultRelevanceArgument =
                new RelevanceArgument(Enumerable.Empty<string>(), false);

            private readonly BotContext botContext;
            private readonly Func<IResponseBodyBuilder> messageBuilderFactory;
            private readonly IList<Action> sideEffects;

            private MessageContext respondingTo;
            private MessageContext message;
            private RelevanceArgument relevanceArgument;

            public Builder(BotContext botContext, Func<IResponseBodyBuilder> messageBuilderFactory)
            {
                Raise.ArgumentNullException.IfIsNull(botContext, nameof(botContext));
                Raise.ArgumentNullException.IfIsNull(messageBuilderFactory, nameof(messageBuilderFactory));

                this.botContext = botContext;
                this.sideEffects = new List<Action>();
                this.messageBuilderFactory = messageBuilderFactory;
                this.relevanceArgument = DefaultRelevanceArgument;
            }

            public Builder RespondingTo(MessageContext message)
            {
                this.respondingTo = message;
                return this;
            }

            public Builder WithMessage(Action<IResponseBodyBuilder> configureBuilder)
            {
                Raise.InvalidOperationException.If(this.respondingTo == null,
                    "Message being responded to has not been specified");

                Raise.InvalidOperationException.If(this.messageBuilderFactory == null,
                    "Message builder factory has not been specified");

                var builder = this.messageBuilderFactory();
                builder.UsingContext(this.respondingTo);
                configureBuilder(builder);

                var sender = this.respondingTo.To;
                var recipient = this.respondingTo.From;
                var body = builder.Build();

                this.message = new MessageContext(sender, recipient, body);

                return this;
            }

            public Builder WithSideEffect(Action sideEffect)
            {
                this.sideEffects.Add(sideEffect);
                return this;
            }

            public Builder WithBotContextChange(Action<BotContext> action)
            {
                return this.WithSideEffect(() => action(this.botContext));
            }

            public Builder RelevantBecause(Action<RelevanceArgument.Builder> configureBuilder)
            {
                var builder = new RelevanceArgument.Builder();
                configureBuilder(builder);
                this.relevanceArgument = builder.Build();

                return this;
            }

            public Response Build()
            {
                Raise.InvalidOperationException.If(this.respondingTo == null,
                    "Message being responded to has not been specified");

                Raise.InvalidOperationException.If(this.relevanceArgument == null,
                    "A relevance argument has not been specified");

                return new Response(this.respondingTo, this.message, this.sideEffects, this.relevanceArgument);
            }
        }
    }
}
