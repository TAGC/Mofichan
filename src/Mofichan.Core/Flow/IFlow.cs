using System;
using Mofichan.Core.Visitor;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// Represents a logical behaviour flow.
    /// </summary>
    public interface IFlow
    {
        /// <summary>
        /// Allows this flow to modify its state and potentially generate responses
        /// using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void Accept(IBehaviourVisitor visitor);
    }

    /// <summary>
    /// Represents a state within a behavioural flow.
    /// </summary>
    public interface IFlowNode
    {
        /// <summary>
        /// Gets the flow node identifier.
        /// </summary>
        /// <value>
        /// The flow node identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets a value indicating whether this node is active within the flow.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        bool IsActive { get; }

        /// <summary>
        /// Connects this node to another node using a specified transition.
        /// </summary>
        /// <param name="node">The node to connect to.</param>
        /// <param name="transition">The transition to connect with.</param>
        void Connect(IFlowNode node, IFlowTransition transition);

        /// <summary>
        /// Allows this flow node to modify its state based on contextual flow state
        /// information.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        void Accept(FlowContext flowContext);

        /// <summary>
        /// Invokes a transition of a flow to this node.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        void TransitionTo(FlowContext flowContext);

        /// <summary>
        /// Allows this node to respond to a logical clock tick.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        void OnTick(FlowContext flowContext);

        /// <summary>
        /// Determines whether this is the suitable node to select as "current" for the specified
        /// message context.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        /// <returns>
        ///   <c>true</c> if this is the "current" node for the message context; otherwise, <c>false</c>.
        /// </returns>
        bool IsCurrentNodeForMessageContext(MessageContext messageContext);
    }

    /// <summary>
    /// Represents a transition between two nodes in a behavioural flow.
    /// </summary>
    public interface IFlowTransition
    {
        /// <summary>
        /// Gets the flow transition identifier.
        /// </summary>
        /// <value>
        /// The flow transition identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets any action that should be invoked as this transition occurs.
        /// </summary>
        /// <value>
        /// The transition action.
        /// </value>
        Action<FlowContext, IFlowTransitionManager> Action { get; }

        /// <summary>
        /// Gets or sets the clock for this transition, representing the number of
        /// ticks until this transition should occur.
        /// </summary>
        /// <value>
        /// The transition clock.
        /// </value>
        int Clock { get; set; }
    }
}
