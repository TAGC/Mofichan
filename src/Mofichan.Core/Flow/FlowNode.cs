using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A basic implementation of <see cref="IFlowNode"/>. 
    /// </summary>
    public class FlowNode : IFlowNode
    {
        private readonly Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory;
        private readonly Action<FlowContext, IFlowTransitionManager> onAcceptAction;
        private readonly IDictionary<IFlowTransition, IFlowNode> transitionMap;
        private readonly IDictionary<string, FlowContext> userFlowContexts;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowNode" /> class.
        /// </summary>
        /// <param name="id">The node identifier.</param>
        /// <param name="onAcceptAction">The action to take on this node accepting a flow context.</param>
        /// <param name="transitionManagerFactory">The transition manager factory.</param>
        public FlowNode(
            string id,
            Action<FlowContext, IFlowTransitionManager> onAcceptAction,
            Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory)
        {
            this.Id = id;
            this.onAcceptAction = onAcceptAction;
            this.transitionManagerFactory = transitionManagerFactory;
            this.transitionMap = new Dictionary<IFlowTransition, IFlowNode>();
            this.userFlowContexts = new Dictionary<string, FlowContext>();
        }

        /// <summary>
        /// Gets the flow node identifier.
        /// </summary>
        /// <value>
        /// The flow node identifier.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets a value indicating whether this  node is active within the flow.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get
            {
                return this.userFlowContexts.Any();
            }
        }

        private IFlowTransitionManager TransitionManager
        {
            get
            {
                return this.transitionManagerFactory(this.transitionMap.Keys);
            }
        }

        /// <summary>
        /// Allows this flow node to modify its state based on contextual flow state
        /// information.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        public void Accept(FlowContext flowContext)
        {
            var userId = (flowContext.Message.From as IUser).UserId;
            Debug.Assert(this.userFlowContexts.ContainsKey(userId),
                "This node should only have accepted the message if the user ID is stored");

            this.userFlowContexts[userId] = flowContext;
            this.onAcceptAction(flowContext, this.TransitionManager);
        }

        /// <summary>
        /// Invokes a transition of a flow to this node.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        public void TransitionTo(FlowContext flowContext)
        {
            var transitioningUserId = (flowContext.Message.From as IUser).UserId;
            this.userFlowContexts[transitioningUserId] = flowContext;
        }

        /// <summary>
        /// Allows this node to respond to a logical clock tick.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        public void OnTick(FlowContext flowContext)
        {
            var transitions = this.transitionMap.Select(it => it.Key);

            /*
             * This flow ends if there are no valid transitions out of this state.
             */
            if (!transitions.Any())
            {
                this.userFlowContexts.Clear();
                return;
            }

            /*
             * Decrement all transition clocks with a positive value.
             */
            transitions.Where(it => it.Clock > 0).ToList().ForEach(it => --it.Clock);

            /*
             * A transition becomes available when the associated clock reaches 0.
             */
            var selectedTransition = transitions.FirstOrDefault(it => it.Clock == 0);

            if (selectedTransition != null)
            {
                var targetNode = this.transitionMap[selectedTransition];
                var transitioningContexts = this.userFlowContexts.Values.ToList();

                selectedTransition.Action?.Invoke(flowContext, this.TransitionManager);

                foreach (var transitioningContext in transitioningContexts)
                {
                    targetNode.TransitionTo(transitioningContext);
                    targetNode.Accept(transitioningContext);
                }

                this.userFlowContexts.Clear();
            }
        }

        /// <summary>
        /// Connects this node to another node using a specified transition.
        /// </summary>
        /// <param name="node">The node to connect to.</param>
        /// <param name="transition">The transition to connect with.</param>
        public void Connect(IFlowNode node, IFlowTransition transition)
        {
            this.transitionMap[transition] = node;
        }

        /// <summary>
        /// Determines whether this is the suitable node to select as "current" for the specified
        /// message context.
        /// </summary>
        /// <param name="messageContext">The message context.</param>
        /// <returns>
        /// <c>true</c> if this is the "current" node for the message context; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCurrentNodeForMessageContext(MessageContext messageContext)
        {
            var userId = (messageContext.From as IUser).UserId;
            return this.userFlowContexts.ContainsKey(userId);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as IFlowNode;

            if (other == null)
            {
                return false;
            }

            return this.Id.Equals(other.Id);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Flow node [{0}]", this.Id);
        }
    }
}