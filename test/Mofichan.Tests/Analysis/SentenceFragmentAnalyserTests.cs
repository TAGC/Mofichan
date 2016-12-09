using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.DataAccess.Analysis;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Analysis
{
    public class SentenceFragmentAnalyserTests
    {
        [Fact]
        public void Sentence_Fragment_Analyser_Should_Decompose_Messages_Appropriately()
        {
            // GIVEN a mock message classifier.
            var mockClassifier = new Mock<IMessageClassifier>();
            mockClassifier
                .Setup(it => it.Classify(It.IsAny<string>()))
                .Returns(Enumerable.Empty<string>());

            // GIVEN a sentence fragment analyser that uses this mock for analysis.
            var fragmentAnalyser = new SentenceFragmentAnalyser(mockClassifier.Object, MockLogger.Instance);

            // GIVEN a message to analyse.
            var message = "Hello Mofi, how are you doing today?";

            // WHEN we try to analyse the message.

#pragma warning disable S2201 // Return values should not be ignored when function calls don't have any side effects
            fragmentAnalyser.Classify(message).ToArray();
#pragma warning restore S2201 // Return values should not be ignored when function calls don't have any side effects

            // THE fragment analyser should decompose the message into fragments before passing them to the delegate.
            mockClassifier.Verify(it => it.Classify("Hello Mofi, how are you doing today?"), Times.Once);
            mockClassifier.Verify(it => it.Classify("Hello Mofi"), Times.Once);
            mockClassifier.Verify(it => it.Classify("how are you doing today?"), Times.Once);
        }

        [Fact]
        public void Sentence_Fragment_Analyser_Should_Union_Classifications_Of_Fragments()
        {
            // GIVEN a mock message classifier.
            var mockClassifier = new Mock<IMessageClassifier>();
            mockClassifier
                .Setup(it => it.Classify("Hello Mofi, how are you doing today?"))
                .Returns(new[] { "directedAtMofichan" });

            mockClassifier
                .Setup(it => it.Classify("Hello Mofi"))
                .Returns(new[] { "directedAtMofichan", "greeting" });

            mockClassifier
                .Setup(it => it.Classify("how are you doing today?"))
                .Returns(new[] { "wellbeing" });

            // GIVEN a sentence fragment analyser that uses this mock for analysis.
            var fragmentAnalyser = new SentenceFragmentAnalyser(mockClassifier.Object, MockLogger.Instance);

            // GIVEN a message to analyse.
            var message = "Hello Mofi, how are you doing today?";

            // WHEN we analyse the message.
            var classifications = fragmentAnalyser.Classify(message);

            // THEN the expected classifications should have been produced.
            classifications.ShouldBe(new[] { "directedAtMofichan", "greeting", "wellbeing" }, ignoreOrder: true);
        }
    }
}
