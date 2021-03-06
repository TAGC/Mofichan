﻿using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
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
            this.mockBehaviour.Setup(it => it.OnNext(It.IsAny<IBehaviourVisitor>()));
            this.mockBehaviour.Setup(it => it.ToString()).Returns("mock");
        }

        protected Mock<IMofichanBehaviour> MockBehaviour
        {
            get
            {
                return this.mockBehaviour;
            }
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
                string.Format("Mofichan, enable {0} behaviour", behaviour));
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
            this.mockBehaviour.Verify(it => it.OnNext(
                It.Is<IBehaviourVisitor>(visitor => visitor is OnMessageVisitor
                    && ((OnMessageVisitor)visitor).Message.Body == message)),
                Times.Once);
        }

        protected void Then_the_mock_behaviour_should_not_have_received__message__(string message)
        {
            this.mockBehaviour.Verify(it => it.OnNext(
                It.Is<IBehaviourVisitor>(visitor => visitor is OnMessageVisitor
                    && ((OnMessageVisitor)visitor).Message.Body == message)),
                Times.Never);
        }
        #endregion
    }
}
