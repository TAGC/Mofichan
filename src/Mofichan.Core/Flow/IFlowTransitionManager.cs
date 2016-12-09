namespace Mofichan.Core.Flow
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
        /// Gets or sets the clock of the transition with the specified identifier.
        /// </summary>
        /// <value>
        /// The transition clock.
        /// </value>
        /// <param name="transitionId">The transition identifier.</param>
        /// <returns>The currently stored transition clock.</returns>
        int this[string transitionId] { get; set; }

        /// <summary>
        /// Makes all transitions impossible by setting their clocks to a negative value.
        /// </summary>
        void MakeTransitionsImpossible();

        /// <summary>
        /// Makes a transition certain by setting its associated clock to 0 ticks and
        /// making all other transitions impossible.
        /// </summary>
        /// <param name="transitionId">The identifier of the transition to make certain.</param>
        void MakeTransitionCertain(string transitionId);
    }
}