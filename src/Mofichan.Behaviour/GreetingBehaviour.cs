using System;
using System.Diagnostics;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
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
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder" />.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        public GreetingBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            ILogger logger)
            : base("S0", responseBuilderFactory, flowManager, logger)
        {
            this.logger = logger.ForContext<GreetingBehaviour>();
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
        [FlowState(id: "S1", distinctUntilChanged: true)]
        public void WithAttention(FlowContext context, IFlowTransitionManager manager)
        {
            var tags = context.Message.Tags;
            var user = context.Message.From as IUser;
            Debug.Assert(user != null, "Message should be from user");

            if (tags.Contains("wellbeing"))
            {
                context.Attention.RenewAttentionTowardsUser(user);
                manager.MakeTransitionCertain("T1,1:wellbeing");
            }
            else if (tags.Contains("greeting"))
            {
                context.Attention.RenewAttentionTowardsUser(user);
                manager.MakeTransitionCertain("T1,1:greeting");
            }
            else
            {
                ConfigureForEventualFlowTermination(manager);
            }
        }

        /// <summary>
        /// Called when Mofichan is greeted.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T1,1:greeting", from: "S1", to: "S1")]
        public void OnGreeted(FlowContext context, IFlowTransitionManager manager)
        {           
            ConfigureForEventualFlowTermination(manager);

            var response = this.ResponseBuilder
                .UsingContext(context.Message)
                .FromTags(prefix: string.Empty, tags: new[] { "greeting,phrase" })
                .FromTags("emote,greeting", "emote,cute")
                .Build();

            this.Respond(context, response);
        }

        /// <summary>
        /// Called when someone asks Mofichan how she's doing.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        [FlowTransition(id: "T1,1:wellbeing", from: "S1", to: "STerm")]
        public void OnWellbeingRequest(FlowContext context, IFlowTransitionManager manager)
        {
            ConfigureForEventualFlowTermination(manager);

            var response = this.ResponseBuilder
                .UsingContext(context.Message)
                .FromTags(prefix: string.Empty, tags: new[] { "wellbeing,phrase" })
                .FromTags("emote,happy", "emote,cute")
                .Build();

            this.Respond(context, response);
        }

        private static void ConfigureForEventualFlowTermination(IFlowTransitionManager manager)
        {
            manager.ClearTransitionWeights();
            manager.MakeTransitionCertain("T1,1:wait");
        }

        private void Respond(FlowContext context, string responseBody)
        {
            var sender = context.Message.From as IUser;
            var responseContext = new MessageContext(from: null, to: sender, body: responseBody);
            var response = new OutgoingMessage { Context = responseContext };
            context.GeneratedResponseHandler(response);
        }
    }
}
