using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// An implementation of <see cref="IMofichanBehaviour"/> that is composed of multiple
    /// sub-behaviours that are internally linked together.
    /// <para></para>
    /// This type can be subclassed to allow closely-related behaviours to be grouped together.
    /// </summary>
    public abstract class BaseMultiBehaviour : IMofichanBehaviour
    {
        private readonly List<IMofichanBehaviour> subBehaviours;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMultiBehaviour"/> class.
        /// </summary>
        /// <param name="subBehaviours">The sub-behaviours to use.</param>
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

        /// <summary>
        /// Gets the behaviour module identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public abstract string Id { get; }

        /// <summary>
        /// Gets the collection of sub-behaviours.
        /// </summary>
        /// <value>
        /// The sub-behaviours.
        /// </value>
        protected IEnumerable<IMofichanBehaviour> SubBehaviours
        {
            get
            {
                return this.subBehaviours;
            }
        }

        /// <summary>
        /// Gets the sub-behaviour closest to the root of the behaviour chain. 
        /// </summary>
        /// <value>
        /// The most upstream sub-behaviour.
        /// </value>
        protected IMofichanBehaviour MostUpstreamSubBehaviour
        {
            get
            {
                return this.subBehaviours.First();
            }
        }

        /// <summary>
        /// Gets the sub-behaviour furthest from the root of the behaviour chain.
        /// </summary>
        /// <value>
        /// The most downstream sub-behaviour.
        /// </value>
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

        /// <summary>
        /// Allows the behaviour to inspect the stack of behaviours Mofichan
        /// will be loaded with.
        /// </summary>
        /// <param name="stack">The behaviour stack.</param>
        /// <remarks>
        /// This method should be invoked before the behaviour <i>chain</i>
        /// is created.
        /// </remarks>
        public virtual void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            this.subBehaviours.ForEach(it => it.InspectBehaviourStack(stack));
        }

        /// <summary>
        /// Initialises the behaviour module.
        /// </summary>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Concat("[", this.Id, "]");
        }
    }
}
