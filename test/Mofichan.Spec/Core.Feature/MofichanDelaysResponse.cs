﻿using System;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Moq;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanDelaysResponse : BaseScenario
    {
        public MofichanDelaysResponse() : base("Mofichan delays her response to appear more human")
        {
            var mockBehaviour = this.ConstructMockBehaviour();

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("delay"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockBehaviour.Object))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                .Then(s => s.Then_Mofichan_should_produce_a_delayed_response());
        }

        private Mock<IMofichanBehaviour> ConstructMockBehaviour()
        {
            var mockBehaviour = new Mock<IMofichanBehaviour>();

            MessageContext respondingTo = null;

            mockBehaviour
                .Setup(it => it.OnNext(It.IsAny<IBehaviourVisitor>()))
                .Callback<IBehaviourVisitor>(visitor =>
                {
                    var onMessageVisitor = visitor as OnMessageVisitor;
                    if (onMessageVisitor != null) respondingTo = onMessageVisitor.Message;

                    SendMockResponse(visitor, respondingTo);
                });

            return mockBehaviour;
        }

        private static void SendMockResponse(IBehaviourVisitor visitor, MessageContext respondingTo)
        {
            visitor.RegisterResponse(rb => rb.To(respondingTo).WithMessage(mb => mb.FromRaw("bar")));
        }

        private void Then_Mofichan_should_produce_a_delayed_response()
        {
            this.SentMessages.ShouldContain(message => message.Delay > TimeSpan.Zero);
        }
    }
}
