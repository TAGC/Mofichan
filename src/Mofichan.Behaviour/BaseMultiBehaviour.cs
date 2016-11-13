using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour
{
    public abstract class BaseMultiBehaviour : IMofichanBehaviour
    {
        private readonly List<IMofichanBehaviour> subBehaviours;

        public BaseMultiBehaviour(params IMofichanBehaviour[] subBehaviours)
        {
            this.subBehaviours = subBehaviours.ToList();

            /*
             * Ensure sub-behaviours are internally linked.
             */
            for (var i = 0; i < this.subBehaviours.Count - 1; i++)
            {
                var upstreamBehaviour = this.subBehaviours[i];
                var downstreamBehaviour = this.subBehaviours[i + 1];

                upstreamBehaviour.LinkTo<IncomingMessage>(downstreamBehaviour);
                downstreamBehaviour.LinkTo<OutgoingMessage>(upstreamBehaviour);
            }
        }

        public abstract string Id { get; }

        protected IEnumerable<IMofichanBehaviour> SubBehaviours
        {
            get
            {
                return this.subBehaviours;
            }
        }

        protected IMofichanBehaviour MostUpstreamSubBehaviour
        {
            get
            {
                return this.subBehaviours.First();
            }
        }

        protected IMofichanBehaviour MostDownstreamSubBehaviour
        {
            get
            {
                return this.subBehaviours.Last();
            }
        }

        public Task Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            this.subBehaviours.ForEach(it => it.InspectBehaviourStack(stack));
        }

        public virtual void Start()
        {
            this.subBehaviours.ForEach(it => it.Start());
        }

        public virtual IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
        {
            return this.MostUpstreamSubBehaviour.LinkTo(target);
        }

        public virtual IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            return this.MostDownstreamSubBehaviour.LinkTo(target);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
            OutgoingMessage messageValue, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            var behaviour = this.MostDownstreamSubBehaviour;
            return behaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader,
            IncomingMessage messageValue, ISourceBlock<IncomingMessage> source, bool consumeToAccept)
        {
            var behaviour = this.MostUpstreamSubBehaviour;
            return behaviour.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public virtual IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        public virtual OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        public virtual void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        public virtual void Complete()
        {
            throw new NotImplementedException();
        }

        public virtual void Fault(Exception exception)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
