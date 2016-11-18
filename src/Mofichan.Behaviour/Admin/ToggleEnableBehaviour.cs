using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.FilterAttributes;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with the administrative ability
    /// to control whether other modules are active.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will allow Mofichan dynamically enable or disable
    /// other behaviour modules, but not add or remove them.
    /// <para></para>
    /// Certain modules (such as the "administrator" module) cannot be enabled or disabled.
    /// </remarks>
    internal class ToggleEnableBehaviour : BaseReflectionBehaviour
    {
        private const RegexOptions MatchOptions = RegexOptions.IgnoreCase;
        private const string IdentityMatch = @"(mofichan|mofi)";
        private const string EnableMatch = @"enable (?<behaviour>\w+) behaviour";
        private const string DisableMatch = @"disable (?<behaviour>\w+) behaviour";
        private const string FullEnableMatch = IdentityMatch + ",? " + EnableMatch;
        private const string FullDisableMatch = IdentityMatch + ",? " + DisableMatch;
        private readonly IDictionary<string, EnableableBehaviourDecorator> behaviourMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleEnableBehaviour"/> class.
        /// </summary>
        public ToggleEnableBehaviour()
        {
            this.behaviourMap = new Dictionary<string, EnableableBehaviourDecorator>();
        }

        /// <summary>
        /// Attempts to enable a behaviour based on its ID.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <returns>A response based on the success or failure of the operation.</returns>
        [RegexIncomingMessageFilter(FullEnableMatch, MatchOptions)]
        [AuthorisationIncomingMessageFilter(
            requiredUserType: UserType.Adminstrator,
            onFailure: "Non-admin user attempted to enable behaviour")]
        public OutgoingMessage? EnableBehaviour(IncomingMessage message)
        {
            return this.ChangeBehaviourEnableState(message, FullEnableMatch, "enabled", true);
        }

        /// <summary>
        /// Attempts to disable a behaviour based on its ID.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        /// <returns>A response based on the success or failure of the operation.</returns>
        [RegexIncomingMessageFilter(FullDisableMatch, MatchOptions)]
        [AuthorisationIncomingMessageFilter(
            requiredUserType: UserType.Adminstrator,
            onFailure: "Non-admin user attempted to disable behaviour")]
        public OutgoingMessage? DisableBehaviour(IncomingMessage message)
        {
            return this.ChangeBehaviourEnableState(message, FullDisableMatch, "disabled", false);
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

        private static OutgoingMessage HandleNonExistentBehaviour(string behaviour, string action, IncomingMessage message)
        {
            var from = message.Context.From as IUser;
            Debug.Assert(from != null, "The message sender should be a user");

            var reply = string.Format("I'm afraid behaviour '{0}' doesn't exist or can't be {1}, {2}",
                behaviour, action, from.Name);

            return message.Reply(reply);
        }

        private OutgoingMessage ChangeBehaviourEnableState(
            IncomingMessage message, string pattern, string enableStateName, bool enableState)
        {
            var match = Regex.Match(message.Context.Body, pattern, MatchOptions);

            string behaviour = match.Groups["behaviour"].Value;

            EnableableBehaviourDecorator decoratedBehaviour;
            if (this.behaviourMap.TryGetValue(behaviour, out decoratedBehaviour))
            {
                decoratedBehaviour.Enabled = enableState;
                var reply = string.Format("'{0}' behaviour is now {1}", behaviour, enableStateName);
                return message.Reply(reply);
            }
            else
            {
                return HandleNonExistentBehaviour(behaviour, enableStateName, message);
            }
        }

        private class EnableableBehaviourDecorator : BaseBehaviourDecorator
        {
            private const string Tick = "✓";
            private const string Cross = "⨉";

            private IObserver<IncomingMessage> downstreamObserver;
            private IObserver<OutgoingMessage> upstreamObserver;

            public EnableableBehaviourDecorator(IMofichanBehaviour delegateBehaviour)
                : base(delegateBehaviour)
            {
                this.Enabled = true;
            }

            public bool Enabled { get; set; }

            public override IDisposable Subscribe(IObserver<IncomingMessage> observer)
            {
                this.downstreamObserver = observer;
                return base.Subscribe(observer);
            }

            public override IDisposable Subscribe(IObserver<OutgoingMessage> observer)
            {
                this.upstreamObserver = observer;
                return base.Subscribe(observer);
            }

            public override void OnNext(IncomingMessage message)
            {
                if (this.Enabled)
                {
                    base.OnNext(message);
                }
                else if (this.downstreamObserver != null)
                {
                    this.downstreamObserver.OnNext(message);
                }
            }

            public override void OnNext(OutgoingMessage message)
            {
                if (this.Enabled)
                {
                    base.OnNext(message);
                }
                else if (this.downstreamObserver != null)
                {
                    this.upstreamObserver.OnNext(message);
                }
            }

            public override string ToString()
            {
                var baseRepr = base.ToString().Trim('[', ']');
                return string.Format("[{0} {1}]", baseRepr, this.Enabled ? Tick : Cross);
            }
        }
    }
}
