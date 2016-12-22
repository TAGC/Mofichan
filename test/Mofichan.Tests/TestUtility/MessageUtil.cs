using System;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;

namespace Mofichan.Tests.TestUtility
{
    public static class MessageUtil
    {
        public static MessageContext MessageFromBody(string body)
        {
            return MessageFromBodyAndSender(body, "Joe Appleseed");
        }

        public static MessageContext MessageFromBodyAndSender(string body, string senderId)
        {
            var sender = new Mock<IUser>();
            sender.SetupGet(it => it.UserId).Returns(senderId);

            return new MessageContext(
                from: sender.Object,
                to: Mock.Of<IMessageTarget>(),
                body: body);
        }

        public static MessageContext ReplyFromBody(string body)
        {
            return new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), body);
        }

        public static MessageContext RespondTo(MessageContext message, string responseBody)
        {
            var sender = message.From as IUser;

            return new MessageContext(Mock.Of<IUser>(), sender, responseBody);
        }

        public static MessageContext ResponseFromBodyAndRecipient(string body, string recipientId)
        {
            var recipient = new Mock<IUser>();
            recipient.SetupGet(it => it.UserId).Returns(recipientId);

            return new MessageContext(
                from: Mock.Of<IMessageTarget>(),
                to: recipient.Object,
                body: body);
        }

        public static IResponseBodyBuilder CreateSimpleMessageBuilder()
        {
            var messageBody = string.Empty;
            var mockResponseBuilder = new Mock<IResponseBodyBuilder>();

            mockResponseBuilder
                .Setup(it => it.FromRaw(It.IsAny<string>()))
                .Returns(mockResponseBuilder.Object)
                .Callback<string>(raw => messageBody += raw);

            mockResponseBuilder
                .Setup(it => it.Build())
                .Returns(() => messageBody);

            return mockResponseBuilder.Object;
        }
    }
}
