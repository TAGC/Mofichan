using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mofichan.DataAccess.Domain;
using Newtonsoft.Json.Linq;

namespace Mofichan.DataAccess
{
    internal class JsonSourceLibrary : ILibrary
    {
        public JsonSourceLibrary(TextReader sourceReader)
        {
            this.Articles = LoadArticles(sourceReader.ReadToEnd());
        }

        public IEnumerable<TaggedMessage> Articles { get; }

        private static IEnumerable<TaggedMessage> LoadArticles(string source)
        {
            JArray articles = JArray.Parse(source);

            return from articleNode in articles.Children()
                   let article = articleNode["article"].Value<string>()
                   let tags = from tagNode in ((JArray)articleNode["tags"]).Children()
                              select tagNode.Value<string>()
                   select TaggedMessage.From(article, tags.ToArray());
        }
    }
}
