using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;
using Connection = System.Tuple<string, string, string>;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A base implementation of <see cref="IFlow"/>. 
    /// </summary>
    public abstract class BaseFlow : IFlow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlow"/> class.
        /// </summary>
        /// <param name="startNodeId">Identifies the starting node within the flow.</param>
        /// <param name="nodes">The nodes used within the flow.</param>
        /// <param name="transitions">The transitions used within the flow.</param>
        /// <param name="connections">The connections between nodes.</param>
        protected BaseFlow(
            string startNodeId,
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions,
            IList<Connection> connections)
        {
            Raise.ArgumentNullException.IfIsNull(nodes, nameof(nodes));
            Raise.ArgumentNullException.IfIsNull(transitions, nameof(transitions));
            Raise.ArgumentNullException.IfIsNull(connections, nameof(connections));
            Raise.ArgumentException.IfNot(nodes.Any(), "The node collection must contain at least one node");
            Raise.ArgumentException.IfNot(nodes.Count(it => it.Id == startNodeId) == 1,
                "Exactly one node within the provided collection should be the starting node");

            this.StartNodeId = startNodeId;
            this.Nodes = nodes.ToArray();
            this.Transitions = transitions.ToArray();
            this.MessageQueue = new Queue<MessageContext>();
            this.FlowContext = new FlowContext();
            this.Connections = connections;

            foreach (var connection in this.Connections)
            {
                this.Connect(connection.Item1, connection.Item2, connection.Item3);
            }

            var startNode = this.Nodes.Single(it => it.Id == this.StartNodeId);
            startNode.TransitionTo();
        }

        /// <summary>
        /// Gets a value indicating whether this flow is complete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this flow is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete
        {
            get
            {
                return this.Nodes.All(it => it.State == FlowNodeState.Inactive);
            }
        }

        /// <summary>
        /// Gets the currently active node within the flow.
        /// </summary>
        /// <value>
        /// The currently active node.
        /// </value>
        /// <remarks>
        /// Only one node within this flow should ever be active at a time.
        /// </remarks>
        protected IFlowNode ActiveNode
        {
            get
            {
                return this.Nodes.Single(it => it.State != FlowNodeState.Inactive);
            }
        }

        /// <summary>
        /// Gets the identifier of the starting node within the generated flow.
        /// </summary>
        /// <value>
        /// The start node identifier.
        /// </value>
        protected string StartNodeId { get; }

        /// <summary>
        /// Gets the collection of nodes within the generated flow.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        protected IEnumerable<IFlowNode> Nodes { get; }

        /// <summary>
        /// Gets the collection of transitions within the generated flow.
        /// </summary>
        /// <value>
        /// The transitions.
        /// </value>
        protected IEnumerable<IFlowTransition> Transitions { get; }

        /// <summary>
        /// Gets the collection of connections between nodes within the generated flow.
        /// </summary>
        /// <value>
        /// The connections between nodes.
        /// </value>
        protected IList<Connection> Connections { get; }

        /// <summary>
        /// Gets a queue containing messages received by this flow.
        /// </summary>
        /// <value>
        /// The message queue.
        /// </value>
        protected Queue<MessageContext> MessageQueue { get; }

        /// <summary>
        /// Gets or sets the latest flow context.
        /// </summary>
        /// <value>
        /// The latest flow context.
        /// </value>
        protected FlowContext FlowContext { get; set; }

        /// <summary>
        /// Allows this flow to modify its state and potentially generate responses
        /// using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Accept(IBehaviourVisitor visitor)
        {
            if (this.IsComplete)
            {
                return;
            }

            var onMessageVisitor = visitor as OnMessageVisitor;
            var onPulseVisitor = visitor as OnPulseVisitor;

            if (onMessageVisitor != null)
            {
                this.MessageQueue.Enqueue(onMessageVisitor.Message);
            }
            else if (onPulseVisitor != null)
            {
                this.Step(onPulseVisitor);
            }
        }

        /// <summary>
        /// Creates a copy of this flow, which includes copies of all of its nodes, transitions and connections.
        /// </summary>
        /// <returns>
        /// A copy of this flow.
        /// </returns>
        public IFlow Copy()
        {
            return this.GetCopy();
        }

        /// <summary>
        /// Creates a copy of this flow. This is a helper method used with <see cref="Copy"/>. 
        /// </summary>
        /// <returns>A copy of this flow.</returns>
        protected abstract IFlow GetCopy();

        /// <summary>
        /// Allows this flow to respond to a logical clock tick.
        /// </summary>
        /// <param name="visitor">The visitor associated with the tick.</param>
        protected abstract void Step(OnPulseVisitor visitor);

        private void Connect(string nodeAId, string nodeBId, string transitionId)
        {
            IFlowNode nodeA = this.Nodes.FirstOrDefault(it => it.Id == nodeAId);
            IFlowNode nodeB = this.Nodes.FirstOrDefault(it => it.Id == nodeBId);
            IFlowTransition transition = this.Transitions.FirstOrDefault(it => it.Id == transitionId);

            Raise.ArgumentException.If(nodeA == null, nameof(nodeAId));
            Raise.ArgumentException.If(nodeB == null, nameof(nodeBId));
            Raise.ArgumentException.If(transition == null, nameof(transitionId));

            nodeA.Connect(nodeB, transition);
        }

        /// <summary>
        /// Builds instances of <see cref="BaseFlow" />.
        /// </summary>
        /// <typeparam name="T">The type of flow being built.</typeparam>
        public abstract class Builder<T> where T : BaseFlow
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Builder{T}"/> class.
            /// </summary>
            protected Builder()
            {
                this.Connections = new List<Connection>();
            }

            /// <summary>
            /// Gets or sets the identifier of the starting node within the generated flow.
            /// </summary>
            /// <value>
            /// The start node identifier.
            /// </value>
            protected string StartNodeId { get; set; }

            /// <summary>
            /// Gets or sets the collection of nodes within the generated flow.
            /// </summary>
            /// <value>
            /// The nodes.
            /// </value>
            protected IEnumerable<IFlowNode> Nodes { get; set; }

            /// <summary>
            /// Gets or sets the collection of transitions within the generated flow.
            /// </summary>
            /// <value>
            /// The transitions.
            /// </value>
            protected IEnumerable<IFlowTransition> Transitions { get; set; }

            /// <summary>
            /// Gets the collection of connections between nodes within the generated flow.
            /// </summary>
            /// <value>
            /// The connections between nodes.
            /// </value>
            protected IList<Connection> Connections { get; }

            /// <summary>
            /// Specifies the identifier of the starting node within the generated flow.
            /// </summary>
            /// <param name="startNodeId">The starting node identifier.</param>
            /// <returns>This builder.</returns>
            public Builder<T> WithStartNodeId(string startNodeId)
            {
                this.StartNodeId = startNodeId;
                return this;
            }

            /// <summary>
            /// Configures the flow to use the provided collection of nodes.
            /// </summary>
            /// <param name="nodes">The nodes to use within the flow.</param>
            /// <returns>This builder.</returns>
            public Builder<T> WithNodes(IEnumerable<IFlowNode> nodes)
            {
                this.Nodes = nodes;
                return this;
            }

            /// <summary>
            /// Configures the flow to use the provided collection of transitions.
            /// </summary>
            /// <param name="transitions">The transitions to use within the flow.</param>
            /// <returns>This builder.</returns>
            public Builder<T> WithTransitions(IEnumerable<IFlowTransition> transitions)
            {
                this.Transitions = transitions;
                return this;
            }

            /// <summary>
            /// Specifies a connection between two nodes in the flow using a particular transition.
            /// </summary>
            /// <param name="nodeAId">The identifier of the first node.</param>
            /// <param name="nodeBId">The identifier of the second node.</param>
            /// <param name="transitionId">The identifier of the transition to connect the two nodes.</param>
            /// <returns>
            /// This builder.
            /// </returns>
            public Builder<T> WithConnection(string nodeAId, string nodeBId, string transitionId)
            {
                this.Connections.Add(Tuple.Create(nodeAId, nodeBId, transitionId));
                return this;
            }

            /// <summary>
            /// Builds a <c>BasicFlow</c> based on the configuration of this builder.
            /// </summary>
            /// <returns>A <c>BasicFlow</c>.</returns>
            public abstract T Build();
        }
    }
}
