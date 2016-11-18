using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object that can compose a collection of <see cref="IMofichanBehaviour"/>
    /// into a chain of responsibility and return the root of that chain.
    /// </summary>
    public interface IBehaviourChainBuilder
    {
        /// <summary>
        /// Composes the provided collection of behaviours into a chain of responsibility.
        /// </summary>
        /// <param name="behaviours">The behaviours to compose into a chain.</param>
        /// <returns>The root of the behaviour chain.</returns>
        /// <remarks>
        /// The order of the behaviours within the composed chain corresponds to their order
        /// within the provided collection.
        /// <para></para>
        /// The first element within the collection will become the root of the chain, and the
        /// last element will become the tail.
        /// </remarks>
        IMofichanBehaviour BuildChain(IEnumerable<IMofichanBehaviour> behaviours);
    }
}
