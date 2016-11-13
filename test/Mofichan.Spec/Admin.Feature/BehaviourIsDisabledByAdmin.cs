﻿using TestStack.BDDfy;

namespace Mofichan.Spec.Admin.Feature
{
    public class BehaviourIsDisabledByAdmin : BaseScenario
    {
        public BehaviourIsDisabledByAdmin() : base("Admin requests that a behaviour is enabled")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("administration"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.MockBehaviour.Object), AddMockBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_request_that_the_mock_behaviour_is_disabled())
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "foo"))
                .Then(s => s.Then_the_mock_behaviour_should_not_have_received_any_messages())
                .TearDownWith(s => s.TearDown());
        }
    }
}
