using System;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.Flow;
using Mofichan.Core.Visitor;
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
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger to use.</param>
        protected BaseFlowBehaviour(
            string startNodeId,
            IFlowManager flowManager,
            ILogger logger)
        {
            this.startNodeId = startNodeId;
            this.flowManager = flowManager;
            this.logger = logger.ForContext<BaseFlowBehaviour>();
        }

        private IFlow Flow { get; set; }

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor" />.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected override void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            this.Flow.Accept(visitor);
            base.HandleMessageVisitor(visitor);
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
            this.Flow.Accept(visitor);
            base.HandlePulseVisitor(visitor);
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
                .WithStartNodeId(this.startNodeId);

            this.Flow = flowBuilderFunction(baseFlowBuilder).Build();
        }
    }
}
