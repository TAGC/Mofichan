using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
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
                yield return TaggedMessage.From("this is happy", Tag.Happy);
                yield return TaggedMessage.From("this is positive", Tag.Positive);
                yield return TaggedMessage.From("this is cute", Tag.Cute);
                yield return TaggedMessage.From("this is happy and positive", Tag.Happy, Tag.Positive);
                yield return TaggedMessage.From("this is happy, positive and cute", Tag.Happy, Tag.Positive, Tag.Cute);
            }
        }

        public static IEnumerable<object> ArticleFilterExamples
        {
            get
            {
                yield return new object[]
                {
                    Tag.Happy.AsGroup(),
                    new[]
                    {
                        "this is happy",
                        "this is happy and positive",
                        "this is happy, positive and cute"
                    }
                };

                yield return new object[]
                {
                    Tag.Happy.And(Tag.Positive).AsGroup(),
                    new[]
                    {
                        "this is happy and positive",
                        "this is happy, positive and cute"
                    }
                };

                yield return new object[]
                {
                    Tag.Happy.Or(Tag.Positive).AsGroup(),
                    new[]
                    {
                        "this is happy",
                        "this is positive",
                        "this is happy and positive",
                        "this is happy, positive and cute"
                    }
                };

                yield return new object[]
                {
                    Tag.Happy.And(Tag.Positive).Or(Tag.Cute).AsGroup(),
                    new[]
                    {
                        "this is cute",
                        "this is happy and positive",
                        "this is happy, positive and cute"
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
            IEnumerable<IEnumerable<Tag>> tagGroup, IEnumerable<string> expectedFilterArticles)
        {
            // GIVEN a tag requirement derived from the tag group.
            var tagRequirement = TagRequirement.From(tagGroup);

            // WHEN we try to filter responses based on the provided tag requirement.
            var actualFilteredArticles = this.articleFilter.FilterByTagRequirement(tagRequirement);

            // THEN the article filter should have returned the expected set of articles.
            actualFilteredArticles.Count().ShouldBe(expectedFilterArticles.Count());
            expectedFilterArticles.ToList().ForEach(it => actualFilteredArticles.ShouldContain(it));
        }
    }
}
