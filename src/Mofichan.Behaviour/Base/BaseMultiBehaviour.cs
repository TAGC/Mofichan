using System;
using System.Collections.Generic;
using System.Linq;
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

                upstreamBehaviour.Subscribe<IncomingMessage>(downstreamBehaviour.OnNext);
                downstreamBehaviour.Subscribe<OutgoingMessage>(upstreamBehaviour.OnNext);
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void Dispose()
        {
            this.subBehaviours.ForEach(it => it.Dispose());
        }

        public virtual void OnNext(IncomingMessage message)
        {
            var behaviour = this.MostUpstreamSubBehaviour;
            behaviour.OnNext(message);
        }

        public virtual void OnNext(OutgoingMessage message)
        {
            var behaviour = this.MostDownstreamSubBehaviour;
            behaviour.OnNext(message);
        }

        public virtual IDisposable Subscribe(IObserver<IncomingMessage> observer)
        {
            return this.MostDownstreamSubBehaviour.Subscribe(observer);
        }

        public virtual IDisposable Subscribe(IObserver<OutgoingMessage> observer)
        {
            return this.MostUpstreamSubBehaviour.Subscribe(observer);
        }

        public override string ToString()
        {
            return string.Concat("[", this.Id, "]");
        }

        public virtual void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public virtual void OnError(Exception error)
        {
            throw new NotImplementedException();
        }
    }
}
