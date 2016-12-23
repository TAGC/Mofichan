using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Core.Flow.AutoFlowManager;
using static Mofichan.Core.Flow.UserDrivenFlowManager;
using static Mofichan.Tests.TestUtility.FlowUtil;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests.Behaviour
{
    public class BaseFlowReflectionBehaviourTests
    {
        #region Mocks
        public class FooBarFlowBehaviour : BaseFlowReflectionBehaviour
        {
            public FooBarFlowBehaviour()
                : base("S0", MockBotContext.Instance, MockLogger.Instance)
            {
                this.RegisterSimpleTransition("T0,0", from: "S0", to: "S0");
                this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
                this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
                this.RegisterSimpleTransition("T1,0:timeout", from: "S1", to: "S0");
                this.Configure<UserDrivenFlow>(Create);
            }

            [FlowState(id: "S0")]
            public void WaitOnBar(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                DecideTransitionFromMatch("bar", "T0,1")(context, manager, visitor);
            }

            [FlowState(id: "S1")]
            public void WaitOnFoo(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                var body = context.Message.Body;

                if (body.Contains("foo"))
                {
                    manager.MakeTransitionCertain("T1,0");
                }
                else
                {
                    manager.MakeTransitionsImpossible();
                    manager["T1,0:timeout"] = (int)new Random().SampleGaussian(100, 10);
                }
            }

            [FlowTransition(id: "T1,0", from: "S1", to: "S0")]
            public void OnFoo(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                var response = "Yes sir, yes sir, three baz full";

                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb.FromRaw(response)));
            }
        }

        public class WarfareFlowBehaviour : BaseFlowReflectionBehaviour
        {
            public WarfareFlowBehaviour()
                : base("S0", MockBotContext.Instance, MockLogger.Instance)
            {
                this.RegisterSimpleTransition("T0,0", from: "S0", to: "S0");
                this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
                this.RegisterSimpleTransition("T1,1", from: "S1", to: "S1");
                this.RegisterSimpleTransition("T1,0:timeout", from: "S1", to: "S0");
                this.Configure<UserDrivenFlow>(Create);
            }

            [FlowState(id: "S0")]
            public void Idle(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
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
            public void Attentive(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
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
                    manager.MakeTransitionsImpossible();
                    manager["T1,0:timeout"] = (int)new Random().SampleGaussian(100, 10);
                }
            }

            [FlowTransition(id: "T1,0:success", from: "S1", to: "S0")]
            public void OnAuthorisedRequest(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                var response = "Launching all warheads in T-10...";

                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb.FromRaw(response)));
            }

            [FlowTransition(id: "T1,0:failure", from: "S1", to: "S0")]
            public void OnUnauthorisedRequest(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                var response = "Sorry - you do not have permission to destroy the world.";

                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb.FromRaw(response)));
            }
        }

        // Tests auto-flows.
        public class CanadianFlowBehaviour : BaseFlowReflectionBehaviour
        {
            public CanadianFlowBehaviour() : base("S0", MockBotContext.Instance, MockLogger.Instance)
            {
                this.RegisterSimpleTransition("T0,1", "S0", "S1");
                this.RegisterSimpleTransition("T1,2", "S1", "S2");
                this.RegisterSimpleTransition("T2,3", "S2", "S3");
                this.RegisterSimpleTransition("T3,1", "S3", "S1");
                this.Configure<AutoFlow>(Create);
            }

            [FlowState(id: "S0")]
            public void Setup(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                context.Extras.User = new MockUser("Terrance", "Terrance");
                manager.MakeTransitionCertain("T0,1");
            }

            [FlowState(id: "S1")]
            public void NotFriend_Buddy(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                IUser recipient = context.Extras.User;

                visitor.RegisterAutonomousOutput(aob => aob
                    .WithMessage(recipient, mb => mb.FromRaw("I'm not your friend, buddy!")));

                manager.MakeTransitionCertain("T1,2");
            }

            [FlowState(id: "S2")]
            public void NotBuddy_Guy(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                IUser recipient = context.Extras.User;

                visitor.RegisterAutonomousOutput(aob => aob
                    .WithMessage(recipient, mb => mb.FromRaw("I'm not your buddy, guy!")));

                manager.MakeTransitionCertain("T2,3");
            }

            [FlowState(id: "S3")]
            public void NotGuy_Friend(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
            {
                IUser recipient = context.Extras.User;

                visitor.RegisterAutonomousOutput(aob => aob
                    .WithMessage(recipient, mb => mb.FromRaw("I'm not your guy, friend!")));

                manager.MakeTransitionCertain("T3,1");
            }
        }
        #endregion

        [Fact]
        public void Foo_Bar_Flow_Behaviour_Should_Behave_As_Expected()
        {
            // GIVEN a foo bar flow behaviour.
            var behaviour = new FooBarFlowBehaviour();

            // GIVEN the ID of some particular user.
            var borgUser = "borg-7.9";

            // EXPECT no response when the behaviour receives an incoming message containing "bar" from this user.
            // Reason: the "bar" transition should not generate any responses when triggered.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(
                MessageFromBodyAndSender("Bar bar black sheep", borgUser)));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.Responses.ShouldBeEmpty();

            // EXPECT a response to the user when the behaviour receives a "foo" message from them.
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(
                MessageFromBodyAndSender("Have you any foo?", borgUser)));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            var response = visitorFactory.Responses.ShouldHaveSingleItem().Message;
            response.Body.ShouldBe("Yes sir, yes sir, three baz full");
            (response.To as IUser).UserId.ShouldBe(borgUser);
        }

        [Fact]
        public void Canadian_Flow_Behaviour_Should_Run_Without_User_Interaction()
        {
            // GIVEN a canadian flow behaviour.
            var behaviour = new CanadianFlowBehaviour();

            // GIVEN a visitor factory.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);

            // EXPECT no outputs when we pulse the behaviour once.
            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.AutononousOutputs.ShouldBeEmpty();

            // EXPECT an output when we pulse the behaviour once more.
            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.AutononousOutputs.ShouldHaveSingleItem().Message.Body.ShouldContain("friend, buddy");

            // EXPECT a second output when we pulse the behaviour once more.
            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.AutononousOutputs.Count().ShouldBe(2);
            visitorFactory.AutononousOutputs.ElementAt(1).Message.Body.ShouldContain("buddy, guy");

            // EXPECT a third output when we pulse the behaviour once more.
            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.AutononousOutputs.Count().ShouldBe(3);
            visitorFactory.AutononousOutputs.ElementAt(2).Message.Body.ShouldContain("guy, friend");

            // EXPECT the first output again when we pulse the behaviour once more.
            behaviour.OnNext(visitorFactory.CreatePulseVisitor());
            visitorFactory.AutononousOutputs.Count().ShouldBe(4);
            visitorFactory.AutononousOutputs.ElementAt(3).Message.Body.ShouldContain("friend, buddy");
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Ignore_Normal_User_If_He_Hasnt_Got_Mofi_Attention()
        {
            WarfareFlowBehaviour behaviour;
            Mock<IUser> johnSmith, _;
            SetupWarfareTest(out behaviour, out johnSmith, out _);

            // WHEN the normal user requests to launch warheads without getting Mofi's attention.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // THEN no responses should have been generated.
            visitorFactory.Responses.ShouldBeEmpty();
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Ignore_Admin_User_If_He_Hasnt_Got_Mofi_Attention()
        {
            WarfareFlowBehaviour behaviour;
            Mock<IUser> _, donaldTrump;
            SetupWarfareTest(out behaviour, out _, out donaldTrump);

            // WHEN the administrator requests to launch warheads without getting Mofi's attention.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // THEN no responses should have been generated.
            visitorFactory.Responses.ShouldBeEmpty();
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Reject_Warhead_Launch_Request_From_Normal_User()
        {
            WarfareFlowBehaviour behaviour;
            Mock<IUser> johnSmith, _;
            SetupWarfareTest(out behaviour, out johnSmith, out _);

            // WHEN the normal user gets Mofi's attention.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Hey!",
                tags: new[] { "directedAtMofichan" })));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // AND requests Mofi to launch all warheads.
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: johnSmith.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // THEN mofi should refuse.
            Predicate<MessageContext> responseToJohn = it => it.To.Equals(johnSmith.Object);
            Predicate<MessageContext> requestRejection = it => it.Body == "Sorry - you do not have permission to destroy the world.";
            visitorFactory.Responses.Select(it => it.Message).ShouldContain(it => responseToJohn(it) && requestRejection(it));
        }

        [Fact]
        public void Warfare_Flow_Behaviour_Should_Accept_Warhead_Launch_Request_From_Admin_User()
        {
            WarfareFlowBehaviour behaviour;
            Mock<IUser> _, donaldTrump;
            SetupWarfareTest(out behaviour, out _, out donaldTrump);

            // WHEN the administrator gets Mofi's attention.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, CreateSimpleMessageBuilder);
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Hey!",
                tags: new[] { "directedAtMofichan" })));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // AND requests Mofi to launch all warheads.
            behaviour.OnNext(visitorFactory.CreateMessageVisitor(new MessageContext(
                from: donaldTrump.Object,
                to: Mock.Of<IUser>(),
                body: "Launch all the nuclear warheads!")));

            behaviour.OnNext(visitorFactory.CreatePulseVisitor());

            // THEN Mofichan should comply.
            Predicate<MessageContext> responseToTrump = it => it.To.Equals(donaldTrump.Object);
            Predicate<MessageContext> requestAcceptance = it => it.Body == "Launching all warheads in T-10...";
            visitorFactory.Responses.Select(it => it.Message).ShouldContain(it => responseToTrump(it) && requestAcceptance(it));
        }

        private static void SetupWarfareTest(
            out WarfareFlowBehaviour behaviour,
            out Mock<IUser> johnSmith,
            out Mock<IUser> donaldTrump)
        {
            // GIVEN a warfare flow behaviour.
            behaviour = new WarfareFlowBehaviour();

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
