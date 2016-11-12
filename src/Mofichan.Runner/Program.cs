using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Mofichan.Backend;
using Mofichan.Behaviour;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Runner
{
    public class Program
    {
        private const string MofichanName = "Mofichan";
        private static readonly string DefaultConfigPath = "mofichan.config";

        public static void Main(string[] args)
        {
            using (var mofichan = CreateMofichan())
            {
                mofichan.Start();
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
                                let paramName = item.Key
                                let paramValue = item.Value
                                select new NamedParameter(paramName, paramValue);

            var backend = container.ResolveNamed<IMofichanBackend>(backendName, backendParams);

            // TODO: refactor behaviour bootstrapping logic.
            var behaviours = new[]
            {
                container.ResolveNamed<IMofichanBehaviour>("greeting")
            };

            var mofichan = new Kernel(MofichanName, backend, behaviours);

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
                .RegisterType<YamlConfigurationLoader>()
                .As<IConfigurationLoader>();

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
    }
}
