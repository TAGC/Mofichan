using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            var tags = this.Tags.Select(it => "#" + it);
            return string.Format("{0} {1}", this.Message, string.Join(", ", tags));
        }
    }
}
