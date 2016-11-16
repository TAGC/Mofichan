using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Backend;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Serilog;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BackendTests
    {
        private class MockBackend : BaseBackend
        {
            public MockBackend(ILogger logger) : base(logger)
            {
                this.DelayHandledTcs = new TaskCompletionSource<object>();
            }

            public TaskCompletionSource<object> DelayHandledTcs { get; }
            public bool OnStartCalled { get; private set; }
            public Mock<IRoom> MockRoom { get; private set; }
            public Mock<IUser> MockUser { get; private set; }

            public override void Start()
            {
                this.OnStartCalled = true;
            }

            public void SimulateReceivingMessage(IncomingMessage message)
            {
                this.OnReceiveMessage(message);
            }

            protected override IRoom GetRoomById(string roomId)
            {
                this.MockRoom = new Mock<IRoom>();
                this.MockRoom.SetupGet(it => it.RoomId).Returns(roomId);
                return this.MockRoom.Object;
            }

            protected override IUser GetUserById(string userId)
            {
                this.MockUser = new Mock<IUser>();
                this.MockUser.SetupGet(it => it.UserId).Returns(userId);
                return this.MockUser.Object;
            }

            protected override Task HandleMessageDelayAsync(MessageContext context)
            {
                this.DelayHandledTcs.SetResult(null);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void Backend_Should_Try_To_Get_Room_Id_When_Join_Requested()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());

            // GIVEN a room ID.
            var roomId = "foo";

            // WHEN I try to join a room using the room ID.
            backend.Join(roomId);

            // THEN the backend should have attempted to find a room using that identifier.
            backend.MockRoom.Object.RoomId.ShouldBe(roomId);
        }

        [Fact]
        public void Backend_Should_Try_To_Get_Room_Id_When_Leave_Requested()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());

            // GIVEN a room ID.
            var roomId = "foo";

            // WHEN I try to leave a room using the room ID.
            backend.Leave(roomId);

            // THEN the backend should have attempted to find a room using that identifier.
            backend.MockRoom.Object.RoomId.ShouldBe(roomId);
        }

        [Fact]
        public void Room_Should_Try_To_Send_Message_When_Receiving_Message_For_Room_Occupant()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());

            // GIVEN a mock room occupant.
            var mockOccupant = new Mock<IRoomOccupant>();
            mockOccupant.Setup(it => it.ReceiveMessage(It.IsAny<string>()));

            // GIVEN an outgoing message directed at the room occupant.
            var messageBody = "hello";
            var messageContext = new MessageContext(Mock.Of<IMessageTarget>(), mockOccupant.Object, messageBody);
            var message = new OutgoingMessage { Context = messageContext };

            // WHEN the backend receives the message
            backend.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the room occupant should receive the message.
            mockOccupant.Verify(it => it.ReceiveMessage(messageBody), Times.Once);
        }

        [Fact]
        public void Room_Occupant_Receiving_Message_Should_Delegate_To_Occupied_Room()
        {
            // GIVEN a mock room.
            var mockRoom = new Mock<IRoom>();
            mockRoom.Setup(it => it.ReceiveMessage(It.IsAny<string>()));

            // GIVEN a room occupant within the mock room.
            var occupant = new RoomOccupant(Mock.Of<IUser>(), mockRoom.Object);

            // GIVEN a mock message to send to the room occupant.
            var messageBody = "hello";

            // WHEN the occupant receives the message.
            occupant.ReceiveMessage(messageBody);

            // THEN the mock room should have received the message.
            mockRoom.Verify(it => it.ReceiveMessage(messageBody));
        }

        [Fact]
        public async Task Backend_Should_Handle_Delayed_Messages_Differently()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());

            // GIVEN a mock recipient.
            var recipient = new Mock<IMessageTarget>();
            recipient.Setup(it => it.ReceiveMessage(It.IsAny<string>()));

            // GIVEN an outgoing message with an associated non-zero delay.
            var messageBody = "hello";
            var delay = TimeSpan.FromMilliseconds(1000);
            var messageContext = new MessageContext(Mock.Of<IMessageTarget>(), recipient.Object, messageBody, delay);
            var message = new OutgoingMessage { Context = messageContext };

            // WHEN the backend receives the message.
            backend.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN it should try to handle the delay.
            var task = backend.DelayHandledTcs.Task;
            (await Task.WhenAny(task, Task.Delay(1000))).ShouldBe(task);

            // AND the recipient should then receive the message.
            recipient.Verify(it => it.ReceiveMessage(messageBody), Times.Once);
        }

        [Fact]
        public void Backend_Should_Send_Received_Messages_To_Linked_Target()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(Mock.Of<ILogger>());

            // GIVEN an incoming message target.
            var target = new Mock<ITargetBlock<IncomingMessage>>();
            target.Setup(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()));

            // GIVEN an incoming message.
            var messageBody = "what's up?";
            var messageContext = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), messageBody);
            var message = new IncomingMessage(messageContext);

            // WHEN the mock backend receives the message.
            backend.SimulateReceivingMessage(message);

            // THEN the target should not have received the message.
            target.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                message,
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Never);

            // WHEN we link the backend to the target.
            backend.LinkTo(target.Object);

            // AND we simulate receiving the message again.
            backend.SimulateReceivingMessage(message);

            // THEN the target should have received the message.
            target.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                message,
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Once);
        }
    }
}
