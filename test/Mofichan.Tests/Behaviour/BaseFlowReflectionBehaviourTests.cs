using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.MessageUtil;
using static Mofichan.Tests.TestUtility.FlowUtil;
using Serilog;

namespace Mofichan.Tests.Behaviour
{
    public class BaseFlowReflectionBehaviourTests
    {
        #region Mocks
        public class FooBarFlowBehaviour : BaseFlowReflectionBehaviour
        {
            public FooBarFlowBehaviour(IFlowDriver flowDriver)
                : base("S0", () => Mock.Of<IResponseBuilder>(), TransitionManagerFactory, flowDriver,
                      new FairFlowTransitionSelector(), new LoggerConfiguration().CreateLogger())
            {
                this.RegisterSimpleTransition("T0,0", from: "S0", to: "S0");
                this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
                this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
                this.RegisterSimpleTransition("T1,0:timeout", from: "S1", to: "S0");
                this.Configure();
            }

            [FlowState(id: "S0")]
            public void WaitOnBar(FlowContext context, IFlowTransitionManager manager)
            {
                DecideTransitionFromMatch("bar", "T0,1", "T0,0")(context, manager);
            }

            [FlowState(id: "S1")]
            public void WaitOnFoo(FlowContext context, IFlowTransitionManager manager)
            {
                var body = context.Message.Body;

                if (body.Contains("foo"))
                {
                    manager.MakeTransitionCertain("T1,0");
                }
                else
                {
                    manager["T1,1"] = 0.7;
                    manager["T1,0:timeout"] = 0.3;
                }
            }

            [FlowTransition(id: "T1,0", from: "S1", to: "S0")]
            public void OnFoo(FlowContext context, IFlowTransitionManager manager)
            {
                var response = "Yes sir, yes sir, three baz full";
                var outgoingMessage = RespondTo(context.Message, response);
                context.GeneratedResponseHandler(outgoingMessage);
            }
        }

        public class WarfareFlowBehaviour : BaseFlowReflectionBehaviour
        {
            public WarfareFlowBehaviour(IFlowDriver flowDriver)
                : base("S0", () => Mock.Of<IResponseBuilder>(), TransitionManagerFactory, flowDriver,
                      new FairFlowTransitionSelector(), new LoggerConfiguration().CreateLogger())
            {
                this.RegisterSimpleTransition("T0,0", from: "S0", to: "S0");
                this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
                this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
                this.RegisterSimpleTransition("T1,0:timeout", from: "S1", to: "S0");
                this.Configure();
            }

            [FlowState(id: "S0")]
            public void Idle(FlowContext context, IFlowTransitionManager manager)
            {
                if (context.Message.Tags.Contains("directedAtMofichan"))
                {
                    manager.MakeTransitionCertain("T0,1");
                }
                else
                {
                    manager.MakeTransitionCertain("T0,0");
                }
            }

            [FlowState(id: "S1")]
            public void Attentive(FlowContext context, IFlowTransitionManager manager)
            {
                if (Regex.IsMatch(context.Message.Body, "launch.*nuclear warheads", RegexOptions.IgnoreCase))
                {
                    if ((context.Message.From as IUser).Type == UserType.Adminstrator)
                    {
                        manager.MakeTransitionCertain("T1,0:success");
                    }
                    else
                    {
                        manager.MakeTransitionCertain("T1,0:failure");
                    }
                }
                else
                {
                    manager["T1,1"] = 0.7;
                    manager["T1,0:timeout"] = 0.3;
                }
            }

            [FlowTransition(id: "T1,0:success", from: "S1", to: "S0")]
            public void OnAuthorisedRequest(FlowContext context, IFlowTransitionManager manager)
            {
                var response = "Launching all warheads in T-10...";
                var outgoingMessage = RespondTo(context.Message, response);
                context.GeneratedResponseHandler(outgoingMessage);
            }

            [FlowTransition(id: "T1,0:failure", from: "S1", to: "S0")]
            public void OnUnauthorisedRequest(FlowContext context, IFlowTransitionManager manager)
            {
                var response = "Sorry - you do not have permission to destroy the world.";
                var outgoingMessage = RespondTo(context.Message, response);
                context.GeneratedResponseHandler(outgoingMessage);
            }
        }
        #endregion

        [Fact]
        public void Foo_Bar_Flow_Behaviour_Should_Behave_As_Expected()
        {
            // GIVEN a foo bar flow behaviour.
            var flowDriver = new ControllableFlowDriver();
            var responses = new List<OutgoingMessage>();
            var behaviour = new FooBarFlowBehaviour(flowDriver);
            behaviour.Subscribe<OutgoingMessage>(it => responses.Add(it));

            // GIVEN the ID of some particular user.
            var borgUser = "borg-7.9";

            // EXPECT no response when the behaviour receives an incoming message containing "bar" from this user.
            // Reason: the "bar" transition should not generate any responses when triggered.
            behaviour.OnNext(MessageFromBodyAndSender("Bar bar black sheep", borgUser));
            flowDriver.StepFlow();
            responses.ShouldBeEmpty();

            // EXPECT a response to the user when the behaviour receives a "foo" message from them.
            behaviour.OnNext(MessageFromBodyAndSender("Have you any foo?", borgUser));
            flowDriver.StepFlow();
            responses.ShouldContain(it => it.Context.Body == "Yes sir, yes sir, three baz full" &&
                                         (it.Context.To as IUser).UserId == borgUser);
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Ignore_Normal_User_If_He_Hasnt_Got_Mofi_Attention()
        {
            var responses = new List<OutgoingMessage>();
            WarfareFlowBehaviour behaviour;
            ControllableFlowDriver flowDriver;
            Mock<IUser> johnSmith, _;
            SetupWarfareTest(responses, out flowDriver, out behaviour, out johnSmith, out _);

            // WHEN the normal user requests to launch warheads without getting Mofi's attention.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            flowDriver.StepFlow();

            // THEN no responses should have been generated.
            responses.ShouldBeEmpty();
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Ignore_Admin_User_If_He_Hasnt_Got_Mofi_Attention()
        {
            var responses = new List<OutgoingMessage>();
            WarfareFlowBehaviour behaviour;
            ControllableFlowDriver flowDriver;
            Mock<IUser> _, donaldTrump;
            SetupWarfareTest(responses, out flowDriver, out behaviour, out _, out donaldTrump);

            // WHEN the administrator requests to launch warheads without getting Mofi's attention.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            flowDriver.StepFlow();

            // THEN no responses should have been generated.
            responses.ShouldBeEmpty();
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Reject_Warhead_Launch_Request_From_Normal_User()
        {
            var responses = new List<OutgoingMessage>();
            WarfareFlowBehaviour behaviour;
            ControllableFlowDriver flowDriver;
            Mock<IUser> johnSmith, _;
            SetupWarfareTest(responses, out flowDriver, out behaviour, out johnSmith, out _);

            // WHEN the normal user gets Mofi's attention.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Hey!",
                tags: new[] { "directedAtMofichan" })));

            flowDriver.StepFlow();

            // AND requests Mofi to launch all warheads.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            flowDriver.StepFlow();

            // THEN mofi should refuse.
            Predicate<MessageContext> responseToJohn = it => it.To == johnSmith.Object;
            Predicate<MessageContext> requestRejection = it => it.Body == "Sorry - you do not have permission to destroy the world.";
            responses.Select(it => it.Context).ShouldContain(it => responseToJohn(it) && requestRejection(it));
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Accept_Warhead_Launch_Request_From_Admin_User()
        {
            var responses = new List<OutgoingMessage>();
            WarfareFlowBehaviour behaviour;
            ControllableFlowDriver flowDriver;
            Mock<IUser> _, donaldTrump;
            SetupWarfareTest(responses, out flowDriver, out behaviour, out _, out donaldTrump);

            // WHEN the administrator gets Mofi's attention.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Hey!",
                tags: new[] { "directedAtMofichan" })));

            flowDriver.StepFlow();

            // AND requests Mofi to launch all warheads.
            behaviour.OnNext(new IncomingMessage(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            flowDriver.StepFlow();

            // THEN Mofichan should comply.
            Predicate<MessageContext> responseToTrump = it => it.To == donaldTrump.Object;
            Predicate<MessageContext> requestAcceptance = it => it.Body == "Launching all warheads in T-10...";
            responses.Select(it => it.Context).ShouldContain(it => responseToTrump(it) && requestAcceptance(it));
        }

        private static void SetupWarfareTest(
            List<OutgoingMessage> responses,
            out ControllableFlowDriver flowDriver,
            out WarfareFlowBehaviour behaviour,
            out Mock<IUser> johnSmith,
            out Mock<IUser> donaldTrump)
        {
            // GIVEN a warfare flow behaviour.
            flowDriver = new ControllableFlowDriver();
            behaviour = new WarfareFlowBehaviour(flowDriver);
            behaviour.Subscribe<OutgoingMessage>(it => responses.Add(it));

            // GIVEN a normal user.
            johnSmith = new Mock<IUser>();
            johnSmith.SetupGet(it => it.UserId).Returns("John Smith");
            johnSmith.SetupGet(it => it.Type).Returns(UserType.NormalUser);

            // GIVEN an administrative user.
            donaldTrump = new Mock<IUser>();
            donaldTrump.SetupGet(it => it.UserId).Returns("Donald Trump");
            donaldTrump.SetupGet(it => it.Type).Returns(UserType.Adminstrator);
        }
    }
}
