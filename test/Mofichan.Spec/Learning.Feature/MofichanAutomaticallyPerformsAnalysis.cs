using System;
using System.Linq;
using Mofichan.Core;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Learning.Feature
{
    public abstract class MofichanAutomaticallyPerformsAnalysis : BaseScenario
    {
        protected MofichanAutomaticallyPerformsAnalysis(string scenarioTitle) : base(scenarioTitle)
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("learning"))
                .And(s => s.Given_Mofichan_is_running())
            .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, "foo"),
                "When Mofichan receives a message that identifies the developer")
            .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "You're the best, Mofi"),
                "When Mofichan receives a message that she can potentially classify")
            .Then(s => s.Mofichan_should_eventually_try_to_perform_her_own_message_analysis())
            .TearDownWith(s => s.TearDown());
        }

        protected abstract void HandleFlow(MessageContext initialMessage);

        private void Mofichan_should_eventually_try_to_perform_her_own_message_analysis()
        {
            int maxPulseAttempts = 10000;

            for(int i=0; i < maxPulseAttempts; i++)
            {
                this.When_behaviours_are_driven_by__pulseCount__pulses(1);

                if (!this.SentMessages.Any())
                {
                    continue;
                }

                var message = this.SentMessages.Single();
                message.Body.ShouldContain("You're the best, Mofi");
                message.Body.ShouldContain("#directedAtMofichan");
                message.Body.ShouldContain("#positive");

                this.HandleFlow(message);
                return;
            }

            throw new ArgumentException("Mofichan did not automatically try to analyse a message");
        }
    }
}
