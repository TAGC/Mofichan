using System;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Flow
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
        public FlowTransition(string id, Action<FlowContext, IFlowTransitionManager> action)
        {
            this.Id = id;
            this.Action = action;
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
        public Action<FlowContext, IFlowTransitionManager> Action { get; }

        /// <summary>
        /// Gets or sets the weight of this transition, representing the probability
        /// that this transition should occur relative to other viable transitions.
        /// </summary>
        /// <value>
        /// The transition weight.
        /// </value>
        public double Weight { get; set; }

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
            return string.Format("Flow transition [{0}]", this.Id);
        }
    }
}