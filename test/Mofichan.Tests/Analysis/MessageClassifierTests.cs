using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Analysis;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Analysis
{
    public class MessageClassifierFixture
    {
        public static IEnumerable<TaggedMessage> TrainingSet
        {
            get
            {
                yield return new TaggedMessage("Hello there Mofi",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hey Mofi",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Heya Mofichan",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hey Mofichan ^-^",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hi Mofi o/",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hey there Mofichan o/",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hello Miriam :I",
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Hello Ivan o/",
                    MessageClassification.Greeting);

                yield return new TaggedMessage("Nice weather today, right Mofi?",
                    MessageClassification.DirectedAtMofichan);

                yield return new TaggedMessage("How are you, Mofi?",
                    MessageClassification.DirectedAtMofichan);

                yield return new TaggedMessage("I think Adam is a bad person",
                    MessageClassification.Unpleasant);

                yield return new TaggedMessage("Susan is a fairly bad person",
                    MessageClassification.Unpleasant);

                yield return new TaggedMessage("John is definitely a bad person",
                        MessageClassification.Unpleasant);

                yield return new TaggedMessage("Amy is fairly nice once you get to know her",
                    MessageClassification.Pleasant);

                yield return new TaggedMessage("I think Steve is a really nice guy",
                    MessageClassification.Pleasant);

                yield return new TaggedMessage("Mofi, you're a really bad chatbot",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Unpleasant);

                yield return new TaggedMessage("Mofi, you're a great chatbot",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Pleasant);

                yield return new TaggedMessage("The capital of France is Paris?");
            }
        }

        public MessageClassifierFixture()
        {
            this.Classifier = new MessageClassifier();
            this.Classifier.Train(TrainingSet);
        }

        public MessageClassifier Classifier { get; }
    }

    public class MessageClassifierTests : IClassFixture<MessageClassifierFixture>
    {
        public static IEnumerable<object[]> TestSet
        {
            get
            {
                yield return new object[]
                {
                    new TaggedMessage("Hey Mofichan ^-^",
                        MessageClassification.DirectedAtMofichan,
                        MessageClassification.Greeting)
                };

                yield return new object[]
                {
                    new TaggedMessage("Hi Mofi o/",
                        MessageClassification.DirectedAtMofichan,
                        MessageClassification.Greeting)
                };

                yield return new object[]
                {
                    new TaggedMessage("Nice weather today Mofi :)",
                        MessageClassification.DirectedAtMofichan)
                };

                yield return new object[]
                {
                    new TaggedMessage("Spain is an interesting place to visit")
                };

                yield return new object[]
                {
                    new TaggedMessage("Mofi please stop being bad",
                    MessageClassification.DirectedAtMofichan,
                    MessageClassification.Unpleasant)
                };

                yield return new object[]
                {
                    new TaggedMessage("I think Megan is pretty nice",
                    MessageClassification.Pleasant)
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
        public void Message_Classifier_Should_Classify_Message_Correctly_After_Training(TaggedMessage taggedMessage)
        {
            // PRE: the message classifier should have been trained by the fixture.

            // WHEN we classify a message.
            var actualClassifications = new HashSet<MessageClassification>(
                this.classifier.Classify(taggedMessage.Message));

            // THEN the expected classifications should have been associated with it.
            var expectedClassifications = new HashSet<MessageClassification>(
                taggedMessage.Classifications);

            actualClassifications.ShouldBe(expectedClassifications);
        }
    }
}
