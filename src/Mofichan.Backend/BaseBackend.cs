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

        protected virtual void SendMessage(MessageContext message)
        {
            message.To.ReceiveMessage(message.Body);
        }

        protected void OnReceiveMessage(IncomingMessage message)
        {
            this.target?.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
    }

    public class RoomOccupant : IRoomOccupant
    {
        private readonly IUser user;
        private readonly IRoom room;

        public RoomOccupant(IUser user, IRoom room)
        {
            this.user = user;
            this.room = room;
        }

        public string Name
        {
            get
            {
                return this.user.Name;
            }
        }

        public IRoom Room
        {
            get
            {
                return this.room;
            }
        }

        public UserType Type
        {
            get
            {
                return this.user.Type;
            }
        }

        public string UserId
        {
            get
            {
                return this.user.UserId;
            }
        }

        public void ReceiveMessage(string message)
        {
            this.room.ReceiveMessage(message);
        }
    }
}
