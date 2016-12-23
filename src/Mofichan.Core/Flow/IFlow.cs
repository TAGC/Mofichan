using System;
using Mofichan.Core.Visitor;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// An enumeration of the possible states of a node within a behavioural flow.
    /// </summary>
    public enum FlowNodeState
    {
        /// <summary>
        /// Represents a node that is inactive within its flow.
        /// </summary>
        Inactive,

        /// <summary>
        /// Represents a node that is active within its flow, but will not
        /// perform any processing when the flow receives new visitors.
        /// </summary>
        Dormant,

        /// <summary>
        /// Represents a node that is active within its flow and will perform
        /// processing when the flow receives new visitors.
        /// </summary>
        Active
    }

    /// <summary>
    /// Represents a logical behaviour flow.
    /// </summary>
    public interface IFlow
    {
        /// <summary>
        /// Gets a value indicating whether this flow is complete.
        /// </summary>
        /// <value>
        /// <c>true</c> if this flow is complete; otherwise, <c>false</c>.
        /// </value>
        bool IsComplete { get; }

        /// <summary>
        /// Allows this flow to modify its state and potentially generate responses
        /// using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void Accept(IBehaviourVisitor visitor);

        /// <summary>
        /// Creates a copy of this flow, which includes copies of all of its nodes, transitions and connections.
        /// </summary>
        /// <returns>A copy of this flow.</returns>
        IFlow Copy();
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
        /// Gets the current state of this node.
        /// </summary>
        /// <value>
        /// This node's current state.
        /// </value>
        FlowNodeState State { get; }

        /// <summary>
        /// Connects this node to another node using a specified transition.
        /// </summary>
        /// <param name="node">The node to connect to.</param>
        /// <param name="transition">The transition to connect with.</param>
        void Connect(IFlowNode node, IFlowTransition transition);

        /// <summary>
        /// Allows this flow node to modify its state based on contextual flow state
        /// information and register responses with a visitor.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        /// <param name="visitor">The flow node visitor.</param>
        void Accept(FlowContext flowContext, IBehaviourVisitor visitor);

        /// <summary>
        /// Invokes a transition of a flow to this node.
        /// </summary>
        void TransitionTo();

        /// <summary>
        /// Allows this node to respond to a logical clock tick.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        /// <param name="visitor">A visitor to register responses to.</param>
        void OnTick(FlowContext flowContext, IBehaviourVisitor visitor);

        /// <summary>
        /// Creates a copy of this node, omitting transition maps.
        /// </summary>
        /// <returns>A copy of this flow node.</returns>
        IFlowNode Copy();
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
        Action<FlowContext, FlowTransitionManager, IBehaviourVisitor> Action { get; }

        /// <summary>
        /// Determines whether this transition is viable.
        /// </summary>
        /// <param name="context">The current flow context.</param>
        /// <returns>
        ///   <c>true</c> if this transition is viable; otherwise, <c>false</c>.
        /// </returns>
        bool IsViable(FlowContext context);

        /// <summary>
        /// Allows this transition to respond to a logical clock tick.
        /// </summary>
        void OnTick();

        /// <summary>
        /// Creates a copy of this transition.
        /// </summary>
        /// <returns>A copy of this flow transition.</returns>
        IFlowTransition Copy();
    }
}
