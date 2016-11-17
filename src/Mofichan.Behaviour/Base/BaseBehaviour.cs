using System;
using System.Collections.Generic;
using System.Reflection;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Base
{
    internal static class Extensions
    {
        /// <summary>
        /// Forms an <see cref="OutgoingMessage"/> in response to an <see cref="IncomingMessage"/>
        /// with a specified body.
        /// </summary>
        /// <param name="message">The message to form a reply to.</param>
        /// <param name="replyBody">The body of the reply.</param>
        /// <returns>The generated reply.</returns>
        public static OutgoingMessage Reply(this IncomingMessage message, string replyBody)
        {
            var from = message.Context.From;
            var to = message.Context.To;

            var replyContext = new MessageContext(from: to, to: from, body: replyBody);

            return new OutgoingMessage { Context = replyContext };
        }

        /// <summary>
        /// Checks the sender of a particular message has an administration role and
        /// throws a <see cref="MofichanAuthorisationException"/> if not.
        /// </summary>
        /// <param name="context">The message context.</param>
        public static void CheckSenderAuthorised(this MessageContext context, string exceptionBody)
        {
            var sender = context.From as IUser;

            if (sender != null && sender.Type != UserType.Adminstrator)
            {
                throw new MofichanAuthorisationException(exceptionBody, context);
            }
        }
    }

    /// <summary>
    /// A base implementation of <see cref="IMofichanBehaviour"/>. 
    /// </summary>
    public abstract class BaseBehaviour : IMofichanBehaviour
    {
        private IObserver<IncomingMessage> downstreamObserver;
        private IObserver<OutgoingMessage> upstreamObserver;
        private bool passThroughMessages;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviour"/> class.
        /// </summary>
        /// <param name="passThroughMessages">
        /// If set to <c>true</c>, unhandled messages will automatically
        /// be passed downstream.
        /// </param>
        protected BaseBehaviour(bool passThroughMessages = true)
        {
            this.passThroughMessages = passThroughMessages;
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
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
            // Override if necssary.
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public IDisposable Subscribe(IObserver<IncomingMessage> observer)
        {
            this.downstreamObserver = observer;
            return null;
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        public IDisposable Subscribe(IObserver<OutgoingMessage> observer)
        {
            this.upstreamObserver = observer;
            return null;
        }

        /// <summary>
        /// Called to notify this observer of an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public void OnNext(IncomingMessage message)
        {
            if (this.CanHandleIncomingMessage(message))
            {
                this.HandleIncomingMessage(message);
            }
            else if (this.passThroughMessages && this.downstreamObserver != null)
            {
                this.downstreamObserver.OnNext(message);
            }
        }

        /// <summary>
        /// Called to notify this observer of an outgoing message.
        /// </summary>
        /// <param name="message">The outgoing message.</param>
        public void OnNext(OutgoingMessage message)
        {
            if (this.CanHandleOutgoingMessage(message))
            {
                this.HandleOutgoingMessage(message);
            }
            else if (this.passThroughMessages && this.upstreamObserver != null)
            {
                this.upstreamObserver.OnNext(message);
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
        /// Determines whether this instance can process the specified incoming message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the incoming message; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanHandleIncomingMessage(IncomingMessage message);

        /// <summary>
        /// Determines whether this instance can process the specified outgoing message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the outgoing messagee; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanHandleOutgoingMessage(OutgoingMessage message);

        /// <summary>
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected abstract void HandleIncomingMessage(IncomingMessage message);

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected abstract void HandleOutgoingMessage(OutgoingMessage message);

        /// <summary>
        /// Sends an incoming message to the downstream observer, if it exists.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected void SendDownstream(IncomingMessage message)
        {
            this.downstreamObserver?.OnNext(message);
        }

        /// <summary>
        /// Sends an outgoing message to the upstream observer, if it exists.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected void SendUpstream(OutgoingMessage message)
        {
            this.upstreamObserver?.OnNext(message);
        }
    }
}
