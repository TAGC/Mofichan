using System;
using Mofichan.Core.Visitor;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A basic implementation of <see cref="IFlowTransition"/>. 
    /// </summary>
    public class FlowTransition : IFlowTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTransition"/> class.
        /// </summary>
        /// <param name="id">The transition identifier.</param>
        public FlowTransition(string id)
            : this(id, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTransition"/> class.
        /// </summary>
        /// <param name="id">The transition identifier.</param>
        /// <param name="action">The action to perform when this transition occurs.</param>
        public FlowTransition(string id, Action<FlowContext, FlowTransitionManager, IBehaviourVisitor> action)
        {
            this.Id = id;
            this.Action = action;
            this.Clock = -1;
        }

        /// <summary>
        /// Gets the flow transition identifier.
        /// </summary>
        /// <value>
        /// The flow transition identifier.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets any action that should be invoked as this transition occurs.
        /// </summary>
        /// <value>
        /// The transition action.
        /// </value>
        public Action<FlowContext, FlowTransitionManager, IBehaviourVisitor> Action { get; }

        /// <summary>
        /// Gets or sets the clock for this transition, representing the number of
        /// ticks until this transition should occur.
        /// </summary>
        /// <value>
        /// The transition clock.
        /// </value>
        public int Clock { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as IFlowTransition;

            if (other == null)
            {
                return false;
            }

            return this.Id.Equals(other.Id);
        }

        /// <summary>
        /// Determines whether this transition is viable.
        /// </summary>
        /// <param name="context">The current flow context.</param>
        /// <returns>
        ///   <c>true</c> if this transition is viable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsViable(FlowContext context)
        {
            return this.Clock == 0;
        }

        /// <summary>
        /// Allows this transition to respond to a logical clock tick.
        /// </summary>
        public void OnTick()
        {
            if (this.Clock > 0)
            {
                this.Clock -= 1;
            }
        }

        /// <summary>
        /// Creates a copy of this transition.
        /// </summary>
        /// <returns>
        /// A copy of this flow transition.
        /// </returns>
        public IFlowTransition Copy()
        {
            var copy = new FlowTransition(this.Id, this.Action);
            copy.Clock = this.Clock;

            return copy;
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
            return string.Format("Flow transition [{0}] (clock: {1}))",
                this.Id, this.Clock);
        }
    }
}