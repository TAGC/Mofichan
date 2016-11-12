using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Backend
{
    public abstract class BaseBackend : IMofichanBackend
    {
        private ITargetBlock<IncomingMessage> target;

        Task IDataflowBlock.Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BaseBackend()
        {
        }

        public IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.target = target;
            return null;
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage message,
            ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            this.SendMessage(message.Context);

            return DataflowMessageStatus.Accepted;
        }

        public abstract void Complete();
        public abstract IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target,
            out bool messageConsumed);
        public abstract void Dispose();
        public abstract void Fault(Exception exception);
        public abstract void Join(string roomId);
        public abstract void Leave(string roomId);
        public abstract void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target);
        public abstract bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target);

        protected abstract IUser GetUserById(string userId);
        protected abstract IRoom GetRoomById(string roomId);
        protected abstract void SendMessage(MessageContext message);

        void IMofichanBackend.Join(string roomId)
        {
            throw new NotImplementedException();
        }

        void IMofichanBackend.Leave(string roomId)
        {
            throw new NotImplementedException();
        }

        DataflowMessageStatus ITargetBlock<OutgoingMessage>.OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage messageValue, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            throw new NotImplementedException();
        }

        IDisposable ISourceBlock<IncomingMessage>.LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        IncomingMessage ISourceBlock<IncomingMessage>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        bool ISourceBlock<IncomingMessage>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<IncomingMessage>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Complete()
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
