using System.Collections.Generic;
using System.Linq;
using Mofichan.Library;
using Mofichan.Library.Response;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Library
{
    public class ArticleFilterTests
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
            }
        }

        public static IEnumerable<object> ArticleFilterExamples
        {
            get
            {
                yield return new object[]
                {
                    "foo",
                    new[]
                    {
                        "this is foo",
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };

                yield return new object[]
                {
                    "foo,bar",
                    new[]
                    {
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };

                yield return new object[]
                {
                    "foo;bar",
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
                    "foo,bar;baz",
                    new[]
                    {
                        "this is baz",
                        "this is foo and bar",
                        "this is foo, bar and baz"
                    }
                };
            }
        }

        private readonly IArticleFilter articleFilter;

        public ArticleFilterTests()
        {
            var mockLibrary = new Mock<ILibrary>();
            mockLibrary.SetupGet(it => it.Articles).Returns(ExampleArticles);

            this.articleFilter = new ArticleFilter(new[] { mockLibrary.Object });
        }

        [Theory]
        [MemberData(nameof(ArticleFilterExamples))]
        internal void Articles_Should_Be_Filtered_Based_On_Tag_Requirements(
            string tagRequirementRepr, IEnumerable<string> expectedFilterArticles)
        {
            // GIVEN a tag requirement derived from its representation.
            var tagRequirement = TagRequirement.Parse(tagRequirementRepr);

            // WHEN we try to filter responses based on the provided tag requirement.
            var actualFilteredArticles = this.articleFilter.FilterByTagRequirement(tagRequirement);

            // THEN the article filter should have returned the expected set of articles.
            actualFilteredArticles.Count().ShouldBe(expectedFilterArticles.Count());
            expectedFilterArticles.ToList().ForEach(it => actualFilteredArticles.ShouldContain(it));
        }
    }
}
