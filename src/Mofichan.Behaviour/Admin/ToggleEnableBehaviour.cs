using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with the administrative ability
    /// to control whether other modules are active.
    /// <para></para>
    /// Adding this module to the behaviour chain will allow Mofichan dynamically enable or disable
    /// other behaviour modules, but not add or remove them.
    /// <para></para>
    /// Certain modules (such as the "administrator" module) cannot be enabled or disabled.
    /// </summary>
    internal class ToggleEnableBehaviour : BaseBehaviour
    {
        internal class EnableableBehaviourDecorator : BaseBehaviourDecorator
        {
            private ITargetBlock<IncomingMessage> downstreamTarget;
            private ITargetBlock<OutgoingMessage> upstreamTarget;

            public EnableableBehaviourDecorator(IMofichanBehaviour delegateBehaviour)
                : base(delegateBehaviour)
            {
                this.Enabled = true;
            }

            public bool Enabled { get; set; }

            public override IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
            {
                this.downstreamTarget = target;
                return base.LinkTo(target, linkOptions);
            }

            public override IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
            {
                this.upstreamTarget = target;
                return base.LinkTo(target, linkOptions);
            }

            public override DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
                IncomingMessage message, ISourceBlock<IncomingMessage> source, bool consumeToAccept)
            {
                if (this.Enabled)
                {
                    return base.OfferMessage(messageHeader, message, source, consumeToAccept);
                }
                else if (this.downstreamTarget != null)
                {
                    return this.downstreamTarget.OfferMessage(messageHeader, message, source, consumeToAccept);
                }
                else
                {
                    return DataflowMessageStatus.Declined;
                }
            }

            public override DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
                OutgoingMessage message, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
            {
                if (this.Enabled)
                {
                    return base.OfferMessage(messageHeader, message, source, consumeToAccept);
                }
                else if (this.upstreamTarget != null)
                {
                    return this.upstreamTarget.OfferMessage(messageHeader, message, source, consumeToAccept);
                }
                else
                {
                    return DataflowMessageStatus.Declined;
                }
            }

            public override string ToString()
            {
                return string.Concat("[Enable wrapped] ", base.ToString());
            }
        }

        private readonly Regex enableBehaviourPattern;
        private readonly Regex disableBehaviourPattern;

        private readonly IDictionary<string, EnableableBehaviourDecorator> behaviourMap;

        public ToggleEnableBehaviour()
        {
            this.behaviourMap = new Dictionary<string, EnableableBehaviourDecorator>();

            // TODO: refactor to inject identity match logic.
            var identityMatch = @"(mofichan|mofi)";

            this.enableBehaviourPattern = new Regex(
                string.Format(@"{0},? enable (?<behaviour>\w+) behaviour", identityMatch),
                RegexOptions.IgnoreCase);

            this.disableBehaviourPattern = new Regex(
                string.Format(@"{0},? disable (?<behaviour>\w+) behaviour", identityMatch),
                RegexOptions.IgnoreCase);
        }

        public override void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            base.InspectBehaviourStack(stack);

            this.behaviourMap.Clear();

            /*
             * We wrap each behaviour (apart from this one) inside a decorator
             * that allows bypassing that behaviour if instructed.
             */
            for (var i = 0; i < stack.Count; i++)
            {
                var behaviour = stack[i];

                if (behaviour.Id == AdministrationBehaviour.AdministrationBehaviourId)
                {
                    continue;
                }

                var wrappedBehaviour = new EnableableBehaviourDecorator(behaviour);
                this.behaviourMap[behaviour.Id] = wrappedBehaviour;
                stack[i] = wrappedBehaviour;
            }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            var fromUser = message.Context.From as IUser;
            var body = message.Context.Body;

            var isRequestValid = this.enableBehaviourPattern.IsMatch(body) ||
                                 this.disableBehaviourPattern.IsMatch(body);

            var isUserAuthorised = fromUser?.Type == UserType.Adminstrator;

            return isRequestValid && isUserAuthorised;
        }

        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            Debug.Assert((message.Context.From as IUser).Type == UserType.Adminstrator);

            var body = message.Context.Body;

            Match enableBehaviourMatch = this.enableBehaviourPattern.Match(body);
            Match disableBehaviourMatch = this.disableBehaviourPattern.Match(body);

            EnableableBehaviourDecorator decoratedBehaviour;
            if (enableBehaviourMatch.Success)
            {
                string behaviour = enableBehaviourMatch.Groups["behaviour"].Value;

                if (this.behaviourMap.TryGetValue(behaviour, out decoratedBehaviour))
                {
                    decoratedBehaviour.Enabled = true;
                    var reply = string.Format("'{0}' behaviour is now enabled", behaviour);
                    this.SendUpstream(message.Reply(reply));
                }
                else
                {
                    this.HandleNonExistentBehaviour(behaviour, "enabled", message);
                }
            }
            else if (disableBehaviourMatch.Success)
            {
                string behaviour = disableBehaviourMatch.Groups["behaviour"].Value;

                if (this.behaviourMap.TryGetValue(behaviour, out decoratedBehaviour))
                {
                    decoratedBehaviour.Enabled = false;
                    var reply = string.Format("'{0}' behaviour is now disabled", behaviour);
                    this.SendUpstream(message.Reply(reply));
                }
                else
                {
                    this.HandleNonExistentBehaviour(behaviour, "disabled", message);
                }
            }
        }

        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return false;
        }

        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private void HandleNonExistentBehaviour(string behaviour, string action, IncomingMessage message)
        {
            var from = message.Context.From as IUser;
            Debug.Assert(from != null);

            var reply = string.Format("I'm afraid behaviour '{0}' doesn't exist or can't be {1}, {2}",
                behaviour, action, from.Name);

            this.SendUpstream(message.Reply(reply));
        }
    }
}
