using System;
using System.Collections.Generic;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Behaviour.Diagnostics
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with diagnostic functions.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will cause Mofichan to intercept messages
    /// passed between other behaviours in the chain and log them using an injected <see cref="ILogger"/>. 
    /// </remarks>
    public class DiagnosticsBehaviour : BaseBehaviour
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsBehaviour"/> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder"/>.</param>
        /// <param name="logger">The logger to use.</param>
        public DiagnosticsBehaviour(Func<IResponseBuilder> responseBuilderFactory, ILogger logger)
            : base(responseBuilderFactory)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Allows the behaviour to inspect the stack of behaviours Mofichan
        /// will be loaded with.
        /// </summary>
        /// <param name="stack">The behaviour stack.</param>
        /// <remarks>
        /// This method should be invoked before the behaviour <i>chain</i>
        /// is created.
        /// </remarks>
        public override void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            base.InspectBehaviourStack(stack);

            /*
             * We wrap each behaviour inside a decorator that intercepts
             * messages and logs them.
             */
            for (var i = 0; i < stack.Count; i++)
            {
                var behaviour = stack[i];

                stack[i] = new LoggingBehaviourDecorator(behaviour, this.logger);
            }
        }

        /// <summary>
        /// Determines whether this instance can process the specified incoming message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the incoming message; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this instance can process the specified outgoing message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the outgoing messagee; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return false;
        }

        /// <summary>
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private class LoggingBehaviourDecorator : BaseBehaviourDecorator
        {
            private const string Pencil = "🖉";

            private readonly ILogger logger;

            public LoggingBehaviourDecorator(IMofichanBehaviour delegateBehaviour, ILogger logger) : base(delegateBehaviour)
            {
                this.logger = logger;
            }

            public override void OnNext(IncomingMessage message)
            {
                var body = message.Context.Body;
                var sender = message.Context.From;

                this.logger.Verbose("Behaviour {BehaviourId} offered incoming message {MessageBody} from {Sender}",
                    this.DelegateBehaviour.Id, body, sender);

                base.OnNext(message);
            }

            public override void OnNext(OutgoingMessage message)
            {
                var body = message.Context.Body;
                var sender = message.Context.From;

                this.logger.Verbose("Behaviour {BehaviourId} offered outgoing message {MessageBody} from {Sender}",
                    this.DelegateBehaviour.Id, body, sender);

                base.OnNext(message);
            }

            public override string ToString()
            {
                var baseRepr = base.ToString().Trim('[', ']');
                return string.Format("[{0} {1}]", baseRepr, Pencil);
            }
        }
    }
}
