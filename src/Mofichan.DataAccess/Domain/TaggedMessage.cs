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

        public override bool Equals(object obj)
        {
            var other = obj as TaggedMessage;

            if (other == null)
            {
                return false;
            }

            return this.Message.Equals(other.Message) &&
                this.Tags.SequenceEqual(other.Tags);
        }

        public override int GetHashCode()
        {
            int hashCode = 17;

            hashCode += 31 * this.Message.GetHashCode();

            foreach (var tag in this.Tags)
            {
                hashCode += 31 * tag.GetHashCode();
            }

            return hashCode;
        }

        public override string ToString()
        {
            var tags = this.Tags.Select(it => "#" + it);
            return string.Format("{0} {1}", this.Message, string.Join(", ", tags));
        }
    }
}
