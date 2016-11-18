using System;
using System.Collections.Generic;
using System.Reactive;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BaseReflectionBehaviourTests
    {
        #region Mocks
        private class MockIncomingMessageFilterAttribute : BaseIncomingMessageFilterAttribute
        {
            private readonly bool permitMessage;

            public MockIncomingMessageFilterAttribute(bool permitMessage)
            {
                this.permitMessage = permitMessage;
            }

            public override void OnNext(IncomingMessage message)
            {
                if (this.permitMessage)
                {
                    this.SendDownstream(message);
                }
            }
        }

        private class MockReflectionBehaviour : BaseReflectionBehaviour
        {
            [MockIncomingMessageFilter(permitMessage: true)]
            public OutgoingMessage? Should_Receive_Message_And_Add_Suffix_Foo(IncomingMessage message)
            {
                return ReflectBackMessageWithSuffix(message, " foo");
            }

            [MockIncomingMessageFilter(permitMessage: true)]
            public OutgoingMessage? Should_Receive_Message_And_Add_Suffix_Bar(IncomingMessage message)
            {
                return ReflectBackMessageWithSuffix(message, " bar");
            }

            [MockIncomingMessageFilter(permitMessage: false)]
            public OutgoingMessage? Should_Not_Receive_Message(IncomingMessage message)
            {
                return ReflectBackMessageWithSuffix(message, " <<shouldn't receive>>");
            }

            private OutgoingMessage ReflectBackMessageWithSuffix(IncomingMessage message, string suffix)
            {
                var mockTarget = Mock.Of<IMessageTarget>();
                var newBody = message.Context.Body + suffix;
                var newContext = new MessageContext(mockTarget, mockTarget, newBody);

                return new OutgoingMessage { Context = newContext };
            }
        }
        #endregion

        [Fact]
        public void Reflection_Behaviour_Should_Pass_Incoming_Messages_To_Methods_Based_On_Attributes()
        {
            // GIVEN an instance of a mock reflection behaviour.
            var behaviour = new MockReflectionBehaviour();

            // GIVEN an incoming message.
            var messageContext = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), "hello,");
            var message = new IncomingMessage(messageContext);

            // GIVEN we are subscribed to responses from the behaviour.
            var responses = new List<string>();
            var observer = Observer.Create<OutgoingMessage>(response => responses.Add(response.Context.Body));
            behaviour.Subscribe(observer);

            // WHEN we pass the message to the behaviour.
            behaviour.OnNext(message);

            // THEN the methods within it should have been invoked based on their applied attributes.
            responses.Count.ShouldBe(2);
            responses.ShouldContain("hello, foo");
            responses.ShouldContain("hello, bar");
        }
    }
}
