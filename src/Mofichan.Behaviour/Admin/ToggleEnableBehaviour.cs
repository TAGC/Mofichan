﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Serilog;

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
    internal class ToggleEnableBehaviour : BaseFlowReflectionBehaviour
    {
        private static readonly string EnableMatch = @"enable (?<behaviour>\w+) behaviour";
        private static readonly string DisableMatch = @"disable (?<behaviour>\w+) behaviour";

        private readonly ILogger logger;
        private readonly IDictionary<string, EnableableBehaviourDecorator> behaviourMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleEnableBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder" />.</param>
        /// <param name="transitionManagerFactory">The transition manager factory.</param>
        /// <param name="flowDriver">The flow driver.</param>
        /// <param name="flowTransitionSelector">The flow transition selector.</param>
        /// <param name="logger">The logger to use.</param>
        public ToggleEnableBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory,
            IFlowDriver flowDriver,
            IFlowTransitionSelector flowTransitionSelector,
            ILogger logger)
            : base("S0", responseBuilderFactory, transitionManagerFactory, flowDriver, flowTransitionSelector, logger)
        {
            this.logger = logger.ForContext<ToggleEnableBehaviour>();
            this.behaviourMap = new Dictionary<string, EnableableBehaviourDecorator>();

            this.RegisterSimpleNode("STerm");
            this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
            this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
            this.RegisterSimpleTransition("T0,Term", from: "S0", to: "STerm");
            this.Configure();
        }

        /// <summary>
        /// Represents the idle flow state.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowState(id: "S0")]
        public void Idle(FlowContext context, IFlowTransitionManager manager)
        {
            var tags = context.Message.Tags;

            if (tags.Contains("directedAtMofichan"))
            {
                manager.MakeTransitionCertain("T0,1");
            }
            else
            {
                manager.MakeTransitionCertain("T0,Term");
            }
        }

        /// <summary>
        /// Represents the state while a user holds Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowState(id: "S1", distinctUntilChanged: true)]
        public void WithAttention(FlowContext context, IFlowTransitionManager manager)
        {
            manager.ClearTransitionWeights();
            manager["T1,1"] = 0.995;
            manager["T1,Term:timeout"] = 1 - manager["T1,1"];

            var messageBody = context.Message.Body;
            bool authorised = (context.Message.From as IUser)?.Type == UserType.Adminstrator;
            bool enableRequest = Regex.IsMatch(messageBody, EnableMatch);
            bool disableRequest = Regex.IsMatch(messageBody, DisableMatch);

            if (enableRequest && authorised)
            {
                var response = this.ChangeBehaviourEnableState(context.Message, EnableMatch, "enabled", true);
                context.GeneratedResponseHandler(response);
            }
            else if (disableRequest && authorised)
            {
                var response = this.ChangeBehaviourEnableState(context.Message, DisableMatch, "disabled", false);
                context.GeneratedResponseHandler(response);
            }
            else if (enableRequest && !authorised)
            {
                throw new MofichanAuthorisationException(
                    "Non-admin user attempted to enable behaviour",
                    context.Message);
            }
            else if (disableRequest && !authorised)
            {
                throw new MofichanAuthorisationException(
                    "Non-admin user attempted to disable behaviour",
                    context.Message);
            }
        }

        /// <summary>
        /// Called when a user loses Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T1,Term:timeout", from: "S1", to: "STerm")]
        public void OnLostAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var user = context.Message.From as IUser;
            this.logger.Debug("Mofichan stopped paying attention to {User}", user.Name);
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

        private static OutgoingMessage HandleNonExistentBehaviour(string behaviour, string action, MessageContext messageContext)
        {
            var from = messageContext.From as IUser;
            Debug.Assert(from != null, "The message sender should be a user");

            var reply = string.Format("I'm afraid behaviour '{0}' doesn't exist or can't be {1}, {2}",
                behaviour, action, from.Name);

            return messageContext.Reply(reply);
        }

        private OutgoingMessage ChangeBehaviourEnableState(
            MessageContext messageContext, string pattern, string enableStateName, bool enableState)
        {
            var match = Regex.Match(messageContext.Body, pattern, RegexOptions.IgnoreCase);

            string behaviour = match.Groups["behaviour"].Value;

            EnableableBehaviourDecorator decoratedBehaviour;
            if (this.behaviourMap.TryGetValue(behaviour, out decoratedBehaviour))
            {
                string reply;

                if (decoratedBehaviour.Enabled == enableState)
                {
                    reply = string.Format("'{0}' behaviour is already {1}", behaviour, enableStateName);
                }
                else
                {
                    decoratedBehaviour.Enabled = enableState;
                    reply = string.Format("'{0}' behaviour is now {1}", behaviour, enableStateName);
                }

                return messageContext.Reply(reply);
            }
            else
            {
                return HandleNonExistentBehaviour(behaviour, enableStateName, messageContext);
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
