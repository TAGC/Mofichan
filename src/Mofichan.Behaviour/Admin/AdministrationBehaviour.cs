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
        internal static string AdministrationBehaviourId = "administration";

        private readonly ILogger logger;

        private IObserver<OutgoingMessage> upstreamObserver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationBehaviour"/> class.
        /// </summary>
        public AdministrationBehaviour(ILogger logger) : base(
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

        public override IDisposable Subscribe(IObserver<OutgoingMessage> observer)
        {
            this.upstreamObserver = observer;
            return base.Subscribe(observer);
        }

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
