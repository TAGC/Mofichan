using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Shouldly;
using Xunit;
using static Mofichan.Core.Flow.UserDrivenFlowManager;
using static Mofichan.Tests.TestUtility.FlowUtil;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests
{
    public class FlowTests
    {
        public static IEnumerable<object[]> FlowBuilders
        {
            get
            {
                yield return new object[] { new UserDrivenFlowManager.UserDrivenFlow.Builder() };
                yield return new object[] { new AutoFlowManager.AutoFlow.Builder() };
            }
        }

        [Theory]
        [MemberData(nameof(FlowBuilders))]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Node_Collection<T>(
            BaseFlow.Builder<T> flowBuilder)
            where T : BaseFlow
        {
            Assert.Throws<ArgumentNullException>(() => flowBuilder
                .WithNodes(null)
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Theory]
        [MemberData(nameof(FlowBuilders))]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Empty_Flow_Node_Collection<T>(
            BaseFlow.Builder<T> flowBuilder)
            where T : BaseFlow
        {
            Assert.Throws<ArgumentException>(() => flowBuilder
                .WithNodes(Enumerable.Empty<IFlowNode>())
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Theory]
        [MemberData(nameof(FlowBuilders))]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Transition_Collection<T>(
            BaseFlow.Builder<T> flowBuilder)
            where T : BaseFlow
        {
            Assert.Throws<ArgumentNullException>(() => flowBuilder
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction),
                    new FlowNode("B", NullStateAction) })
                .WithTransitions(null)
                .Build());
        }

        [Theory]
        [MemberData(nameof(FlowBuilders))]
        public void Exception_Should_Be_Thrown_When_Connecting_Node_To_Non_Existent_Node<T>(
            BaseFlow.Builder<T> flowBuilder)
            where T : BaseFlow
        {
            var nodes = new[]
            {
                new FlowNode("S0", NullStateAction)
            };

            var transitions = new[]
            {
                    new FlowTransition("T0,0")
            };

            Assert.Throws<ArgumentException>(() => ConfigureDefaultFlowBuilder(flowBuilder, nodes, transitions)
                .WithConnection("S0", "<<NON_EXISTENT>>", "T0,0")
                .Build());
        }

        [Theory]
        [MemberData(nameof(FlowBuilders))]
        public void Exception_Should_Be_Thrown_When_Connecting_Nodes_With_Non_Existent_Transition<T>(
            BaseFlow.Builder<T> flowBuilder)
            where T : BaseFlow
        {
            var nodes = new[]
            {
                new FlowNode("S0", NullStateAction),
                new FlowNode("S1", NullStateAction)
            };

            var transitions = Enumerable.Empty<IFlowTransition>();

            Assert.Throws<ArgumentException>(() => ConfigureDefaultFlowBuilder(flowBuilder, nodes, transitions)
                .WithConnection("S0", "S1", "<<NON_EXISTENT>>")
                .Build());
        }

        [Fact]
        public void Expected_Transition_Should_Occur_When_Flow_Accepts_Message()
        {
            // GIVEN node S0
            var nodes = new[]
            {
                new FlowNode("S0", (context, manager, visitor) =>
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

                    return null;
                }),
                new FlowNode("S1", NullStateAction)
            };

            // GIVEN transitions T0,0: foo and T0,0: bar
            int fooTransitionCount = 0;
            int barTransitionCount = 0;

            var transitions = new[]
            {
                new FlowTransition("T0,0:foo", (c, m, v) => fooTransitionCount += 1),
                new FlowTransition("T0,0:bar", (c, m, v) => barTransitionCount += 1)
            };

            // GIVEN a flow created from the node and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = ConfigureDefaultFlowBuilder(new UserDrivenFlow.Builder(), nodes, transitions)
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
        public void Flow_Should_Be_Able_To_Generate_Outgoing_Messages_On_Transition()
        {
            // GIVEN nodes S0 and S1 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S0 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1")),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,0"))
            };

            // GIVEN transitions T0,0, T0,1, T1,0 and T1,1 where T1,0 will generate a response.
            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,1"),
                new FlowTransition("T1,0", (context, _, visitor) => visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb.FromRaw("Yes sir, yes sir, three baz full"))))
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = ConfigureDefaultFlowBuilder(new UserDrivenFlow.Builder(), nodes, transitions)
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
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1")),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,2")),
                new FlowNode("S2", NullStateAction),
            };

            // GIVEN transitions T0,1 and T1,2.
            bool barTransitionOccurred = false;
            bool fooTransitionOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,1", (c, m, v) => barTransitionOccurred = true),
                new FlowTransition("T1,2", (c, m, v) => fooTransitionOccurred = true),
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = ConfigureDefaultFlowBuilder(new UserDrivenFlow.Builder(), nodes, transitions)
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
                new FlowNode("S0", (context, manager, visitor) =>
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

                    return null;
                }),
                new FlowNode("S1", NullStateAction),
                new FlowNode("S2", NullStateAction)
            };

            // GIVEN transitions T0,0, T0,1, T0,2
            var barTransitionUsers = new List<string>();
            var fooTransitionUsers = new List<string>();

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1",
                    (context, m, v) => barTransitionUsers.Add((context.Message.From as IUser).UserId)),
                new FlowTransition("T0,2",
                    (context, m, v) => fooTransitionUsers.Add((context.Message.From as IUser).UserId))
            };

            // GIVEN a flow template created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var template = ConfigureDefaultFlowBuilder(new UserDrivenFlow.Builder(), nodes, transitions)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S0", "S2", "T0,2")
                .Build();

            barTransitionUsers.ShouldBeEmpty();
            fooTransitionUsers.ShouldBeEmpty();

            // GIVEN the ID of two users.
            var tom = "Tom";
            var jerry = "Jerry";

            // GIVEN "flow A".
            var flowA = template.Copy();

            // EXPECT that the A->B transition occurs within flow A when Tom says "bar".
            flowA.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("How bar you?", tom)));
            flowA.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldContain(tom);
            fooTransitionUsers.ShouldNotContain(tom);

            // GIVEN "flow B".
            var flowB = template.Copy();

            // EXPECT that the A->C transition occurs within flow B when Jerry says "foo".
            flowB.Accept(visitorFactory.CreateMessageVisitor(MessageFromBodyAndSender("I am foo.", jerry)));
            flowB.Accept(visitorFactory.CreatePulseVisitor());
            barTransitionUsers.ShouldNotContain(jerry);
            fooTransitionUsers.ShouldContain(jerry);
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
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1")),
                new FlowNode("S1", (context, manager, visitor) =>
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

                    return null;
                })
            };

            // GIVEN a set of transitions where T1,0 will generate a response.
            bool timeoutOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,0", (context, _, visitor) => visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb.FromRaw("Yes sir, yes sir, three baz full")))),
                new FlowTransition("T1,0:timeout", (context, m, v) => timeoutOccurred = true)
            };

            // GIVEN a flow created from these nodes and transitions.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            var flow = ConfigureDefaultFlowBuilder(new UserDrivenFlow.Builder(), nodes, transitions)
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
