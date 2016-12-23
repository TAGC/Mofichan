using System.Collections.Generic;
using System.Linq;
using PommaLabs.Thrower;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// Represents an object used to help instance <see cref="IFlowNode"/> manage
    /// the weight distributions of their associated transitions.
    /// <para></para>
    /// These distributions will likely change based on the messages each node receives.
    /// </summary>
    public class FlowTransitionManager
    {
        private readonly IDictionary<string, FlowTransition> transitionMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTransitionManager" /> class.
        /// </summary>
        /// <param name="transitions">The transitions to manage.</param>
        public FlowTransitionManager(IEnumerable<FlowTransition> transitions)
        {
            this.transitionMap = transitions.ToDictionary(it => it.Id, it => it);
        }

        /// <summary>
        /// Gets or sets the clock of the transition with the specified identifier.
        /// </summary>
        /// <value>
        /// The transition clock.
        /// </value>
        /// <param name="transitionId">The transition identifier.</param>
        /// <returns>The currently stored transition clock.</returns>
        public int this[string transitionId]
        {
            get
            {
                return this.transitionMap[transitionId].Clock;
            }

            set
            {
                this.transitionMap[transitionId].Clock = value;
            }
        }

        /// <summary>
        /// Makes all transitions impossible by setting their clocks to a negative value.
        /// </summary>
        public void MakeTransitionsImpossible()
        {
            var transitionIdentifiers = this.transitionMap.Keys.ToList();
            transitionIdentifiers.ForEach(it => this[it] = -1);
        }

        /// <summary>
        /// Makes a transition certain by setting its associated clock to 0 ticks and
        /// making all other transitions impossible.
        /// </summary>
        /// <param name="transitionId">The identifier of the transition to make certain.</param>
        public void MakeTransitionCertain(string transitionId)
        {
            Raise.ArgumentException.IfNot(this.transitionMap.ContainsKey(transitionId));

            this.MakeTransitionsImpossible();
            this[transitionId] = 0;
        }
    }
}
