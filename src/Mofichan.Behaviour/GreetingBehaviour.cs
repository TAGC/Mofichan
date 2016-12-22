using System;
using System.Diagnostics;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Serilog;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with greeting-related functionality.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will allow Mofichan to respond to people greeting her.
    /// </remarks>
    public sealed class GreetingBehaviour : BaseFlowReflectionBehaviour
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingBehaviour" /> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        public GreetingBehaviour(BotContext botContext, IFlowManager flowManager, ILogger logger)
            : base("S0", botContext, flowManager, logger)
        {
            this.RegisterSimpleNode("STerm");
            this.RegisterAttentionGuardNode("S0", "T0,1", "T0,Term");
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
        [FlowState(id: "S1", distinctUntilChanged: true)]
        public void WithAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var tags = context.Message.Tags;
            var user = context.Message.From as IUser;
            Debug.Assert(user != null, "Message should be from user");

            if (tags.Contains("wellbeing"))
            {
                context.Visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "wellbeing,phrase" })
                        .FromTags("emote,happy", "emote,cute"))
                    .WithSideEffect(() => this.BotContext.Attention.RenewAttentionTowardsUser(user))
                    .RelevantBecause(it => it.SuitsMessageTags("wellbeing"))
                    .Build());
            }
            else if (tags.Contains("greeting"))
            {
                context.Visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "greeting,phrase" })
                        .FromTags("emote,greeting", "emote,cute"))
                    .WithSideEffect(() => this.BotContext.Attention.RenewAttentionTowardsUser(user))
                    .RelevantBecause(it => it.SuitsMessageTags("greeting"))
                    .Build());
            }

            manager.MakeTransitionCertain("T1,Term");
        }
    }
}
