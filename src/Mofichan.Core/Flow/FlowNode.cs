using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Visitor;
using NodeAction = System.Func<
    Mofichan.Core.Flow.FlowContext,
    Mofichan.Core.Flow.FlowTransitionManager,
    Mofichan.Core.Visitor.IBehaviourVisitor,
    Mofichan.Core.Flow.FlowNodeState?>;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A basic implementation of <see cref="IFlowNode"/>. 
    /// </summary>
    public class FlowNode : IFlowNode
    {
        private readonly IDictionary<FlowTransition, FlowNode> transitionMap;
        private readonly NodeAction onAcceptAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowNode" /> class.
        /// </summary>
        /// <param name="id">The node identifier.</param>
        /// <param name="onAcceptAction">The action to take on this node accepting a flow context.</param>
        public FlowNode(string id, NodeAction onAcceptAction)
        {
            this.Id = id;
            this.onAcceptAction = onAcceptAction;
            this.transitionMap = new Dictionary<FlowTransition, FlowNode>();
        }

        /// <summary>
        /// Gets the flow node identifier.
        /// </summary>
        /// <value>
        /// The flow node identifier.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the current state of this node.
        /// </summary>
        /// <value>
        /// This node's current state.
        /// </value>
        public FlowNodeState State { get; set; }

        private FlowTransitionManager TransitionManager
        {
            get
            {
                return new FlowTransitionManager(this.transitionMap.Keys);
            }
        }

        /// <summary>
        /// Allows this flow node to modify its state based on contextual flow state
        /// information and register responses with a visitor.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        /// <param name="visitor">The flow node visitor.</param>
        public void Accept(FlowContext flowContext, IBehaviourVisitor visitor)
        {
            FlowNodeState? newState = this.onAcceptAction(flowContext, this.TransitionManager, visitor);

            if (newState.HasValue)
            {
                this.State = newState.Value;
            }
        }

        /// <summary>
        /// Invokes a transition of a flow to this node.
        /// </summary>
        public void TransitionTo()
        {
            this.State = FlowNodeState.Active;
        }

        /// <summary>
        /// Allows this node to respond to a logical clock tick.
        /// </summary>
        /// <param name="flowContext">The flow context.</param>
        /// <param name="visitor">A visitor to register responses to.</param>
        public void OnTick(FlowContext flowContext, IBehaviourVisitor visitor)
        {
            var transitions = this.transitionMap.Select(it => it.Key).ToList();

            /*
             * This flow ends if there are no valid transitions out of this state.
             */
            if (!transitions.Any())
            {
                this.State = FlowNodeState.Inactive;
                return;
            }

            transitions.ForEach(it => it.OnTick());

            var selectedTransition = transitions.FirstOrDefault(it => it.IsViable(flowContext));

            if (selectedTransition != null)
            {
                this.State = FlowNodeState.Inactive;
                var targetNode = this.transitionMap[selectedTransition];
                selectedTransition.Action?.Invoke(flowContext, this.TransitionManager, visitor);
                targetNode.TransitionTo();
            }
        }

        /// <summary>
        /// Connects this node to another node using a specified transition.
        /// </summary>
        /// <param name="node">The node to connect to.</param>
        /// <param name="transition">The transition to connect with.</param>
        public void Connect(IFlowNode node, IFlowTransition transition)
        {
            this.transitionMap[(FlowTransition)transition] = (FlowNode)node;
        }

        /// <summary>
        /// Copies this node. All transition mappings are omitted from the copy.
        /// </summary>
        /// <returns>A copy of this node, without transition mappings.</returns>
        public IFlowNode Copy()
        {
            var copy = new FlowNode(this.Id, this.onAcceptAction);
            copy.State = this.State;

            return copy;
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
            return string.Format("Flow node [{0}] ({1})", this.Id, this.State);
        }
    }
}