using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
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
    internal class ToggleEnableBehaviour : BaseBehaviour
    {
        private readonly Regex enableBehaviourPattern;
        private readonly Regex disableBehaviourPattern;

        private readonly IDictionary<string, EnableableBehaviourDecorator> behaviourMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleEnableBehaviour"/> class.
        /// </summary>
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

            return this.enableBehaviourPattern.IsMatch(body) ||
                this.disableBehaviourPattern.IsMatch(body);
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
            message.Context.CheckSenderAuthorised("Non-admin user attempted to change behaviour enable state");

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
        /// Handles the non existent behaviour.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        private void HandleNonExistentBehaviour(string behaviour, string action, IncomingMessage message)
        {
            var from = message.Context.From as IUser;
            Debug.Assert(from != null, "The message sender should be a user");

            var reply = string.Format("I'm afraid behaviour '{0}' doesn't exist or can't be {1}, {2}",
                behaviour, action, from.Name);

            this.SendUpstream(message.Reply(reply));
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
