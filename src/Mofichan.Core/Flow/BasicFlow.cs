using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A basic implementation of <see cref="IFlow"/>. 
    /// </summary>
    public class BasicFlow : IFlow
    {
        private readonly string startNodeId;
        private readonly IFlowNode[] nodes;
        private readonly IFlowTransition[] transitions;
        private readonly Queue<MessageContext> messageQueue;
        private readonly FlowContext baseContext;

        private FlowContext latestFlowContext;

        private BasicFlow(
            IFlowManager flowManager,
            string startNodeId,
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions,
            ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(flowManager, nameof(flowManager));
            Raise.ArgumentNullException.IfIsNull(nodes, nameof(nodes));
            Raise.ArgumentNullException.IfIsNull(transitions, nameof(transitions));
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger));
            Raise.ArgumentException.IfNot(nodes.Any(), "The node collection must contain at least one node");
            Raise.ArgumentException.IfNot(nodes.Count(it => it.Id == startNodeId) == 1,
                "Exactly one node within the provided collection should be the starting node");

            this.startNodeId = startNodeId;
            this.nodes = nodes.ToArray();
            this.transitions = transitions.ToArray();
            this.baseContext = new FlowContext();
            this.messageQueue = new Queue<MessageContext>();
            this.latestFlowContext = this.baseContext;
        }

        /// <summary>
        /// Allows this flow to modify its state and potentially generate responses
        /// using the provided visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Accept(IBehaviourVisitor visitor)
        {
            var onMessageVisitor = visitor as OnMessageVisitor;
            var onPulseVisitor = visitor as OnPulseVisitor;

            this.latestFlowContext = this.latestFlowContext.FromVisitor(visitor);

            if (onMessageVisitor != null)
            {
                this.messageQueue.Enqueue(onMessageVisitor.Message);
            }
            else if (onPulseVisitor != null)
            {
                this.Step(onPulseVisitor);
            }
        }

        private void Step(OnPulseVisitor visitor)
        {
            if (this.messageQueue.Any())
            {
                this.Process(this.messageQueue.Dequeue(), visitor);
            }

            var activeNodes = this.nodes.Where(it => it.IsActive).ToList();

            foreach (var node in activeNodes)
            {
                node.OnTick(this.latestFlowContext);
            }
        }

        private void Process(MessageContext message, OnPulseVisitor visitor)
        {
            var flowContext = this.baseContext.FromMessage(message).FromVisitor(visitor);
            var currentNode = this.nodes.FirstOrDefault(it => it.IsCurrentNodeForMessageContext(message));

            /*
             * If the user is not yet within the flow, we select the
             * starting node and "transition" to it in order to get
             * the user within it.
             */
            if (currentNode == null)
            {
                currentNode = this.nodes.Single(it => it.Id == this.startNodeId);
                currentNode.TransitionTo(flowContext);
            }

            currentNode.Accept(flowContext);

            this.latestFlowContext = flowContext;
        }

        private void Connect(string nodeAId, string nodeBId, string transitionId)
        {
            IFlowNode nodeA = this.nodes.FirstOrDefault(it => it.Id == nodeAId);
            IFlowNode nodeB = this.nodes.FirstOrDefault(it => it.Id == nodeBId);
            IFlowTransition transition = this.transitions.FirstOrDefault(it => it.Id == transitionId);

            Raise.ArgumentException.If(nodeA == null, nameof(nodeAId));
            Raise.ArgumentException.If(nodeB == null, nameof(nodeBId));
            Raise.ArgumentException.If(transition == null, nameof(transitionId));

            nodeA.Connect(nodeB, transition);
        }

        /// <summary>
        /// Builds instances of <see cref="BasicFlow"/>. 
        /// </summary>
        public class Builder
        {
            private readonly IList<Tuple<string, string, string>> connections;

            private ILogger logger;
            private IFlowManager manager;
            private string startNodeId;
            private IEnumerable<IFlowNode> nodes;
            private IEnumerable<IFlowTransition> transitions;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.connections = new List<Tuple<string, string, string>>();
            }

            /// <summary>
            /// Specifies the <see cref="ILogger"/> for the flow. 
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <returns>This builder.</returns>
            public Builder WithLogger(ILogger logger)
            {
                this.logger = logger;
                return this;
            }

            /// <summary>
            /// Specifies the <see cref="IFlowManager"/> for the flow. 
            /// </summary>
            /// <param name="manager">The flow manager.</param>
            /// <returns>This builder.</returns>
            public Builder WithManager(IFlowManager manager)
            {
                this.manager = manager;
                return this;
            }

            /// <summary>
            /// Specifies the identifier of the starting node within the flow.
            /// </summary>
            /// <param name="startNodeId">The starting node identifier.</param>
            /// <returns>This builder.</returns>
            public Builder WithStartNodeId(string startNodeId)
            {
                this.startNodeId = startNodeId;
                return this;
            }

            /// <summary>
            /// Configures the flow to use the provided collection of nodes.
            /// </summary>
            /// <param name="nodes">The nodes to use within the flow.</param>
            /// <returns>This builder.</returns>
            public Builder WithNodes(IEnumerable<IFlowNode> nodes)
            {
                this.nodes = nodes;
                return this;
            }

            /// <summary>
            /// Configures the flow to use the provided collection of transitions.
            /// </summary>
            /// <param name="transitions">The transitions to use within the flow.</param>
            /// <returns>This builder.</returns>
            public Builder WithTransitions(IEnumerable<IFlowTransition> transitions)
            {
                this.transitions = transitions;
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
            public Builder WithConnection(string nodeAId, string nodeBId, string transitionId)
            {
                this.connections.Add(Tuple.Create(nodeAId, nodeBId, transitionId));
                return this;
            }

            /// <summary>
            /// Builds a <c>BasicFlow</c> based on the configuration of this builder.
            /// </summary>
            /// <returns>A <c>BasicFlow</c>.</returns>
            public BasicFlow Build()
            {
                var flow = new BasicFlow(this.manager, this.startNodeId, this.nodes, this.transitions, this.logger);

                foreach (var connection in this.connections)
                {
                    flow.Connect(connection.Item1, connection.Item2, connection.Item3);
                }

                return flow;
            }
        }
    }
}
