using System;
using System.Collections.Generic;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// An implementation of <see cref="IFlowManager"/>. 
    /// </summary>
    public class FlowManager : IFlowManager
    {
        private readonly Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory;
        private readonly IFlowDriver flowDriver;
        private readonly IFlowTransitionSelector transitionSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowManager"/> class.
        /// </summary>
        /// <param name="transitionManagerFactory">The transition manager factory.</param>
        /// <param name="transitionSelector">The transition selector.</param>
        /// <param name="flowDriver">The flow driver.</param>
        public FlowManager(Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory,
            IFlowTransitionSelector transitionSelector,
            IFlowDriver flowDriver)
        {
            this.transitionManagerFactory = transitionManagerFactory;
            this.transitionSelector = transitionSelector;
            this.flowDriver = flowDriver;
        }

        /// <summary>
        /// Occurs when flows should perform their next step.
        /// </summary>
        public event EventHandler OnNextStep
        {
            add
            {
                this.flowDriver.OnNextStep += value;
            }

            remove
            {
                this.flowDriver.OnNextStep -= value;
            }
        }

        /// <summary>
        /// Gets the flow transition selector.
        /// </summary>
        /// <value>
        /// The flow transition selector.
        /// </value>
        public IFlowTransitionSelector TransitionSelector
        {
            get
            {
                return this.transitionSelector;
            }
        }

        /// <summary>
        /// Builds instances of <see cref="IFlowTransitionManager" /> that can be used to manage
        /// a provided collection of <see cref="IFlowTransition" />.
        /// </summary>
        /// <param name="transitions">The transitions to manage.</param>
        /// <returns>
        /// A flow transition manager for <paramref name="transitions" />.
        /// </returns>
        public IFlowTransitionManager BuildTransitionManager(IEnumerable<IFlowTransition> transitions)
        {
            return this.transitionManagerFactory(transitions);
        }

        /// <summary>
        /// Selects a flow transition from the given collection.
        /// </summary>
        /// <param name="possibleTransitions">The set of possible transitions.</param>
        /// <returns>
        /// One member of the set, based on this instance's selection criteria.
        /// </returns>
        public IFlowTransition Select(IEnumerable<IFlowTransition> possibleTransitions)
        {
            return this.TransitionSelector.Select(possibleTransitions);
        }
    }
}
