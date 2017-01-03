using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.Sleep
{
    public class JohnSmithPutsMofiToSleep : BaseScenario
    {
        public JohnSmithPutsMofiToSleep() : base("John Smith tries to put Mofi to sleep")
        {
            var command = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object),
                        AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, command))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                .Then(s => s.Then_the_mock_behaviour_should_have_received_a_visitor())
                    .And(s => s.Then_Mofichan_should_have_sent_response_containing__substring__("not authorised"))
                .WithExamples(new ExampleTable("command")
                {
                    { "Mofi, go to sleep" },
                    { "Mofichan, may you rest in a deep and dreamless slumber" }
                })
                .TearDownWith(s => s.TearDown());
        }
    }
}
