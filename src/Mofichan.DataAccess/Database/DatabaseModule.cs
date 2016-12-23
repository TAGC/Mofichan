using System;
using Autofac;
using Mofichan.Core;
using Mofichan.Core.BotState;
using MongoDB.Driver;
using static Mofichan.Core.Utility.Extensions;

namespace Mofichan.DataAccess.Database
{
    /// <summary>
    /// An Autofac module that registers classes to aid in data storage and retrieval.
    /// </summary>
    /// <seealso cref="Autofac.Module" />
    public class DatabaseModule : Module
    {
        private readonly BotConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseModule"/> class.
        /// </summary>
        /// <param name="configuration">The bot configuration.</param>
        public DatabaseModule(BotConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <exception cref="System.Exception">Invalid database adapter: " + databaseAdapter</exception>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            var databaseAdapter = this.configuration.SelectedDatabaseAdapter.ToLowerInvariant();
            var databaseAdapterConfig = this.configuration.DatabaseAdapterConfiguration;

            switch (databaseAdapter)
            {
                case "in_memory":
                    builder.RegisterType<InMemoryRepository>().As<IRepository>();
                    break;

                case "mongodb":
                    string mongoUser = databaseAdapterConfig.TryGetValueWithDefault("user", string.Empty);
                    string mongoPassword = databaseAdapterConfig.TryGetValueWithDefault("password", string.Empty);
                    string mongoHostname = databaseAdapterConfig.TryGetValueWithDefault("hostname", "localhost");
                    string mongoPort = databaseAdapterConfig.TryGetValueWithDefault("port", "41428");
                    string mongoCredentials = string.Empty;

                    if (!string.IsNullOrWhiteSpace(mongoUser) && !string.IsNullOrWhiteSpace(mongoPassword))
                    {
                        mongoCredentials = mongoUser + ":" + mongoPassword + "@";
                    }

                    var mongoConnectionString = string.Format("mongodb://{0}{1}:{2}/mofichan",
                        mongoCredentials, mongoHostname, mongoPort);

                    builder.RegisterInstance(new MongoClient(mongoConnectionString));

                    builder.RegisterType<MongoDbRepository>()
                        .As<IRepository>()
                        .SingleInstance();
                    break;

                default:
                    // TODO: replace with mofichan configuration exception.
                    throw new Exception("Invalid database adapter: " + databaseAdapter);
            }

            builder.RegisterType<RepositoryBasedMemoryManager>()
                .As<IQueryableMemoryManager>()
                .As<IMemoryManager>();
        }
    }
}
