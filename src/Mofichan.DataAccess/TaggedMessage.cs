using System.Collections.Generic;

namespace Mofichan.DataAccess
{
    internal struct TaggedMessage
    {
        private TaggedMessage(string message, params string[] tags)
        {
            this.Message = message;
            this.Tags = tags;
        }

        public string Message { get; }

        public IEnumerable<string> Tags { get; }

        public static TaggedMessage From(string body, params string[] tags)
        {
            return new TaggedMessage(body, tags);
        }
    }
}
