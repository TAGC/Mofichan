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
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlowBehaviour" /> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        protected BaseFlowBehaviour(ILogger logger)
        {
            this.logger = logger.ForContext<BaseFlowBehaviour>();
        }

        private IFlowManager FlowManager { get; set; }

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor" />.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected override void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            this.FlowManager.Accept(visitor);
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
            this.FlowManager.Accept(visitor);
            base.HandlePulseVisitor(visitor);
        }

        /// <summary>
        /// Sets the flow manager.
        /// </summary>
        /// <param name="manager">The flow manager.</param>
        protected void SetFlowManager(IFlowManager manager)
        {
            this.FlowManager = manager;
        }
    }
}
