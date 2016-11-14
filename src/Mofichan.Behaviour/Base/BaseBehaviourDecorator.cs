using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A base implementation of an <see cref="IMofichanBehaviour"/> that wraps around another. 
    /// </summary>
    public abstract class BaseBehaviourDecorator : IMofichanBehaviour
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviourDecorator"/> class.
        /// </summary>
        /// <param name="DelegateBehaviour">The delegate behaviour.</param>
        protected BaseBehaviourDecorator(IMofichanBehaviour DelegateBehaviour)
        {
            this.DelegateBehaviour = DelegateBehaviour;
        }

        protected IMofichanBehaviour DelegateBehaviour { get; }

        public virtual Task Completion
        {
            get
            {
                return DelegateBehaviour.Completion;
            }
        }

        public virtual string Id
        {
            get
            {
                return DelegateBehaviour.Id;
            }
        }

        public virtual void Complete()
        {
            DelegateBehaviour.Complete();
        }

        public virtual OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
        {
            return DelegateBehaviour.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public virtual IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target, out bool messageConsumed)
        {
            return DelegateBehaviour.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public virtual void Dispose()
        {
            DelegateBehaviour.Dispose();
        }

        public virtual void Fault(Exception exception)
        {
            DelegateBehaviour.Fault(exception);
        }

        public virtual void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            DelegateBehaviour.InspectBehaviourStack(stack);
        }

        public virtual IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
        {
            return DelegateBehaviour.LinkTo(target, linkOptions);
        }

        public virtual IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            return DelegateBehaviour.LinkTo(target, linkOptions);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage messageValue, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            return DelegateBehaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IncomingMessage messageValue, ISourceBlock<IncomingMessage> source, bool consumeToAccept)
        {
            return DelegateBehaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            DelegateBehaviour.ReleaseReservation(messageHeader, target);
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            DelegateBehaviour.ReleaseReservation(messageHeader, target);
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            return DelegateBehaviour.ReserveMessage(messageHeader, target);
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            return DelegateBehaviour.ReserveMessage(messageHeader, target);
        }

        public virtual void Start()
        {
            DelegateBehaviour.Start();
        }

        public override string ToString()
        {
            return DelegateBehaviour.ToString();
        }
    }
}
