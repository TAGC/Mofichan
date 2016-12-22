using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanIgnoresHerself : BaseScenario
    {
        private readonly Mock<IMofichanBehaviour> mockBehaviour;

        public MofichanIgnoresHerself() : base("Mofichan sees her own message")
        {
            this.mockBehaviour = new Mock<IMofichanBehaviour>();
            this.mockBehaviour.Setup(it => it.OnNext(It.IsAny<IBehaviourVisitor>()));

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviour.Object),
                        "Given Mofichan is configured with a mock object")
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.MofichanUser, "foo"),
                        "When Mofichan sees a message from herself")
                .Then(s => s.Then_the_mock_behaviour_should_not_have_received_any_messages());
        }

        private void Then_the_mock_behaviour_should_not_have_received_any_messages()
        {
            this.mockBehaviour.Verify(it => it.OnNext(It.IsAny<IBehaviourVisitor>()),
                Times.Never);
        }
    }
}
