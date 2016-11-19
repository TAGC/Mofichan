using System;
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
        private static readonly string DefaultBotName = "Mofichan";
        private static readonly string DefaultDeveloperName = "ThymineC";

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
            var backendProperties = configuration
                .GetSection(backendName)
                .GetChildren()
                .Where(it => it.Key != null)
                .Where(it => it.Key != "name")
                .ToDictionary(it => it.Key, it => it.Value);

            // TODO: improve this.
            var configBuilder = new BotConfiguration.Builder();
            configBuilder.SetBotName(DefaultBotName);
            configBuilder.SetDeveloperName(DefaultDeveloperName);
            configBuilder.SetSelectedBackend(backendName.ToLowerInvariant());

            foreach (var property in backendProperties)
            {
                configBuilder.WithBackendSetting(property.Key, property.Value);
            }

            return configBuilder.Build();
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
