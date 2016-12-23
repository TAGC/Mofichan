using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Mofichan.Backend;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.BotState;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Serilog;
using Serilog.Events;

namespace Mofichan.Runner
{
    /// <summary>
    /// Launches Mofichan as a console application.
    /// </summary>
    public static class Program
    {
        private static readonly string DefaultConfigPath = "mofichan.config";

        /// <summary>
        /// The program entry-point.
        /// </summary>
        /// <param name="args">The program arguments.</param>
        public static void Main(string[] args)
        {
            using (var mofichan = CreateMofichan())
            {
                mofichan.Start();

                Console.WriteLine("Started Mofichan");
                Console.WriteLine("Press any key to shut down...");
                Console.Read();
            }
        }

        private static Kernel CreateMofichan()
        {
            IContainer container = BuildContainer();

            var botConfiguration = container.Resolve<BotConfiguration>();
            var backendName = botConfiguration.SelectedBackend;
            var backendParams = from item in botConfiguration.BackendConfiguration
                                let paramName = item.Key
                                let paramValue = item.Value
                                select new NamedParameter(paramName, paramValue);

            var backend = container.ResolveNamed<IMofichanBackend>(backendName, backendParams);

            // TODO: refactor behaviour bootstrapping logic.
            var behaviours = new[]
            {
                container.ResolveNamed<IMofichanBehaviour>("delay"),
                container.ResolveNamed<IMofichanBehaviour>("administration"),
                container.ResolveNamed<IMofichanBehaviour>("diagnostics"),
                container.ResolveNamed<IMofichanBehaviour>("attention"),
                container.ResolveNamed<IMofichanBehaviour>("greeting")
            };

            var chainBuilder = container.Resolve<IBehaviourChainBuilder>();
            var pulseDriver = container.Resolve<IPulseDriver>();
            var messageClassifier = container.Resolve<IMessageClassifier>();
            var visitorFactory = container.Resolve<IBehaviourVisitorFactory>();
            var responseSelector = container.Resolve<IResponseSelector>();
            var rootLogger = container.Resolve<ILogger>();

            var mofichan = new Kernel(backend, behaviours, chainBuilder, pulseDriver, messageClassifier,
                visitorFactory, responseSelector, rootLogger);

            return mofichan;
        }

        private static IContainer BuildContainer()
        {
            Func<Type, string> getBehaviourName = it =>
                it.Name.ToLowerInvariant().Replace("behaviour", string.Empty);

            Func<Type, string> getBackendName = it =>
                it.Name.ToLowerInvariant().Replace("backend", string.Empty);

            var behaviourAssembly = typeof(BaseBehaviour).GetTypeInfo().Assembly;
            var backendAssembly = typeof(BaseBackend).GetTypeInfo().Assembly;
            var containerBuilder = new ContainerBuilder();

            // Register configuration.
            var botConfig = new ConfigurationLoader().LoadConfiguration(DefaultConfigPath);
            containerBuilder.Register(_ => botConfig);

            // Register generic parts.
            containerBuilder
                .RegisterInstance(CreateRootLogger());

            containerBuilder
                .RegisterType<BehaviourChainBuilder>()
                .As<IBehaviourChainBuilder>();

            containerBuilder
                .RegisterType<FlowTransitionManager>()
                .As<IFlowTransitionManager>();

            containerBuilder
                .RegisterType<PulseDrivenAttentionManager>()
                .As<IAttentionManager>()
                .WithParameter("mu", 100)
                .WithParameter("sigma", 10)
                .SingleInstance();

            containerBuilder
                .Register(c => new Heart(c.Resolve<ILogger>()) { Rate = TimeSpan.FromMilliseconds(100) })
                .As<IPulseDriver>()
                .SingleInstance();

            containerBuilder
                .RegisterType<FlowManager>()
                .As<IFlowManager>();

            containerBuilder
                .RegisterType<BotContext>();

            containerBuilder
                .RegisterType<BehaviourVisitorFactory>()
                .As<IBehaviourVisitorFactory>();

            containerBuilder
                .RegisterType<VectorSimilarityEvaluator>()
                .As<IRelevanceArgumentEvaluator>();

            containerBuilder
                .RegisterType<PulseDrivenResponseSelector>()
                .WithParameter("responseWindow", 2)
                .As<IResponseSelector>();

            // Register data access modules.
            containerBuilder
                .RegisterModule<DataAccess.Analysis.AnalysisModule>()
                .RegisterModule<DataAccess.Response.ResponseModule>()
                .RegisterModule(new DataAccess.Database.DatabaseModule(botConfig));

            // Register behaviour plugins.
            containerBuilder
                .RegisterAssemblyTypes(behaviourAssembly)
                .AssignableTo(typeof(IMofichanBehaviour))
                .Named<IMofichanBehaviour>(getBehaviourName)
                .AsImplementedInterfaces();

            // Register backend plugins.
            containerBuilder
                .RegisterAssemblyTypes(backendAssembly)
                .AssignableTo(typeof(IMofichanBackend))
                .Named<IMofichanBackend>(getBackendName)
                .AsImplementedInterfaces();

            return containerBuilder.Build();
        }

        private static ILogger CreateRootLogger()
        {
            var template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{ThreadId}] {SourceContext}   {Message}{NewLine}{Exception}";
            var logPath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "logs", "app-{Date}.log");

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithThreadId()
                .WriteTo.LiterateConsole(restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: template)
                .WriteTo.RollingFile(logPath, restrictedToMinimumLevel: LogEventLevel.Debug, outputTemplate: template)
                .WriteTo.Elasticsearch("http://elk:9200")
                .CreateLogger();
        }
    }
}
