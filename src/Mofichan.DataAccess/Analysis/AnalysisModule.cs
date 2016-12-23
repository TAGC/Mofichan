using Autofac;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.DataAccess.Analysis
{
    /// <summary>
    /// An Autofac module that registers classes to aid in message analysis.
    /// </summary>
    /// <seealso cref="Autofac.Module" />
    public class AnalysisModule : Autofac.Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CompositeBayesianClassifier.Factory>()
                .WithParameter("requiredConfidenceRatio", 0.5)
                .AsSelf()
                .SingleInstance();

            builder.Register(context =>
            {
                var factory = context.Resolve<CompositeBayesianClassifier.Factory>();
                var trainingSet = context.Resolve<IQueryableMemoryManager>().LoadAnalyses();
                return factory.From(trainingSet);
            }).Named<IMessageClassifier>("classifier");

            builder.RegisterDecorator<IMessageClassifier>(
                (c, inner) => new SentenceFragmentAnalyser(inner, c.Resolve<ILogger>()),
                fromKey: "classifier");
        }
    }
}
