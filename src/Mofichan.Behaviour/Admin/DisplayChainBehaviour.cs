using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
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

        private readonly ILogger logger;

        private IList<IMofichanBehaviour> behaviourStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayChainBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder" />.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        public DisplayChainBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            ILogger logger)
            : base("S0", responseBuilderFactory, flowManager, logger)
        {
            this.logger = logger.ForContext<DisplayChainBehaviour>();

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
            var messageBody = context.Message.Body;
            bool authorised = (context.Message.From as IUser)?.Type == UserType.Adminstrator;
            bool displayChainRequest = Regex.IsMatch(messageBody, DisplayChainMatch, RegexOptions.IgnoreCase);

            manager.ClearTransitionWeights();
            manager["T1,1"] = 0.998;
            manager["T1,Term:timeout"] = 1 - manager["T1,1"];

            if (displayChainRequest && authorised)
            {
                var response = context.Message.Reply("My behaviour chain: " + this.BuildBehaviourChainRepresentation());
                context.GeneratedResponseHandler(response);
            }
            else if (displayChainRequest)
            {
                throw new MofichanAuthorisationException(
                    "Non-admin user attempted to display behaviour chain",
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
