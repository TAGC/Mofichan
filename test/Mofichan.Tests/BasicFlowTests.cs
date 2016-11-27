using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
using Moq;
using Serilog;
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
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory) })
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Driver()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(null)
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory) })
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Transition_Selector()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(null)
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithStartNodeId("A")
                .WithNodes(new[] {
                    new FlowNode("A", NullStateAction, TransitionManagerFactory),
                    new FlowNode("B", NullStateAction, TransitionManagerFactory)
                })
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Generated_Response_Handler()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(null)
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
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithNodes(null)
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Empty_Flow_Node_Collection()
        {
            Assert.Throws<ArgumentException>(() => new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithNodes(Enumerable.Empty<IFlowNode>())
                .WithTransitions(Enumerable.Empty<IFlowTransition>())
                .Build());
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Flow_Constructed_With_Null_Flow_Transition_Collection()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(Mock.Of<IFlowTransitionSelector>())
                .WithGeneratedResponseHandler(DiscardResponse)
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
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("foo", "T0,1", "T0,0"), TransitionManagerFactory),
                new FlowNode("S1", NullStateAction, TransitionManagerFactory)
            };

            // GIVEN transitions T0,0 and T0,1.
            int transition_0_0_count = 0;
            int transition_0_1_count = 0;

            var transitions = new[]
            {
                new FlowTransition("T0,0", (c, m) => transition_0_0_count += 1),
                new FlowTransition("T0,1", (c, m) => transition_0_1_count += 1)
            };

            // GIVEN a flow created from these nodes and transitions.
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .Build();

            // WHEN the flow accepts a message containing "bar".
            flow.Accept(MessageFromBody("bar bar black sheep"));

            // AND we step the flow.
            driver.StepFlow();

            // THEN transition T0,0 should have occurred.
            transition_0_0_count.ShouldBe(1);
            transition_0_1_count.ShouldBe(0);

            // WHEN the flow accepts a message containing "foo".
            flow.Accept(MessageFromBody("have you any foo?"));

            // AND we step the flow.
            driver.StepFlow();

            // THEN transition T0,1 should have occurred.
            transition_0_0_count.ShouldBe(1);
            transition_0_1_count.ShouldBe(1);
        }

        [Fact]
        public void Flow_Should_Appropriately_Correlate_Users_With_Their_Positions_In_The_Flow()
        {
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S2 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1", "T0,0"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,2", "T1,1"), TransitionManagerFactory),
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
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
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
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", rick_C137));
            driver.StepFlow();
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the bar transition occurs for Rick C137 when the flow accepts a message containing "bar" from him.
            // Reason: this is the first transition - no prior transitions are necessary.
            flow.Accept(MessageFromBodyAndSender("Bar bar black sheep", rick_C137));
            driver.StepFlow();
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the foo transition occurs for Rick C137 when the flow accepts a message containing "foo" from him.
            // Reason: flow should now associate Rick C137 with node B.
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", rick_C137));
            driver.StepFlow();
            fooTransitionUsers.ShouldContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT no interesting transition occurs when the flow accepts a message containing "foo" from Rick J18.
            // Reason: Flow should still be at node A for Rick J18, as he is a different user.
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", rick_J18));
            driver.StepFlow();
            fooTransitionUsers.ShouldContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT the bar transition occurs for Rick J18 when the flow accepts a message containing "bar" from him.
            // Reason: this is the first transition for Rick J18 - no prior transitions are necessary.
            flow.Accept(MessageFromBodyAndSender("Bar bar black sheep", rick_J18));
            driver.StepFlow();
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldContain(rick_J18);

            // EXPECT the foo transition occurs for Rick J18 when the flow accepts a message containing "foo" from him.
            // Reason: flow should now associate Rick J18 with node B, along with Rick C137.
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", rick_J18));
            driver.StepFlow();
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
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1", "T0,0"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,0", "T1,1"), TransitionManagerFactory)
            };

            // GIVEN transitions T0,0, T0,1, T1,0 and T1,1 where T1,0 will generate a response.
            var bazReply = ReplyFromBody("Yes sir, yes sir, three baz full");

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,1"),
                new FlowTransition("T1,0", (context, _) => context.GeneratedResponseHandler(bazReply))
            };

            // GIVEN a flow created from these nodes and transitions.
            var responses = new List<OutgoingMessage>();
            Action<OutgoingMessage> responseHandler = it => responses.Add(it);
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
                .WithGeneratedResponseHandler(responseHandler)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S0", "T1,0")
                .WithConnection("S1", "S1", "T1,1")
                .Build();

            // GIVEN the ID of some user.
            var morty = "Morty";

            // EXPECT no response is generated when the flow accepts a message containing "bar" from Morty.
            // Reason: The "bar" transition should not generate any response when triggered.
            flow.Accept(MessageFromBodyAndSender("Bar bar black sheep", morty));
            driver.StepFlow();
            responses.ShouldBeEmpty();

            // EXPECT a response is generated when the flow accepts a message containing "foo" from Morty.
            // Reason: the "foo" transition should generate a response when triggered.
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", morty));
            driver.StepFlow();
            responses.Select(it => it.Context.Body).ShouldContain("Yes sir, yes sir, three baz full");
            responses.Clear();

            // EXPECT a response is generated when accepting these two messages from Morty again.
            // Reason: The flow should have gone back to the initial state after the previous step.
            flow.Accept(MessageFromBodyAndSender("Bar bar black sheep", morty));
            driver.StepFlow();
            flow.Accept(MessageFromBodyAndSender("Have you any foo?", morty));
            driver.StepFlow();
            responses.Select(it => it.Context.Body).ShouldContain("Yes sir, yes sir, three baz full");
        }

        [Fact]
        public void Flows_Should_Support_Multiple_Transitions_For_Accepted_Messages()
        {
            // GIVEN nodes S0, S1 and S2 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S2 occurs if "foo" is received
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1", "T0,E"), TransitionManagerFactory),
                new FlowNode("S1", DecideTransitionFromMatch("foo", "T1,2", "T1,E"), TransitionManagerFactory),
                new FlowNode("S2", NullStateAction, TransitionManagerFactory),
                new FlowNode("SE",  NullStateAction, TransitionManagerFactory)
            };

            // GIVEN transitions T0,1 and T1,2.
            bool barTransitionOccurred = false;
            bool fooTransitionOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,1", (c, m) => barTransitionOccurred = true),
                new FlowTransition("T1,2", (c, m) => fooTransitionOccurred = true),
                new FlowTransition("T0,E", (c, m) => { throw new Exception(); }),
                new FlowTransition("T1,E", (c, m) => { throw new Exception(); }),
            };

            // GIVEN a flow created from these nodes and transitions.
            var responses = new List<OutgoingMessage>();
            Action<OutgoingMessage> responseHandler = it => responses.Add(it);
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
                .WithGeneratedResponseHandler(responseHandler)
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S2", "T1,2")
                .WithConnection("S0", "SE", "T0,E")
                .WithConnection("S1", "SE", "T1,E")
                .Build();

            barTransitionOccurred.ShouldBeFalse();
            fooTransitionOccurred.ShouldBeFalse();

            // WHEN the flow accepts a message containing both "bar" and "foo".
            flow.Accept(MessageFromBody("Bar bar black sheep, have you any foo?"));
            driver.StepFlow();
            driver.StepFlow();

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
            var responses = new List<OutgoingMessage>();
            Action<OutgoingMessage> responseHandler = it => responses.Add(it);
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
                .WithGeneratedResponseHandler(responseHandler)
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
            flow.Accept(MessageFromBodyAndSender("How bar you?", rick_C137));
            driver.StepFlow();
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldNotContain(rick_J18);

            // EXPECT that the A->C transition occurs when Rick J18 says "foo".
            flow.Accept(MessageFromBodyAndSender("I am foo.", rick_J18));
            driver.StepFlow();
            barTransitionUsers.ShouldContain(rick_C137);
            barTransitionUsers.ShouldNotContain(rick_J18);
            fooTransitionUsers.ShouldNotContain(rick_C137);
            fooTransitionUsers.ShouldContain(rick_J18);
        }

        [Fact]
        public void Flows_Should_Support_Stochatic_Transitions()
        {
            // GIVEN nodes S0 and S1 where:
            //  - S0 -> S1 occurs if "bar" is received
            //  - S1 -> S0 occurs if "foo" is received
            //  - S1 -> S0 occurs after timeout
            var nodes = new[]
            {
                new FlowNode("S0", DecideTransitionFromMatch("bar", "T0,1", "T0,0"), TransitionManagerFactory),
                new FlowNode("S1", (context, manager) =>
                {
                    var body = context.Message.Body;

                    if (body.Contains("foo"))
                    {
                        manager.MakeTransitionCertain("T1,0");
                    }
                    else
                    {
                        manager["T1,0"] = 0;
                        manager["T1,1"] = 0.7;
                        manager["T1,0:timeout"] = 0.3;
                    }
                }, TransitionManagerFactory)
            };

            // GIVEN transitions T0,0, T0,1, T1,0 and T1,1 where T1,0 will generate a response.
            var bazReply = ReplyFromBody("Yes sir, yes sir, three baz full");
            bool timeoutOccurred = false;

            var transitions = new[]
            {
                new FlowTransition("T0,0"),
                new FlowTransition("T0,1"),
                new FlowTransition("T1,1"),
                new FlowTransition("T1,0", (context, _) => context.GeneratedResponseHandler(bazReply)),
                new FlowTransition("T1,0:timeout", (context, _) => timeoutOccurred = true)
            };

            // GIVEN a flow created from these nodes and transitions.
            bool responseReceived = false;
            Action<OutgoingMessage> responseHandler = _ => responseReceived = true;
            var driver = new ControllableFlowDriver();
            var flow = CreateDefaultFlowBuilder(nodes, transitions)
                .WithDriver(driver)
                .WithGeneratedResponseHandler(responseHandler)
                .WithConnection("S0", "S0", "T0,0")
                .WithConnection("S0", "S1", "T0,1")
                .WithConnection("S1", "S0", "T1,0")
                .WithConnection("S1", "S0", "T1,0:timeout")
                .WithConnection("S1", "S1", "T1,1")
                .Build();

            // GIVEN the ID of some user.
            var morty = "Morty";

            // WHEN we send a bar message.
            flow.Accept(MessageFromBodyAndSender("Bar bar black sheep", morty));
            driver.StepFlow();
            responseReceived.ShouldBeFalse();
            timeoutOccurred.ShouldBeFalse();

            /*
             * NOTE: assuming the production logic is correct,
             * this test may still erroneously fail once in every
             * 79,792,116,643,197,484,941,359,310,347,280,169,276,268,887,577,
             * 841,205,319,215,091,361,069,243,856,187,025,004,245,853,966,969,
             * 799,168,589,861,588,058,296,736,284,487,896,230,867,500,627,583,
             * 205,566,840,832 runs (give or take).
             * 
             * TODO: improve reliability within a few quadrillion quadrillion years time.
             */
            int runs = 0;
            while (runs < 1000)
            {
                runs += 1;
                driver.StepFlow();
                responseReceived.ShouldBeFalse();

                if (timeoutOccurred)
                {
                    return; // Passed.
                }
            }

            throw new ArgumentException("Expected timeout transition did not occur");
        }
    }
}
