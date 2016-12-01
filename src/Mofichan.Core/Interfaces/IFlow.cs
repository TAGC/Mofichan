using System;
using System.Collections.Generic;
using Mofichan.Core.Flow;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents a logical behaviour flow.
    /// </summary>
    public interface IFlow : IDisposable
    {
        /// <summary>
        /// Allows this flow to modify its state based on information
        /// gained from a provided message.
        /// </summary>
        /// <param name="message">The message to accept.</param>
        void Accept(IncomingMessage message);
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
        /// Gets a value indicating whether this  node is active within the flow.
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
        /// Invokes a transition of a flow out of this node, if possible.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        void TransitionFrom(FlowContext flowContext);

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
        /// Gets or sets the weight of this transition, representing the probability
        /// that this transition should occur relative to other viable transitions.
        /// </summary>
        /// <value>
        /// The transition weight.
        /// </value>
        double Weight { get; set; }
    }

    /// <summary>
    /// Represents an object used to select a flow transition out of
    /// a collection of possible candidates.
    /// </summary>
    public interface IFlowTransitionSelector
    {
        /// <summary>
        /// Selects a flow transition from the given collection.
        /// </summary>
        /// <param name="possibleTransitions">The set of possible transitions.</param>
        /// <returns>One member of the set, based on this instance's selection criteria.</returns>
        IFlowTransition Select(IEnumerable<IFlowTransition> possibleTransitions);
    }

    /// <summary>
    /// Represents an object used to drive a behavioural flow by firing events
    /// to signal when the next steps in flows should occur.
    /// </summary>
    public interface IFlowDriver
    {
        /// <summary>
        /// Occurs when flows should perform their next step.
        /// </summary>
        event EventHandler OnNextStep;
    }
}
