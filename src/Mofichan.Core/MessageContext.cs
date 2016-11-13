using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core
{
    public struct MessageContext
    {
        public MessageContext(IMessageTarget from, IMessageTarget to, string body, TimeSpan? delay = null)
        {
            this.From = from;
            this.To = to;
            this.Body = body;
            this.Delay = delay ?? TimeSpan.Zero;
        }

        public IMessageTarget From { get; }
        public IMessageTarget To { get; }
        public string Body { get; }
        public TimeSpan Delay { get; }

        public override string ToString()
        {
            return string.Format("Message (from={0}, to={1}, body={2} delay={3})",
                this.From, this.To, this.Body, this.Delay);
        }
    }

    public struct IncomingMessage
    {
        public IncomingMessage(MessageContext context, MessageContext? potentialReply = null)
        {
            this.Context = context;
            this.PotentialReply = potentialReply;
        }

        public MessageContext Context { get; }
        public MessageContext? PotentialReply { get; }

        public IncomingMessage WithReply(MessageContext reply)
        {
            return new IncomingMessage(this.Context, reply);
        }

        public override string ToString()
        {
            return string.Format("Incoming message (context={0}, potential reply={1})",
                this.Context, this.PotentialReply);
        }
    }

    public struct OutgoingMessage
    {
        public MessageContext Context { set;  get; }

        public override string ToString()
        {
            return string.Format("Outgoing message (context={0}, delay={1})", this.Context);
        }
    }
}
