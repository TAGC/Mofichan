using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.BotState;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
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

        private readonly IDictionary<string, EnableableBehaviourDecorator> behaviourMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleEnableBehaviour" /> class.
        /// </summary>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="botContext">The bot context.</param>
        /// <param name="logger">The logger to use.</param>
        public ToggleEnableBehaviour(BotContext botContext, IFlowManager flowManager, ILogger logger)
            : base("S0", botContext, flowManager, logger)
        {
            this.behaviourMap = new Dictionary<string, EnableableBehaviourDecorator>();

            this.RegisterSimpleNode("STerm");
            this.RegisterAttentionGuardNode("S0", "T0,1", "T0,Term");
            this.RegisterAttentionGuardTransition("T1,1:wait", "T1,Term", "S1", "S1");
            this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
            this.RegisterSimpleTransition("T0,Term", from: "S0", to: "STerm");
            this.RegisterSimpleTransition("T1,Term", from: "S1", to: "STerm");
            this.Configure();
        }

        /// <summary>
        /// Represents the state while a user holds Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        /// <exception cref="MofichanAuthorisationException">
        /// Non-admin user attempted to enable behaviour
        /// or
        /// Non-admin user attempted to disable behaviour
        /// </exception>
        [FlowState(id: "S1")]
        public void WithAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var messageBody = context.Message.Body;
            var user = context.Message.From as IUser;

            Debug.Assert(user != null, "The message should be from a user");

            bool authorised = (context.Message.From as IUser)?.Type == UserType.Adminstrator;
            bool enableRequest = Regex.IsMatch(messageBody, EnableMatch, RegexOptions.IgnoreCase);
            bool disableRequest = Regex.IsMatch(messageBody, DisableMatch, RegexOptions.IgnoreCase);

            manager.MakeTransitionCertain("T1,Term");

            if (enableRequest && authorised)
            {
                this.ChangeBehaviourEnableState(context.Visitor, context.Message, EnableMatch, "enabled", true);
            }
            else if (disableRequest && authorised)
            {
                this.ChangeBehaviourEnableState(context.Visitor, context.Message, DisableMatch, "disabled", false);
            }
            else if (enableRequest && !authorised)
            {
                HandleAuthorisationFailure(context.Visitor, context.Message,
                    "Non-admin user attempted to enable behaviour");
            }
            else if (disableRequest && !authorised)
            {
                HandleAuthorisationFailure(context.Visitor, context.Message,
                    "Non-admin user attempted to disable behaviour");
            }
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

        private static void HandleAuthorisationFailure(IBehaviourVisitor visitor, MessageContext incomingMessage,
            string exceptionMessage)
        {
            var exception = new MofichanAuthorisationException(exceptionMessage, incomingMessage);
            var user = incomingMessage.From as IUser;
            Debug.Assert(user != null, "The message sender should be a user");

            visitor.RegisterResponse(rb => rb
                .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                .WithSideEffect(() => { throw exception; }));
        }

        private static void HandleNonExistentBehaviour(IBehaviourVisitor visitor, MessageContext incomingMessage,
            string behaviour, string action)
        {
            var user = incomingMessage.From as IUser;
            Debug.Assert(user != null, "The message sender should be a user");

            var reply = string.Format("I'm afraid behaviour '{0}' doesn't exist or can't be {1}, {2}",
                behaviour, action, user.Name);

            visitor.RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw(reply))
                .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                .RelevantBecause(it => it.GuaranteesRelevance()));
        }

        private void ChangeBehaviourEnableState(IBehaviourVisitor visitor, MessageContext incomingMessage,
            string pattern, string enableStateName, bool enableState)
        {
            var user = incomingMessage.From as IUser;
            var match = Regex.Match(incomingMessage.Body, pattern, RegexOptions.IgnoreCase);
            string behaviour = match.Groups["behaviour"].Value;
            EnableableBehaviourDecorator decoratedBehaviour;

            Debug.Assert(user != null, "The message should be from a user");

            if (this.behaviourMap.TryGetValue(behaviour, out decoratedBehaviour))
            {
                string reply;
                Action enableStateChange = () => { };

                if (decoratedBehaviour.Enabled == enableState)
                {
                    reply = string.Format("'{0}' behaviour is already {1}", behaviour, enableStateName);
                }
                else
                {
                    enableStateChange = () => decoratedBehaviour.Enabled = enableState;
                    reply = string.Format("'{0}' behaviour is now {1}", behaviour, enableStateName);
                }

                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb.FromRaw(reply))
                    .WithSideEffect(enableStateChange)
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                    .RelevantBecause(it => it.GuaranteesRelevance()));
            }
            else
            {
                HandleNonExistentBehaviour(visitor, incomingMessage, behaviour, enableStateName);
            }
        }

        private class EnableableBehaviourDecorator : BaseBehaviourDecorator
        {
            private const string Tick = "✓";
            private const string Cross = "⨉";

            private IObserver<IBehaviourVisitor> observer;

            public EnableableBehaviourDecorator(IMofichanBehaviour delegateBehaviour)
                : base(delegateBehaviour)
            {
                this.Enabled = true;
            }

            public bool Enabled { get; set; }

            public override IDisposable Subscribe(IObserver<IBehaviourVisitor> observer)
            {
                this.observer = observer;
                return base.Subscribe(observer);
            }

            public override void OnNext(IBehaviourVisitor visitor)
            {
                if (this.Enabled)
                {
                    base.OnNext(visitor);
                }
                else if (this.observer != null)
                {
                    this.observer.OnNext(visitor);
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
