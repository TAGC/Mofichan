using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Analysis;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Analysis
{
    public class MessageClassifierTests
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
                    new TaggedMessage("Spain is a nice place to visit")
                };
            }
        }

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

                yield return new TaggedMessage("The capital of France is Paris?");
            }
        }

        private readonly MessageClassifier classifier;

        public MessageClassifierTests()
        {
            this.classifier = new MessageClassifier();
        }

        [Theory]
        [MemberData(nameof(TestSet))]
        public void Message_Classifier_Should_Classify_Message_When_Trained(TaggedMessage taggedMessage)
        {
            // GIVEN the classifier has been trained using the training set.
            this.classifier.Train(TrainingSet);

            // WHEN we classify a message.
            var actualClassifications = this.classifier.Classify(taggedMessage.Message);

            // THEN the expected classifications should have been associated with it.
            var expectedClassifications = taggedMessage.Classifications;

            actualClassifications.Count().ShouldBe(expectedClassifications.Count());
            expectedClassifications.ToList().ForEach(it => actualClassifications.ShouldContain(it));
        }
    }
}
