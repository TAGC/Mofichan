using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Backend
{
    public abstract class BaseBackend : IMofichanBackend
    {
        public BaseBackend()
        {
        }

        public abstract Task Completion { get; }

        public abstract void Start();
        public abstract void Complete();
        public abstract MessageContext ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target, out bool messageConsumed);
        public abstract void Fault(Exception exception);
        public abstract void Join(string roomId);
        public abstract void Leave(string roomId);
        public abstract IDisposable LinkTo(ITargetBlock<MessageContext> target, DataflowLinkOptions linkOptions);
        public abstract DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, MessageContext messageValue, ISourceBlock<MessageContext> source, bool consumeToAccept);
        public abstract void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target);
        public abstract bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target);
        public abstract void Dispose();
    }
}
