using System.Collections.Generic;
using System.Linq;
using Autofac;
using Mofichan.DataAccess;
using Mofichan.DataAccess.Domain;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Learning.Feature
{
    [Story(Title = "Learning-related Functionality")]
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle = null) : base(scenarioTitle)
        {
        }

        protected void Then_Mofichan_should_have_responded_acknowledging_she_learnt_the_analysis()
        {
            var responses = this.SentMessages.Select(it => it.Body.ToLowerInvariant());

            responses.ShouldContain(response => response.Contains("sav") && response.Contains("analysis"));
        }

        protected void Then_the_repository_should_contain_an_analysis_item(string expectedMessage,
            IEnumerable<string> expectedTags)
        {
            var repository = this.Container.Resolve<IRepository>();
            var articles = repository.All<AnalysisArticle>().Select(it => it.Article).ToList();

            articles.ShouldContain(it => it.Message.Equals(expectedMessage) &&
                it.Tags.SequenceEqual(expectedTags));
        }
    }
}
