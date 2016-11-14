using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
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
    }

    /// <summary>
    /// A base implementation of <see cref="IMofichanBehaviour"/>. 
    /// </summary>
    public abstract class BaseBehaviour : IMofichanBehaviour
    {
        private ITargetBlock<IncomingMessage> downstreamTarget;
        private ITargetBlock<OutgoingMessage> upstreamTarget;
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

        Task IDataflowBlock.Completion
        {
            get
            {
                throw new NotImplementedException();
            }
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
                return this
                    .GetType()
                    .GetTypeInfo()
                    .Name
                    .Replace("Behaviour", string.Empty)
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

        public virtual void Complete()
        {
            // Override if necessary.
        }

        public virtual void Fault(Exception exception)
        {
            // Override if necessary.
            throw exception;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Override if necessary.
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
        /// This method will only be invoked if <code>CanHandleIncomingMessage(message)</code> is <code>true</code>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected abstract void HandleIncomingMessage(IncomingMessage message);

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <code>CanHandleOutgoingMessage(message)</code> is <code>true</code>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected abstract void HandleOutgoingMessage(OutgoingMessage message);

        #region Incoming Message Handling
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IncomingMessage message,
            ISourceBlock<IncomingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            if (this.CanHandleIncomingMessage(message))
            {
                this.HandleIncomingMessage(message);
                return DataflowMessageStatus.Accepted;
            }
            else if (this.passThroughMessages && this.downstreamTarget != null)
            {
                return this.downstreamTarget.OfferMessage(messageHeader, message, this, consumeToAccept);
            }
            else
            {
                return DataflowMessageStatus.Declined;
            }
        }

        public IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.downstreamTarget = target;
            return null;
        }

        public IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target,
            out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        protected void SendDownstream(IncomingMessage message)
        {
            this.downstreamTarget.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
        #endregion

        #region Outgoing Message Handling

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage message,
            ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            if (this.CanHandleOutgoingMessage(message))
            {
                this.HandleOutgoingMessage(message);
                return DataflowMessageStatus.Accepted;
            }
            else if (this.passThroughMessages)
            {
                return this.upstreamTarget.OfferMessage(messageHeader, message, this, consumeToAccept);
            }
            else
            {
                return DataflowMessageStatus.Declined;
            }
        }

        public IDisposable LinkTo(ITargetBlock<OutgoingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.upstreamTarget = target;
            return null;
        }

        public OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
        {
            throw new NotImplementedException();
        }

        protected void SendUpstream(OutgoingMessage message)
        {
            this.upstreamTarget.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }
        #endregion

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
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
