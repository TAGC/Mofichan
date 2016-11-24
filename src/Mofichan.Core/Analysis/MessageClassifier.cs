using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                   where classifier.Classify(message)
                   select classification;
        }

        private static IEnumerable<MessageClassification> GetClassifications()
        {
            return Enum.GetValues(typeof(MessageClassification)).Cast<MessageClassification>();
        }

        private class BinaryBayesianClassifier
        {
            private static readonly IEnumerable<string> IgnoredTerms = StopWords;

            private readonly IDictionary<string, double> positiveLikelihoods;
            private readonly IDictionary<string, double> negativeLikelihoods;

            private static IEnumerable<string> StopWords
            {
                get
                {
                    var assembly = typeof(MessageClassifier).GetTypeInfo().Assembly;
                    var resourcePath = "Mofichan.Core.Resources.BayesianStopWords.txt";

                    using (var stream = assembly.GetManifestResourceStream(resourcePath))
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            yield return line.Trim().ToLowerInvariant();
                        }
                    }
                }
            }

            public BinaryBayesianClassifier(
                IEnumerable<string> positiveExamples,
                IEnumerable<string> negativeExamples)
            {
                var combinedLikelihoods = CalculateLikelihoods(positiveExamples, negativeExamples);

                this.positiveLikelihoods = combinedLikelihoods[true];
                this.negativeLikelihoods = combinedLikelihoods[false];
            }

            public bool Classify(string message)
            {
                var wordFrequencies = GetWordFrequenciesWithinString(message);
                var positivePosterior = CalculatePosterior(this.positiveLikelihoods, wordFrequencies);
                var negativePosterior = CalculatePosterior(this.negativeLikelihoods, wordFrequencies);

                return positivePosterior > negativePosterior;
            }

            private static double CalculatePosterior(
                IDictionary<string, double> likelihoods,
                IDictionary<string, int> wordFrequencies)
            {
                var terms = from pair in likelihoods
                            let word = pair.Key
                            let likelihood = pair.Value
                            let occurrences = wordFrequencies.TryGetValueWithDefault(word, 0)
                            select occurrences * Math.Log(likelihood);

                /*
                 * Note: the class prior is not included in this calculation.
                 * 
                 * Only the class-conditional likelihoods are considered for the time being.
                 */
                return terms.Sum();
            }

            private static IDictionary<bool, IDictionary<string, double>> CalculateLikelihoods(
                IEnumerable<string> positiveExamples, IEnumerable<string> negativeExamples)
            {
                IDictionary<string, int> positiveWordFreqs = GetWordFrequenciesWithinExamples(positiveExamples);
                IDictionary<string, int> negativeWordFreqs = GetWordFrequenciesWithinExamples(negativeExamples);
                IEnumerable<string> vocabulary = positiveWordFreqs.Keys.Union(negativeWordFreqs.Keys);

                var posteriors = new Dictionary<bool, IDictionary<string, double>>();
                posteriors[true] = CalculateLikelihoodsForClass(positiveWordFreqs, vocabulary);
                posteriors[false] = CalculateLikelihoodsForClass(negativeWordFreqs, vocabulary);

                return posteriors;
            }

            private static IDictionary<string, double> CalculateLikelihoodsForClass(
                IDictionary<string, int> classWordFrequencies,
                IEnumerable<string> vocabulary)
            {
                double totalWordsForClass = classWordFrequencies.Sum(it => it.Value);
                int vocabularySize = vocabulary.Count();

                return (from term in vocabulary
                        let occurrencesOfTermInClass = classWordFrequencies.TryGetValueWithDefault(term, 0)
                        let termLikelihood = (1 + occurrencesOfTermInClass) / (vocabularySize + totalWordsForClass)
                        select new { term, termLikelihood })
                       .ToDictionary(it => it.term, it => it.termLikelihood);
            }

            private static IDictionary<string, int> GetWordFrequenciesWithinExamples(IEnumerable<string> examples)
            {
                return (from example in examples
                        let exampleWordFreqs = GetWordFrequenciesWithinString(example)
                        from kvp in exampleWordFreqs
                        group kvp by kvp.Key)
                        .ToDictionary(x => x.Key, it => it.Sum(y => y.Value));
            }

            private static IDictionary<string, int> GetWordFrequenciesWithinString(string input)
            {
                return Regex.Matches(input, @"[\w']+")
                     .Cast<Match>()
                     .Select(it => it.Value)
                     .Where(x => x != string.Empty)
                     .Where(x => !IgnoredTerms.Contains(x.ToLowerInvariant()))
                     .GroupBy(x => x)
                     .ToDictionary(x => x.Key, x => x.Count());
            }
        }
    }
}
