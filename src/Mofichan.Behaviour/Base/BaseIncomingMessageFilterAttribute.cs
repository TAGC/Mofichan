using System;
using System.Reactive.Subjects;
using Mofichan.Core;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A type of attribute that can be applied to methods accepting a <see cref="IncomingMessage"/>
    /// to filter when they get invoked based on certain conditions.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class BaseIncomingMessageFilterAttribute : Attribute, ISubject<IncomingMessage>
    {
        private IObserver<IncomingMessage> observer;

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
        /// Called to notify this observer of an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public abstract void OnNext(IncomingMessage message);

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public IDisposable Subscribe(IObserver<IncomingMessage> observer)
        {
            this.observer = observer;
            return null;
        }

        /// <summary>
        /// Sends an incoming message to the downstream observer, if it exists.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected void SendDownstream(IncomingMessage message)
        {
            this.observer?.OnNext(message);
        }
    }
}
