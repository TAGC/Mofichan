using System.Collections.Generic;
using Mofichan.Core;

namespace Mofichan.Library
{
    internal struct TaggedMessage
    {
        public static TaggedMessage From(string body, params Tag[] tags)
        {
            return new TaggedMessage(body, tags);
        }

        private TaggedMessage(string message, params Tag[] tags)
        {
            this.Message = message;
            this.Tags = tags;
        }

        public string Message { get; }
        public IEnumerable<Tag> Tags { get; }
    }
}
