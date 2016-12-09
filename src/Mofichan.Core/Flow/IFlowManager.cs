using System.Collections.Generic;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// Represents a facade service that aids in managing behavioural flows.
    /// </summary>
    public interface IFlowManager
    {
        /// <summary>
        /// Builds instances of <see cref="IFlowTransitionManager"/> that can be used to manage
        /// a provided collection of <see cref="IFlowTransition"/>. 
        /// </summary>
        /// <param name="transitions">The transitions to manage.</param>
        /// <returns>A flow transition manager for <paramref name="transitions"/>.</returns>
        IFlowTransitionManager BuildTransitionManager(IEnumerable<IFlowTransition> transitions);
    }
}
