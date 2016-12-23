using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;

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
        /// Initializes a new instance of the <see cref="BaseMultiBehaviour" /> class.
        /// </summary>
        /// <param name="chainBuilder">
        /// The object to use for composing <paramref name="subBehaviours"/> into a chain.
        /// </param>
        /// <param name="subBehaviours">The sub-behaviours to use.</param>
        public BaseMultiBehaviour(IBehaviourChainBuilder chainBuilder, params IMofichanBehaviour[] subBehaviours)
        {
            this.subBehaviours = subBehaviours.ToList();
            chainBuilder.BuildChain(this.subBehaviours);
        }

        /// <summary>
        /// Gets the behaviour module identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public virtual string Id
        {
            get
            {
                return this.GetType().GetTypeInfo().Name.Replace("Behaviour", string.Empty)
                    .ToLowerInvariant();
            }
        }

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
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public virtual void OnCompleted()
        {
            this.subBehaviours.ForEach(it => it.OnCompleted());
        }

        /// <summary>
        /// Called to notify this observer of an incoming visitor.
        /// </summary>
        /// <param name="visitor">The incoming visitor.</param>
        public virtual void OnNext(IBehaviourVisitor visitor)
        {
            var behaviour = this.MostUpstreamSubBehaviour;
            behaviour.OnNext(visitor);
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public virtual void OnError(Exception error)
        {
            this.subBehaviours.ForEach(it => it.OnError(error));
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public virtual IDisposable Subscribe(IObserver<IBehaviourVisitor> observer)
        {
            return this.MostDownstreamSubBehaviour.Subscribe(observer);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("[", this.Id, "]");
        }
    }
}
