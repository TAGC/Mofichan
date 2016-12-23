using System.Text.RegularExpressions;
using Mofichan.Core;

namespace Mofichan.Spec.Learning.Feature
{
    public class AutomaticAnalysisAbortEarly : MofichanAutomaticallyPerformsAnalysis
    {
        public AutomaticAnalysisAbortEarly() : base("Mofichan tries to automatically analyse a message and admin aborts")
        {
        }

        protected override void HandleFlow(MessageContext initialMessage)
        {
            this.SentMessages.Clear();
            this.When_Mofichan_receives_a_message(this.DeveloperUser, "Skip that one Mofi");
            this.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow * 2);
            this.Then_Mofichan_should_have_sent_response_with_pattern("skip", RegexOptions.IgnoreCase);
        }
    }

    public class AutomaticAnalysisAbortAfterCorrection : MofichanAutomaticallyPerformsAnalysis
    {
        public AutomaticAnalysisAbortAfterCorrection() : base(
            "Mofichan tries to automatically analyse a message, admin attempts correction and then aborts")
        {
        }

        protected override void HandleFlow(MessageContext initialMessage)
        {
            this.SentMessages.Clear();

            this.When_Mofichan_receives_a_message(this.DeveloperUser, "That's wrong Mofi");
            this.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow * 2);
            this.Then_Mofichan_should_have_sent_response_containing__substring__("?");

            this.When_Mofichan_receives_a_message(this.DeveloperUser, "Skip that one Mofi");
            this.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow * 2);
            this.Then_Mofichan_should_have_sent_response_with_pattern("skip", RegexOptions.IgnoreCase);
        }
    }
}
