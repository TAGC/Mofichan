using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mofichan.Core
{
    public struct ReplyContext
    {
        public ReplyContext(MessageContext message, MessageContext? reply = null)
        {
            this.Message = message;
            this.Reply = reply;
        }

        public MessageContext Message { get; }
        public MessageContext? Reply { get; }

        public ReplyContext WithReply(MessageContext reply)
        {
            return new ReplyContext(this.Message, reply);
        }
    }
}
