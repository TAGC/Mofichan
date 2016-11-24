using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Library.Analysis
{
    internal class MessageClassifier : IMessageClassifier
    {
        private IDictionary<Tag, BinaryBayesianClassifier> classifierMap;

        public void Train(IEnumerable<TaggedMessage> trainingSet, double requiredConfidenceRatio)
        {
            this.classifierMap = (from classification in GetClassifications()
                                  let memberSplit = from o in trainingSet
                                                    group o.Message by o.Tags.Contains(classification)
                                  let members = memberSplit.FirstOrDefault(it => it.Key)
                                  let nonMembers = memberSplit.FirstOrDefault(it => !it.Key)
                                  let classifier = new BinaryBayesianClassifier(requiredConfidenceRatio,
                                      members ?? Enumerable.Empty<string>(),
                                      nonMembers ?? Enumerable.Empty<string>())
                                  select new { classification, classifier })
                                  .ToDictionary(it => it.classification, it => it.classifier);
        }

        public IEnumerable<Tag> Classify(string message)
        {
            return from pair in this.classifierMap
                   let classification = pair.Key
                   let classifier = pair.Value
                   where classifier.Classify(message, classification)
                   select classification;
        }

        private static IEnumerable<Tag> GetClassifications()
        {
            return Enum.GetValues(typeof(Tag)).Cast<Tag>();
        }
    }
}
