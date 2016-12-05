using System;
using Mofichan.Backend;
using Mofichan.Core.Interfaces;

namespace Mofichan.Tests.TestUtility
{
    public class MockUser : User
    {
        public MockUser(string id, string name, UserType userType = UserType.NormalUser)
        {
            this.Name = name;
            this.UserId = id;
            this.Type = userType;
        }

        public override string Name { get; }

        public override UserType Type { get; }

        public override string UserId { get; }

        public override void ReceiveMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}
