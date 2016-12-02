using System;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A type of <see cref="IMofichanBehaviour"/> that allows aspects of Mofichan's
    /// behaviour to be modelled as discrete-time stochastic processes.
    /// </summary>
    /// <seealso cref="IFlow"/>
    public abstract class BaseFlowBehaviour : BaseBehaviour
    {
        private readonly IFlowManager flowManager;
        private readonly ILogger logger;
        private readonly string startNodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlowBehaviour" /> class.
        /// </summary>
        /// <param name="startNodeId">Identifies the starting node in the flow.</param>
        /// <param name="responseBuilderFactory">The response builder factory.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="passThroughMessages">If set to <c>true</c>, passes through unhandled messages.</param>
        protected BaseFlowBehaviour(
            string startNodeId,
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            ILogger logger,
            bool passThroughMessages = true)
            : base(responseBuilderFactory, passThroughMessages)
        {
            this.startNodeId = startNodeId;
            this.flowManager = flowManager;
            this.logger = logger.ForContext<BaseFlowBehaviour>();
        }

        private IFlow Flow { get; set; }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public override void OnCompleted()
        {
            base.OnCompleted();
            this.Flow?.Dispose();
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
            return this.Flow != null;
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
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            this.Flow.Accept(message);
            this.SendDownstream(message);
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
        /// Registers the flow used by this instance.
        /// </summary>
        /// <param name="flowBuilderFunction">
        /// Determines the properties of the flow that gets registered.
        /// </param>
        protected void RegisterFlow(Func<BasicFlow.Builder, BasicFlow.Builder> flowBuilderFunction)
        {
            var baseFlowBuilder = new BasicFlow.Builder()
                .WithLogger(this.logger)
                .WithManager(this.flowManager)
                .WithGeneratedResponseHandler(this.SendUpstream)
                .WithStartNodeId(this.startNodeId);

            this.Flow = flowBuilderFunction(baseFlowBuilder).Build();
        }
    }
}
