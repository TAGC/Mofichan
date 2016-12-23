﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.BehaviourOutputs
{
    /// <summary>
    /// Represents a simple output generated by a behaviour.
    /// <para></para>
    /// These can consist of both outgoing messages and actions to perform. They are
    /// not generally performed in response to a message received from another user.
    /// </summary>
    public class SimpleOutput
    {
        private SimpleOutput(MessageContext message, IEnumerable<Action> sideEffects)
        {
            this.Message = message;
            this.SideEffects = sideEffects;
        }

        /// <summary>
        /// Gets the outgoing message component of this response. This may be null if no outgoing
        /// message is part of this response.
        /// </summary>
        /// <value>
        /// The potential outgoing message.
        /// </value>
        public MessageContext Message { get; }

        /// <summary>
        /// Gets the side effects that ought to be performed if this response is selected.
        /// </summary>
        /// <value>
        /// The side effects.
        /// </value>
        public IEnumerable<Action> SideEffects { get; }

        /// <summary>
        /// Derives an output from a new outgoing message.
        /// </summary>
        /// <param name="message">The new outgoing message.</param>
        /// <returns>A derived version of this output.</returns>
        public SimpleOutput DeriveFromNewMessage(MessageContext message)
        {
            return new SimpleOutput(message, this.SideEffects);
        }

        /// <summary>
        /// Accepts this output, indicating that its messages and side effects
        /// will be acted on.
        /// </summary>
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
            return string.Format("Simple output '{0}' with {1} action(s)",
                this.Message?.Body, this.SideEffects.Count());
        }

        /// <summary>
        /// Builds instances of <see cref="SimpleOutput"/>.
        /// </summary>
        public class Builder
        {
            private readonly BotContext botContext;
            private readonly Func<IResponseBodyBuilder> messageBuilderFactory;
            private readonly IList<Action> sideEffects;

            private MessageContext message;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="botContext">The bot context.</param>
            /// <param name="messageBuilderFactory">A factory for producing message builders.</param>
            public Builder(BotContext botContext, Func<IResponseBodyBuilder> messageBuilderFactory)
            {
                Raise.ArgumentNullException.IfIsNull(botContext, nameof(botContext));
                Raise.ArgumentNullException.IfIsNull(messageBuilderFactory, nameof(messageBuilderFactory));

                this.botContext = botContext;
                this.sideEffects = new List<Action>();
                this.messageBuilderFactory = messageBuilderFactory;
            }

            /// <summary>
            /// Configures the output to use the configured message.
            /// </summary>
            /// <param name="recipient">The intended message recipient.</param>
            /// <param name="configureBuilder">A callback to configure the message builder.</param>
            /// <returns>
            /// This builder.
            /// </returns>
            public Builder WithMessage(IMessageTarget recipient, Action<IResponseBodyBuilder> configureBuilder)
            {
                return this.WithMessage(null, recipient, configureBuilder);
            }

            /// <summary>
            /// Configures the output to use the configured message.
            /// </summary>
            /// <param name="sender">The message sender.</param>
            /// <param name="recipient">The intended message recipient.</param>
            /// <param name="configureBuilder">A callback to configure the message builder.</param>
            /// <returns>
            /// This builder.
            /// </returns>
            public Builder WithMessage(IMessageTarget sender, IMessageTarget recipient,
                Action<IResponseBodyBuilder> configureBuilder)
            {
                Raise.InvalidOperationException.If(this.messageBuilderFactory == null,
                    "Message builder factory has not been specified");

                var builder = this.messageBuilderFactory();
                configureBuilder(builder);

                var body = builder.Build();
                this.message = new MessageContext(sender, recipient, body);

                return this;
            }

            /// <summary>
            /// Configures the output to perform the specified side effect if accepted.
            /// </summary>
            /// <param name="sideEffect">The side effect.</param>
            /// <returns>This builder.</returns>
            public Builder WithSideEffect(Action sideEffect)
            {
                this.sideEffects.Add(sideEffect);
                return this;
            }

            /// <summary>
            /// Configures the output to perform the specified bot context change if accepted.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <returns>This builder.</returns>
            public Builder WithBotContextChange(Action<BotContext> action)
            {
                return this.WithSideEffect(() => action(this.botContext));
            }

            /// <summary>
            /// Builds an output based on the configuration of this builder.
            /// </summary>
            /// <returns>An output.</returns>
            public SimpleOutput Build()
            {
                return new SimpleOutput(this.message, this.sideEffects);
            }
        }
    }
}