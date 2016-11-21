using System.Collections.Generic;

namespace Mofichan.Library
{
    internal struct TaggedArticle
    {
        public static TaggedArticle From(string body, params string[] tags)
        {
            return new TaggedArticle(body, tags);
        }

        private TaggedArticle(string article, params string[] tags)
        {
            this.Article = article;
            this.Tags = tags;
        }

        public string Article { get; }
        public IEnumerable<string> Tags { get; }
    }
}
