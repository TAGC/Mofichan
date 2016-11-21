using System.Collections.Generic;

namespace Mofichan.Library
{
    internal struct TaggedArticle
    {
        public static TaggedArticle From(string body, params string[] tags)
        {
            return new TaggedArticle(body, tags);
        }

        private TaggedArticle(string body, params string[] tags)
        {
            this.Body = body;
            this.Tags = tags;
        }

        public string Body { get; }
        public IEnumerable<string> Tags { get; }
    }
}
