using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.DisplayChain
{
    public class DisplayChainRequestedByJohnSmith : BaseScenario
    {
        public DisplayChainRequestedByJohnSmith() : base("John Smith requests to see the behaviour chain")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                   .And(s => s.Given_Mofichan_is_running())
               .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "Mofichan, show your behaviour chain"))
               .Then(s => s.Then_Mofichan_should_have_sent_response_containing__substring__("not authorised"));
        }
    }
}
