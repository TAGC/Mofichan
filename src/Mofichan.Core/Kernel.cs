using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core
{
    public class Kernel : IMessageContextHandler
    {
        public Kernel(IMofichanBackend backend, IEnumerable<IMofichanBehaviour> behaviours)
        {
        }

        #region Dataflow Methods
        public Task Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public MessageContext ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public IDisposable LinkTo(ITargetBlock<MessageContext> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, MessageContext messageValue, ISourceBlock<MessageContext> source, bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
