using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Core.Analysis
{
    public struct TaggedMessage
    {
        public TaggedMessage(string message, IEnumerable<MessageClassification> classifications)
            : this(message, classifications.ToArray())
        {
        }

        public TaggedMessage(string message, params MessageClassification[] classifications)
        {
            this.Message = message;
            this.Classifications = classifications;
        }

        public string Message { get; }
        public IEnumerable<MessageClassification> Classifications { get; }
    }
}
