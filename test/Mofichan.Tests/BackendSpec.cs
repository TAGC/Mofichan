using Mofichan.Backend;
using Mofichan.Core.Interfaces;
using Moq;
using Serilog;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BackendSpec
    {
        private class MockBackend : BaseBackend
        {
            public bool OnStartCalled { get; private set; }
            public string RoomId { get; private set; }
            public string UserId { get; private set; }

            public MockBackend(ILogger logger) : base(logger)
            {
            }

            public override void Start()
            {
                this.OnStartCalled = true;
            }

            protected override IRoom GetRoomById(string roomId)
            {
                this.RoomId = roomId;
                return Mock.Of<IRoom>();
            }

            protected override IUser GetUserById(string userId)
            {
                this.UserId = userId;
                return Mock.Of<IUser>();
            }
        }

        [Fact]
        public void Backend_Should_Try_To_Get_Room_Id_When_Join_Requested()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());
            backend.RoomId.ShouldBeNull();

            // GIVEN a room ID.
            var roomId = "foo";

            // WHEN I try to join a room using the room ID.
            backend.Join(roomId);

            // THEN the backend should have attempted to find a room using that identifier.
            backend.RoomId.ShouldBe(roomId);
        }

        [Fact]
        public void Backend_Should_Try_To_Get_Room_Id_When_Leave_Requested()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());
            backend.RoomId.ShouldBeNull();

            // GIVEN a room ID.
            var roomId = "foo";

            // WHEN I try to leave a room using the room ID.
            backend.Leave(roomId);

            // THEN the backend should have attempted to find a room using that identifier.
            backend.RoomId.ShouldBe(roomId);
        }
    }
}
