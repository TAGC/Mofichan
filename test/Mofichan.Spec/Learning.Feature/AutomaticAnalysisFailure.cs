using Mofichan.Core;

namespace Mofichan.Spec.Learning.Feature
{
    public class AutomaticAnalysisFailure : MofichanAutomaticallyPerformsAnalysis
    {
        public AutomaticAnalysisFailure() : base("Mofichan fails to automatically analyse a message with correct classifications")
        {
        }

        protected override void HandleFlow(MessageContext initialMessage)
        {
            this.SentMessages.Clear();
            this.When_Mofichan_receives_a_message(this.DeveloperUser, "That's wrong Mofi");
            this.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow * 2);
            this.Then_Mofichan_should_have_sent_response_containing__substring__("?");
            
            // Sabotage!
            this.When_Mofichan_receives_a_message(this.DeveloperUser, "Try #directedAtMofichan #negative");
            this.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow * 2);
            this.Then_Mofichan_should_have_responded_acknowledging_she_learnt_the_analysis();
            this.Then_the_repository_should_contain_an_analysis_item(
                "You're the best, Mofi", new[] { "directedAtMofichan", "negative" });
        }
    }
}
