using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WeightVector = System.Collections.Generic.IList<double>;

namespace Mofichan.Core.Relevance
{
    /// <summary>
    /// A type of <see cref="IRelevanceArgumentEvaluator"/> that uses an algorithm similar
    /// to the one used by ElasticSearch to evaluate the strength of relevance arguments for
    /// a particular context.
    /// </summary>
    /// <remarks>
    /// This works by creating weight vectors for both the context and each of the arguments,
    /// and determining argument scores by how similar the vector associated with it is to the
    /// vector representing the context.
    /// </remarks>
    public class VectorSimilarityEvaluator : IRelevanceArgumentEvaluator
    {
        /// <summary>
        /// Evaluates the specified arguments, associating a score with each one representing the suitability
        /// of that argument towards <paramref name="message" />.
        /// <para></para>
        /// The higher the associated score, the stronger the relevance argument is.
        /// </summary>
        /// <param name="arguments">The collection of relevance arguments.</param>
        /// <param name="message">The message to assess relevance against.</param>
        /// <returns>The arguments in the order they were given, with their associated scores.</returns>
        public IEnumerable<Tuple<RelevanceArgument, double>> Evaluate(IEnumerable<RelevanceArgument> arguments,
            MessageContext message)
        {
            IEnumerable<Tuple<RelevanceArgument, double>> scoredArguments;

            if (CheckForGuaranteedRelevance(arguments, message, out scoredArguments))
            {
                return scoredArguments;
            }

            var tags = arguments
                .SelectMany(it => it.MessageTagArguments)
                .Concat(message.Tags)
                .Distinct()
                .ToList();

            var tagWeights = (from tag in tags
                              let weight = CalculateTagWeight(arguments, tag)
                              select new { tag, weight })
                             .ToDictionary(it => it.tag, it => it.weight);

            var argVectors = from argument in arguments
                             let vector = from tag in tags
                                          let argumentHasTag = argument.MessageTagArguments.Contains(tag)
                                          select argumentHasTag ? tagWeights[tag] : 0
                             select new ArgumentVector { Argument = argument, Vector = vector.ToList() };

            WeightVector messageVector = (from tag in tags
                                          let messageHasTag = message.Tags.Contains(tag)
                                          select messageHasTag ? tagWeights[tag] : 0)
                                          .ToList();

            var argSimilarities = from argument in arguments
                                  join argVector in argVectors on argument equals argVector.Argument
                                  let similarity = CalculateCosineSimilarity(messageVector, argVector.Vector)
                                  select new { argument, similarity };

            double maxSimilarity = argSimilarities.Select(it => it.similarity).Max();
            double minSimilarity = argSimilarities.Select(it => it.similarity).Min();
            double range = maxSimilarity - minSimilarity;

            if (Math.Abs(range) < double.Epsilon)
            {
                scoredArguments = from o in argSimilarities select Tuple.Create(o.argument, 1.0);
            }
            else
            {
                scoredArguments = from o in argSimilarities
                                  let normalisedScore = (o.similarity - minSimilarity) / range
                                  select Tuple.Create(o.argument, normalisedScore);
            }

            return scoredArguments;
        }

        private static bool CheckForGuaranteedRelevance(IEnumerable<RelevanceArgument> arguments,
            MessageContext message, out IEnumerable<Tuple<RelevanceArgument, double>> scoredArguments)
        {
            RelevanceArgument guaranteeArgument;

            try
            {
                guaranteeArgument = arguments.SingleOrDefault(it => it.GuaranteeRelevance);
            }
            catch (InvalidOperationException e)
            {
                var exceptionMsg = string.Format("Multiple arguments guarantee relevance to '{0}' in collection: {1}",
                    message.Body, string.Join(", ", arguments));

                throw new ArgumentException(exceptionMsg, e);
            }

            if (guaranteeArgument == null)
            {
                scoredArguments = null;
                return false;
            }

            scoredArguments = from argument in arguments
                              let score = argument.Equals(guaranteeArgument) ? 1.0 : 0.0
                              select Tuple.Create(argument, score);

            return true;
        }

        private static double CalculateCosineSimilarity(WeightVector p, WeightVector q)
        {
            Debug.Assert(p.Count == q.Count, "The vectors must have the same number of dimensions");

            double dotProduct = p.Zip(q, (x, y) => x * y).Sum();
            double magnitudeA = Math.Sqrt(p.Sum(term => Math.Pow(term, 2)));
            double magnitudeB = Math.Sqrt(q.Sum(term => Math.Pow(term, 2)));

            return dotProduct / (magnitudeA * magnitudeB);
        }

        /// <summary>
        /// Calculates the weight of a message tag.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The weight of the tag.</returns>
        /// <remarks>
        /// The algorithm used to calculate this weight is based on the "Inverse document frequency" algorithm
        /// used by ElasticSearch:
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/guide/current/scoring-theory.html"/>.
        /// </remarks>
        private static double CalculateTagWeight(IEnumerable<RelevanceArgument> arguments, string tag)
        {
            var numArguments = arguments.Count();
            var tagOccurrances = arguments.Count(it => it.MessageTagArguments.Contains(tag));

            return 1 + Math.Log(numArguments / (double)(tagOccurrances + 1));
        }

        private struct ArgumentVector
        {
            public RelevanceArgument Argument { get; set; }

            public WeightVector Vector { get; set; }
        }
    }
}
