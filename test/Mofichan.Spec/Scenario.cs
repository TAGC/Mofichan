using TestStack.BDDfy;
using Xunit;

namespace Mofichan.Spec
{
    public abstract class Scenario
    {
        private readonly string scenarioTitle;

        public Scenario(string scenarioTitle = null)
        {
            this.scenarioTitle = scenarioTitle;
        }

        [Fact]
        public void Execute()
        {
            this.BDDfy(scenarioTitle: this.scenarioTitle);
        }
    }
}
