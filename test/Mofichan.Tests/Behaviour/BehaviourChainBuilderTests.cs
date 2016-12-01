using System;
using System.Linq;
using System.Reactive;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Behaviour
{
    public class BehaviourChainBuilderTests
    {
        #region Mocks
        private class MockBehaviour : BaseBehaviour
        {
            private readonly string suffix;

            public MockBehaviour(string toAppend) : base(() => Mock.Of<IResponseBuilder>())
            {
                this.suffix = toAppend;
            }

            protected override bool CanHandleIncomingMessage(IncomingMessage message)
            {
                return true;
            }

            protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
            {
                return false;
            }

            protected override void HandleIncomingMessage(IncomingMessage message)
            {
                var newBody = message.Context.Body + suffix;

                var oldContext = message.Context;
                var newContext = new MessageContext(oldContext.From, oldContext.To, newBody, oldContext.Delay);
                var newMessage = new IncomingMessage(newContext, message.PotentialReply);

                this.SendDownstream(newMessage);
            }

            protected override void HandleOutgoingMessage(OutgoingMessage message)
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        private readonly BehaviourChainBuilder chainBuilder;

        public BehaviourChainBuilderTests()
        {
            this.chainBuilder = new BehaviourChainBuilder();
        }

        [Fact]
        public void Behaviour_Chain_Builder_Should_Throw_Exception_If_Behaviour_Collection_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
                () => this.chainBuilder.BuildChain(null));
        }

        [Fact]
        public void Behaviour_Chain_Builder_Should_Throw_Exception_If_Behaviour_Collection_Is_Empty()
        {
            Assert.Throws<ArgumentException>(
                () => this.chainBuilder.BuildChain(Enumerable.Empty<IMofichanBehaviour>()));
        }

        [Fact]
        public void Behaviour_Chain_Builder_Should_Return_Provided_Behaviour_If_Only_One_Provided()
        {
            // GIVEN a mock behaviour.
            var mockBehaviour = Mock.Of<IMofichanBehaviour>();

            // WHEN we build a chain from the behaviour.
            var root = this.chainBuilder.BuildChain(new[] { mockBehaviour });

            // THEN the root of the chain should be the behaviour itself.
            root.ShouldBe(mockBehaviour);
        }

        [Fact]
        public void Behaviour_Chain_Builder_Should_Build_Expected_Chain_From_Behaviour_Collection()
        {
            // GIVEN a collection of mock behaviours.
            var mockBehaviours = new[]
            {
                new MockBehaviour(" foo"),
                new MockBehaviour(" bar"),
                new MockBehaviour(" baz")
            };

            // GIVEN an incoming message.
            var messageContext = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), "test,");
            var message = new IncomingMessage(messageContext);

            // GIVEN we are subscribed to the last behaviour in the chain.
            string receivedMessageBody = null;
            var observer = Observer.Create<IncomingMessage>(it => receivedMessageBody = it.Context.Body);
            mockBehaviours.Last().Subscribe(observer);

            // WHEN we build a chain from the behaviours.
            var root = this.chainBuilder.BuildChain(mockBehaviours);

            // WHEN we pass the incoming message to the root of the chain.
            root.OnNext(message);

            // THEN the expected message body should have been produced at the tail of the chain.
            receivedMessageBody.ShouldBe("test, foo bar baz");
        }
    }
}
