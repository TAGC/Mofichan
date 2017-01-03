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
using static Mofichan.Core.Flow.UserDrivenFlowManager;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with the administrative ability
    /// to sleep and reawaken.
    /// </summary>
    /// <remarks>
    /// While Mofichan is asleep, all behaviours in the chain below <see cref="SleepHelper"/> will
    /// stop receiving <see cref="IBehaviourVisitor"/>, causing Mofichan to stop responding to
    /// messages and pausing most background state changes.
    /// </remarks>
    internal class SleepBehaviour : BaseBehaviour
    {
        private readonly FlowBlocker flowBlocker;
        private readonly SleepHelper sleepHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SleepBehaviour"/> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="logger">The logger.</param>
        public SleepBehaviour(BotContext botContext, ILogger logger)
        {
            this.flowBlocker = new FlowBlocker();
            this.sleepHelper = new SleepHelper(this.flowBlocker, botContext, logger);
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
            stack.Insert(0, this.sleepHelper);
            stack.Insert(1, this.flowBlocker);
        }

        private class FlowBlocker : BaseBehaviour
        {
            public bool FlowBlocked { get; set; }

            /// <summary>
            /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor" />.
            /// <para></para>
            /// Subclasses should call the base implementation of this method to pass the
            /// visitor downstream.
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            protected override void HandleMessageVisitor(OnMessageVisitor visitor)
            {
                if (!this.FlowBlocked)
                {
                    base.HandleMessageVisitor(visitor);
                }
            }

            /// <summary>
            /// Invoked when this behaviour is visited by an <see cref="OnPulseVisitor" />.
            /// <para></para>
            /// Subclasses should call the base implementation of this method to pass the
            /// visitor downstream.
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            protected override void HandlePulseVisitor(OnPulseVisitor visitor)
            {
                if (!this.FlowBlocked)
                {
                    base.HandlePulseVisitor(visitor);
                }
            }
        }

        private class SleepHelper : BaseFlowReflectionBehaviour
        {
            private static readonly string SleepMatch = @"(sleep|deep and dreamless slumber)";
            private static readonly string AwakenMatch = @"(wake|reawaken)";
            private readonly FlowBlocker flowBlocker;

            public SleepHelper(FlowBlocker flowBlocker, BotContext botContext, ILogger logger)
                : base("S0", botContext, logger)
            {
                this.flowBlocker = flowBlocker;
                this.RegisterSimpleNode("STerm");
                this.RegisterAttentionGuardNode("S0", "T0,1", "T0,Term");
                this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
                this.RegisterSimpleTransition("T0,Term", from: "S0", to: "STerm");
                this.RegisterSimpleTransition("T1,Term", from: "S1", to: "STerm");
                this.Configure<UserDrivenFlow>(Create);
            }

            private bool MofiSleeping
            {
                get
                {
                    return this.flowBlocker.FlowBlocked;
                }

                set
                {
                    this.flowBlocker.FlowBlocked = value;
                }
            }

            /// <summary>
            /// Represents the state while a user holds Mofichan's attention.
            /// </summary>
            /// <param name="context">The flow context.</param>
            /// <param name="manager">The transition manager.</param>
            /// <param name="visitor">The visitor.</param>
            /// <exception cref="MofichanAuthorisationException">
            /// Thrown if non-admin user attempts to put Mofi to sleep or wake her up.
            /// </exception>
            [FlowState(id: "S1")]
            public void WithAttention(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                var messageBody = context.Message.Body;
                var user = context.Message.From as IUser;

                Debug.Assert(user != null, "The message should be from a user");

                bool authorised = user.Type == UserType.Adminstrator;
                bool sleepRequest = Regex.IsMatch(messageBody, SleepMatch, RegexOptions.IgnoreCase);
                bool awakenRequest = Regex.IsMatch(messageBody, AwakenMatch, RegexOptions.IgnoreCase);

                manager.MakeTransitionCertain("T1,Term");

                if (sleepRequest && !this.MofiSleeping && authorised)
                {
                    visitor.RegisterResponse(rb => rb
                        .To(context.Message)
                        .WithMessage(mb => mb.FromRaw("*Goes to sleep*"))
                        .WithSideEffect(() => this.MofiSleeping = true)
                        .RelevantBecause(it => it.GuaranteesRelevance()));
                }
                else if (awakenRequest && this.MofiSleeping && authorised)
                {
                    visitor.RegisterResponse(rb => rb
                        .To(context.Message)
                        .WithMessage(mb => mb.FromRaw("*Wakes up*"))
                        .WithSideEffect(() => this.MofiSleeping = false)
                        .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                        .RelevantBecause(it => it.GuaranteesRelevance()));
                }
                else if (awakenRequest && !this.MofiSleeping && authorised)
                {
                    visitor.RegisterResponse(rb => rb
                        .To(context.Message)
                        .WithMessage(mb => mb.FromRaw("I'm already awake."))
                        .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                        .RelevantBecause(it => it.GuaranteesRelevance()));
                }
                else if (sleepRequest && !this.MofiSleeping && !authorised)
                {
                    HandleAuthorisationFailure(visitor, context.Message,
                        "Non-admin user attempted to put Mofichan to sleep");
                }
                else if (awakenRequest && !this.MofiSleeping  && !authorised)
                {
                    HandleAuthorisationFailure(visitor, context.Message,
                        "Non-admin user attempted to awaken Mofichan");
                }
            }

            private static void HandleAuthorisationFailure(IBehaviourVisitor visitor, MessageContext incomingMessage,
                string exceptionMessage)
            {
                var exception = new MofichanAuthorisationException(exceptionMessage, incomingMessage);
                var user = incomingMessage.From as IUser;
                Debug.Assert(user != null, "The message sender should be a user");

                visitor.RegisterResponse(rb => rb
                    .To(incomingMessage)
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                    .WithSideEffect(() => { throw exception; }));
            }
        }
    }
}
