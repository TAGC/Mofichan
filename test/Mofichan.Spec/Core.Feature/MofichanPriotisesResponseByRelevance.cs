using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanPriotisesResponseByRelevance : BaseScenario
    {
        public MofichanPriotisesResponseByRelevance()
            : base("Mofichan prioritises her response to a message based on its relevance")
        {
            var mockBehaviour = this.ConstructMockBehaviour();

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockBehaviour.Object))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message_with_tags(this.JohnSmithUser, "Hi Mofi, how are you?",
                        "directedAtMofichan", "greeting", "wellbeing"))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                .Then(s => s.Then_Mofichan_should_have_sent__body__("I'm okay thanks, how are you?"));
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

                    SendMockResponses(visitor, respondingTo);
                });

            return mockBehaviour;
        }

        private static void SendMockResponses(IBehaviourVisitor visitor, MessageContext respondingTo)
        {
            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("Hello there!"))
                .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan", "greeting")));

            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb.FromRaw("I'm okay thanks, how are you?"))
                .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan", "greeting", "wellbeing")));
        }
    }
}
