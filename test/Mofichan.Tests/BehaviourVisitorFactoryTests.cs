using System;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BehaviourVisitorFactoryTests
    {
        [Fact]
        public void Factory_Should_Create_Visitors_Using_Supplied_Message_Builder_Factory()
        {
            // GIVEN a mock message builder factory.
            bool mockBuilderUsed = false;

            Func<IResponseBodyBuilder> messageBuilderFactory = () =>
            {
                var mockBuilder = new Mock<IResponseBodyBuilder>();
                mockBuilder
                    .Setup(it => it.FromRaw(It.IsAny<string>()))
                    .Callback(() => mockBuilderUsed = true);

                return mockBuilder.Object;
            };

            // GIVEN an instance of a visitor factory constructed with the message builder factory.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance, messageBuilderFactory);

            // WHEN we create a visitor.
            var visitor = visitorFactory.CreateMessageVisitor(new MessageContext());

            // AND we try to register a response.
            visitor.RegisterResponse(rb => rb.WithMessage(mb => mb.FromRaw("foo")));

            // THEN the specified message builder factory should have been used to create the message builder.
            mockBuilderUsed.ShouldBeTrue();
        }

        [Fact]
        public void Pulse_Visitors_Should_Permit_Responses_To_Last_Provided_Message()
        {
            // GIVEN an instance of a visitor factory.
            var visitorFactory = new BehaviourVisitorFactory(MockBotContext.Instance,
                () => Mock.Of<IResponseBodyBuilder>());

            // GIVEN a message.
            var expectedMessage = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");

            // WHEN we create an OnMessageVisitor provided with the message.
            visitorFactory.CreateMessageVisitor(expectedMessage);

            // AND we create an OnPulseVisitor.
            var onPulseVisitor = visitorFactory.CreatePulseVisitor();

            // WHEN we register a response with the visitor using the message as the response context.
            onPulseVisitor.RegisterResponse(rb => rb.To(expectedMessage));

            // THEN the generated response should contain the message used to create the OnMessageVisitor.
            var response = onPulseVisitor.Responses.ShouldHaveSingleItem();
            var actualMessage = response.RespondingTo;

            actualMessage.ShouldBe(expectedMessage);
        }
    }
}
