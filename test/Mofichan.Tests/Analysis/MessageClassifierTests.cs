using System.Collections.Generic;
using Mofichan.Core;
using Mofichan.Library;
using Mofichan.Library.Analysis;
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
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Hey Mofi",
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Heya Mofichan",
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Hey Mofichan ^-^",
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Hi Mofi o/",
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Hey there Mofichan o/",
                    Tag.DirectedAtMofichan,
                    Tag.Greeting);

                yield return TaggedMessage.From("Hello Miriam :I",
                    Tag.Greeting);

                yield return TaggedMessage.From("Hello Ivan o/",
                    Tag.Greeting);

                yield return TaggedMessage.From("Nice weather today, right Mofi?",
                    Tag.DirectedAtMofichan);

                yield return TaggedMessage.From("How are you, Mofi?",
                    Tag.DirectedAtMofichan);

                yield return TaggedMessage.From("I think Adam is a bad person",
                    Tag.Negative);

                yield return TaggedMessage.From("Susan is a fairly bad person",
                    Tag.Negative);

                yield return TaggedMessage.From("John is definitely a bad person",
                    Tag.Negative);

                yield return TaggedMessage.From("Amy is fairly nice once you get to know her",
                    Tag.Positive);

                yield return TaggedMessage.From("I think Steve is a really nice guy",
                    Tag.Positive);

                yield return TaggedMessage.From("Mofi, you're a really bad chatbot",
                    Tag.DirectedAtMofichan,
                    Tag.Negative);

                yield return TaggedMessage.From("Mofi, you're a great chatbot",
                    Tag.DirectedAtMofichan,
                    Tag.Positive);

                yield return TaggedMessage.From("The capital of France is Paris?");
            }
        }

        public MessageClassifierFixture()
        {
            double requiredConfidenceRatioForUnitTesting = 0.9;

            this.Classifier = new MessageClassifier();
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
                        Tag.DirectedAtMofichan,
                        Tag.Greeting)
                };

                yield return new object[]
                {
                    TaggedMessage.From("Hi Mofi o/",
                        Tag.DirectedAtMofichan,
                        Tag.Greeting)
                };

                yield return new object[]
                {
                    TaggedMessage.From("Nice weather today Mofi :)",
                        Tag.DirectedAtMofichan)
                };

                yield return new object[]
                {
                    TaggedMessage.From("Spain is an interesting place to visit")
                };

                yield return new object[]
                {
                    TaggedMessage.From("Mofi please stop being bad",
                    Tag.DirectedAtMofichan,
                    Tag.Negative)
                };

                yield return new object[]
                {
                    TaggedMessage.From("I think Megan is pretty nice",
                    Tag.Positive)
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
            var actualClassifications = new HashSet<Tag>(
                this.classifier.Classify(taggedMessage.Message));

            // THEN the expected tags should have been associated with it.
            var expectedClassifications = new HashSet<Tag>(
                taggedMessage.Tags);

            actualClassifications.ShouldBe(expectedClassifications);
        }
    }
}
