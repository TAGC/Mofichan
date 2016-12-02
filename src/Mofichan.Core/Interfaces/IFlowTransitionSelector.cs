using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object used to select a flow transition out of
    /// a collection of possible candidates.
    /// </summary>
    public interface IFlowTransitionSelector
    {
        /// <summary>
        /// Selects a flow transition from the given collection.
        /// </summary>
        /// <param name="possibleTransitions">The set of possible transitions.</param>
        /// <returns>One member of the set, based on this instance's selection criteria.</returns>
        IFlowTransition Select(IEnumerable<IFlowTransition> possibleTransitions);
    }
}