using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Core.Utility;

namespace Mofichan.Core.Analysis
{
    public class MessageClassifier
    {
        private IDictionary<MessageClassification, BinaryBayesianClassifier> classifierMap;

        public void Train(IEnumerable<TaggedMessage> trainingSet)
        {
            this.classifierMap = (from classification in GetClassifications()
                                  let memberSplit = from o in trainingSet
                                                    group o.Message by o.Classifications.Contains(classification)
                                  let members = memberSplit.FirstOrDefault(it => it.Key)
                                  let nonMembers = memberSplit.FirstOrDefault(it => !it.Key)
                                  let classifier = new BinaryBayesianClassifier(
                                      members ?? Enumerable.Empty<string>(),
                                      nonMembers ?? Enumerable.Empty<string>())
                                  select new { classification, classifier })
                                  .ToDictionary(it => it.classification, it => it.classifier);
        }

        public IEnumerable<MessageClassification> Classify(string message)
        {
            return from pair in this.classifierMap
                   let classification = pair.Key
                   let classifier = pair.Value
                   where classifier.Classify(message, classification)
                   select classification;
        }

        private static IEnumerable<MessageClassification> GetClassifications()
        {
            return Enum.GetValues(typeof(MessageClassification)).Cast<MessageClassification>();
        }

        private class BinaryBayesianClassifier
        {
            private readonly IDictionary<string, double> positivePosteriors;
            private readonly IDictionary<string, double> negativePosteriors;

            public BinaryBayesianClassifier(
                IEnumerable<string> positiveExamples,
                IEnumerable<string> negativeExamples)
            {
                double numPositives = positiveExamples.Count();
                double numNegatives = negativeExamples.Count();
                double totalExamples = numPositives + numNegatives;
                var combinedPosteriors = CalculatePosteriors(positiveExamples, negativeExamples);

                this.positivePosteriors = combinedPosteriors[true];
                this.negativePosteriors = combinedPosteriors[false];
            }

            public bool Classify(string message, MessageClassification temp)
            {
                var wordFrequencies = GetWordFrequencies(message).WithDefaultValue(0);
                var positiveLikelihood = CalculateLikelihood(this.positivePosteriors, wordFrequencies);
                var negativeLikelihood = CalculateLikelihood(this.negativePosteriors, wordFrequencies);

                return positiveLikelihood > negativeLikelihood;
            }

            private static double CalculateLikelihood(
                IDictionary<string, double> posteriors,
                IDictionary<string, int> wordFrequencies)
            {
                var totalWords = wordFrequencies.Sum(it => it.Value);

                var terms = from pair in posteriors
                            let word = pair.Key
                            let posterior = pair.Value
                            let occurrences = wordFrequencies[word]
                            select Math.Pow(posterior, occurrences);

                /*
                 * Note: the class prior is not included in this calculation.
                 * 
                 * Only the posteriors are considered for the time being.
                 */
                return terms.Aggregate(1.0, (e, a) => e * a);
            }

            private static IDictionary<bool, IDictionary<string, double>> CalculatePosteriors(
                IEnumerable<string> positiveExamples, IEnumerable<string> negativeExamples)
            {
                IDictionary<string, int> positiveWordFreqs = GetWordFrequencies(positiveExamples);
                IDictionary<string, int> negativeWordFreqs = GetWordFrequencies(negativeExamples);
                IEnumerable<string> vocabulary = positiveWordFreqs.Keys.Union(negativeWordFreqs.Keys);

                var posteriors = new Dictionary<bool, IDictionary<string, double>>();
                posteriors[true] = CreatePosteriorsForClass(positiveWordFreqs, vocabulary);
                posteriors[false] = CreatePosteriorsForClass(negativeWordFreqs, vocabulary);

                return posteriors;
            }

            private static IDictionary<string, double> CreatePosteriorsForClass(
                IDictionary<string, int> classWordFrequencies,
                IEnumerable<string> vocabulary)
            {
                var posteriors = new Dictionary<string, double>();

                double totalWordsForClass = classWordFrequencies.Sum(it => it.Value);

                return (from term in vocabulary
                        let positiveOccurrencesOfTerm = classWordFrequencies[term]
                        let termPosterior = positiveOccurrencesOfTerm / totalWordsForClass
                        select new { term, termPosterior })
                       .ToDictionary(it => it.term, it => it.termPosterior);
            }

            private static IDictionary<string, int> GetWordFrequencies(IEnumerable<string> examples)
            {
                return (from example in examples
                        let exampleWordFreqs = GetWordFrequencies(example)
                        from kvp in exampleWordFreqs
                        group kvp by kvp.Key)
                        .ToDictionary(x => x.Key, it => it.Sum(y => y.Value))
                        .WithDefaultValue(0);
            }

            private static IDictionary<string, int> GetWordFrequencies(string input)
            {
                return Regex.Matches(input, @"[\w']+")
                     .Cast<Match>()
                     .Select(it => it.Value)
                     .Where(x => x != string.Empty)
                     .GroupBy(x => x)
                     .ToDictionary(x => x.Key, x => x.Count());
            }
        }
    }
}
