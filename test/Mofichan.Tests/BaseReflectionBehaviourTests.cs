using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.FilterAttributes;
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
                return ReflectBackMessageWithSuffix(message, " baz");
            }
            
            [RegexIncomingMessageFilter(regex: "abc(?=def)", options: RegexOptions.IgnoreCase)]
            public OutgoingMessage? Should_Invoke_On_Match(IncomingMessage message)
            {
                return BuildMessageWithBody("Matched: " + message.Context.Body);
            }

            private static OutgoingMessage ReflectBackMessageWithSuffix(IncomingMessage message, string suffix)
            {
                return BuildMessageWithBody(message.Context.Body + suffix);
            }

            private static OutgoingMessage BuildMessageWithBody(string body)
            {
                var mockTarget = Mock.Of<IMessageTarget>();
                var context = new MessageContext(mockTarget, mockTarget, body);

                return new OutgoingMessage { Context = context };
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
            responses.ShouldContain("hello, foo");
            responses.ShouldContain("hello, bar");
            responses.ShouldNotContain("hello, baz");
        }

        [Fact]
        public void Reflection_Behaviour_Should_Support_Regex_Based_Filtering()
        {
            // GIVEN an instance of a mock reflection behaviour.
            var behaviour = new MockReflectionBehaviour();

            // GIVEN a set of incoming messages
            Func<string, IncomingMessage> messageBuilder = message =>
            {
                var mockTarget = Mock.Of<IMessageTarget>();
                var messageContext = new MessageContext(mockTarget, mockTarget, message);
                return new IncomingMessage(messageContext);
            };

            var abc = messageBuilder("abc");
            var abcdef = messageBuilder("abcdef");
            var aBcDeF = messageBuilder("aBcDeF");

            // GIVEN we are subscribed to responses from the behaviour.
            var responses = new List<string>();
            var observer = Observer.Create<OutgoingMessage>(response => responses.Add(response.Context.Body));
            behaviour.Subscribe(observer);

            // WHEN we pass "abc" to the observer.
            behaviour.OnNext(abc);

            // THEN we should not have received a "matched" response.
            responses.ShouldNotContain("Matched: abc");
            responses.Clear();

            // WHEN we pass "abcdef" to the observer.
            behaviour.OnNext(abcdef);

            // THEN we should have received a "matched" response.
            responses.ShouldContain("Matched: abcdef");
            responses.Clear();

            // WHEN we pass "aBcDeF" to the observer.
            behaviour.OnNext(aBcDeF);

            // THEN we should have received a "matched" response.
            responses.ShouldContain("Matched: aBcDeF");
            responses.Clear();
        }
    }
}
