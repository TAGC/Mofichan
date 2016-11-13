using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature.ToggleBehaviour
{
    [Story(Title = "Administration Functionality - Toggling Behaviour Enable States")]
    public abstract class BaseScenario : Scenario
    {
        protected const string AddMockBehaviourTemplate = "Given Mofichan is configured with a mock behaviour";

        private readonly Mock<IMofichanBehaviour> mockBehaviour;

        protected BaseScenario(string scenarioTitle) : base(scenarioTitle)
        {
            this.mockBehaviour = new Mock<IMofichanBehaviour>();
            this.mockBehaviour.SetupGet(it => it.Id).Returns("mock");
            this.mockBehaviour.Setup(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()));
        }

        protected Mock<IMofichanBehaviour> MockBehaviour
        {
            get
            {
                return this.mockBehaviour;
            }
        }

        protected void TearDown()
        {
            this.Mofichan.Dispose();
        }

        #region When
        protected void When_I_request_that__behaviour__behaviour_is_enabled(string behaviour)
        {
            this.When_Mofichan_receives_a_message(this.DeveloperUser,
                string.Format("Mofichan, enable {0} behaviour", behaviour));
        }

        protected void When_I_request_that__behaviour__behaviour_is_disabled(string behaviour)
        {
            this.When_Mofichan_receives_a_message(this.DeveloperUser,
                string.Format("Mofichan, disable {0} behaviour", behaviour));
        }

        protected void When_John_Smith_requests_that__behaviour__behaviour_is_enabled(string behaviour)
        {
            this.When_Mofichan_receives_a_message(this.JohnSmithUser,
                string.Format("Mofichan, disable {0} behaviour", behaviour));
        }

        protected void When_John_Smith_requests_that__behaviour__behaviour_is_disabled(string behaviour)
        {
            this.When_Mofichan_receives_a_message(this.JohnSmithUser,
                string.Format("Mofichan, disable {0} behaviour", behaviour));
        }
        #endregion

        #region Then
        protected void Then_the_mock_behaviour_should_have_received__message__(string message)
        {
            this.mockBehaviour.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.Is<IncomingMessage>(msg => msg.Context.Body == message),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Once());
        }

        protected void Then_the_mock_behaviour_should_not_have_received_any_messages()
        {
            this.mockBehaviour.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Never());
        }
        #endregion
    }
}
