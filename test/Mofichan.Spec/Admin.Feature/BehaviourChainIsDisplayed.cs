using Mofichan.Core.Interfaces;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature
{
    public abstract class BehaviourChainIsDisplayedBase : Scenario
    {
        protected BehaviourChainIsDisplayedBase(string scenarioTitle) : base(scenarioTitle)
        {
        }

        protected static IMofichanBehaviour ConstructMockBehaviourWithId(string id)
        {
            var mock = new Mock<IMofichanBehaviour>();
            mock.SetupGet(it => it.Id).Returns(id);
            mock.Setup(it => it.ToString()).Returns("[" + id + "]");

            return mock.Object;
        }
    }

    public class BehaviourChainIsDisplayed : BehaviourChainIsDisplayedBase
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
                .Then(s => s.Then_Mofichan_should_have_sent_response_containing__substring__(substring));
        }
    }

    public class BehaviourChainIsDisplayedWithDisabledBehaviour : BehaviourChainIsDisplayedBase
    {
        private const string AddMockBehaviourTemplate = "Given Mofichan is configured with a mock behaviour";

        public BehaviourChainIsDisplayedWithDisabledBehaviour() : base("Admin requests to see Mofichan's behaviour chain after disabling a behaviour")
        {
            var mockA = ConstructMockBehaviourWithId("mockA");
            var mockB = ConstructMockBehaviourWithId("mockB");
            var mockC = ConstructMockBehaviourWithId("mockC");

            string substring = "[administration] ⇄ [mockA ✓] ⇄ [mockB ⨉] ⇄ [mockC ✓]";

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockA), AddMockBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockB), AddMockBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockC), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                    .And(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, "Mofichan, disable mockB behaviour"),
                        "Given that I've requested to disable the 'mockB' behaviour")
                .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, "Mofichan, show your behaviour chain"))
                .Then(s => s.Then_Mofichan_should_have_sent_response_containing__substring__(substring));
        }
    }
}
