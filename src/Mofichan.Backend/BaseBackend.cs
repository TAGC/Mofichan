using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Backend
{
    public abstract class BaseBackend : IMofichanBackend
    {
        private ITargetBlock<IncomingMessage> target;

        public BaseBackend()
        {
        }

        #region Dataflow Members
        Task IDataflowBlock.Completion
        {
            get
            {
                throw new NotImplementedException();
            }
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

        public virtual IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target,
            out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public virtual void Join(string roomId)
        {
            var room = this.GetRoomById(roomId);
            room.Join();
        }

        public virtual void Leave(string roomId)
        {
            var room = this.GetRoomById(roomId);
            room.Leave();
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
        #endregion

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        public abstract void Start();
        protected abstract IUser GetUserById(string userId);
        protected abstract IRoom GetRoomById(string roomId);
        protected abstract void SendMessage(MessageContext message);

        protected void OnReceiveMessage(IncomingMessage message)
        {
            this.target?.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
    }
}
