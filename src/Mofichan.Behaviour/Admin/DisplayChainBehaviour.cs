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
    /// <para></para>
    /// Adding this module to the behaviour chain will allow Mofichan to respond to admin requests
    /// to show Mofichan's configured behaviour chain.
    /// <para></para>
    /// This chain will also represent the enable state of enableable behaviour modules.
    /// </summary>
    public class DisplayChainBehaviour : BaseBehaviour
    {
        private const string BehaviourChainConnector = " ⇄ ";

        private readonly Regex displayChainPattern;
        private IList<IMofichanBehaviour> behaviourStack;

        public DisplayChainBehaviour()
        {
            // TODO: refactor to inject identity match logic.
            var identityMatch = @"(mofichan|mofi)";

            this.displayChainPattern = new Regex(
                string.Format(@"{0},? (display|show( your)?) behaviour chain", identityMatch),
                RegexOptions.IgnoreCase);
        }

        public override void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            this.behaviourStack = stack;
        }

        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            var fromUser = message.Context.From as IUser;
            var body = message.Context.Body;

            var isRequestValid = this.displayChainPattern.IsMatch(body);
            var isUserAuthorised = fromUser?.Type == UserType.Adminstrator;

            return isRequestValid && isUserAuthorised && behaviourStack != null;
        }

        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            Debug.Assert((message.Context.From as IUser).Type == UserType.Adminstrator);

            var reply = message.Reply("My behaviour chain: " + BuildBehaviourChainRepresentation());
            this.SendUpstream(reply);
        }

        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return false;
        }

        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private string BuildBehaviourChainRepresentation()
        {
            Debug.Assert(this.behaviourStack != null);
            Debug.Assert(this.behaviourStack.Any());

            var reprBuilder = new StringBuilder();

            for (var i = 0; i < behaviourStack.Count - 1; i++)
            {
                reprBuilder.AppendFormat("{0}{1}", behaviourStack[i], BehaviourChainConnector);
            }

            reprBuilder.Append(behaviourStack.Last());
            return reprBuilder.ToString();
        }
    }
}
