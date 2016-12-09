using System.Text.RegularExpressions;
using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.ToggleBehaviour
{
    public class NonExistentBehaviourIsEnabled : BaseScenario
    {
        public NonExistentBehaviourIsEnabled() : base("Admin requests that a non-existent behaviour is enabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_request_that__behaviour__behaviour_is_enabled("NON_EXISTENT_BEHAVIOUR"))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(2))
                .Then(s => s.Then_Mofichan_should_have_sent_response_with_pattern(
                        "behaviour 'NON_EXISTENT_BEHAVIOUR' doesn't exist", RegexOptions.IgnoreCase))
                .TearDownWith(s => s.TearDown());
        }
    }

    public class NonExistentBehaviourIsDisabled : BaseScenario
    {
        public NonExistentBehaviourIsDisabled() : base("Admin requests that a non-existent behaviour is disabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_request_that__behaviour__behaviour_is_enabled("NON_EXISTENT_BEHAVIOUR"))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(2))
                .Then(s => s.Then_Mofichan_should_have_sent_response_with_pattern(
                        "behaviour 'NON_EXISTENT_BEHAVIOUR' doesn't exist", RegexOptions.IgnoreCase))
                .TearDownWith(s => s.TearDown());
        }
    }
}
