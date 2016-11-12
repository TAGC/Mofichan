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
        private ITargetBlock<ReplyContext> downstreamTarget;
        private ITargetBlock<ReplyContext> upstreamTarget;
        private bool passThroughMessages;

        public abstract Task Completion { get; }

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

        public abstract void Dispose();

        protected abstract bool CanHandleIncomingMessage(ReplyContext context);
        protected abstract bool CanHandleOutgoingMessage(ReplyContext context);
        protected abstract void HandleIncomingMessage(ReplyContext context);
        protected abstract void HandleOutgoingMessage(ReplyContext context);

        void IReplyContextHandler.Start()
        {
            throw new NotImplementedException();
        }

        #region DataFlow Members
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, ReplyContext replyContext,
            ISourceBlock<ReplyContext> source, bool consumeToAccept)
        {
            var message = replyContext.Message;

            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            if (message.Direction == MessageContext.MessageDirection.Outgoing
                && this.CanHandleOutgoingMessage(replyContext))
            {
                this.HandleOutgoingMessage(replyContext);
            }
            else if (this.CanHandleIncomingMessage(replyContext))
            {
                this.HandleIncomingMessage(replyContext);
            }

            if (this.passThroughMessages && this.downstreamTarget != null)
            {
                this.downstreamTarget.OfferMessage(messageHeader, replyContext, source, consumeToAccept);
                return DataflowMessageStatus.Accepted;
            }

            return DataflowMessageStatus.Declined;
        }

        IDisposable ISourceBlock<ReplyContext>.LinkTo(ITargetBlock<ReplyContext> target,
            DataflowLinkOptions linkOptions)
        {
            if (this.downstreamTarget == null)
            {
                this.downstreamTarget = target;
            }
            else if (this.upstreamTarget == null)
            {
                this.upstreamTarget = target;
            }
            else
            {
                throw new InvalidOperationException("The downstream and upstream targets have already been linked");
            }

            return null;
        }

        ReplyContext ISourceBlock<ReplyContext>.ConsumeMessage(DataflowMessageHeader messageHeader,
            ITargetBlock<ReplyContext> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        bool ISourceBlock<ReplyContext>.ReserveMessage(DataflowMessageHeader messageHeader,
            ITargetBlock<ReplyContext> target)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<ReplyContext>.ReleaseReservation(DataflowMessageHeader messageHeader,
            ITargetBlock<ReplyContext> target)
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
    }
}
