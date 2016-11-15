using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> causes Mofichan to ignore her own messages.
    /// </summary>
    public class SelfIgnoreBehaviour : BaseBehaviour
    {
        /// <summary>
        /// Determines whether this instance can process the specified incoming message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the incoming message; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            var from = message.Context.From as IUser;

            return from?.Type == UserType.Self;
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
            // Do nothing - messages from Mofichan are ignored.
        }

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
