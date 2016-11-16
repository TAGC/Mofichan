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
        public void Behaviour_Should_Decline_Incoming_Message_Offer_If_Downstream_Target_Is_Null()
        {
            // GIVEN a mock behaviour that not been linked to a downstream target.
            var behaviour = new MockBehaviour();

            // GIVEN an incoming message.
            var message = default(IncomingMessage);

            // WHEN we offer the message to the behaviour.
            var status = behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<IncomingMessage>>(), false);

            // THEN the behaviour should have declined the offer.
            status.ShouldBe(DataflowMessageStatus.Declined);
        }

        [Fact]
        public void Behaviour_Should_Decline_Incoming_Message_Offer_If_Not_Passing_Through_Unhandled_Messages()
        {
            // GIVEN a mock behaviour that will not automatically pass through unhandled message.
            var behaviour = new MockBehaviour(false);
            behaviour.SimulateCanHandleIncomingMessage = false;

            // GIVEN the behaviour is linked to a mock target.
            behaviour.LinkTo(Mock.Of<ITargetBlock<IncomingMessage>>());

            // GIVEN an incoming message.
            var message = default(IncomingMessage);

            // WHEN we offer the message to the behaviour.
            var status = behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<IncomingMessage>>(), false);

            // THEN the behaviour should have declined the offer.
            status.ShouldBe(DataflowMessageStatus.Declined);
        }

        [Fact]
        public void Behaviour_Should_Decline_Outgoing_Message_Offer_If_Upstream_Target_Is_Null()
        {
            // GIVEN a mock behaviour that not been linked to an upstream target.
            var behaviour = new MockBehaviour();

            // GIVEN an outgoing message.
            var message = default(OutgoingMessage);

            // WHEN we offer the message to the behaviour.
            var status = behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the behaviour should have declined the offer.
            status.ShouldBe(DataflowMessageStatus.Declined);
        }

        [Fact]
        public void Behaviour_Should_Decline_Outgoing_Message_Offer_If_Not_Passing_Through_Unhandled_Messages()
        {
            // GIVEN a mock behaviour that will not automatically pass through unhandled message.
            var behaviour = new MockBehaviour(false);
            behaviour.SimulateCanHandleIncomingMessage = false;

            // GIVEN the behaviour is linked to a mock target.
            behaviour.LinkTo(Mock.Of<ITargetBlock<OutgoingMessage>>());

            // GIVEN an outgoing message.
            var message = default(OutgoingMessage);

            // WHEN we offer the message to the behaviour.
            var status = behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the behaviour should have declined the offer.
            status.ShouldBe(DataflowMessageStatus.Declined);
        }

        [Fact]
        public void Behaviour_Should_Pass_Unhandled_Incoming_Message_To_Downstream_Target()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour(true);

            // GIVEN a downstream target for incoming messages.
            var downstreamTarget = new Mock<ITargetBlock<IncomingMessage>>();
            downstreamTarget.Setup(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()));

            // GIVEN the behaviour is linked to the target.
            behaviour.LinkTo(downstreamTarget.Object);

            // GIVEN an incoming message to pass.
            var message = default(IncomingMessage);

            // GIVEN the behaviour cannot handle the incoming message.
            behaviour.SimulateCanHandleIncomingMessage = false;

            // WHEN the behaviour is offered the message.
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<IncomingMessage>>(), false);

            // THEN the downstream target should have been offered the message.
            downstreamTarget.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                message,
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void Behaviour_Should_Pass_Unhandled_Outgoing_Message_To_Upstream_Target()
        {
            // GIVEN a mock behaviour.
            var behaviour = new MockBehaviour();

            // GIVEN an upstream target for outgoing messages.
            var upstreamTarget = new Mock<ITargetBlock<OutgoingMessage>>();
            upstreamTarget.Setup(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<OutgoingMessage>(),
                It.IsAny<ISourceBlock<OutgoingMessage>>(),
                It.IsAny<bool>()));

            // GIVEN the behaviour is linked to the target.
            behaviour.LinkTo(upstreamTarget.Object);

            // GIVEN an outgoing message to pass.
            var message = default(OutgoingMessage);

            // GIVEN the behaviour cannot handle the outgoing message.
            behaviour.SimulateCanHandleOutgoingMessage = false;

            // WHEN the behaviour is offered the message.
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the upstream target should have been offered the message.
            upstreamTarget.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                message,
                It.IsAny<ISourceBlock<OutgoingMessage>>(),
                It.IsAny<bool>()), Times.Once);
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
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<IncomingMessage>>(), false);

            // THEN the behaviour should not have tried to handle the message.
            behaviour.HandledIncomingMessage.ShouldBeNull();

            // WHEN the mock behaviour is able to handle the message.
            behaviour.SimulateCanHandleIncomingMessage = true;

            // AND the behaviour is offered the message.
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<IncomingMessage>>(), false);

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
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the behaviour should not have tried to handle the message.
            behaviour.HandledOutgoingMessage.ShouldBeNull();

            // WHEN the mock behaviour is able to handle the message.
            behaviour.SimulateCanHandleOutgoingMessage = true;

            // AND the behaviour is offered the message.
            behaviour.OfferMessage(default(DataflowMessageHeader), message,
                Mock.Of<ISourceBlock<OutgoingMessage>>(), false);

            // THEN the behaviour should have tried to handle the message.
            behaviour.HandledOutgoingMessage.ShouldBe(message);
        }
    }
}
