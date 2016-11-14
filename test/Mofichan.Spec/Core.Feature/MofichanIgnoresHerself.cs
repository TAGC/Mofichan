using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanIgnoresHerself : BaseScenario
    {
        private readonly Mock<IMofichanBehaviour> mockBehaviour;

        public MofichanIgnoresHerself() : base("Mofichan sees her own message")
        {
            this.mockBehaviour = new Mock<IMofichanBehaviour>();
            this.mockBehaviour.Setup(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()));

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("selfignore"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviour.Object),
                        "Given Mofichan is configured with a mock object")
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.MofichanUser, "foo"),
                        "When Mofichan sees a message from herself")
                .Then(s => s.Then_the_mock_behaviour_should_not_have_received_any_messages());
        }

        private void Then_the_mock_behaviour_should_not_have_received_any_messages()
        {
            this.mockBehaviour.Verify(it => it.OfferMessage(
                It.IsAny<DataflowMessageHeader>(),
                It.IsAny<IncomingMessage>(),
                It.IsAny<ISourceBlock<IncomingMessage>>(),
                It.IsAny<bool>()), Times.Never());
        }
    }
}
