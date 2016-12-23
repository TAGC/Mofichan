using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests
{
    public class BehaviourVisitorTests
    {
        public static IEnumerable<object[]> Visitors
        {
            get
            {
                var tom = new MockUser("Tom", "Tom");
                var jerry = new MockUser("Jerry", "Jerry");
                var message = new MessageContext(tom, jerry, "Meow");

                yield return new object[]
                {
                    new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder),
                    message
                };

                yield return new object[]
                {
                    new OnPulseVisitor(new[] { message }, MockBotContext.Instance, CreateSimpleMessageBuilder),
                    message
                };
            }
        }

        [Fact]
        public void Exception_Should_Be_Thrown_By_On_Message_Visitor_If_Message_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(null, MockBotContext.Instance, () => Mock.Of<IResponseBodyBuilder>()));
        }

        public void Exception_Should_Be_Thrown_By_On_Pulse_Visitor_If_Valid_Response_Contexts_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(null, MockBotContext.Instance, () => Mock.Of<IResponseBodyBuilder>()));
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Bot_Context_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(new MessageContext(), null, () => Mock.Of<IResponseBodyBuilder>()));

            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(Enumerable.Empty<MessageContext>(), null, () => Mock.Of<IResponseBodyBuilder>()));
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Message_Builder_Factory_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(new MessageContext(), MockBotContext.Instance, null));

            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(Enumerable.Empty<MessageContext>(), MockBotContext.Instance, null));
        }

        [Fact]
        public void On_Message_Visitor_Should_Assume_Responses_Target_Carried_Message()
        {
            // GIVEN a message.
            var tom = new MockUser("Tom", "Tom");
            var jerry = new MockUser("Jerry", "Jerry");
            var message = new MessageContext(tom, jerry, "Meow");

            // GIVEN an on message visitor carrying the message.
            var visitor = new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder);

            // WHEN we register a response with the visitor without specfying "To".
            visitor.RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            // THEN the visitor should contain a response to the original message.
            var response = visitor.Responses.ShouldHaveSingleItem();
            response.Message.Body.ShouldBe("Don't hurt me!");
            response.Message.From.ShouldBe(jerry);
            response.Message.To.ShouldBe(tom);
        }

        [Fact]
        public void On_Pulse_Visitor_Should_Validate_Response_Context()
        {
            // GIVEN a message.
            var tom = new MockUser("Tom", "Tom");
            var jerry = new MockUser("Jerry", "Jerry");
            var message = new MessageContext(tom, jerry, "Meow");

            // GIVEN an on pulse visitor that only permits responses to this message.
            var visitor = new OnPulseVisitor(new[] { message }, MockBotContext.Instance, CreateSimpleMessageBuilder);

            // EXPECT we can register a response if the response context is the original message.
            visitor.RegisterResponse(rb => rb
                .To(message)
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            // EXPECT an exception is thrown if the response context is not specified.
            Assert.Throws<InvalidOperationException>(() => visitor.RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared"))));

            // EXPECT an exception is thrown if the response context is an invalid message.
            var invalidMessageContext = new MessageContext(tom, jerry, "Boo!");

            Assert.Throws<InvalidOperationException>(() => visitor.RegisterResponse(rb => rb
                .To(invalidMessageContext)
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared"))));
        }

        [Theory]
        [MemberData(nameof(Visitors))]
        public void Visitor_Should_Contain_Expected_Responses_After_Registration(
            IBehaviourVisitor visitor, MessageContext respondingTo)
        {
            // WHEN we register responses with the visitor.
            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("Go away!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:angry")));

            // THEN the visitor should contain the expected responses.
            var responses = visitor.Responses.ToArray();
            responses.Length.ShouldBe(2);

            responses[0].Message.Body.ShouldBe("Don't hurt me!");
            responses[0].RelevanceArgument.MessageTagArguments.ShouldHaveSingleItem().ShouldBe("mood:scared");

            responses[1].Message.Body.ShouldBe("Go away!");
            responses[1].RelevanceArgument.MessageTagArguments.ShouldHaveSingleItem().ShouldBe("mood:angry");
        }

        [Theory]
        [MemberData(nameof(Visitors))]
        public void Visitor_Should_Contain_Expected_Autonomous_Outputs_After_Registration(
            IBehaviourVisitor visitor, MessageContext _)
        {
            // WHEN we register two autonomous outputs with the visitor.
            var tom = new MockUser("Tom", "Tom");
            var sideEffectATriggered = false;
            var sideEffectBTriggered = false;

            visitor.RegisterAutonomousOutput(ob => ob
                .WithMessage(tom, mb => mb.FromRaw("Hello Tom!"))
                .WithSideEffect(() => sideEffectATriggered = true));

            visitor.RegisterAutonomousOutput(ob => ob
                .WithMessage(tom, mb => mb.FromRaw("Hi there Tom!"))
                .WithSideEffect(() => sideEffectBTriggered = true));

            // THEN the visitor should contain the expected autonomous outputs.
            var outputs = visitor.AutonomousOutputs.ToArray();
            outputs.Length.ShouldBe(2);

            outputs[0].Message.Body.ShouldBe("Hello Tom!");
            outputs[0].Message.To.ShouldBe(tom);
            outputs[0].SideEffects.Count().ShouldBe(1);

            outputs[1].Message.Body.ShouldBe("Hi there Tom!");
            outputs[1].Message.To.ShouldBe(tom);
            outputs[1].SideEffects.Count().ShouldBe(1);

            // AND the side effects of these outputs should be triggered when the outputs are accepted.
            sideEffectATriggered.ShouldBeFalse();
            sideEffectBTriggered.ShouldBeFalse();

            visitor.AutonomousOutputs.ToList().ForEach(it => it.Accept());

            sideEffectATriggered.ShouldBeTrue();
            sideEffectBTriggered.ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(Visitors))]
        public void Visitor_Should_Allow_Modification_Of_Contained_Responses(
            IBehaviourVisitor visitor, MessageContext respondingTo)
        {
            // GIVEN two responses registered with the visitor.
            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("Go away!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:angry")));

            // GIVEN a function that modifies responses.
            Func<Response, Response> f = response =>
            {
                var body = response.Message.Body;
                var sender = response.Message.From;
                var receiver = response.Message.To;
                var newBody = string.Format("'{0}' - {1}", body, (sender as IUser).Name);
                var newMessage = new MessageContext(sender, receiver, newBody);

                return response.DeriveFromNewMessage(newMessage);
            };

            // WHEN we request to modify the responses collected by the visitor using the function.
            visitor.ModifyResponses(f);

            // THEN the responses should have been modified as expected.
            var responses = visitor.Responses.ToArray();
            responses.Length.ShouldBe(2);

            responses[0].Message.Body.ShouldBe("'Don't hurt me!' - Jerry");
            responses[0].RelevanceArgument.MessageTagArguments.ShouldHaveSingleItem().ShouldBe("mood:scared");

            responses[1].Message.Body.ShouldBe("'Go away!' - Jerry");
            responses[1].RelevanceArgument.MessageTagArguments.ShouldHaveSingleItem().ShouldBe("mood:angry");
        }

        [Theory]
        [MemberData(nameof(Visitors))]
        public void Visitor_Should_Allow_Modification_Of_Contained_Autonomous_Outputs(
            IBehaviourVisitor visitor, MessageContext respondingTo)
        {
            // GIVEN two autonomous outputs registered with the visitor.
            visitor.RegisterAutonomousOutput(aob => aob
                .WithMessage(respondingTo.To, respondingTo.From, mb => mb.FromRaw("Don't hurt me!")));

            visitor.RegisterAutonomousOutput(aob => aob
                .WithMessage(respondingTo.To, respondingTo.From, mb => mb.FromRaw("Go away!")));

            // GIVEN a function that modifies autonomous outputs.
            Func<SimpleOutput, SimpleOutput> f = output =>
            {
                var body = output.Message.Body;
                var sender = output.Message.From;
                var receiver = output.Message.To;
                var newBody = string.Format("'{0}' - {1}", body, (sender as IUser).Name);
                var newMessage = new MessageContext(sender, receiver, newBody);

                return output.DeriveFromNewMessage(newMessage);
            };

            // WHEN we request to modify the outputs collected by the visitor using the function.
            visitor.ModifyAutonomousOutputs(f);

            // THEN the outputs should have been modified as expected.
            var outputs = visitor.AutonomousOutputs.ToArray();
            outputs.Length.ShouldBe(2);

            outputs[0].Message.Body.ShouldBe("'Don't hurt me!' - Jerry");

            outputs[1].Message.Body.ShouldBe("'Go away!' - Jerry");
        }
    }
}
