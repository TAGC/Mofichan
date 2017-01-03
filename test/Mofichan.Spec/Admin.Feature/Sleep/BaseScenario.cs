using System.Text.RegularExpressions;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Moq;
using Shouldly;

namespace Mofichan.Spec.Admin.Feature.Sleep
{
    public abstract class BaseScenario : Scenario
    {
        protected const string AddMockBehaviourTemplate = "Given Mofichan is configured with a mock behaviour";

        private readonly Mock<IMofichanBehaviour> mockBehaviour;

        protected BaseScenario(string scenarioTitle) : base(scenarioTitle)
        {
            this.mockBehaviour = new Mock<IMofichanBehaviour>();
            this.mockBehaviour.SetupGet(it => it.Id).Returns("mock");
            this.mockBehaviour
                .Setup(it => it.OnNext(It.IsAny<IBehaviourVisitor>()))
                .Callback(() => this.VisitorReceived = true);
            this.mockBehaviour.Setup(it => it.ToString()).Returns("mock");
        }

        protected Mock<IMofichanBehaviour> MockBehaviour
        {
            get
            {
                return this.mockBehaviour;
            }
        }

        protected bool VisitorReceived { get; private set; }

        #region When
        protected void When_previously_received_visitors_are_dismissed()
        {
            this.VisitorReceived = false;
        }
        #endregion

        #region Then
        protected void Then_mofichan_should_have_notified_that_she_is_going_to_sleep()
        {
            this.Then_Mofichan_should_have_sent_response_with_pattern("sleep", RegexOptions.IgnoreCase);
        }

        protected void Then_mofichan_should_have_notified_that_she_is_waking_up()
        {
            this.Then_Mofichan_should_have_sent_response_with_pattern("wak(e|ing)", RegexOptions.IgnoreCase);
        }

        protected void Then_the_mock_behaviour_should_have_received_a_visitor()
        {
            this.VisitorReceived.ShouldBeTrue();
        }

        protected void Then_the_mock_behaviour_should_not_have_received_a_visitor()
        {
            this.VisitorReceived.ShouldBeFalse();
        }
        #endregion
    }
}
