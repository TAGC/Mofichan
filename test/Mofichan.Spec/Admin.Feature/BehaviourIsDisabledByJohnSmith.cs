using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature
{
    public class BehaviourIsDisabledByJohnSmith : BaseScenario
    {
        public BehaviourIsDisabledByJohnSmith() : base("John Smith requests that a behaviour is disabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                    .And(s => s.When_I_request_that_the_mock_behaviour_is_enabled(),
                        "Given that I have already requested that the mock behaviour is enabled")
                .When(s => s.When_John_Smith_requests_that_the_mock_behaviour_is_disabled(),
                        "When John Smith tries to disable the mock behaviour")
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                .Then(s => s.Then_the_mock_behaviour_should_have_received__message__("foo"),
                    "Then the mock behaviour should have received the message regardless")
                .TearDownWith(s => s.TearDown());
        }
    }
}
