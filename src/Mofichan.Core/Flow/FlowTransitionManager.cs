using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// An implementation of <see cref="IFlowTransitionManager"/>.
    /// </summary>
    public class FlowTransitionManager : IFlowTransitionManager
    {
        private readonly Dictionary<string, IFlowTransition> transitionMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTransitionManager"/> class.
        /// </summary>
        /// <param name="transitions">The transitions to manage.</param>
        public FlowTransitionManager(IEnumerable<IFlowTransition> transitions)
        {
            this.transitionMap = transitions.ToDictionary(it => it.Id, it => it);
        }

        /// <summary>
        /// Gets or sets the weight of the transition with the specified identifier.
        /// </summary>
        /// <value>
        /// The transition weight.
        /// </value>
        /// <param name="transitionId">The transition identifier.</param>
        /// <returns>The currently stored transition weight.</returns>
        public double this[string transitionId]
        {
            get
            {
                return this.transitionMap[transitionId].Weight;
            }

            set
            {
                this.transitionMap[transitionId].Weight = value;
            }
        }

        /// <summary>
        /// Clears the weights of all managed transitions.
        /// </summary>
        public void ClearTransitionWeights()
        {
            var transitionIdentifiers = this.transitionMap.Keys.ToList();
            transitionIdentifiers.ForEach(it => this[it] = 0);
        }

        /// <summary>
        /// Makes a transition certain by assigning zero weight to all other managed transitions.
        /// </summary>
        /// <param name="transitionId">The identifier of the transition to make certain.</param>
        public void MakeTransitionCertain(string transitionId)
        {
            Raise.ArgumentException.IfNot(this.transitionMap.ContainsKey(transitionId));

            this.ClearTransitionWeights();
            this[transitionId] = 1;
        }
    }
}
