using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.Sleep
{
    public class AdminReawakensMofi : BaseScenario
    {
        public AdminReawakensMofi() : base("An admin reawakens a sleeping Mofi")
        {
            var command = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object),
                        AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, "Go to sleep Mofi"))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                    .And(s => s.When_previously_received_visitors_are_dismissed())
                .Then(s => s.Then_the_mock_behaviour_should_not_have_received_a_visitor())
                    .And(s => s.Then_mofichan_should_have_notified_that_she_is_going_to_sleep())
                .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, command))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                .Then(s => s.Then_the_mock_behaviour_should_have_received_a_visitor())
                    .And(s => s.Then_mofichan_should_have_notified_that_she_is_waking_up())
                .WithExamples(new ExampleTable("command")
                {
                    { "Mofi, wake up again" },
                    { "Mofichan, reawaken" }
                })
                .TearDownWith(s => s.TearDown());
        }
    }
}
