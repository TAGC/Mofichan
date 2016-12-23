using System;
using System.Collections.Generic;
using System.Reflection;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A base implementation of <see cref="IMofichanBehaviour"/>. 
    /// </summary>
    public abstract class BaseBehaviour : IMofichanBehaviour
    {
        private IObserver<IBehaviourVisitor> observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviour" /> class.
        /// </summary>
        protected BaseBehaviour()
        {
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
            // Override if necessary.
        }

        /// <summary>
        /// Initialises the behaviour module.
        /// </summary>
        public virtual void Start()
        {
            // Override if necessary.
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public virtual void OnCompleted()
        {
            // Override if necessary.
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            // Override if necessary.
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public IDisposable Subscribe(IObserver<IBehaviourVisitor> observer)
        {
            this.observer = observer;
            return null;
        }

        /// <summary>
        /// Called to notify this observer of an incoming visitor.
        /// </summary>
        /// <param name="visitor">The incoming visitor.</param>
        public void OnNext(IBehaviourVisitor visitor)
        {
            if (visitor is OnMessageVisitor)
            {
                this.HandleMessageVisitor((OnMessageVisitor)visitor);
            }
            else if (visitor is OnPulseVisitor)
            {
                this.HandlePulseVisitor((OnPulseVisitor)visitor);
            }
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

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor"/>.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected virtual void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            this.observer?.OnNext(visitor);
        }

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnPulseVisitor"/>.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected virtual void HandlePulseVisitor(OnPulseVisitor visitor)
        {
            this.observer?.OnNext(visitor);
        }
    }
}
