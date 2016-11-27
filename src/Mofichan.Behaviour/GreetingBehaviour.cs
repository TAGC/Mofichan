using System;
using System.Collections.Generic;
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
        /// <param name="transitionManagerFactory">The transition manager factory.</param>
        /// <param name="flowDriver">The flow driver.</param>
        /// <param name="flowTransitionSelector">The flow transition selector.</param>
        /// <param name="logger">The logger to use.</param>
        public GreetingBehaviour(
            Func<IResponseBuilder> responseBuilderFactory,
            Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory,
            IFlowDriver flowDriver,
            IFlowTransitionSelector flowTransitionSelector,
            ILogger logger)
            : base("S0", responseBuilderFactory, transitionManagerFactory, flowDriver, flowTransitionSelector, logger)
        {
            this.logger = logger.ForContext<GreetingBehaviour>();
            this.RegisterSimpleNode("STerm");
            this.RegisterSimpleTransition("T0,Term", from: "S0", to: "STerm");
            this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
            this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
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
            if (context.Message.Tags.Contains("directedAtMofichan"))
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
            var tags = context.Message.Tags;

            if (tags.Contains("wellbeing"))
            {
                manager.MakeTransitionCertain("T1,1:wellbeing");
            }
            else if (tags.Contains("greeting"))
            {
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

        private static void ConfigureForEventualFlowTermination(IFlowTransitionManager manager)
        {
            manager.ClearTransitionWeights();
            manager["T1,1"] = 0.998;
            manager["T1,Term:timeout"] = 1 - manager["T1,1"];
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
