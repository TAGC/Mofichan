using System;
using Mofichan.Core;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class MessageContextSpec
    {
        [Fact]
        public void Message_Context_Should_Default_To_Zero_Delay()
        {
            // WHEN a message context is created with no specified delay.
            var context = new MessageContext(from: null, to: null, body: null);

            // THEN it should default to having zero delay.
            context.Delay.ShouldBe(TimeSpan.Zero);
        }

        [Fact]
        public void Incoming_Message_Should_Default_To_Having_No_Potential_Reply()
        {
            // WHEN an incoming message is created with no specified potential reply.
            var message = new IncomingMessage(context: default(MessageContext));

            // THEN the potential reply should be null.
            message.PotentialReply.ShouldBeNull();
        }
    }
}
