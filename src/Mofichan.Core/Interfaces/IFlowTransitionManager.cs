namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object used to help instance <see cref="IFlowNode"/> manage
    /// the weight distributions of their associated transitions.
    /// <para></para>
    /// These distributions will likely change based on the messages each node receives.
    /// </summary>
    public interface IFlowTransitionManager
    {
        /// <summary>
        /// Gets or sets the weight of the transition with the specified identifier.
        /// </summary>
        /// <value>
        /// The transition weight.
        /// </value>
        /// <param name="transitionId">The transition identifier.</param>
        /// <returns>The currently stored transition weight.</returns>
        double this[string transitionId] { get; set; }

        /// <summary>
        /// Clears the weights of all managed transitions.
        /// </summary>
        void ClearTransitionWeights();

        /// <summary>
        /// Makes a transition certain by assigning zero weight to all other managed transitions.
        /// </summary>
        /// <param name="transitionId">The identifier of the transition to make certain.</param>
        void MakeTransitionCertain(string transitionId);
    }
}