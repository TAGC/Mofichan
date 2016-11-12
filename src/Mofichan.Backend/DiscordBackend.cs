using System;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Backend
{
    public sealed class DiscordBackend : BaseBackend
    {
        public DiscordBackend()
        {

        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        protected override IRoom GetRoomById(string roomId)
        {
            throw new NotImplementedException();
        }

        protected override IUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        protected override void SendMessage(MessageContext message)
        {
            throw new NotImplementedException();
        }
    }
}
