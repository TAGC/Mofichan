using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests.Behaviour
{
    public class BehaviourTests
    {
        private class MockBehaviour : BaseBehaviour
        {
            public bool SimulateCanHandleMessage { get; set; }
            public MessageContext HandledMessage { get; private set; }

            protected override void HandleMessageVisitor(OnMessageVisitor visitor)
            {
                if (this.SimulateCanHandleMessage)
                {
                    this.HandledMessage = visitor.Message;
                }

                base.HandleMessageVisitor(visitor);
            }
        }

        [Fact]
        public void Behaviour_Should_Pass_Unhandled_Incoming_Message_To_Downstream_Observer()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();

            // GIVEN a downstream observer for incoming messages.
            var downstreamObserver = new Mock<IObserver<IBehaviourVisitor>>();
            downstreamObserver.Setup(it => it.OnNext(It.IsAny<IBehaviourVisitor>()));

            // GIVEN the observer is subscribed to the behaviour.
            behaviour.Subscribe(downstreamObserver.Object);

            // GIVEN an incoming message to pass.
            var message = new MessageContext();

            // GIVEN the behaviour cannot handle the incoming message.
            behaviour.SimulateCanHandleMessage = false;

            // WHEN the behaviour is visited.
            var visitor = new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitor);

            // THEN the downstream observer should also have been visited.
            downstreamObserver.Verify(it => it.OnNext(visitor), Times.Once);
        }

        [Fact]
        public void Behaviour_Should_Attempt_To_Handle_Incoming_Message_If_And_Only_If_It_Can_Handle_Incoming_Message()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();
            behaviour.HandledMessage.ShouldBeNull();

            // GIVEN a mock incoming message.
            var message = new MessageContext();

            // WHEN the mock behaviour is not able to handle a message.
            behaviour.SimulateCanHandleMessage = false;

            // AND the behaviour is offered a visitor carrying the message.
            behaviour.OnNext(new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder));

            // THEN the behaviour should not have tried to handle the message.
            behaviour.HandledMessage.ShouldBeNull();

            // WHEN the mock behaviour is able to handle the message.
            behaviour.SimulateCanHandleMessage = true;

            // AND the behaviour is again offered a visitor carrying the message.
            behaviour.OnNext(new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder));

            // THEN the behaviour should have tried to handle the message.
            behaviour.HandledMessage.ShouldBe(message);
        }
    }
}
