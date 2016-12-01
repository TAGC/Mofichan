using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
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
            public MockFlowBehaviour(string startNodeId, IFlowDriver flowDriver)
                : base(startNodeId, () => Mock.Of<IResponseBuilder>(), flowDriver,
                      new FairFlowTransitionSelector(), Mock.Of<ILogger>())
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
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1", "T0,0"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,0", "T1,1"), TransitionManagerFactory),
            };

            // GIVEN transitions T0,0, T0,1, T1,0 and T1,1 where T1,0 will generate a response.
            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,1"),
                new FlowTransition("T1,0", (context, _) => context.GeneratedResponseHandler(
                    ResponseFromBodyAndRecipient(
                        body: "Yes sir, yes sir, three baz full",
                        recipientId: (context.Message.From as IUser).UserId)))
            };

            // GIVEN a mock flow behaviour that registers a flow with these nodes.
            var responses = new List<OutgoingMessage>();
            var driver = new ControllableFlowDriver();
            var mockBehaviour = new MockFlowBehaviour("S0", driver);
            mockBehaviour.Subscribe<OutgoingMessage>(it => responses.Add(it));
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(new LoggerConfiguration().CreateLogger())
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
            mockBehaviour.OnNext(MessageFromBodyAndSender("Bar bar black sheep", borgUser));
            driver.StepFlow();
            responses.ShouldBeEmpty();

            // EXPECT a response to the user when the behaviour receives a "foo" message from them.
            mockBehaviour.OnNext(MessageFromBodyAndSender("Have you any foo?", borgUser));
            driver.StepFlow();
            responses.ShouldContain(it => it.Context.Body == "Yes sir, yes sir, three baz full" &&
                                         (it.Context.To as IUser).UserId == borgUser);
        }

        [Fact]
        public void Flow_Behaviour_Should_Catch_And_Handle_Authorisation_Exceptions_Thrown_By_Nodes()
        {
            // GIVEN a node that throws an authorisation exception on accepting messages.
            var nodes = new[]
            {
                new FlowNode("S0", (c, m) => { throw new MofichanAuthorisationException(c.Message); },
                    TransitionManagerFactory)
            };

            var transitions = Enumerable.Empty<IFlowTransition>();

            // GIVEN a mock flow behaviour that registers a flow with this node.
            var responses = new List<OutgoingMessage>();
            var driver = new ControllableFlowDriver();
            var mockBehaviour = new MockFlowBehaviour("S0", driver);
            mockBehaviour.Subscribe<OutgoingMessage>(it => responses.Add(it));
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithNodes(nodes)
                .WithTransitions(transitions)
                .WithStartNodeId("S0"));

            // GIVEN a mock user.
            var janeway = new Mock<IUser>();
            janeway.SetupGet(it => it.UserId).Returns("Janeway");
            janeway.SetupGet(it => it.Name).Returns("Janeway");

            // WHEN a message is received by the behaviour frm the user.
            var messageContext = new MessageContext(from: janeway.Object, to: Mock.Of<IUser>(), body: "foo");
            mockBehaviour.OnNext(new IncomingMessage(messageContext));
            driver.StepFlow();

            // THEN an appropriate response should have been returned.
            var response = responses.ShouldHaveSingleItem();
            response.Context.Body.ShouldMatch("not authorised");
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
            var responses = new List<OutgoingMessage>();
            var driver = new ControllableFlowDriver();
            var mockBehaviour = new MockFlowBehaviour("S0", driver);
            mockBehaviour.Subscribe<OutgoingMessage>(it => responses.Add(it));
            mockBehaviour.RegisterFlow(builder => builder
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithNodes(nodes)
                .WithTransitions(transitions)
                .WithConnection("S0", "S1", "T0,1")
                .WithStartNodeId("S0"));

            // GIVEN a mock user.
            var janeway = new Mock<IUser>();
            janeway.SetupGet(it => it.UserId).Returns("Janeway");
            janeway.SetupGet(it => it.Name).Returns("Janeway");

            // WHEN a message is received by the behaviour frm the user.
            var messageContext = new MessageContext(from: janeway.Object, to: Mock.Of<IUser>(), body: "foo");
            mockBehaviour.OnNext(new IncomingMessage(messageContext));
            driver.StepFlow();
            driver.StepFlow();

            // THEN an appropriate response should have been returned.
            var response = responses.ShouldHaveSingleItem();
            response.Context.Body.ShouldMatch("not authorised");
        }
    }
}
