using System;
using System.Collections.Generic;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Serilog;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> augments Mofichan to have administrative capabilities.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will provide functions that administrators can invoke.
    /// <para></para>
    /// This behaviour will also intercept <see cref="MofichanAuthorisationException"/> raised by
    /// downstream behaviours and cause Mofichan to send a response to the user.
    /// </remarks>
    public class AdministrationBehaviour : BaseMultiBehaviour
    {
        internal const string AdministrationBehaviourId = "administration";

        private readonly ILogger logger;
        private readonly AuthorisationFailureHandler authExceptionHandler;

        private IObserver<OutgoingMessage> upstreamObserver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder" />.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="chainBuilder">The object to use for composing sub-behaviours into a chain.</param>
        /// <param name="logger">The logger to use.</param>
        public AdministrationBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            IBehaviourChainBuilder chainBuilder,
            ILogger logger)
            : base(chainBuilder,
            new ToggleEnableBehaviour(responseBuilderFactory, flowManager, logger),
            new DisplayChainBehaviour(responseBuilderFactory, flowManager, logger))
        {
            this.logger = logger.ForContext<AdministrationBehaviour>();
            this.authExceptionHandler = new AuthorisationFailureHandler(this.TrySendUpstream, logger);
        }

        /// <summary>
        /// Gets the behaviour module identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public override string Id
        {
            get
            {
                return AdministrationBehaviourId;
            }
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public override IDisposable Subscribe(IObserver<OutgoingMessage> observer)
        {
            this.upstreamObserver = observer;
            return base.Subscribe(observer);
        }

        /// <summary>
        /// Called to notify this observer of an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public override void OnNext(IncomingMessage message)
        {
            try
            {
                base.OnNext(message);
            }
            catch (MofichanAuthorisationException e)
            {
                this.authExceptionHandler.Handle(e);
            }
        }

        private void TrySendUpstream(OutgoingMessage message)
        {
            this.upstreamObserver?.OnNext(message);
        }
    }
}
