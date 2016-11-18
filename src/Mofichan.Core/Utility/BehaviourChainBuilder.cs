using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.Utility
{
    /// <summary>
    /// A default implementation of <see cref="IBehaviourChainBuilder"/> 
    /// </summary>
    public class BehaviourChainBuilder : IBehaviourChainBuilder
    {
        /// <summary>
        /// Composes the provided collection of behaviours into a chain of responsibility.
        /// </summary>
        /// <param name="behaviours">The behaviours to compose into a chain.</param>
        /// <returns>
        /// The root of the behaviour chain.
        /// </returns>
        /// <remarks>
        /// The order of the behaviours within the composed chain corresponds to their order
        /// within the provided collection.
        /// <para></para>
        /// The first element within the collection will become the root of the chain, and the
        /// last element will become the tail.
        /// </remarks>
        public IMofichanBehaviour BuildChain(IEnumerable<IMofichanBehaviour> behaviours)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(),
                "There needs to be at least one behaviour to form the chain");

            var behaviourList = behaviours.ToList();

            for (var i = 0; i < behaviourList.Count - 1; i++)
            {
                var upstreamBehaviour = behaviourList[i];
                var downstreamBehaviour = behaviourList[i + 1];

                upstreamBehaviour.Subscribe<IncomingMessage>(downstreamBehaviour.OnNext);
                downstreamBehaviour.Subscribe<OutgoingMessage>(upstreamBehaviour.OnNext);
            }

            return behaviourList[0];
        }
    }
}
