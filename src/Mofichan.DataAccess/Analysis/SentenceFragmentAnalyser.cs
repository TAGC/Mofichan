using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.DataAccess.Analysis
{
    /// <summary>
    /// A type of <see cref="IMessageClassifier"/> that breaks messages down into sentence fragments
    /// and uses a delegate message classifier to classify each fragment individually.
    /// <para></para>
    /// The classifications returned by instances of this class are unions of the classifications
    /// assigned to each fragment.
    /// </summary>
    public class SentenceFragmentAnalyser : IMessageClassifier
    {
        private readonly IMessageClassifier delegateClassifier;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceFragmentAnalyser"/> class.
        /// </summary>
        /// <param name="delegateClassifier">The classifier to use for classifying sentence fragments.</param>
        /// <param name="logger">The logger to use.</param>
        public SentenceFragmentAnalyser(IMessageClassifier delegateClassifier, ILogger logger)
        {
            this.delegateClassifier = delegateClassifier;
            this.logger = logger.ForContext<SentenceFragmentAnalyser>();
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
            var fragments = GetFragments(message).Prepend(message);

            this.logger.Verbose("Decomposed {Message} into {Fragments}", message, fragments);

            return fragments.SelectMany(it => this.delegateClassifier.Classify(it)).Distinct();
        }

        private static IEnumerable<string> GetFragments(string message)
        {
            return Regex.Split(message, @"([,]|[.])\s*");
        }
    }
}
