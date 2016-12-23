using System.Collections.Generic;

namespace Mofichan.DataAccess.Domain
{
    internal class TaggedMessage
    {
        public string Message { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public static TaggedMessage From(string body, params string[] tags)
        {
            return new TaggedMessage { Message = body, Tags = tags };
        }
    }
}
