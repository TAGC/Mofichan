using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A type of <see cref="IFlowTransitionSelector"/> that selects a flow transition
    /// from a set of possible candidates probabilistically, with each transition
    /// having a chance of being selected proportional to its weight.
    /// </summary>
    public class FairFlowTransitionSelector : IFlowTransitionSelector
    {
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="FairFlowTransitionSelector"/> class.
        /// </summary>
        public FairFlowTransitionSelector()
        {
            this.random = new Random();
        }

        /// <summary>
        /// Selects a flow transition from the given collection.
        /// </summary>
        /// <param name="possibleTransitions">The set of possible transitions.</param>
        /// <returns>
        /// One member of the set, based on this instance's selection criteria.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The normalised weights do not add up to 1.</exception>
        public IFlowTransition Select(IEnumerable<IFlowTransition> possibleTransitions)
        {
            Raise.ArgumentNullException.IfIsNull(possibleTransitions, nameof(possibleTransitions));
            Raise.ArgumentException.IfNot(possibleTransitions.Any());

            var normalisedWeights = Normalise(possibleTransitions.Select(it => it.Weight));
            Debug.Assert(Math.Abs(normalisedWeights.Sum() - 1) < 0.001, "Normalised weights should sum to 1");

            var rand = this.random.NextDouble();
            
            foreach (var pair in possibleTransitions.Zip(normalisedWeights, Tuple.Create))
            {
                IFlowTransition transition = pair.Item1;
                double normalisedWeight = pair.Item2;

                if (rand < normalisedWeight)
                {
                    return transition;
                }

                rand -= normalisedWeight;
            }

            throw new InvalidOperationException("The normalised weights do not add up to 1.");
        }

        private static IEnumerable<double> Normalise(IEnumerable<double> unnormalisedValues)
        {
            var numValues = unnormalisedValues.Count();
            Debug.Assert(numValues >= 1, "There should be at least one value");

            double total = unnormalisedValues.Sum();

            if (total == 0)
            {
                return Enumerable.Repeat(1 / (double)numValues, numValues);
            }
            else
            {
                return unnormalisedValues.Select(it => it / total);
            }
        }
    }
}
