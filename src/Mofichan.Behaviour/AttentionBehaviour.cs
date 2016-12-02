using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Serilog;

namespace Mofichan.Behaviour
{
    // TODO
    /* This behaviour should ultimately be solely responsible for controlling Mofichan's
     * attention to particular users.
     * 
     * This will probably involve creating a "BotContext" class shared globally among all
     * behaviours that stores global bot context information. Among other things, this context
     * will keep track of which users Mofichan should be paying attention to.
     * 
     * This property will be controlled by this behaviour and queried by other behaviours.
     */

    /// <summary>
    /// A type of <see cref="IMofichanBehaviour"/> that is responsible for managing
    /// her attention to particular users.
    /// </summary>
    public sealed class AttentionBehaviour : BaseFlowReflectionBehaviour
    {
        private static readonly string AttentionPattern = @"^\s*" + Constants.IdentityMatch + @"[\s?!.]*$";

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttentionBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">The response builder factory.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger.</param>
        public AttentionBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            ILogger logger)
            : base("S0", responseBuilderFactory, flowManager, logger)
        {
            this.logger = logger.ForContext<AttentionBehaviour>();
            this.RegisterSimpleNode("STerm");
            this.RegisterSimpleTransition("T0,Term", "S0", "STerm");
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
            var messageBody = context.Message.Body;

            if (Regex.IsMatch(messageBody, AttentionPattern, RegexOptions.IgnoreCase))
            {
                manager.MakeTransitionCertain("T0,1");
            }
            else
            {
                manager.MakeTransitionCertain("T0,Term");
            }
        }

        /// <summary>
        /// Called when a user gets Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T0,1", from: "S0", to: "S1")]
        public void OnAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var response = this.ResponseBuilder
                .UsingContext(context.Message)
                .FromAnyOf("hm?", "yes?", "hi?")
                .FromTags("emote,inquisitive")
                .Build();

            this.Respond(context, response);
        }

        /// <summary>
        /// Represents the state while a user holds Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowState(id: "S1", distinctUntilChanged: true)]
        public void GainedAttention(FlowContext context, IFlowTransitionManager manager)
        {
            manager.MakeTransitionCertain("T1,1");
        }

        /// <summary>
        /// Called continuously as Mofichan's attention towards a particular user ebbs.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T1,1", from: "S1", to: "S1")]
        public void LosingAttention(FlowContext context, IFlowTransitionManager manager)
        {
            manager["T1,1"] -= 0.0001;
            manager["T1,Term"] = 1 - manager["T1,1"];
        }

        /// <summary>
        /// Called when a user loses Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T1,Term", from: "S1", to: "STerm")]
        public void LostAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var user = context.Message.From as IUser;
            this.logger.Debug("Mofichan stopped paying attention to {User}", user.Name);
        }

        // TODO: refactor to eliminate duplication of this method.
        private void Respond(FlowContext context, string responseBody)
        {
            var sender = context.Message.From as IUser;
            var responseContext = new MessageContext(from: null, to: sender, body: responseBody);
            var response = new OutgoingMessage { Context = responseContext };
            context.GeneratedResponseHandler(response);
        }
    }
}
