using System;
using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents a facade service that aids in managing behavioural flows.
    /// </summary>
    public interface IFlowManager
    {
        /// <summary>
        /// Occurs when flows should perform their next step.
        /// </summary>
        event EventHandler OnNextStep;

        /// <summary>
        /// Gets the flow transition selector.
        /// </summary>
        /// <value>
        /// The flow transition selector.
        /// </value>
        IFlowTransitionSelector TransitionSelector { get; }

        /// <summary>
        /// Gets the flow-driven attention manager.
        /// </summary>
        /// <value>
        /// The attention manager.
        /// </value>
        IAttentionManager Attention { get; }

        /// <summary>
        /// Builds instances of <see cref="IFlowTransitionManager"/> that can be used to manage
        /// a provided collection of <see cref="IFlowTransition"/>. 
        /// </summary>
        /// <param name="transitions">The transitions to manage.</param>
        /// <returns>A flow transition manager for <paramref name="transitions"/>.</returns>
        IFlowTransitionManager BuildTransitionManager(IEnumerable<IFlowTransition> transitions);

        /// <summary>
        /// Selects a flow transition from the given collection.
        /// </summary>
        /// <param name="possibleTransitions">The set of possible transitions.</param>
        /// <returns>One member of the set, based on this instance's selection criteria.</returns>
        IFlowTransition Select(IEnumerable<IFlowTransition> possibleTransitions);
    }
}
