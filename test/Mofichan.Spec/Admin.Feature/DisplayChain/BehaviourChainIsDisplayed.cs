using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.DisplayChain
{
    public class BehaviourChainIsDisplayed : BaseScenario
    {
        private const string AddMockBehaviourTemplate = "Given Mofichan is configured with a mock behaviour";

        public BehaviourChainIsDisplayed() : base("Admin requests to see Mofichan's behaviour chain")
        {
            var mockA = ConstructMockBehaviourWithId("mockA");
            var mockB = ConstructMockBehaviourWithId("mockB");
            var mockC = ConstructMockBehaviourWithId("mockC");

            string substring = "[administration] ⇄ [mockA ✓] ⇄ [mockB ✓] ⇄ [mockC ✓]";

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockA), AddMockBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockB), AddMockBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockC), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, "Mofichan, show your behaviour chain"))
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(2))
                .Then(s => s.Then_Mofichan_should_have_sent_response_containing__substring__(substring));
        }
    }
}
