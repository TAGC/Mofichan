using System.Linq;
using Autofac;
using Mofichan.DataAccess;
using Mofichan.DataAccess.Database;
using Mofichan.DataAccess.Domain;
using Serilog;
using TestStack.BDDfy;

namespace Mofichan.Spec.Learning.Feature
{
    [Story(Title = "Learning-related Functionality")]
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle = null) : base(scenarioTitle)
        {
        }

        protected override ContainerBuilder CreateContainerBuilder()
        {
            // Transfer all knowledge from production repository to test in-memory one.
            var builder = base.CreateContainerBuilder();

            builder.RegisterType<MongoDbRepository>()
                .Named<IRepository>("Real");

            builder.Register(c =>
            {
                var testRepo = new InMemoryRepository(c.Resolve<ILogger>());
                var productionRepo = c.ResolveNamed<IRepository>("Real");
                productionRepo.All<AnalysisArticle>().ToList().ForEach(it => testRepo.Add(it));

                return testRepo;
            }).As<IRepository>().SingleInstance();

            return builder;
        }
    }
}
