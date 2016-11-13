using System.Text.RegularExpressions;
using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.ToggleBehaviour
{
    public class AdminBehaviourIsEnabled : BaseScenario
    {
        public AdminBehaviourIsEnabled() : base("Admin requests that admin behaviour is enabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .And(s => s.Given_Mofichan_is_running())
            .When(s => s.When_I_request_that__behaviour__behaviour_is_enabled("administration"))
            .Then(s => s.Then_Mofichan_should_have_sent_response_with_pattern(
                    "behaviour 'administration'.*can't be enabled", RegexOptions.IgnoreCase))
            .TearDownWith(s => s.TearDown());
        }
    }

    public class AdminBehaviourIsDisabled : BaseScenario
    {
        public AdminBehaviourIsDisabled() : base("Admin requests that admin behaviour is disabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .And(s => s.Given_Mofichan_is_running())
            .When(s => s.When_I_request_that__behaviour__behaviour_is_disabled("administration"))
            .Then(s => s.Then_Mofichan_should_have_sent_response_with_pattern(
                    "behaviour 'administration'.*can't be disabled", RegexOptions.IgnoreCase))
            .TearDownWith(s => s.TearDown());
        }
    }
}
