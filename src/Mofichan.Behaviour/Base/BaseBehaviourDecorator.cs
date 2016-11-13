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
        private readonly IMofichanBehaviour delegateBehaviour;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviourDecorator"/> class.
        /// </summary>
        /// <param name="delegateBehaviour">The delegate behaviour.</param>
        protected BaseBehaviourDecorator(IMofichanBehaviour delegateBehaviour)
        {
            this.delegateBehaviour = delegateBehaviour;
        }

        public virtual Task Completion
        {
            get
            {
                return delegateBehaviour.Completion;
            }
        }

        public virtual string Id
        {
            get
            {
                return delegateBehaviour.Id;
            }
        }

        public virtual void Complete()
        {
            delegateBehaviour.Complete();
        }

        public virtual OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
        {
            return delegateBehaviour.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public virtual IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target, out bool messageConsumed)
        {
            return delegateBehaviour.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public virtual void Dispose()
        {
            delegateBehaviour.Dispose();
        }

        public virtual void Fault(Exception exception)
        {
            delegateBehaviour.Fault(exception);
        }

        public virtual void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            delegateBehaviour.InspectBehaviourStack(stack);
        }

        public virtual IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
        {
            return delegateBehaviour.LinkTo(target, linkOptions);
        }

        public virtual IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            return delegateBehaviour.LinkTo(target, linkOptions);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage messageValue, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            return delegateBehaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IncomingMessage messageValue, ISourceBlock<IncomingMessage> source, bool consumeToAccept)
        {
            return delegateBehaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            delegateBehaviour.ReleaseReservation(messageHeader, target);
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            delegateBehaviour.ReleaseReservation(messageHeader, target);
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            return delegateBehaviour.ReserveMessage(messageHeader, target);
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            return delegateBehaviour.ReserveMessage(messageHeader, target);
        }

        public virtual void Start()
        {
            delegateBehaviour.Start();
        }

        public override string ToString()
        {
            return delegateBehaviour.ToString();
        }
    }
}
