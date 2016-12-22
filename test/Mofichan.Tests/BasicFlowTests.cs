using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.FlowUtil;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests
{
    public class BasicFlowTests
    {
        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Logger()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(null)
                .WithManager(Mock.Of<IFlowManager>())
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory) })
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Manager()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(MockLogger.Instance)
                .WithManager(null)
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory) })
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Node_Collection()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(MockLogger.Instance)
                .WithManager(Mock.Of<IFlowManager>())
                .WithNodes(null)
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Empty_Flow_Node_Collection()
        {
            Assert.Throws<ArgumentException>(() => new BasicFlow.Builder()
                .WithLogger(MockLogger.Instance)
                .WithManager(Mock.Of<IFlowManager>())
                .WithNodes(Enumerable.Empty<IFlowNode>())
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Transition_Collection()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(MockLogger.Instance)
                .WithManager(Mock.Of<IFlowManager>())
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory) })
                .WithTransitions(null)
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_When_Connecting_Node_To_Non_Existent_Node()
        {
            var nodes = new[]
            {
                new FlowNode("S0", NullStateAction, TransitionManagerFactory)
            };

            var transitions = new[]
            {
                    new FlowTransition("T0,0")
            };

            Assert.Throws<ArgumentException>(() => CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "<<NON_EXISTENT>>", "T0,0")
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_When_Connecting_Nodes_With_Non_Existent_Transition()
        {
            var nodes = new[]
            {
                new FlowNode("S0", NullStateAction, TransitionManagerFactory),
                new FlowNode("S1", NullStateAction, TransitionManagerFactory)
            };

            var transitions = Enumerable.Empty<IFlowTransition>();

            Assert.Throws<ArgumentException>(() => CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S1", "<<NON_EXISTENT>>")
                .Build());
        }

        [Fact]
        public void Expected_Transition_Should_Occur_When_Flow_Accepts_Message()
        {
            // GIVEN node S0
            var nodes = new[]
            {
                new FlowNode("S0", (context, manager) =>
                {
                    manager.MakeTransitionsImpossible();

                    if (context.Message.Body.Contains("foo"))
                    {
                        manager.MakeTransitionCertain("T0,0:foo");
                    }
                    else if (context.Message.Body.Contains("bar"))
                    {
                        manager.MakeTransitionCertain("T0,0:bar");
                    }
                }, TransitionManagerFactory),
                new FlowNode("S1", NullStateAction, TransitionManagerFactory)
            };

            // GIVEN transitions T0,0: foo and T0,0: bar
            int fooTransitionCount = 0;
            int barTransitionCount = 0;

            var transitions = new[]
            {
                new FlowTransition("T0,0:foo", (c, m) => fooTransitionCount += 1),
                new FlowTransition("T0,0:bar", (c, m) => barTransitionCount += 1)
            };

            // GIVEN a flow created from the node and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S0", "T0,0:foo")
                .WithConnection("S0", "S0", "T0,0:bar")
                .Build();

            // GIVEN a foo, bar and baz message.
            var fooMessage = MessageFromBody("foo");
            var barMessage = MessageFromBody("bar");
            var bazMessage = MessageFromBody("baz");

            // EXPECT T0,0:bar to occur when the flow receives a message containing "bar".
            flow.Accept(visitorFactory.CreateMessageVisitor(barMessage));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionCount.ShouldBe(1);
            fooTransitionCount.ShouldBe(0);

            // EXPECT T0,0:foo to occur when the flow receives a message containing "foo".
            flow.Accept(visitorFactory.CreateMessageVisitor(fooMessage));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionCount.ShouldBe(1);
            fooTransitionCount.ShouldBe(1);

            // EXPECT no transition to occur when the flow receives a message containing "baz".
            flow.Accept(visitorFactory.CreateMessageVisitor(bazMessage));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionCount.ShouldBe(1);
            fooTransitionCount.ShouldBe(1);

            // EXPECT T0,0:foo to occur again when the flow receives another message containing "foo".
            flow.Accept(visitorFactory.CreateMessageVisitor(fooMessage));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionCount.ShouldBe(1);
            fooTransitionCount.ShouldBe(2);
        }

        [Fact]
        public void Flow_Should_Appropriately_Correlate_Users_With_Their_Positions_In_The_Flow()
        {
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S2 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,2"), TransitionManagerFactory),
                new FlowNode("S2", NullStateAction, TransitionManagerFactory),
            };

            // GIVEN the transitions, where T0,1 and T1,2 are "interesting".
            var barTransitionUsers = new List<string>();
            var fooTransitionUsers = new List<string>();

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T1,1"),
                new FlowTransition("T0,1",
                    (context, _) => barTransitionUsers.Add((context.Message.From as IUser).UserId)),
                new FlowTransition("T1,2",
                    (context, m) => fooTransitionUsers.Add((context.Message.From as IUser).UserId))
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S1", "T1,1")
                .WithConnection("S1", "S2", "T1,2")
                .Build();

            // GIVEN the IDs of two users.
            var rick_C137 = "Rick_C137";
            var rick_J18 = "Rick_J18";

            // EXPECT no interesting transition occurs when the flow accepts a message containing "foo" from Rick C137.
            // Reason: transition to node B needs to have happened first.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", rick_C137)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the bar transition occurs for Rick C137 when the flow accepts a message containing "bar" from him.
            // Reason: this is the first transition - no prior transitions are necessary.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Bar bar black sheep", rick_C137)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the foo transition occurs for Rick C137 when the flow accepts a message containing "foo" from him.
            // Reason: flow should now associate Rick C137 with node B.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", rick_C137)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            fooTransitionUsers.ShouldContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT no interesting transition occurs when the flow accepts a message containing "foo" from Rick J18.
            // Reason: Flow should still be at node A for Rick J18, as he is a different user.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", rick_J18)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            fooTransitionUsers.ShouldContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the bar transition occurs for Rick J18 when the flow accepts a message containing "bar" from him.
            // Reason: this is the first transition for Rick J18 - no prior transitions are necessary.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Bar bar black sheep", rick_J18)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldContain(rick_J18);

            // EXPECT the foo transition occurs for Rick J18 when the flow accepts a message containing "foo" from him.
            // Reason: flow should now associate Rick J18 with node B, along with Rick C137.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", rick_J18)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            fooTransitionUsers.ShouldContain(rick_C137);
            fooTransitionUsers.ShouldContain(rick_J18);
        }

        [Fact]
        public void Flow_Should_Be_Able_To_Generate_Outgoing_Messages_On_Transition()
        {
            // GIVEN nodes S0 and S1 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S0 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,0"), TransitionManagerFactory)
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

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S0", "T1,0")
                .WithConnection("S1", "S1", "T1,1")
                .Build();

            // GIVEN the ID of some user.
            var morty = "Morty";

            // EXPECT no response is generated when the flow accepts a message containing "bar" from Morty.
            // Reason: The "bar" transition should not generate any response when triggered.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Bar bar black sheep", morty)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.ShouldBeEmpty();

            // EXPECT a response is generated when the flow accepts a message containing "foo" from Morty.
            // Reason: the "foo" transition should generate a response when triggered.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", morty)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.Select(it => it.Message.Body).ShouldContain("Yes sir, yes sir, three baz full");

            // EXPECT a response is generated when accepting these two messages from Morty again.
            // Reason: The flow should have gone back to the initial state after the previous step.
            visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Bar bar black sheep", morty)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Have you any foo?", morty)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.Select(it => it.Message.Body).ShouldContain("Yes sir, yes sir, three baz full");
        }

        [Fact]
        public void Flows_Should_Support_Multiple_Transitions_For_Accepted_Messages()
        {
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S2 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,2"), TransitionManagerFactory),
                new FlowNode("S2", NullStateAction, TransitionManagerFactory),
            };

            // GIVEN transitions T0,1 and T1,2.
            bool barTransitionOccurred = false;
            bool fooTransitionOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,1", (c, m) => barTransitionOccurred = true),
                new FlowTransition("T1,2", (c, m) => fooTransitionOccurred = true),
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S2", "T1,2")
                .Build();

            barTransitionOccurred.ShouldBeFalse();
            fooTransitionOccurred.ShouldBeFalse();

            // WHEN the flow accepts a message containing both "bar" and "foo".
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBody("Bar bar black sheep, have you any foo?")));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            flow.Accept(visitorFactory.CreatePulseVisitor());

            // THEN both transitions should have been triggered.
            barTransitionOccurred.ShouldBeTrue();
            fooTransitionOccurred.ShouldBeTrue();
        }

        [Fact]
        public void Nodes_Should_Support_Multiple_Possible_Transitions()
        {
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S0 -> S2 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", (context, manager) =>
                {
                    var body = context.Message.Body;

                    if (body.Contains("bar"))
                    {
                        manager.MakeTransitionCertain("T0,1");
                    }
                    else if (body.Contains("foo"))
                    {
                        manager.MakeTransitionCertain("T0,2");
                    }
                    else
                    {
                        manager.MakeTransitionCertain("T0,0");
                    }
                }, TransitionManagerFactory),
                new FlowNode("S1", NullStateAction, TransitionManagerFactory),
                new FlowNode("S2", NullStateAction, TransitionManagerFactory)
            };

            // GIVEN transitions T0,0, T0,1, T0,2
            var barTransitionUsers = new List<string>();
            var fooTransitionUsers = new List<string>();

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1",
                    (context, _) => barTransitionUsers.Add((context.Message.From as IUser).UserId)),
                new FlowTransition("T0,2",
                    (context, _) => fooTransitionUsers.Add((context.Message.From as IUser).UserId))
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S0", "S2", "T0,2")
                .Build();

            barTransitionUsers.ShouldBeEmpty();
            fooTransitionUsers.ShouldBeEmpty();

            // GIVEN the IDs of two users.
            var rick_C137 = "Rick_C137";
            var rick_J18 = "Rick_J18";

            // EXPECT that the A->B transition occurs when Rick C137 says "bar".
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("How bar you?", rick_C137)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT that the A->C transition occurs when Rick J18 says "foo".
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("I am foo.", rick_J18)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldContain(rick_J18);
        }

        [Fact]
        public void Flows_Should_Support_Stochatic_Transitions()
        {
            // GIVEN Gaussian distribution parameters to determine a transition timeout.
            var mu = 100;
            var sigma = 10;

            // GIVEN nodes S0 and S1 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S0 occurs if "foo" is received
            //  - S1 -> S0 occurs after timeout
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1"), TransitionManagerFactory),
                new FlowNode("S1", (context, manager) =>
                {
                    var body = context.Message.Body;

                    if (body.Contains("foo"))
                    {
                        manager.MakeTransitionCertain("T1,0");
                    }
                    else
                    {
                        manager.MakeTransitionsImpossible();
                        manager["T1,0:timeout"] = (int)new Random().SampleGaussian(mu, sigma);
                    }
                }, TransitionManagerFactory)
            };

            // GIVEN a set of transitions where T1,0 will generate a response.
            bool timeoutOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,0", (context, _) => context.Visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb.FromRaw("Yes sir, yes sir, three baz full")))),
                new FlowTransition("T1,0:timeout", (context, _) => timeoutOccurred = true)
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S0", "T1,0")
                .WithConnection("S1", "S0", "T1,0:timeout")
                .Build();

            // GIVEN the ID of some user.
            var morty = "Morty";

            // WHEN we send a bar message but not a foo message.
            flow.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("Bar bar black sheep", morty)));
            flow.Accept(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.ShouldBeEmpty();
            timeoutOccurred.ShouldBeFalse();

            // THEN the flow should timeout after an expected number of steps.
            int numSteps = 0;
            int lowStepBound = mu - sigma * 4;
            int highStepBound = mu + sigma * 4;

            // Timeout should not occur after [lowStepBound] steps with 99.994% (4-sigma) probability.
            for (; numSteps < lowStepBound; numSteps++)
            {
                flow.Accept(visitorFactory.CreatePulseVisitor());
            }

            visitorFactory.Responses.ShouldBeEmpty();
            timeoutOccurred.ShouldBeFalse();

            // Timeout should occur after [highStepBound] steps with 99.994% (4-sigma) probability.
            for (; numSteps < highStepBound; numSteps++)
            {
                flow.Accept(visitorFactory.CreatePulseVisitor());
            }

            visitorFactory.Responses.ShouldBeEmpty();
            timeoutOccurred.ShouldBeTrue();
        }
    }
}
