using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
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
        /// Initializes a new instance of the <see cref="DiagnosticsBehaviour" /> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public DiagnosticsBehaviour(ILogger logger)
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

        private class LoggingBehaviourDecorator : BaseBehaviourDecorator
        {
            private const string Pencil = "✐";

            private readonly ILogger logger;

            public LoggingBehaviourDecorator(IMofichanBehaviour delegateBehaviour, ILogger logger) : base(delegateBehaviour)
            {
                this.logger = logger.ForContext<LoggingBehaviourDecorator>();
            }

            public override void OnNext(IBehaviourVisitor visitor)
            {
                var onMessageVisitor = visitor as OnMessageVisitor;

                if (onMessageVisitor != null)
                {
                    var body = onMessageVisitor.Message.Body;
                    var sender = onMessageVisitor.Message.From;

                    this.logger.Verbose("Behaviour {BehaviourId} received message visitor (message={MessageBody}) " +
                        "(sender={Sender})", this.DelegateBehaviour.Id, body, sender);
                }

                base.OnNext(visitor);
            }

            public override string ToString()
            {
                var baseRepr = base.ToString().Trim('[', ']');
                return string.Format("[{0} {1}]", baseRepr, Pencil);
            }
        }
    }
}
