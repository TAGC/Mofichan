using System;
using System.Linq;
using System.Reactive.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests.Behaviour
{
    public class BehaviourChainBuilderTests
    {
        #region Mocks
        private class MockBehaviour : BaseBehaviour
        {
            private readonly string suffix;

            public MockBehaviour(string toAppend)
            {
                this.suffix = toAppend;
            }

            protected override void HandleMessageVisitor(OnMessageVisitor visitor)
            {
                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromRaw(visitor.Responses.Count() + " : ")
                        .FromRaw(visitor.Message.Body + suffix)));

                base.HandleMessageVisitor(visitor);
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

            // GIVEN an incoming message visitor.
            var message = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), "test,");
            var visitor = new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder);

            // WHEN we build a chain from the behaviours.
            var root = this.chainBuilder.BuildChain(mockBehaviours);

            // AND we pass the visitor to the root of the chain.
            root.OnNext(visitor);

            // THEN the visitor should have visited the behaviours in the expected order.
            var responses = visitor.Responses.Select(it => it.Message.Body).ToArray();

            responses.Length.ShouldBe(3);
            responses.ShouldContain("0 : test, foo");
            responses.ShouldContain("1 : test, bar");
            responses.ShouldContain("2 : test, baz");
        }
    }
}
