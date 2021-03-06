﻿using System;
using System.Threading.Tasks;
using Mofichan.Backend;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
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
            }

            public bool OnStartCalled { get; private set; }
            public Mock<IRoom> MockRoom { get; private set; }
            public Mock<IUser> MockUser { get; private set; }

            public override void Start()
            {
                this.OnStartCalled = true;
            }

            public void SimulateReceivingMessage(MessageContext message)
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
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void Backend_Should_Try_To_Get_Room_Id_When_Join_Requested()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(MockLogger.Instance);

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
            var backend = new MockBackend(MockLogger.Instance);

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
            var backend = new MockBackend(MockLogger.Instance);

            // GIVEN a mock room occupant.
            var mockOccupant = new Mock<IRoomOccupant>();
            mockOccupant.Setup(it => it.ReceiveMessage(It.IsAny<string>()));

            // GIVEN an outgoing message directed at the room occupant.
            var messageBody = "hello";
            var message = new MessageContext(Mock.Of<IMessageTarget>(), mockOccupant.Object, messageBody);

            // WHEN the backend receives the message
            backend.OnNext(message);

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
            var tcs = new TaskCompletionSource<string>();

            // GIVEN a mock backend.
            var backend = new MockBackend(MockLogger.Instance);

            // GIVEN a mock recipient.
            var recipient = new Mock<IMessageTarget>();
            recipient
                .Setup(it => it.ReceiveMessage(It.IsAny<string>()))
                .Callback<string>(recv => tcs.SetResult(recv));

            // GIVEN an outgoing message with an associated non-zero delay.
            var messageBody = "hello";
            var delay = TimeSpan.FromMilliseconds(1000);
            var message = new MessageContext(Mock.Of<IMessageTarget>(), recipient.Object, messageBody, delay);

            // WHEN the backend receives the message.
            backend.OnNext(message);

            // THEN it should try to handle the delay.
            (await Task.WhenAny(tcs.Task, Task.Delay(1000))).ShouldBe(tcs.Task);

            // AND the recipient should then receive the message.
            tcs.Task.Result.ShouldBe(messageBody);
        }

        [Fact]
        public void Backend_Should_Send_Received_Messages_To_Subscribed_Observer()
        {
            // GIVEN a mock backend.
            var backend = new MockBackend(MockLogger.Instance);

            // GIVEN an incoming message observer.
            var observer = new Mock<IObserver<MessageContext>>();
            observer.Setup(it => it.OnNext(It.IsAny<MessageContext>()));

            // GIVEN an incoming message.
            var messageBody = "what's up?";
            var message = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), messageBody);

            // WHEN the mock backend receives the message.
            backend.SimulateReceivingMessage(message);

            // THEN the observer should not have received the message.
            observer.Verify(it => it.OnNext(message), Times.Never);

            // WHEN we subscribe the observer to the behaviour.
            backend.Subscribe(observer.Object);

            // AND we simulate receiving the message again.
            backend.SimulateReceivingMessage(message);

            // THEN the observer should have received the message.
            observer.Verify(it => it.OnNext(message), Times.Once);
        }
    }
}
