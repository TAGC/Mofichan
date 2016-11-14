using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Backend
{
    /// <summary>
    /// A base implementation of <see cref="IMofichanBackend" /></summary>
    /// <seealso cref="Mofichan.Core.Interfaces.IMofichanBackend" />
    public abstract class BaseBackend : IMofichanBackend
    {
        private ITargetBlock<IncomingMessage> target;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBackend"/> class.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        protected BaseBackend(ILogger logger)
        {
            this.Logger = logger;
        }

        protected ILogger Logger { get; }

        #region Dataflow Members
        Task IDataflowBlock.Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IDisposable LinkTo(ITargetBlock<IncomingMessage> target, DataflowLinkOptions linkOptions)
        {
            this.target = target;
            return null;
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage message,
            ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
        {
            if (consumeToAccept)
            {
                bool consumeSuccessful;
                source.ConsumeMessage(messageHeader, this, out consumeSuccessful);

                Debug.Assert(consumeSuccessful);
            }

            this.SendMessage(message.Context);

            return DataflowMessageStatus.Accepted;
        }

        public virtual IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target,
            out bool messageConsumed)
        {
            throw new NotImplementedException();
        }

        public virtual void Join(string roomId)
        {
            var room = this.GetRoomById(roomId);
            room.Join();
        }

        public virtual void Leave(string roomId)
        {
            var room = this.GetRoomById(roomId);
            room.Leave();
        }

        bool ISourceBlock<IncomingMessage>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        void ISourceBlock<IncomingMessage>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Complete()
        {
            throw new NotImplementedException();
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Initialises the backend.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Override if necessary.
        }

        /// <summary>
        /// Tries to retrieve a user by its ID.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user corresponding to the ID, if it exists.</returns>
        protected abstract IUser GetUserById(string userId);

        /// <summary>
        /// Tries to retrieve a room by its ID.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>The room corresponding to the ID, if it exists.</returns>
        protected abstract IRoom GetRoomById(string roomId);

        /// <summary>
        /// Sends the message into the wider world.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected virtual void SendMessage(MessageContext message)
        {
            if (message.Delay > TimeSpan.Zero)
            {
                this.Logger.Debug("Sending {MessageBody} to {Recipient} with {Delay} delay",
                    message.Body, message.To, message.Delay);
                Task.Run(() => HandleMessageDelayAsync(message))
                    .ContinueWith(_ => message.To.ReceiveMessage(message.Body));
            }
            else
            {
                this.Logger.Debug("Sending {MessageBody} to {Recipient} with no delay",
                    message.Body, message.To);
                message.To.ReceiveMessage(message.Body);
            }

        }

        /// <summary>
        /// Called when a message is received by this backend.
        /// </summary>
        /// <param name="message">The received message.</param>
        protected void OnReceiveMessage(IncomingMessage message)
        {
            var fromSelf = message.Context.From is IUser
                && (message.Context.From as IUser).Type == UserType.Self;

            if (fromSelf)
            {
                this.Logger.Verbose("Received {MessageBody} from Mofichan herself",
                    message.Context.Body);
            }
            else
            {
                this.Logger.Debug("Received {MessageBody} from {Sender}",
                    message.Context.Body, message.Context.From);
            }

            this.target?.OfferMessage(default(DataflowMessageHeader), message, this, false);
        }

        /// <summary>
        /// Handles the case where an outgoing message has a configured delay.
        /// </summary>
        /// <param name="context">The outgoing message context.</param>
        /// <returns>A task representing this execution.</returns>
        protected virtual Task HandleMessageDelayAsync(MessageContext context)
        {
            // Override if necessary.
            return Task.CompletedTask;
        }
    }

    public abstract class User : IUser
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public abstract UserType Type { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public abstract string UserId { get; }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        public abstract void ReceiveMessage(string message);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// A base implementation of <see cref="IRoomOccupant"/>. 
    /// </summary>
    /// <seealso cref="Mofichan.Core.Interfaces.IRoomOccupant" />
    public class RoomOccupant : IRoomOccupant
    {
        private readonly IUser user;
        private readonly IRoom room;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomOccupant"/> class.
        /// </summary>
        /// <param name="user">The user to wrap.</param>
        /// <param name="room">The room to wrap.</param>
        public RoomOccupant(IUser user, IRoom room)
        {
            this.user = user;
            this.room = room;
        }

        /// <summary>
        /// Gets the occupant name.
        /// </summary>
        /// <value>
        /// The occupant name.
        /// </value>
        public string Name
        {
            get
            {
                return this.user.Name;
            }
        }

        /// <summary>
        /// Gets the room the occupant is within.
        /// </summary>
        /// <value>
        /// The occupant's room.
        /// </value>
        public IRoom Room
        {
            get
            {
                return this.room;
            }
        }

        /// <summary>
        /// Gets the type of this occupant.
        /// </summary>
        /// <value>
        /// The occupant's type.
        /// </value>
        public UserType Type
        {
            get
            {
                return this.user.Type;
            }
        }

        /// <summary>
        /// Gets the occupant's user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public string UserId
        {
            get
            {
                return this.user.UserId;
            }
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        public void ReceiveMessage(string message)
        {
            this.room.ReceiveMessage(message);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.user.ToString();
        }
    }
}
