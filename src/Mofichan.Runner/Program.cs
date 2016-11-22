using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Mofichan.Backend;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Library;
using Serilog;

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
                Console.ReadKey();
            }
        }

        private static Kernel CreateMofichan()
        {
            IContainer container = BuildContainer();

            var botConfiguration = container
                .Resolve<IConfigurationLoader>()
                .LoadConfiguration(DefaultConfigPath);

            var backendName = botConfiguration.SelectedBackend;
            var backendParams = from item in botConfiguration.BackendConfiguration
                                let paramName = item.Key.ToLowerInvariant()
                                let paramValue = item.Value
                                select new NamedParameter(paramName, paramValue);

            var backend = container.ResolveNamed<IMofichanBackend>(backendName, backendParams);

            // TODO: refactor behaviour bootstrapping logic.
            var behaviours = new[]
            {
                container.ResolveNamed<IMofichanBehaviour>("selfignore"),
                container.ResolveNamed<IMofichanBehaviour>("delay"),
                container.ResolveNamed<IMofichanBehaviour>("administration"),
                container.ResolveNamed<IMofichanBehaviour>("diagnostics"),
                container.ResolveNamed<IMofichanBehaviour>("greeting")
            };

            var chainBuilder = container.Resolve<IBehaviourChainBuilder>();
            var rootLogger = container.Resolve<ILogger>();

            var mofichan = new Kernel(backend, behaviours, chainBuilder, rootLogger);

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

            // Register generic parts.
            containerBuilder
                .RegisterType<ConfigurationLoader>()
                .As<IConfigurationLoader>();

            containerBuilder.RegisterInstance(CreateRootLogger());
            containerBuilder
                .RegisterType<BehaviourChainBuilder>()
                .As<IBehaviourChainBuilder>();

            // Register library module.
            containerBuilder.RegisterModule<LibraryModule>();

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
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.LiterateConsole(outputTemplate: template)
                .WriteTo.RollingFile(logPath, outputTemplate: template)
                .CreateLogger();
        }
    }
}
