using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour
{
    public abstract class BaseBehaviour : IMofichanBehaviour
    {
        private ITargetBlock<IncomingMessage> downstreamTarget;
        private ITargetBlock<OutgoingMessage> upstreamTarget;
        private bool passThroughMessages;

        Task IDataflowBlock.Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual void Start()
        {
            // Override if necessary.
        }

        public virtual void Complete()
        {
            // Override if necessary.
        }

        public virtual void Fault(Exception exception)
        {
            // Override if necessary.
            throw exception;
        }

        public abstract void Dispose();

        protected abstract bool CanHandleIncomingMessage(IncomingMessage message);
        protected abstract bool CanHandleOutgoingMessage(OutgoingMessage message);
        protected abstract void HandleIncomingMessage(IncomingMessage message);
        protected abstract void HandleOutgoingMessage(OutgoingMessage message);

        #region Incoming Message Handling
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IncomingMessage message,
            ISourceBlock<IncomingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            if (this.CanHandleIncomingMessage(message))
            {
                this.HandleIncomingMessage(message);
                return DataflowMessageStatus.Accepted;
            }
            else if (this.passThroughMessages)
            {
                return this.downstreamTarget.OfferMessage(messageHeader, message, this, consumeToAccept);
            }
            else
            {
                return DataflowMessageStatus.Declined;
            }
        }

        public IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.downstreamTarget = target;
            return null;
        }

        public IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target,
            out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        protected void SendDownstream(IncomingMessage message)
        {
            this.downstreamTarget.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
        #endregion

        #region Outgoing Message Handling

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage message,
            ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            if (this.CanHandleOutgoingMessage(message))
            {
                this.HandleOutgoingMessage(message);
                return DataflowMessageStatus.Accepted;
            }
            else if (this.passThroughMessages)
            {
                return this.upstreamTarget.OfferMessage(messageHeader, message, this, consumeToAccept);
            }
            else
            {
                return DataflowMessageStatus.Declined;
            }
        }

        public IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.upstreamTarget = target;
            return null;
        }

        public OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        protected void SendUpstream(OutgoingMessage message)
        {
            this.upstreamTarget.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
        #endregion

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
