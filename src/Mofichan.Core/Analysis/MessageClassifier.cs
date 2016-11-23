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
            var likelihoods = (from pair in this.classifierMap
                               let classification = pair.Key
                               let classifier = pair.Value
                               let likelihood = classifier.GetMembershipLikelihood(message, classification)
                               select new { classification, likelihood })
                               .ToDictionary(it => it.classification, it => it.likelihood);

            /**
             * Normalising purely for convenience during tests.
             */
            var normalised = Normalise(likelihoods.Select(it => it.Value));
            var averageLikelihood = normalised.Average();

            var normalisedLikelihoods = likelihoods.Keys.Zip(
                normalised, (classification, likelihood) => new { classification, likelihood });

            return from entry in normalisedLikelihoods
                   where entry.likelihood >= averageLikelihood
                   select entry.classification;
        }

        private static IEnumerable<double> Normalise(IEnumerable<double> input)
        {
            var min = input.Min();
            var max = input.Max();
            var range = max - min;

            return from o in input select (o - min) / range;
        }

        private static IEnumerable<MessageClassification> GetClassifications()
        {
            return Enum.GetValues(typeof(MessageClassification)).Cast<MessageClassification>();
        }

        private class BinaryBayesianClassifier
        {
            private readonly double prior;
            private readonly IDictionary<string, double> posteriors;

            public BinaryBayesianClassifier(
                IEnumerable<string> positiveExamples,
                IEnumerable<string> negativeExamples)
            {
                this.prior = CalculatePrior(positiveExamples, negativeExamples);
                this.posteriors = CalculatorPosteriors(positiveExamples);
            }

            public double GetMembershipLikelihood(string message, MessageClassification temp)
            {
                var wordFrequencies = GetWordFrequencies(message);
                var uniqueWords = wordFrequencies.Select(it => it.Key);

                var wordPosteriors = from word in uniqueWords
                                     let posterior = this.posteriors[word]
                                     select posterior;

                var likelihood = prior * wordPosteriors.Aggregate((a, e) => a * e);

                return likelihood;
            }

            private static double CalculatePrior(IEnumerable<string> positiveExamples,
                IEnumerable<string> negativeExamples)
            {
                double numPositives = positiveExamples.Count();
                double numNegatives = negativeExamples.Count();

                return numPositives / (numPositives + numNegatives);
            }

            private static IDictionary<string, double> CalculatorPosteriors(IEnumerable<string> positiveExamples)
            {
                var allWordFreqs = (from example in positiveExamples
                                    let exampleWordFreqs = GetWordFrequencies(example)
                                    from kvp in exampleWordFreqs
                                    group kvp by kvp.Key)
                                    .ToDictionary(x => x.Key, it => it.Sum(y => y.Value));

                var uniqueWords = allWordFreqs.Select(it => it.Key);
                var numUniqueWords = uniqueWords.Count();
                var totalWordCount = allWordFreqs.Sum(it => it.Value);

                double defaultPosterior;
                if (positiveExamples.Any())
                {
                    defaultPosterior = 1 / (double)(numUniqueWords + totalWordCount);
                }
                else
                {
                    defaultPosterior = 0;
                }

                return (from word in uniqueWords
                        let wordFreq = allWordFreqs[word]
                        let posterior = (1 + wordFreq) / (double)(numUniqueWords + totalWordCount)
                        select new { word, posterior })
                        .ToDictionary(it => it.word, it => it.posterior)
                        .WithDefaultValue(defaultPosterior);
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
