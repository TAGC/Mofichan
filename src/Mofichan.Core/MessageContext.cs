using Mofichan.Core.Interfaces;

namespace Mofichan.Core
{
    public struct MessageContext
    {
        public enum MessageDirection
        {
            Incoming,
            Outgoing
        }

        public MessageContext(IMessageTarget from, IMessageTarget to, MessageDirection direction, string body)
        {
            this.From = from;
            this.To = to;
            this.Direction = direction;
            this.Body = body;
        }

        public IMessageTarget From { get; }
        public IMessageTarget To { get; }
        public MessageDirection Direction { get; }
        public string Body { get; }
    }
}
