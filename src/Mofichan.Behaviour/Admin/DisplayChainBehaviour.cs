using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with the administrative ability
    /// to display her behaviour chain.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will allow Mofichan to respond to admin requests
    /// to show Mofichan's configured behaviour chain.
    /// <para></para>
    /// This chain will also represent the enable state of enableable behaviour modules.
    /// </remarks>
    public class DisplayChainBehaviour : BaseBehaviour
    {
        private const string BehaviourChainConnector = " ⇄ ";

        private readonly Regex displayChainPattern;
        private IList<IMofichanBehaviour> behaviourStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayChainBehaviour"/> class.
        /// </summary>
        public DisplayChainBehaviour()
        {
            // TODO: refactor to inject identity match logic.
            var identityMatch = @"(mofichan|mofi)";

            this.displayChainPattern = new Regex(
                string.Format(@"{0},? (display|show( your)?) behaviour chain", identityMatch),
                RegexOptions.IgnoreCase);
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
            this.behaviourStack = stack;
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
            var body = message.Context.Body;
            return this.displayChainPattern.IsMatch(body);
        }

        /// <summary>
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <exception cref="Core.Exceptions.MofichanAuthorisationException">If user is not an admin.</exception>
        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            message.Context.CheckSenderAuthorised("Non-admin user attempted to display behaviour chain");

            var reply = message.Reply("My behaviour chain: " + this.BuildBehaviourChainRepresentation());
            this.SendUpstream(reply);
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
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the behaviour chain representation.
        /// </summary>
        /// <returns>A string representation of the current behaviour chain.</returns>
        private string BuildBehaviourChainRepresentation()
        {
            Debug.Assert(this.behaviourStack != null, "The behaviour stack should not be null");
            Debug.Assert(this.behaviourStack.Any(), "The behaviour stack should not be empty");

            var reprBuilder = new StringBuilder();

            for (var i = 0; i < this.behaviourStack.Count - 1; i++)
            {
                reprBuilder.AppendFormat("{0}{1}", this.behaviourStack[i], BehaviourChainConnector);
            }

            reprBuilder.Append(this.behaviourStack.Last());
            return reprBuilder.ToString();
        }
    }
}
