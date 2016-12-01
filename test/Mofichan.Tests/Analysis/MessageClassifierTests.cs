using System.Collections.Generic;
using Mofichan.Core;
using Mofichan.Library;
using Mofichan.Library.Analysis;
using Serilog;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Analysis
{
    public class MessageClassifierFixture
    {
        private static IEnumerable<TaggedMessage> TrainingSet
        {
            get
            {
                yield return TaggedMessage.From("Hello there Mofi",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Hey Mofi",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Heya Mofichan",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Hey Mofichan ^-^",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Hi Mofi o/",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Hey there Mofichan o/",
                    "directedAtMofichan",
                    "greeting");

                yield return TaggedMessage.From("Hello Miriam :I",
                    "greeting");

                yield return TaggedMessage.From("Hello Ivan o/",
                    "greeting");

                yield return TaggedMessage.From("Nice weather today, right Mofi?",
                    "directedAtMofichan");

                yield return TaggedMessage.From("How are you, Mofi?",
                    "directedAtMofichan");

                yield return TaggedMessage.From("I think Adam is a bad person",
                    "negative");

                yield return TaggedMessage.From("Susan is a fairly bad person",
                    "negative");

                yield return TaggedMessage.From("John is definitely a bad person",
                    "negative");

                yield return TaggedMessage.From("Amy is fairly nice once you get to know her",
                    "positive");

                yield return TaggedMessage.From("I think Steve is a really nice guy",
                    "positive");

                yield return TaggedMessage.From("Mofi, you're a really bad chatbot",
                    "directedAtMofichan",
                    "negative");

                yield return TaggedMessage.From("Mofi, you're a great chatbot",
                    "directedAtMofichan",
                    "positive");

                yield return TaggedMessage.From("The capital of France is Paris?");
            }
        }

        public MessageClassifierFixture()
        {
            double requiredConfidenceRatioForUnitTesting = 0.9;

            this.Classifier = new MessageClassifier(new LoggerConfiguration().CreateLogger());
            this.Classifier.Train(TrainingSet, requiredConfidenceRatioForUnitTesting);
        }

        internal MessageClassifier Classifier { get; }
    }

    public class MessageClassifierTests : IClassFixture<MessageClassifierFixture>
    {
        public static IEnumerable<object[]> TestSet
        {
            get
            {
                yield return new object[]
                {
                    TaggedMessage.From("Hey Mofichan ^-^",
                        "directedAtMofichan",
                        "greeting")
                };

                yield return new object[]
                {
                    TaggedMessage.From("Hi Mofi o/",
                        "directedAtMofichan",
                        "greeting")
                };

                yield return new object[]
                {
                    TaggedMessage.From("Nice weather today Mofi :)",
                        "directedAtMofichan")
                };

                yield return new object[]
                {
                    TaggedMessage.From("Spain is an interesting place to visit")
                };

                yield return new object[]
                {
                    TaggedMessage.From("Mofi please stop being bad",
                    "directedAtMofichan",
                    "negative")
                };

                yield return new object[]
                {
                    TaggedMessage.From("I think Megan is pretty nice",
                    "positive")
                };
            }
        }

        private readonly MessageClassifier classifier;

        public MessageClassifierTests(MessageClassifierFixture classifierFixture)
        {
            this.classifier = classifierFixture.Classifier;
        }

        [Theory]
        [MemberData(nameof(TestSet))]
        internal void Message_Classifier_Should_Classify_Message_Correctly_After_Training(TaggedMessage taggedMessage)
        {
            // PRE: the message classifier should have been trained by the fixture.

            // WHEN we classify a message.
            var actualClassifications = new HashSet<string>(
                this.classifier.Classify(taggedMessage.Message));

            // THEN the expected tags should have been associated with it.
            var expectedClassifications = new HashSet<string>(
                taggedMessage.Tags);

            actualClassifications.ShouldBe(expectedClassifications);
        }
    }
}
