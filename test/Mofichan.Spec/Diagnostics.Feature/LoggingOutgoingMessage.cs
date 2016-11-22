using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using TestStack.BDDfy;

namespace Mofichan.Spec.Diagnostics.Feature
{
    public class LoggingOutgoingMessage : BaseScenario
    {
        private readonly MockBehaviour mockBehaviour;

        public LoggingOutgoingMessage() : base("An outgoing message handled by a behaviour is logged")
        {
            this.mockBehaviour = new MockBehaviour();

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("diagnostics"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviour),
                        "Given Mofichan is configured with a mock behaviour")
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_the_mock_behaviour_sends_an_outgoing_message_upstream(new MockUser(), "bar"))
                .Then(s => s.Then_a_log_should_have_been_created_matching_pattern(
                        "behaviour \"diagnostics\" offered outgoing message \"bar\" from \"Joe Somebody\""));
        }

        private void When_the_mock_behaviour_sends_an_outgoing_message_upstream(IUser user, string message)
        {
            var recipient = Mock.Of<IUser>();
            var context = new MessageContext(from: user, to: recipient, body: message);

            this.mockBehaviour.SendMessageUpstream(new OutgoingMessage { Context = context });
        }

        private class MockBehaviour : BaseBehaviour
        {
            public MockBehaviour() : base(() => Mock.Of<IResponseBuilder>())
            {
            }

            public override string Id
            {
                get
                {
                    return "Mocky Behaviour";
                }
            }

            public void SendMessageUpstream(OutgoingMessage message)
            {
                this.SendUpstream(message);
            }

            protected override bool CanHandleIncomingMessage(IncomingMessage message)
            {
                return false;
            }

            protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
            {
                return false;
            }

            protected override void HandleIncomingMessage(IncomingMessage message)
            {
                throw new NotImplementedException();
            }

            protected override void HandleOutgoingMessage(OutgoingMessage message)
            {
                throw new NotImplementedException();
            }
        }
    }
}
