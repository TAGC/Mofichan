using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Mofichan.DataAccess;
using Mofichan.DataAccess.Domain;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Learning.Feature
{
    public class MofichanLearnsNewAnalysis : BaseScenario
    {
        public MofichanLearnsNewAnalysis() : base("Mofichan is taught a new message analysis")
        {
            var command = default(string);
            var analysisBody = default(string);
            var tags = default(IEnumerable<string>);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("curiosity"))
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.DeveloperUser, command))
                    .And(s => s.When_behaviours_are_driven_by__pulseCount__pulses(2))
                .Then(s => s.Then_Mofichan_should_have_responded_acknowledging_she_learnt_the_analysis())
                    .And(s => s.Then_the_repository_should_contain_an_analysis_item(analysisBody, tags))
                .WithExamples(new ExampleTable("command", "analysisBody", "tags")
                {
                    {
                        "Mofi, learn analysis \"Why hello there Mofi\" #directedAtMofichan #greeting",
                        "Why hello there Mofi",
                        new[] { "directedAtMofichan", "greeting" }
                    },
                    {
                        "Mofi, learn analysis \"Are you doing well today?\" #wellbeing",
                        "Are you doing well today?",
                        new[] { "wellbeing" }
                    },
                    {
                        "Mofi, learn analysis \"You're really pleasant Mofichan\" #directedAtMofichan #positive",
                        "You're really pleasant Mofichan",
                        new[] { "directedAtMofichan", "positive" }
                    },
                })
                .TearDownWith(s => s.TearDown());
        }

        private void Then_the_repository_should_contain_an_analysis_item(string expectedMessage,
            IEnumerable<string> expectedTags)
        {
            var repository = this.Container.Resolve<IRepository>();
            var articles = repository.All<AnalysisArticle>().Select(it => it.Article).ToList();

            articles.ShouldContain(it => it.Message.Equals(expectedMessage) &&
                it.Tags.SequenceEqual(expectedTags));
        }

        private void Then_Mofichan_should_have_responded_acknowledging_she_learnt_the_analysis()
        {
            var response = this.SentMessages.ShouldHaveSingleItem().Body.ToLowerInvariant();
            response.ShouldContain("sav");
            response.ShouldContain("analysis");
        }
    }
}
