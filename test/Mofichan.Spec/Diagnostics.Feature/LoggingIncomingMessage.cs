using Mofichan.Core.Interfaces;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Diagnostics.Feature
{
    public class LoggingIncomingMessage : BaseScenario
    {
        public LoggingIncomingMessage() : base("An incoming message handled by a behaviour is logged")
        {
            var mockBehaviour = new Mock<IMofichanBehaviour>();
            mockBehaviour.SetupGet(it => it.Id).Returns("mock");

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("diagnostics"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockBehaviour.Object),
                        "Given Mofichan is configured with a mock behaviour")
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(new MockUser(), "foo"))
                .Then(s => s.Then_a_log_should_have_been_created_matching_pattern(
                        "behaviour \"mock\" offered incoming message \"foo\" from \"Joe Somebody\""));
        }
    }
}
