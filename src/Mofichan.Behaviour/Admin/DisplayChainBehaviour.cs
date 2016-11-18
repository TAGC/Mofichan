using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.FilterAttributes;
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
    public class DisplayChainBehaviour : BaseReflectionBehaviour
    {
        private const string IdentityMatch = @"(mofichan|mofi)";
        private const string CommandMatch = @"(display|show( your)?) behaviour chain";
        private const string DisplayChainMatch = IdentityMatch + ",? " + CommandMatch;
        private const string BehaviourChainConnector = " ⇄ ";

        private IList<IMofichanBehaviour> behaviourStack;

        /// <summary>
        /// Returns a string representation of the the behaviour chain.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <returns>A response containing the behaviour chain representation.</returns>
        [RegexIncomingMessageFilter(DisplayChainMatch, RegexOptions.IgnoreCase)]
        [AuthorisationIncomingMessageFilter(
            requiredUserType: UserType.Adminstrator,
            onFailure: "Non-admin user attempted to display behaviour chain")]
        public OutgoingMessage? DisplayChain(IncomingMessage message)
        {
            return message.Reply("My behaviour chain: " + this.BuildBehaviourChainRepresentation());
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
