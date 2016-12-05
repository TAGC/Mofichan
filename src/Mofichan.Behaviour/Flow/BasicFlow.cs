using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Behaviour.Flow
{
    /// <summary>
    /// A basic implementation of <see cref="IFlow"/>. 
    /// </summary>
    public class BasicFlow : IFlow
    {
        private readonly string startNodeId;
        private readonly IFlowNode[] nodes;
        private readonly IFlowTransition[] transitions;
        private readonly IFlowManager flowManager;
        private readonly Queue<IncomingMessage> messageQueue;
        private readonly AuthorisationFailureHandler authExceptionHandler;
        private readonly FlowContext baseContext;

        private FlowContext latestFlowContext;

        private BasicFlow(
            IFlowManager flowManager,
            Action<OutgoingMessage> generatedResponseHandler,
            string startNodeId,
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions,
            ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(flowManager, nameof(flowManager));
            Raise.ArgumentNullException.IfIsNull(generatedResponseHandler, nameof(generatedResponseHandler));
            Raise.ArgumentNullException.IfIsNull(nodes, nameof(nodes));
            Raise.ArgumentNullException.IfIsNull(transitions, nameof(transitions));
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger));
            Raise.ArgumentException.IfNot(nodes.Any(), "The node collection must contain at least one node");
            Raise.ArgumentException.IfNot(nodes.Count(it => it.Id == startNodeId) == 1,
                "Exactly one node within the provided collection should be the starting node");

            this.flowManager = flowManager;
            this.startNodeId = startNodeId;
            this.nodes = nodes.ToArray();
            this.transitions = transitions.ToArray();
            this.baseContext = new FlowContext(this.flowManager.TransitionSelector, this.flowManager.Attention,
                generatedResponseHandler);
            this.messageQueue = new Queue<IncomingMessage>();
            this.authExceptionHandler = new AuthorisationFailureHandler(generatedResponseHandler, logger);
            this.latestFlowContext = this.baseContext;

            this.flowManager.OnNextStep += this.NextStep;
        }

        /// <summary>
        /// Allows this flow to modify its state based on information
        /// gained from a provided message.
        /// </summary>
        /// <param name="message">The message to accept.</param>
        public void Accept(IncomingMessage message)
        {
            this.messageQueue.Enqueue(message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.flowManager.OnNextStep -= this.NextStep;
        }

        private void Process(IncomingMessage message)
        {
            var messageContext = message.Context;
            var flowContext = this.baseContext.FromMessage(messageContext);
            var currentNode = this.nodes.FirstOrDefault(it => it.IsCurrentNodeForMessageContext(messageContext));

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

            try
            {
                currentNode.Accept(flowContext);
            }
            catch (MofichanAuthorisationException e)
            {
                this.authExceptionHandler.Handle(e);
            }

            this.latestFlowContext = flowContext;
        }

        private void NextStep(object sender, EventArgs e)
        {
            if (this.messageQueue.Any())
            {
                this.Process(this.messageQueue.Dequeue());
            }

            var activeNodes = this.nodes.Where(it => it.IsActive).ToList();

            foreach (var node in activeNodes)
            {
                try
                {
                    node.TransitionFrom(this.latestFlowContext);
                }
                catch (MofichanAuthorisationException exception)
                {
                    this.authExceptionHandler.Handle(exception);
                }
            }
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
            private Action<OutgoingMessage> generatedResponseHandler;
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
            /// Specifies the callback to handle responses generated within the flow.
            /// </summary>
            /// <param name="generatedResponseHandler">The generated response handler.</param>
            /// <returns>This builder.</returns>
            public Builder WithGeneratedResponseHandler(Action<OutgoingMessage> generatedResponseHandler)
            {
                this.generatedResponseHandler = generatedResponseHandler;
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
                var flow = new BasicFlow(this.manager, this.generatedResponseHandler,
                    this.startNodeId, this.nodes, this.transitions, this.logger);

                foreach (var connection in this.connections)
                {
                    flow.Connect(connection.Item1, connection.Item2, connection.Item3);
                }

                return flow;
            }
        }
    }
}
