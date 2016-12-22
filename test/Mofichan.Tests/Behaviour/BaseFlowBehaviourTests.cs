using System;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Serilog;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.FlowUtil;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests.Behaviour
{
    public class BaseFlowBehaviourTests
    {
        private class MockFlowBehaviour : BaseFlowBehaviour
        {
            public MockFlowBehaviour(string startNodeId, IFlowManager flowManager)
                : base(startNodeId, flowManager, MockLogger.Instance)
            {
            }

            public new void RegisterFlow(Func<BasicFlow.Builder, BasicFlow.Builder> flowBuilderFunction)
            {
                base.RegisterFlow(flowBuilderFunction);
            }
        }

        [Fact]
        public void Flow_Behaviour_Should_Produce_Expected_Responses_When_Configured_With_Appropriate_Flows()
        {
            // GIVEN nodes S0 and S1, where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S0 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,0"), TransitionManagerFactory),
            };

            // GIVEN transitions T0,0, T0,1, T1,0 and T1,1 where T1,0 will generate a response.
            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,1"),
                new FlowTransition("T1,0", (context, _) => context.Visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb.FromRaw("Yes sir, yes sir, three baz full"))))
            };

            // GIVEN a mock flow behaviour that registers a flow with these nodes.
            var manager = new FlowManager(t => new FlowTransitionManager(t));
            var mockBehaviour = new MockFlowBehaviour("S0", manager);
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(MockLogger.Instance)
                .WithNodes(nodes)
                .WithTransitions(transitions)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S0", "T1,0")
                .WithConnection("S1", "S1", "T1,1"));

            // GIVEN the ID of some particular user.
            var borgUser = "borg-7.9";

            // EXPECT no response when the behaviour receives an incoming message containing "bar" from this user.
            // Reason: the "bar" transition should not generate any responses when triggered.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            mockBehaviour.OnNext(visitorFactory.CreateMessageVisitor(
                MessageFromBodyAndSender("Bar bar black sheep", borgUser)));

            mockBehaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.ShouldBeEmpty();

            // EXPECT a response to the user when the behaviour receives a "foo" message from them.
            mockBehaviour.OnNext(visitorFactory.CreateMessageVisitor(
                MessageFromBodyAndSender("Have you any foo?", borgUser)));

            mockBehaviour.OnNext(visitorFactory.CreatePulseVisitor());

            var response = visitorFactory.Responses.ShouldHaveSingleItem().Message;
            response.Body.ShouldBe("Yes sir, yes sir, three baz full");
            (response.To as IUser).UserId.ShouldBe(borgUser);
        }

        [Fact]
        public void Flow_Behaviour_Nodes_Should_Throw_Authorisation_Exceptions_If_Necessary()
        {
            // GIVEN a node that throws an authorisation exception on accepting messages.
            var nodes = new[]
            {
                new FlowNode("S0", (c, m) => { throw new MofichanAuthorisationException(c.Message); },
                    TransitionManagerFactory)
            };

            var transitions = Enumerable.Empty<IFlowTransition>();

            // GIVEN a mock flow behaviour that registers a flow with this node.
            var manager = new FlowManager(t => new FlowTransitionManager(t));
            var mockBehaviour = new MockFlowBehaviour("S0", manager);
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(MockLogger.Instance)
                .WithNodes(nodes)
                .WithTransitions(transitions)
                .WithStartNodeId("S0"));

            // GIVEN a mock user.
            var janeway = new Mock<IUser>();
            janeway.SetupGet(it => it.UserId).Returns("Janeway");
            janeway.SetupGet(it => it.Name).Returns("Janeway");

            // EXPECT an exception is thrown when a message is received by the behaviour from the user.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);

            Assert.Throws<MofichanAuthorisationException>(() =>
            {
                mockBehaviour.OnNext(visitorFactory.CreateMessageVisitor(
                    new MessageContext(from: janeway.Object, to: Mock.Of<IUser>(), body: "foo")));

                mockBehaviour.OnNext(visitorFactory.CreatePulseVisitor());
            });
        }

        [Fact]
        public void Flow_Behaviour_Should_Catch_And_Handle_Authorisation_Exceptions_Thrown_During_Transitions()
        {
            // GIVEN two nodes and a transition that throws an authorisation exception.
            var nodes = new[]
            {
                new FlowNode("S0", (c, m) => m.MakeTransitionCertain("T0,1"), TransitionManagerFactory),
                new FlowNode("S1", NullStateAction, TransitionManagerFactory),
            };

            var transitions = new[]
            {
                new FlowTransition("T0,1", (c, _) => { throw new MofichanAuthorisationException(c.Message); })
            };

            // GIVEN a mock flow behaviour that registers a flow with this node.
            var manager = new FlowManager(t => new FlowTransitionManager(t));
            var mockBehaviour = new MockFlowBehaviour("S0", manager);
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(MockLogger.Instance)
                .WithNodes(nodes)
                .WithTransitions(transitions)
                .WithConnection("S0", "S1", "T0,1")
                .WithStartNodeId("S0"));

            // GIVEN a mock user.
            var janeway = new Mock<IUser>();
            janeway.SetupGet(it => it.UserId).Returns("Janeway");
            janeway.SetupGet(it => it.Name).Returns("Janeway");

            // EXPECT an exception is thrown when a message is received by the behaviour from the user.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);

            Assert.Throws<MofichanAuthorisationException>(() =>
            {
                mockBehaviour.OnNext(visitorFactory.CreateMessageVisitor(
                new MessageContext(from: janeway.Object, to: Mock.Of<IUser>(), body: "foo")));

                mockBehaviour.OnNext(visitorFactory.CreatePulseVisitor());
                mockBehaviour.OnNext(visitorFactory.CreatePulseVisitor());
            });
        }
    }
}
