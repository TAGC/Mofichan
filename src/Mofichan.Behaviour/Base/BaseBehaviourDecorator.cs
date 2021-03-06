﻿using System;
using System.Collections.Generic;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;

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
        /// <param name="delegateBehaviour">The delegate behaviour.</param>
        protected BaseBehaviourDecorator(IMofichanBehaviour delegateBehaviour)
        {
            this.DelegateBehaviour = delegateBehaviour;
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
                return this.DelegateBehaviour.Id;
            }
        }

        /// <summary>
        /// Gets the delegate behaviour.
        /// </summary>
        /// <value>
        /// The delegate behaviour.
        /// </value>
        protected IMofichanBehaviour DelegateBehaviour { get; }

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
            this.DelegateBehaviour.InspectBehaviourStack(stack);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public virtual void OnCompleted()
        {
            this.DelegateBehaviour.OnCompleted();
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public virtual void OnError(Exception error)
        {
            this.DelegateBehaviour.OnError(error);
        }

        /// <summary>
        /// Called to notify this observer of an incoming visitor.
        /// </summary>
        /// <param name="visitor">The incoming visitor.</param>
        public virtual void OnNext(IBehaviourVisitor visitor)
        {
            this.DelegateBehaviour.OnNext(visitor);
        }

        /// <summary>
        /// Initialises the behaviour module.
        /// </summary>
        public virtual void Start()
        {
            this.DelegateBehaviour.Start();
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
            return this.DelegateBehaviour.Subscribe(observer);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.DelegateBehaviour.ToString();
        }
    }
}
