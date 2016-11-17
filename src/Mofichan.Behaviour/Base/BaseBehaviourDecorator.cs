using System;
using System.Collections.Generic;
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

        public virtual string Id
        {
            get
            {
                return DelegateBehaviour.Id;
            }
        }

        public virtual void Dispose()
        {
            DelegateBehaviour.Dispose();
        }

        public virtual void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
        {
            DelegateBehaviour.InspectBehaviourStack(stack);
        }

        public virtual void OnCompleted()
        {
            ((IObserver<IncomingMessage>)DelegateBehaviour).OnCompleted();
        }

        public virtual void OnError(Exception error)
        {
            ((IObserver<IncomingMessage>)DelegateBehaviour).OnError(error);
        }

        public virtual void OnNext(OutgoingMessage value)
        {
            DelegateBehaviour.OnNext(value);
        }

        public virtual void OnNext(IncomingMessage value)
        {
            DelegateBehaviour.OnNext(value);
        }

        public virtual void Start()
        {
            DelegateBehaviour.Start();
        }

        public virtual IDisposable Subscribe(IObserver<OutgoingMessage> observer)
        {
            return DelegateBehaviour.Subscribe(observer);
        }

        public virtual IDisposable Subscribe(IObserver<IncomingMessage> observer)
        {
            return DelegateBehaviour.Subscribe(observer);
        }

        public override string ToString()
        {
            return DelegateBehaviour.ToString();
        }
    }
}
