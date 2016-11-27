using System;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Serilog;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// Handles situations in which a user makes a request to Mofichan that they lack
    /// the requisite authorisation for.
    /// </summary>
    public class AuthorisationFailureHandler
    {
        private readonly Action<OutgoingMessage> generatedResponseHandler;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorisationFailureHandler"/> class.
        /// </summary>
        /// <param name="generatedResponseHandler">A callback to invoke when a response is generated.</param>
        /// <param name="logger">A logger to record authorisation failure events.</param>
        public AuthorisationFailureHandler(Action<OutgoingMessage> generatedResponseHandler, ILogger logger)
        {
            this.generatedResponseHandler = generatedResponseHandler;
            this.logger = logger.ForContext<AuthorisationFailureHandler>();
        }

        /// <summary>
        /// Handles the authorisation exception by logging it and generating a suitable response
        /// for the requestor.
        /// </summary>
        /// <param name="e">The authorisation exception object.</param>
        public void Handle(MofichanAuthorisationException e)
        {
            var messageContext = e.MessageContext;

            this.logger.Information(e, "Failed to authorise {User} for request: {Request}",
                messageContext.From, messageContext.Body);

            var sender = messageContext.From as IUser;

            if (sender != null)
            {
                var reply = messageContext.Reply(string.Format("I'm afraid you're not authorised to do that, {0}",
                    sender.Name));

                this.generatedResponseHandler(reply);
            }
        }
    }
}
