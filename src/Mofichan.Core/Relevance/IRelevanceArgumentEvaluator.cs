using System;
using System.Collections.Generic;

namespace Mofichan.Core.Relevance
{
    /// <summary>
    /// Represents an object used to evaluate the suitability of each member within a collection
    /// of relevance arguments towards a particular message.
    /// </summary>
    public interface IRelevanceArgumentEvaluator
    {
        /// <summary>
        /// Evaluates the specified arguments, associating a score with each one representing the suitability
        /// of that argument towards <paramref name="message"/>.
        /// <para></para>
        /// The higher the associated score, the stronger the relevance argument is.
        /// </summary>
        /// <param name="arguments">The collection of relevance arguments.</param>
        /// <param name="message">The message to assess relevance against.</param>
        /// <returns>The arguments in the order they were given, with their associated scores.</returns>
        IEnumerable<Tuple<RelevanceArgument, double>> Evaluate(IEnumerable<RelevanceArgument> arguments,
            MessageContext message);
    }
}
