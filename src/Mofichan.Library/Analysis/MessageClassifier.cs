using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Library.Analysis
{
    internal class MessageClassifier : IMessageClassifier
    {
        private readonly ILogger logger;

        private IDictionary<Tag, BinaryBayesianClassifier> classifierMap;

        public MessageClassifier(ILogger logger)
        {
            this.logger = logger.ForContext<MessageClassifier>();
        }

        public void Train(IEnumerable<TaggedMessage> trainingSet, double requiredConfidenceRatio)
        {
            this.logger.Debug("Training started. Required confidence ratio = {RequiredConfidenceRatio}",
                requiredConfidenceRatio);

            this.classifierMap = (from classification in GetClassifications()
                                  let memberSplit = from o in trainingSet
                                                    group o.Message by o.Tags.Contains(classification)
                                  let members = memberSplit.FirstOrDefault(it => it.Key)
                                  let nonMembers = memberSplit.FirstOrDefault(it => !it.Key)
                                  let classifier = new BinaryBayesianClassifier(
                                      classification.ToString(),
                                      requiredConfidenceRatio,
                                      members ?? Enumerable.Empty<string>(),
                                      nonMembers ?? Enumerable.Empty<string>(),
                                      logger)
                                  select new { classification, classifier })
                                  .ToDictionary(it => it.classification, it => it.classifier);

            this.logger.Debug("Training complete - created bayesian classifiers for {Classifications}",
                this.classifierMap.Keys);
        }

        public IEnumerable<Tag> Classify(string message)
        {
            return from pair in this.classifierMap
                   let classification = pair.Key
                   let classifier = pair.Value
                   where classifier.Classify(message)
                   select classification;
        }

        private static IEnumerable<Tag> GetClassifications()
        {
            return Enum.GetValues(typeof(Tag)).Cast<Tag>();
        }
    }
}
