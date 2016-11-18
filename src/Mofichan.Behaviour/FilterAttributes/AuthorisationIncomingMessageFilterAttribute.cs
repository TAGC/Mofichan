using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.FilterAttributes
{
    /// <summary>
    /// A type of <see cref="BaseIncomingMessageFilterAttribute"/> that checks the sender
    /// has appropriate authorisation to perform the command and throws a(n)
    /// <see cref="MofichanAuthorisationException"/> if not. 
    /// </summary>
    public class AuthorisationIncomingMessageFilterAttribute : BaseIncomingMessageFilterAttribute
    {
        private readonly UserType requiredUserType;
        private readonly string authorisationFailureMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorisationIncomingMessageFilterAttribute"/> class.
        /// </summary>
        /// <param name="requiredUserType">The required type of the user to perform the command.</param>
        /// <param name="onFailure">The message to include in thrown exceptions.</param>
        public AuthorisationIncomingMessageFilterAttribute(UserType requiredUserType, string onFailure)
        {
            this.requiredUserType = requiredUserType;
            this.authorisationFailureMessage = onFailure;
        }

        /// <summary>
        /// Called to notify this observer of an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <exception cref="MofichanAuthorisationException"></exception>
        public override void OnNext(IncomingMessage message)
        {
            var user = message.Context.From as IUser;

            /*
             * We forward messages not from a user. Should this be the case?
             */
            if (user == null || user.Type == requiredUserType)
            {
                this.SendDownstream(message);
            }
            else
            {
                throw new MofichanAuthorisationException(
                    this.authorisationFailureMessage, message.Context);
            }
        }
    }
}
