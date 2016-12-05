using System;
using System.Collections.Generic;
using System.Dynamic;
using Mofichan.DataAccess.Response;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.DataAccess
{
    internal static class ArticleResolverTestExtensions
    {
        public static ExpandoObject With(this ExpandoObject expando, string key, object value)
        {
            ((IDictionary<string, object>)expando)[key] = value;

            return expando;
        }
    }

    public class ArticleResolverTests
    {
        public static IEnumerable<object> Examples
        {
            get
            {
                yield return new object[]
                {
                    "Hello ${message.sender.name}!",

                    new ExpandoObject().With("message",
                        new ExpandoObject().With("sender",
                            new ExpandoObject().With("name", "John"))),

                    "Hello John!"
                };

                yield return new object[]
                {
                    "This is for ${message.recipient.name}!",

                    new ExpandoObject().With("message",
                        new ExpandoObject().With("recipient",
                            new ExpandoObject().With("name", "Amy"))),

                    "This is for Amy!"
                };

                yield return new object[]
                {
                    "Today is ${datetime.weekday} and I am ${mofichan.mood}",

                    new ExpandoObject().With("datetime",
                        new ExpandoObject().With("weekday", "Friday"))
                                       .With("mofichan",
                        new ExpandoObject().With("mood", "happy")),

                    "Today is Friday and I am happy"
                };
            }
        }

        private readonly IArticleResolver articleResolver;

        public ArticleResolverTests()
        {
            this.articleResolver = new ArticleResolver();
        }

        [Fact]
        internal void Article_Resolver_Should_Throw_Exception_If_Article_Cannot_Be_Resolved_From_Context()
        {
            // GIVEN a context with a given set of information.
            var context = new ExpandoObject().With("foo", "bar");

            // GIVEN an article relying on information not in the context.
            var article = "Today I was all like ${baz}";

            // EXPECT an exception is thrown when we try to resolve the article.
            Assert.Throws<ArgumentException>(() => this.articleResolver.Resolve(article, context));
        }

        [Theory]
        [MemberData(nameof(Examples))]
        internal void Article_Resolver_Should_Resolve_Article_From_Context_As_Expected(
            string unresolvedArticle,
            ExpandoObject context,
            string expectedResolvedArticle)
        {
            // WHEN we resolve the article using the given context.
            string actualResolvedArticle = this.articleResolver.Resolve(unresolvedArticle, context);

            // THEN the article should have been resolved as expected.
            actualResolvedArticle.ShouldBe(expectedResolvedArticle);
        }
    }
}
