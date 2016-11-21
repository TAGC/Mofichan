using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mofichan.Library;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Library
{
    public class JsonSourceLibraryTests
    {
        public static IEnumerable<object> Examples
        {
            get
            {
                yield return new object[]
                {
                    new StringBuilder("[")
                        .Append(BuildJsonArticle("this is foo", "foo"))
                        .Append("]")
                        .ToString(),

                    new[]
                    {
                        TaggedArticle.From("this is foo", "foo")
                    }
                };

                yield return new object[]
                {
                    new StringBuilder("[")
                        .Append(BuildJsonArticle("this is foo", "foo")).Append(",")
                        .Append(BuildJsonArticle("this is foo and bar", "foo", "bar"))
                        .Append("]")
                        .ToString(),

                    new[]
                    {
                        TaggedArticle.From("this is foo", "foo"),
                        TaggedArticle.From("this is foo and bar", "foo", "bar")
                    }
                };

                yield return new object[]
                {
                    new StringBuilder("[")
                        .Append(BuildJsonArticle("this is foo", "foo")).Append(",")
                        .Append(BuildJsonArticle("this is foo and bar", "foo", "bar")).Append(",")
                        .Append(BuildJsonArticle("this is foo, bar and baz", "foo", "bar", "baz"))
                        .Append("]")
                        .ToString(),

                    new[]
                    {
                        TaggedArticle.From("this is foo", "foo"),
                        TaggedArticle.From("this is foo and bar", "foo", "bar"),
                        TaggedArticle.From("this is foo, bar and baz", "foo", "bar", "baz")
                    }
                };
            }
        }

        private static string BuildJsonArticle(string article, params string[] tags)
        {
            var tagList = string.Join(",", tags.Select(it => "\"" + it + "\""));

            return "{ \"article\": \"" + article + "\", \"tags\": [" + tagList + "]}";
        }

        [Theory]
        [MemberData(nameof(Examples))]
        internal void Json_Source_Library_Should_Provide_Expected_Articles_When_Loaded_From_Json_Source(
            string jsonSource, IEnumerable<TaggedArticle> expectedArticles)
        {
            // GIVEN a JsonSourceLibrary constructed using the provided source.
            var library = new JsonSourceLibrary(new StringReader(jsonSource));

            // EXPECT that the library provides the expected set of articles.
            var actualArticles = library.Articles;

            actualArticles.Count().ShouldBe(expectedArticles.Count());

            foreach (var pair in actualArticles.Zip(expectedArticles, (actual, expected) => new { actual, expected }))
            {
                pair.actual.Article.ShouldBe(pair.expected.Article);
                pair.actual.Tags.Count().ShouldBe(pair.expected.Tags.Count());
                pair.expected.Tags.ToList().ForEach(it => pair.actual.Tags.ShouldContain(it));
            }
        }
    }
}
