using System;
using System.Diagnostics;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanDelaysResponse : BaseScenario
    {
        private IObserver<OutgoingMessage> observer;

        public MofichanDelaysResponse() : base("Mofichan delays her response to appear more human")
        {
            var mockBehaviour = this.ConstructMockBehaviour();

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("delay"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockBehaviour.Object))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                .Then(s => s.Then_Mofichan_should_produce_a_delayed_response());
        }

        private Mock<IMofichanBehaviour> ConstructMockBehaviour()
        {
            var mockBehaviour = new Mock<IMofichanBehaviour>();
            mockBehaviour
                .Setup(it => it.Subscribe(It.IsAny<IObserver<OutgoingMessage>>()))
                .Callback<IObserver<OutgoingMessage>>(linkedObserver => this.observer = linkedObserver);

            mockBehaviour
                .Setup(it => it.OnNext(It.IsAny<IncomingMessage>()))
                .Callback(this.SendMockResponse);

            return mockBehaviour;
        }

        private void SendMockResponse()
        {
            var mockResponseContext = new MessageContext(
                from: this.MofichanUser, to: this.JohnSmithUser, body: "bar");

            OutgoingMessage mockResponse = new OutgoingMessage { Context = mockResponseContext };

            Debug.Assert(mockResponse.Context.Delay == TimeSpan.Zero,
                "There should be no delay by default");

            this.observer.OnNext(mockResponse);
        }

        private void Then_Mofichan_should_produce_a_delayed_response()
        {
            this.SentMessages.ShouldContain(response => response.Context.Delay > TimeSpan.Zero);
        }
    }
}
