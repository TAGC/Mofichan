﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Discord;
using Discord.WebSocket;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Backend
{
    /// <summary>
    /// Represents a collection of Discord-specific settings.
    /// </summary>
    public struct DiscordSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSettings"/> struct.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <param name="adminId">The admin identifier.</param>
        public DiscordSettings(string botId, string adminId)
        {
            this.BotId = botId;
            this.AdminId = adminId;
        }

        /// <summary>
        /// Gets the bot identifier.
        /// </summary>
        /// <value>
        /// The bot identifier.
        /// </value>
        public string BotId { get; }

        /// <summary>
        /// Gets the admin identifier.
        /// </summary>
        /// <value>
        /// The admin identifier.
        /// </value>
        public string AdminId { get; }
    }

    /// <summary>
    /// An implementation of <see cref="IMofichanBackend"/> that allows her to talk to
    /// people on Discord.
    /// </summary>
    public sealed class DiscordBackend : BaseBackend
    {
        private readonly string apiToken;
        private readonly string adminId;
        private readonly DiscordSocketClient client;
        private readonly BufferBlock<MessageContext> outgoingMessageQueue;
        private readonly CancellationTokenSource cancellationSource;

        private Task sendMessagesTask;
        private DiscordSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBackend" /> class.
        /// </summary>
        /// <param name="token">Mofichan's API token.</param>
        /// <param name="admin_id">The Discord identifier of Mofichan's sole administrator.</param>
        /// <param name="logger">The logger to use.</param>
        public DiscordBackend(string token, string admin_id, ILogger logger) : base(logger.ForContext<DiscordBackend>())
        {
            this.apiToken = token;
            this.adminId = admin_id;
            this.client = new DiscordSocketClient();
            this.outgoingMessageQueue = new BufferBlock<MessageContext>();
            this.cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initialises the backend.
        /// </summary>
        public override void Start()
        {
            this.client.LoginAsync(TokenType.Bot, this.apiToken).Wait();
            this.client.ConnectAsync().Wait();
            this.Logger.Information("Successfully connected to Discord bot account");

            this.client.MessageReceived += this.HandleIncomingMessage;

            var botId = this.client.CurrentUser.Id.ToString();
            this.settings = new DiscordSettings(botId, this.adminId);
            this.sendMessagesTask = this.SendOutgoingMessagesAsync(this.cancellationSource.Token);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.cancellationSource.Cancel();

            try
            {
                this.sendMessagesTask?.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    Logger.Verbose(e, "Expected task cancellation exception thrown");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                this.cancellationSource.Dispose();
            }

            this.client.Dispose();
            this.Logger.Debug("Disposed {Client}", this.client);
        }

        /// <summary>
        /// Sends the message into the wider world.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected override void SendMessage(MessageContext message)
        {
            this.Logger.Verbose("Queueing message {MessageBody} to {Recipient} with {Delay} delay",
                message.Body, message.To, message.Delay);

            this.outgoingMessageQueue.Post(message);
        }

        /// <summary>
        /// Handles the case where an outgoing message has a configured delay.
        /// </summary>
        /// <param name="context">The outgoing message context.</param>
        /// <returns>
        /// A task representing this execution.
        /// </returns>
        protected override async Task HandleMessageDelayAsync(MessageContext context)
        {
            DiscordRoom room;

            if (context.To is RoomOccupant && (context.To as RoomOccupant).Room is DiscordRoom)
            {
                room = (context.To as RoomOccupant).Room as DiscordRoom;
            }
            else if (context.To is DiscordRoom)
            {
                room = context.To as DiscordRoom;
            }
            else
            {
                return;
            }

            /*
             * Simulate bot typing before message gets sent in room.
             */
            using (room.EnterTypingState())
            {
                await Task.Delay(context.Delay);
            }
        }

        /// <summary>
        /// Tries to retrieve a room by its ID.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>
        /// The room corresponding to the ID, if it exists.
        /// </returns>
        protected override IRoom GetRoomById(string roomId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to retrieve a user by its ID.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The user corresponding to the ID, if it exists.
        /// </returns>
        protected override Core.Interfaces.IUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        private async Task SendOutgoingMessagesAsync(CancellationToken token)
        {
            MessageContext context;
            while (true)
            {
                context = await this.outgoingMessageQueue.ReceiveAsync(token);
                
                if (context.Delay != TimeSpan.Zero)
                {
                    await this.HandleMessageDelayAsync(context);
                }

                this.Logger.Debug("Sending message {MessageBody} to {Recipient}",
                    context.Body, context.To);

                context.To.ReceiveMessage(context.Body);
            }
        }

        private Task HandleIncomingMessage(SocketMessage message)
        {
            var user = new DiscordUser(message.Author, this.settings);
            var room = new DiscordRoom(message.Channel);

            var from = new RoomOccupant(user, room);
            var to = room;
            var body = message.Content;

            var incomingMessage = new MessageContext(from, to, body);
            this.OnReceiveMessage(incomingMessage);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Represents a Discord user.
    /// </summary>
    /// <seealso cref="Mofichan.Core.Interfaces.IUser" />
    public sealed class DiscordUser : User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordUser"/> class.
        /// </summary>
        /// <param name="user">The user instance to adapt.</param>
        /// <param name="settings">The current Discord settings.</param>
        public DiscordUser(Discord.IUser user, DiscordSettings settings)
        {
            this.UserId = user.Id.ToString();
            this.Name = user.Username;

            if (this.UserId == settings.BotId)
            {
                this.Type = UserType.Self;
            }
            else if (this.UserId == settings.AdminId)
            {
                this.Type = UserType.Adminstrator;
            }
            else
            {
                this.Type = UserType.NormalUser;
            }
        }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public override string UserId { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public override UserType Type { get; }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        public override void ReceiveMessage(string message)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a Discord room.
    /// </summary>
    /// <seealso cref="Mofichan.Core.Interfaces.IRoom" />
    public sealed class DiscordRoom : IRoom
    {
        private readonly ISocketMessageChannel channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRoom"/> class.
        /// </summary>
        /// <param name="channel">The Discord socket channel to wrap.</param>
        public DiscordRoom(ISocketMessageChannel channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Gets the room name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return this.channel.Name;
            }
        }

        /// <summary>
        /// Gets the room identifier.
        /// </summary>
        /// <value>
        /// The room identifier.
        /// </value>
        public string RoomId
        {
            get
            {
                return this.channel.Id.ToString();
            }
        }

        /// <summary>
        /// Enters a "user typing" context.
        /// </summary>
        /// <returns>A token that ends the context when disposed.</returns>
        public IDisposable EnterTypingState()
        {
            return this.channel.EnterTypingState();
        }

        /// <summary>
        /// Connects Mofichan to this room.
        /// </summary>
        public void Join()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disconnects Mofichan from this room.
        /// </summary>
        public void Leave()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        public void ReceiveMessage(string message)
        {
            this.channel.SendMessageAsync(message).Wait();
        }
    }
}
