using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    [Story(Title = "Core Functionality")]
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle) : base(scenarioTitle)
        {
        }
    }
}
