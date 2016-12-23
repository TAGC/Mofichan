using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Mofichan.Core;

namespace Mofichan.Runner
{
    /// <summary>
    /// The default implementation of <see cref="IConfigurationLoader"/>
    /// to use for loading Mofichan's settings.
    /// </summary>
    public class ConfigurationLoader : IConfigurationLoader
    {
        /// <summary>
        /// Loads Mofichan's configuration from a configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file.</param>
        /// <returns>
        /// The configuration specified in the file.
        /// </returns>
        public BotConfiguration LoadConfiguration(string configPath)
        {
            IConfigurationRoot configuration = BuildConfigurationRoot(configPath);

            var backendName = this.GetRequiredConfigValue("core:backend", configuration);
            var backendProperties = GetSectionProperties(configuration, backendName);

            var databaseAdapterName = this.GetRequiredConfigValue("core:database_adapter", configuration);
            var databaseAdapterProperties = GetSectionProperties(configuration, databaseAdapterName);

            var configBuilder = new BotConfiguration.Builder();
            configBuilder.SetSelectedBackend(backendName.ToLowerInvariant());
            configBuilder.SetSelectedDatabaseAdapter(databaseAdapterName.ToLowerInvariant());

            foreach (var property in backendProperties)
            {
                configBuilder.WithBackendSetting(property.Key, property.Value);
            }

            foreach (var property in databaseAdapterProperties)
            {
                configBuilder.WithDatabaseAdapterSetting(property.Key, property.Value);
            }

            return configBuilder.Build();
        }

        private static IDictionary<string, string> GetSectionProperties(IConfigurationRoot configuration, string sectionName)
        {
            return configuration
                .GetSection(sectionName)
                .GetChildren()
                .Where(it => it.Key != null)
                .Where(it => it.Key != "name")
                .ToDictionary(it => it.Key.ToLowerInvariant(), it => it.Value);
        }

        private static IConfigurationRoot BuildConfigurationRoot(string configPath)
        {
            var basePath = AppContext.BaseDirectory;

            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(configPath + ".json", true)
                .AddEnvironmentVariables()
                .Build();
        }

        private string GetRequiredConfigValue(string key, IConfiguration config)
        {
            var value = config[key];

            if (value == null)
            {
                throw new ArgumentException("Configuration value missing for " + key);
            }

            return value;
        }
    }
}
