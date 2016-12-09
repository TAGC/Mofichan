using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
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
                var mockSender = new Mock<IUser>();
                mockSender.SetupGet(it => it.Name).Returns("Tom");

                var mockRecipient = new Mock<IUser>();
                mockRecipient.SetupGet(it => it.Name).Returns("Jerry");

                var message = new MessageContext(mockSender.Object, mockRecipient.Object, "Meow");

                yield return new object[]
                {
                    new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder)
                };

                yield return new object[]
                {
                    new OnPulseVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder)
                };
            }
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Message_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(null, MockBotContext.Instance, () => Mock.Of<IResponseBodyBuilder>()));

            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(null, MockBotContext.Instance, () => Mock.Of<IResponseBodyBuilder>()));
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Bot_Context_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(new MessageContext(), null, () => Mock.Of<IResponseBodyBuilder>()));

            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(new MessageContext(), null, () => Mock.Of<IResponseBodyBuilder>()));
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Message_Builder_Factory_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OnMessageVisitor(new MessageContext(), MockBotContext.Instance, null));

            Assert.Throws<ArgumentNullException>(() =>
                new OnPulseVisitor(new MessageContext(), MockBotContext.Instance, null));
        }

        [Theory]
        [MemberData(nameof(Visitors))]
        public void Visitor_Should_Contain_Expected_Responses_After_Registration(IBehaviourVisitor visitor)
        {
            // WHEN we register responses with the visitor.
            visitor.RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            visitor.RegisterResponse(rb => rb
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
        public void Visitor_Should_Allow_Modification_Of_Contained_Responses(IBehaviourVisitor visitor)
        {
            // GIVEN two responses registered with the visitor.
            visitor.RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("Don't hurt me!"))
                .RelevantBecause(it => it.SuitsMessageTags("mood:scared")));

            visitor.RegisterResponse(rb => rb
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
    }
}
