using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature
{
    public class BehaviourIsEnabledByJohnSmith : BaseScenario
    {
        public BehaviourIsEnabledByJohnSmith() : base("John Smith requests that a behaviour is enabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                    .And(s => s.When_I_request_that__behaviour__behaviour_is_disabled("mock"),
                        "Given that I have already requested that the mock behaviour is disabled")
                .When(s => s.When_John_Smith_requests_that__behaviour__behaviour_is_enabled("mock"),
                        "When John Smith tries to enable the mock behaviour")
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                .Then(s => s.Then_the_mock_behaviour_should_not_have_received_any_messages(),
                    "Then the mock behaviour should not have received the message regardless")
                .TearDownWith(s => s.TearDown());
        }
    }
}
