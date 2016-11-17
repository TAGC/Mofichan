using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BehaviourTests
    {
        private class MockBehaviour : BaseBehaviour
        {
            public MockBehaviour(bool passThroughMessages = true) : base(passThroughMessages)
            {
            }

            public bool SimulateCanHandleIncomingMessage { get; set; }
            public bool SimulateCanHandleOutgoingMessage { get; set; }
            public IncomingMessage? HandledIncomingMessage { get; private set; }
            public OutgoingMessage? HandledOutgoingMessage { get; private set; }

            protected override bool CanHandleIncomingMessage(IncomingMessage message)
            {
                return this.SimulateCanHandleIncomingMessage;
            }

            protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
            {
                return this.SimulateCanHandleOutgoingMessage;
            }

            protected override void HandleIncomingMessage(IncomingMessage message)
            {
                this.HandledIncomingMessage = message;
            }

            protected override void HandleOutgoingMessage(OutgoingMessage message)
            {
                this.HandledOutgoingMessage = message;
            }
        }

        [Fact]
        public void Behaviour_Should_Pass_Unhandled_Incoming_Message_To_Downstream_Observer()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour(true);

            // GIVEN a downstream observer for incoming messages.
            var downstreamObserver = new Mock<IObserver<IncomingMessage>>();
            downstreamObserver.Setup(it => it.OnNext(It.IsAny<IncomingMessage>()));

            // GIVEN the observer is subscribed to the behaviour.
            behaviour.Subscribe(downstreamObserver.Object);

            // GIVEN an incoming message to pass.
            var message = default(IncomingMessage);

            // GIVEN the behaviour cannot handle the incoming message.
            behaviour.SimulateCanHandleIncomingMessage = false;

            // WHEN the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the downstream observer should have been offered the message.
            downstreamObserver.Verify(it => it.OnNext(message), Times.Once);
        }

        [Fact]
        public void Behaviour_Should_Pass_Unhandled_Outgoing_Message_To_Upstream_Observer()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();

            // GIVEN an upstream observer for outgoing messages.
            var upstreamObserver = new Mock<IObserver<OutgoingMessage>>();
            upstreamObserver.Setup(it => it.OnNext(It.IsAny<OutgoingMessage>()));

            // GIVEN the observer is subscribed to the behaviour.
            behaviour.Subscribe(upstreamObserver.Object);

            // GIVEN an outgoing message to pass.
            var message = default(OutgoingMessage);

            // GIVEN the behaviour cannot handle the outgoing message.
            behaviour.SimulateCanHandleOutgoingMessage = false;

            // WHEN the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the upstream observer should have been offered the message.
            upstreamObserver.Verify(it => it.OnNext(message), Times.Once);
        }

        [Fact]
        public void Behaviour_Should_Attempt_To_Handle_Incoming_Message_If_And_Only_If_It_Can_Handle_Incoming_Message()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();
            behaviour.HandledIncomingMessage.ShouldBeNull();
            behaviour.HandledOutgoingMessage.ShouldBeNull();

            // GIVEN a mock incoming message.
            var message = default(IncomingMessage);

            // WHEN the mock behaviour is not able to handle a message.
            behaviour.SimulateCanHandleIncomingMessage = false;

            // AND the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the behaviour should not have tried to handle the message.
            behaviour.HandledIncomingMessage.ShouldBeNull();

            // WHEN the mock behaviour is able to handle the message.
            behaviour.SimulateCanHandleIncomingMessage = true;

            // AND the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the behaviour should have tried to handle the message.
            behaviour.HandledIncomingMessage.ShouldBe(message);
        }

        [Fact]
        public void Behaviour_Should_Attempt_To_Handle_Outgoing_Message_If_And_Only_If_It_Can_Handle_Outgoing_Message()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();
            behaviour.HandledIncomingMessage.ShouldBeNull();
            behaviour.HandledOutgoingMessage.ShouldBeNull();

            // GIVEN a mock outgoing message.
            var message = default(OutgoingMessage);

            // WHEN the mock behaviour is not able to handle a message.
            behaviour.SimulateCanHandleOutgoingMessage = false;

            // AND the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the behaviour should not have tried to handle the message.
            behaviour.HandledOutgoingMessage.ShouldBeNull();

            // WHEN the mock behaviour is able to handle the message.
            behaviour.SimulateCanHandleOutgoingMessage = true;

            // AND the behaviour is offered the message.
            behaviour.OnNext(message);

            // THEN the behaviour should have tried to handle the message.
            behaviour.HandledOutgoingMessage.ShouldBe(message);
        }
    }
}
