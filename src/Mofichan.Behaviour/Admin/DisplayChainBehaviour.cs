using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.BotState;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Serilog;

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
    public class DisplayChainBehaviour : BaseFlowReflectionBehaviour
    {
        private const string BehaviourChainConnector = " ⇄ ";

        private static readonly string DisplayChainMatch = @"(display|show( your)?) behaviour chain";

        private IList<IMofichanBehaviour> behaviourStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayChainBehaviour" /> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        public DisplayChainBehaviour(BotContext botContext, IFlowManager flowManager, ILogger logger)
            : base("S0", botContext, flowManager, logger)
        {
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
        /// Thrown if non-admin user attempts to display behaviour chain
        /// </exception>
        [FlowState(id: "S1", distinctUntilChanged: true)]
        public void WithAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var messageBody = context.Message.Body;
            var user = context.Message.From as IUser;
            Debug.Assert(user != null, "The message should be from a user");

            bool authorised = user.Type == UserType.Adminstrator;
            bool displayChainRequest = Regex.IsMatch(messageBody, DisplayChainMatch, RegexOptions.IgnoreCase);

            manager.MakeTransitionCertain("T1,Term");

            if (displayChainRequest && authorised)
            {
                context.Visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromRaw("My behaviour chain: " + this.BuildBehaviourChainRepresentation()))
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                    .RelevantBecause(it => it.GuaranteesRelevance()));
            }
            else if (displayChainRequest)
            {
                var exception = new MofichanAuthorisationException(
                    "Non-admin user attempted to display behaviour chain", context.Message);

                context.Visitor.RegisterResponse(rb => rb
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                    .WithSideEffect(() => { throw exception; }));
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
