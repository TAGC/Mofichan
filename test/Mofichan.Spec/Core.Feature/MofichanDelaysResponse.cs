using System;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanDelaysResponse : BaseScenario
    {
        private ITargetBlock<OutgoingMessage> target;

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
                .Setup(it => it.LinkTo(
                    It.IsAny<ITargetBlock<OutgoingMessage>>(),
                    It.IsAny<DataflowLinkOptions>()))
                .Callback<ITargetBlock<OutgoingMessage>, DataflowLinkOptions>(
                    (linkedTarget, _) => target = linkedTarget);

            mockBehaviour
                .Setup(it => it.OfferMessage(
                    It.IsAny<DataflowMessageHeader>(),
                    It.IsAny<IncomingMessage>(),
                    It.IsAny<ISourceBlock<IncomingMessage>>(),
                    It.IsAny<bool>()))
                .Callback(SendMockResponse);

            return mockBehaviour;
        }

        private void SendMockResponse()
        {
            var mockResponseContext = new MessageContext(
                from: this.MofichanUser, to: this.JohnSmithUser, body: "bar");

            OutgoingMessage mockResponse = new OutgoingMessage { Context = mockResponseContext };

            // We confirm that there's no delay set on the generated responses by default.
            Debug.Assert(mockResponse.Context.Delay == TimeSpan.Zero);

            target.OfferMessage(default(DataflowMessageHeader), mockResponse, null, false);
        }

        private void Then_Mofichan_should_produce_a_delayed_response()
        {
            this.SentMessages.ShouldContain(response => response.Context.Delay > TimeSpan.Zero);
        }
    }
}
