using System;
using System.Collections.Generic;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// An implementation of <see cref="IFlowManager"/>. 
    /// </summary>
    public class FlowManager : IFlowManager
    {
        private readonly Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowManager" /> class.
        /// </summary>
        /// <param name="transitionManagerFactory">The transition manager factory.</param>
        public FlowManager(Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory)
        {
            this.transitionManagerFactory = transitionManagerFactory;
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
    }
}
