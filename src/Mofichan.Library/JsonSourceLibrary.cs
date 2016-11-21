using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Mofichan.Library
{
    internal class JsonSourceLibrary : ILibrary
    {
        public JsonSourceLibrary(TextReader sourceReader)
        {
            this.Articles = LoadArticles(sourceReader.ReadToEnd());
        }

        public IEnumerable<TaggedArticle> Articles { get; }

        private static IEnumerable<TaggedArticle> LoadArticles(string source)
        {
            JArray articles = JArray.Parse(source);

            return from articleNode in articles.Children()
                   let article = articleNode["article"].Value<string>()
                   let tags = from tagNode in ((JArray)articleNode["tags"]).Children()
                              select tagNode.Value<string>()
                   select TaggedArticle.From(article, tags.ToArray());
        }
    }
}
