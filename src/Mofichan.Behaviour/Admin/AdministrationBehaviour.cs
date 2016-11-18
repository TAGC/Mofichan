using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;
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

        private IObserver<OutgoingMessage> upstreamObserver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationBehaviour" /> class.
        /// </summary>
        /// <param name="chainBuilder">The object to use for composing sub-behaviours into a chain.</param>
        /// <param name="logger">The logger to use.</param>
        public AdministrationBehaviour(IBehaviourChainBuilder chainBuilder, ILogger logger) : base(
            chainBuilder,
            new ToggleEnableBehaviour(),
            new DisplayChainBehaviour())
        {
            this.logger = logger.ForContext<AdministrationBehaviour>();
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
                this.logger.Information(e, "Failed to authorise {User} for request: {Request}",
                    message.Context.From, message.Context.Body);

                var sender = message.Context.From as IUser;

                if (sender != null && this.upstreamObserver != null)
                {
                    var reply = message.Reply(string.Format("I'm afraid you're not authorised to do that, {0}",
                        sender.Name));

                    this.upstreamObserver.OnNext(reply);
                }
            }
        }
    }
}
