using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;

namespace Mofichan.Tests.TestUtility
{
    public static class MessageUtil
    {
        public static IncomingMessage MessageFromBody(string body)
        {
            return MessageFromBodyAndSender(body, "Joe Appleseed");
        }

        public static IncomingMessage MessageFromBodyAndSender(string body, string senderId)
        {
            var sender = new Mock<IUser>();
            sender.SetupGet(it => it.UserId).Returns(senderId);

            var context = new MessageContext(
                from: sender.Object,
                to: Mock.Of<IMessageTarget>(),
                body: body);

            return new IncomingMessage(context);
        }

        public static OutgoingMessage ReplyFromBody(string body)
        {
            var context = new MessageContext(Mock.Of<IMessageTarget>(), Mock.Of<IMessageTarget>(), body);
            return new OutgoingMessage { Context = context };
        }

        public static OutgoingMessage RespondTo(MessageContext messageContext, string responseBody)
        {
            var sender = messageContext.From as IUser;

            var outgoingMessageContext = new MessageContext(
                Mock.Of<IUser>(), sender, responseBody);

            var outgoingMessage = new OutgoingMessage { Context = outgoingMessageContext };
            return outgoingMessage;
        }

        public static OutgoingMessage ResponseFromBodyAndRecipient(string body, string recipientId)
        {
            var recipient = new Mock<IUser>();
            recipient.SetupGet(it => it.UserId).Returns(recipientId);

            var context = new MessageContext(
                from: Mock.Of<IMessageTarget>(),
                to: recipient.Object,
                body: body);

            return new OutgoingMessage { Context = context };
        }
    }
}
