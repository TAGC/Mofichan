using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.ToggleBehaviour
{
    public class BehaviourIsDisabledByJohnSmith : BaseScenario
    {
        public BehaviourIsDisabledByJohnSmith() : base("John Smith requests that a behaviour is disabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                    .And(s => s.When_I_request_that__behaviour__behaviour_is_enabled("mock"),
                        "Given that I have already requested that the mock behaviour is enabled")
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(1))
                .When(s => s.When_John_Smith_requests_that__behaviour__behaviour_is_disabled("mock"),
                        "When John Smith tries to disable the mock behaviour")
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(2))
                .Then(s => s.Then_the_mock_behaviour_should_have_received__message__("foo"))
                    .And(s => s.Then_Mofichan_should_have_sent_response_containing__substring__("not authorised"))
                .TearDownWith(s => s.TearDown());
        }
    }
}
