using Mofichan.Core.Interfaces;
using Moq;

namespace Mofichan.Spec.Admin.Feature.DisplayChain
{
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle) : base(scenarioTitle)
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
}
