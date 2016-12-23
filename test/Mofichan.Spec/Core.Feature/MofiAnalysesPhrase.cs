using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofiAnalysesPhrase : BaseScenario
    {
        public MofiAnalysesPhrase() : base("Mofichan analyses a phrase given to her")
        {
            var command = default(string);
            var tags = default(IEnumerable<string>);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("analysis"))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, command))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(ResponseWindow))
                .Then(s => s.Then_Mofichan_should_have_responded_with_expected_analysis(tags))
                .WithExamples(new ExampleTable("command", "tags")
                {
                    { "Mofi, perform analysis \"hello Mofi\"", new[] { "directedAtMofichan", "greeting" } },
                    { "Mofi, perform analysis: \"how are you doing?\"", new[] { "wellbeing" } },
                    { "Mofi, analyse: \"you're the best!\"", new[] { "positive" } },
                    { "Mofi, analyse phrase: \"you're the worst!\"", new[] { "negative" } },
                    { "Mofi, analyse phrase: \"That's right Mofi\"", new[] { "directedAtMofichan", "affirmation" } },
                    { "Mofi, analyse phrase: \"That's correct Mofi\"", new[] { "directedAtMofichan", "affirmation" } },
                    { "Mofi, analyse phrase: \"That's wrong Mofi\"", new[] { "directedAtMofichan", "refutation" } },
                    { "Mofi, analyse phrase: \"That's incorrect Mofichan\"", new[] { "directedAtMofichan", "refutation" } },
                })
                .TearDownWith(s => s.TearDown());
        }

        private void Then_Mofichan_should_have_responded_with_expected_analysis(IEnumerable<string> expectedTags)
        {
            var response = this.SentMessages.ShouldHaveSingleItem().Body;
            var actualTags = Regex.Matches(response, @"(?<=#)\w+").OfType<Match>().Select(it => it.Value);

            actualTags.ShouldBe(expectedTags, ignoreOrder: true);
        }
    }
}
