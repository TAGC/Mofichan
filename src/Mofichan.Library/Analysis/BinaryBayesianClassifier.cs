using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mofichan.Core;
using Mofichan.Core.Utility;
using Serilog;

namespace Mofichan.Library.Analysis
{
    internal class BinaryBayesianClassifier
    {
        /// <summary>
        /// Determines how confident the classifier must be about a message
        /// in order to declare that it should be positively classified.
        /// </summary>
        /// <remarks>
        /// The lower this ratio, the more confident the classifier must be.
        /// <para></para>
        /// This ratio should be in the range [0, 1].
        /// </remarks>
        private readonly double requiredConfidenceRatio;

        private static readonly IEnumerable<string> IgnoredTerms = StopWords;

        private readonly string classifierId;
        private readonly IDictionary<string, double> positiveLikelihoods;
        private readonly IDictionary<string, double> negativeLikelihoods;
        private readonly ILogger logger;

        private static IEnumerable<string> StopWords
        {
            get
            {
                var assembly = typeof(MessageClassifier).GetTypeInfo().Assembly;
                var resourcePath = "Mofichan.Library.Resources.AnalysisLib.stopwords.txt";

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
            string classifierId,
            double requiredConfidenceRatio,
            IEnumerable<string> positiveExamples,
            IEnumerable<string> negativeExamples,
            ILogger logger)
        {
            var combinedLikelihoods = CalculateLikelihoods(positiveExamples, negativeExamples);

            this.classifierId = classifierId;
            this.positiveLikelihoods = combinedLikelihoods[true];
            this.negativeLikelihoods = combinedLikelihoods[false];
            this.requiredConfidenceRatio = requiredConfidenceRatio;
            this.logger = logger.ForContext<BinaryBayesianClassifier>();
        }

        public bool Classify(string message)
        {
            var preprocessedMessage = PreprocessMessage(message);

            var wordFrequencies = GetWordFrequenciesWithinString(preprocessedMessage);
            var positiveLogPosterior = CalculateLogPosterior(this.positiveLikelihoods, wordFrequencies);
            var negativeLogPosterior = CalculateLogPosterior(this.negativeLikelihoods, wordFrequencies);
            
            this.logger.Verbose("{ClassifierId} Classifying message with word frequencies: {WordFrequencies}",
                this.classifierId, wordFrequencies);
            this.logger.Verbose("{ClassifierId} log(P[positive_posterior]) = {PositiveLogPosterior}",
                this.classifierId, positiveLogPosterior);
            this.logger.Verbose("{ClassifierId} log(P[negative_posterior]) = {NegativeLogPosterior}",
                this.classifierId, negativeLogPosterior);

            /*
             * Ratios < 1 mean that there is more confidence in a positive classification than a
             * negative classification.
             * 
             * The lower the ratio, the more confident that message should be classified positively.
             */
            double confidenceRatio = Math.Exp(negativeLogPosterior - positiveLogPosterior);
            bool positiveClassification = confidenceRatio <= this.requiredConfidenceRatio;

            this.logger.Verbose("{ClassifierId} Calculated confidence ratio = {ConfidenceRatio}",
                this.classifierId, confidenceRatio);
            this.logger.Verbose("{ClassifierId} Classification success = {ClassificationSuccess}",
                this.classifierId, positiveClassification);

            return positiveClassification;
        }

        private static double CalculateLogPosterior(
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
                 .Select(it => it.Value.ToLowerInvariant())
                 .Where(x => x != string.Empty)
                 .Where(x => !IgnoredTerms.Contains(x))
                 .GroupBy(x => x)
                 .ToDictionary(x => x.Key, x => x.Count());
        }

        private static string PreprocessMessage(string message)
        {
            return Regex.Matches(message, @"[\w']+")
                .Cast<Match>()
                .Select(it => it.Value.ToLowerInvariant())
                .Aggregate((e, a) => e + " " + a);
        }
    }
}
