using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Backend
{
    public sealed class DiscordBackend : BaseBackend
    {
        private readonly string apiToken;
        private readonly DiscordSocketClient client;
        private readonly string adminId;

        private DiscordSettings settings;

        public DiscordBackend(string token, string adminId)
        {
            this.apiToken = token;
            this.adminId = adminId;
            this.client = new DiscordSocketClient();
        }

        public override void Start()
        {
            this.client.LoginAsync(TokenType.Bot, this.apiToken).Wait();
            this.client.ConnectAsync().Wait();

            this.client.MessageReceived += HandleIncomingMessage;

            var botId = this.client.CurrentUser.Id.ToString();
            this.settings = new DiscordSettings(botId, this.adminId);
        }

        private Task HandleIncomingMessage(SocketMessage message)
        {
            var user = new DiscordUser(message.Author, this.settings);
            var room = new DiscordRoom(message.Channel);

            var from = new RoomOccupant(user, room);
            var to = room;
            var body = message.Content;

            var context = new MessageContext(from, to, body);
            var incomingMessage = new IncomingMessage(context);

            this.OnReceiveMessage(incomingMessage);

            return Task.CompletedTask;
        }

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

        protected override IRoom GetRoomById(string roomId)
        {
            throw new NotImplementedException();
        }

        protected override Core.Interfaces.IUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }
    }

    public struct DiscordSettings
    {
        public DiscordSettings(string botId, string adminId)
        {
            this.BotId = botId;
            this.AdminId = adminId;
        }

        public string BotId { get; }
        public string AdminId { get; }
    }

    public sealed class DiscordUser : Core.Interfaces.IUser
    {
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

        public string UserId { get; }
        public string Name { get; }
        public UserType Type { get; }

        public void ReceiveMessage(string message)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DiscordRoom : IRoom
    {
        private readonly ISocketMessageChannel channel;

        public DiscordRoom(ISocketMessageChannel channel)
        {
            this.channel = channel;
        }

        public string Name
        {
            get
            {
                return channel.Name;
            }
        }

        public string RoomId
        {
            get
            {
                return channel.Id.ToString();
            }
        }

        public IDisposable EnterTypingState()
        {
            return this.channel.EnterTypingState();
        }

        public void Join()
        {
        }

        public void Leave()
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(string message)
        {
            this.channel.SendMessageAsync(message).Wait();
        }
    }
}
