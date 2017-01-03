using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.DataAccess.Domain;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.DataAccess.Analysis
{
    internal class CompositeBayesianClassifier : IMessageClassifier
    {
        private readonly ILogger logger;

        private IDictionary<string, BinaryBayesianClassifier> classifierMap;

        private CompositeBayesianClassifier(ILogger logger)
        {
            this.logger = logger.ForContext<CompositeBayesianClassifier>();
        }

        /// <summary>
        /// Attempts to classify the provided message. This method will return a set of
        /// tags that represents all classifications that are judged applicable
        /// for the message.
        /// </summary>
        /// <param name="message">The message to attempt to classify.</param>
        /// <returns>
        /// The set of tags that are applicable for the message. May be empty.
        /// </returns>
        public IEnumerable<string> Classify(string message)
        {
            return from pair in this.classifierMap
                   let classification = pair.Key
                   let classifier = pair.Value
                   where classifier.Classify(message)
                   select classification;
        }

        private static IEnumerable<string> GetClassifications(IEnumerable<TaggedMessage> trainingSet)
        {
            return new HashSet<string>(trainingSet.SelectMany(it => it.Tags));
        }

        private void Train(IEnumerable<TaggedMessage> trainingSet, double requiredConfidenceRatio)
        {
            this.logger.Debug("Training started. Required confidence ratio = {RequiredConfidenceRatio}",
                requiredConfidenceRatio);

            this.classifierMap = (from classification in GetClassifications(trainingSet)
                                  let memberSplit = from o in trainingSet
                                                    group o.Message by o.Tags.Contains(classification)
                                  let members = memberSplit.FirstOrDefault(it => it.Key)
                                  let nonMembers = memberSplit.FirstOrDefault(it => !it.Key)
                                  let classifier = new BinaryBayesianClassifier(
                                      classification,
                                      requiredConfidenceRatio,
                                      members ?? Enumerable.Empty<string>(),
                                      nonMembers ?? Enumerable.Empty<string>(),
                                      this.logger)
                                  select new { classification, classifier })
                                  .ToDictionary(it => it.classification, it => it.classifier);

            this.logger.Debug("Training complete - created bayesian classifiers for {Classifications}",
                this.classifierMap.Keys);
        }

        public class Factory
        {
            private readonly IDictionary<long, CompositeBayesianClassifier> cache;
            private readonly ILogger logger;
            private readonly double requiredConfidenceRatio;

            public Factory(double requiredConfidenceRatio, ILogger logger)
            {
                Raise.ArgumentNullException.IfIsNull(logger, nameof(logger));

                this.requiredConfidenceRatio = requiredConfidenceRatio;
                this.logger = logger;
                this.cache = new Dictionary<long, CompositeBayesianClassifier>();
            }

            public CompositeBayesianClassifier From(IEnumerable<TaggedMessage> trainingSet)
            {
                long trainingSetHash = trainingSet.Aggregate((long)17, (a, e) => (31 * e.GetHashCode()) + a);

                CompositeBayesianClassifier classifier;
                if (!this.cache.TryGetValue(trainingSetHash, out classifier))
                {
                    classifier = new CompositeBayesianClassifier(this.logger);
                    classifier.Train(trainingSet, this.requiredConfidenceRatio);
                    this.cache[trainingSetHash] = classifier;
                }

                return classifier;
            }
        }
    }
}
