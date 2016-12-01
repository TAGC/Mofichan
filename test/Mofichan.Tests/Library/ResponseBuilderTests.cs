using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Library;
using Mofichan.Library.Response;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Library
{
    public class ResponseBuilderTests
    {
        private static IEnumerable<TaggedMessage> ExampleArticles
        {
            get
            {
                yield return TaggedMessage.From("this is foo", "foo");
                yield return TaggedMessage.From("this is bar", "bar");
                yield return TaggedMessage.From("this is baz", "baz");
                yield return TaggedMessage.From("this is foo and bar", "foo", "bar");
                yield return TaggedMessage.From("this is foo, bar and baz", "foo", "bar", "baz");
                yield return TaggedMessage.From(
                    "${message.from.name} says ${message.body} to ${message.to.name}",
                    "requires-context");
            }
        }

        public static IEnumerable<object> ResponseFromTagExamples
        {
            get
            {
                yield return new object[]
                {
                    new[] { "foo" },
                    new[]
                    {
                        "this is foo",
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };

                yield return new object[]
                {
                    new[] { "foo", "bar" },
                    new[]
                    {
                        "this is foo",
                        "this is bar",
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };

                yield return new object[]
                {
                    new[] { "foo,bar" },
                    new[]
                    {
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };

                yield return new object[]
                {
                    new[] { "foo,bar", "baz" },
                    new[]
                    {
                        "this is baz",
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };
            }
        }

        private readonly IResponseBuilder responseBuilder;

        public ResponseBuilderTests()
        {
            var mockLibrary = new Mock<ILibrary>();
            mockLibrary.SetupGet(it => it.Articles).Returns(ExampleArticles);

            var articleFilter = new ArticleFilter(new[] { mockLibrary.Object });
            var articleResolver = new ArticleResolver();
            this.responseBuilder = new ResponseBuilder(articleFilter, articleResolver);
        }

        [Fact]
        public void Response_Builder_Should_Resolve_Message_Context_Placeholders()
        {
            // GIVEN a message context.
            var from = new Mock<IUser>();
            var to = new Mock<IUser>();
            var body = "Wingardium Leviosahhhh";

            from.SetupGet(it => it.Name).Returns("Ron Weasley");
            to.SetupGet(it => it.Name).Returns("Hermione Granger");

            var messageContext = new MessageContext(from: from.Object, to: to.Object, body: body);

            // WHEN we provide this message context to the response builder.
            this.responseBuilder.UsingContext(messageContext);

            // AND we configure the response builder to produce a response requiring a message context.
            this.responseBuilder.FromTags(prefix: string.Empty, tags: new[] { "requires-context" });

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should contain the context-specific information.
            response.ShouldBe("Ron Weasley says Wingardium Leviosahhhh to Hermione Granger");
        }

        [Fact]
        public void Response_Builder_Should_Allow_Responses_From_Raw_Strings()
        {
            // WHEN we configure the response builder to use a raw response part.
            var rawPart = "This is a plain, raw response string";
            this.responseBuilder.FromRaw(rawPart);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should be the raw string.
            response.ShouldBe(rawPart);
        }

        [Theory]
        [MemberData(nameof(ResponseFromTagExamples))]
        public void Response_Builder_Should_Choose_Appropriate_Response_Based_On_Tags(
            string[] tags, string[] possibleResponses)
        {
            // WHEN we configure the response builder to create a response from provided tags.
            this.responseBuilder.FromTags(prefix: string.Empty, tags: tags);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN an appropriate response should have been chosen.
            response.ShouldBeOneOf(possibleResponses);
        }

        [Theory]
        [MemberData(nameof(ResponseFromTagExamples))]
        public void Response_Builder_Should_Never_Choose_Tagged_Response_With_Zero_Probability(
            string[] tags, string[] _)
        {
            // WHEN we configure the response builder to create a response from provided tags with zero chance.
            this.responseBuilder.FromTags(chance: 0.0, tags: tags);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should be empty.
            response.ShouldBeEmpty();
        }

        [Fact]
        public void Response_Builder_Should_Choose_One_Of_Provided_Phrases()
        {
            // GIVEN a collection of possible phrases within a response.
            var phrases = new[]
            {
                "hey",
                "hi there",
                "how are you?"
            };

            // WHEN we configure the response builder to create a response from the provided phrases.
            this.responseBuilder.FromAnyOf(prefix: string.Empty, phrases: phrases);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should be one of the provided phrases.
            response.ShouldBeOneOf(phrases);
        }

        [Fact]
        public void Response_Builder_Should_Never_Choose_Phrase_With_Zero_Probability()
        {
            // GIVEN a collection of possible phrases within a response.
            var phrases = new[]
            {
                "hey",
                "hi there",
                "how are you?"
            };

            // WHEN we configure the response builder to create a response from the provided phrases with zero chance.
            this.responseBuilder.FromAnyOf(chance: 0.0, phrases: phrases);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should be empty.
            response.ShouldBeEmpty();
        }

        [Fact]
        public void Response_Builder_Should_Build_Response_From_Multiple_Parts()
        {
            // WHEN we configure the builder to start the response from a raw string.
            var rawPart = "Hello,";
            this.responseBuilder.FromRaw(rawPart);

            // AND we configure the builder to continue the response based on provided tags.
            this.responseBuilder.FromTags("foo", "baz");

            // AND we configure the builder to continue the response with a possible emote.
            var emotes = new[]
            {
                ":3",
                ":)",
                ":D"
            };

            this.responseBuilder.FromAnyOf(emotes);

            // AND we build the response.
            var response = this.responseBuilder.Build();

            // THEN the response should be one of an expected set.
            var possibleResponseTemplate = "Hello, {0}{1}";

            var possiblePhrases = new[]
            {
                "this is foo",
                "this is baz",
                "this is foo and bar",
                "this is foo, bar and baz"
            };

            var possibleResponses = from phrase in possiblePhrases
                                    from possibleEmote in emotes.Select(it => " " + it).Append(string.Empty)
                                    select string.Format(possibleResponseTemplate, phrase, possibleEmote);

            response.ShouldBeOneOf(possibleResponses.ToArray());
        }
    }
}
