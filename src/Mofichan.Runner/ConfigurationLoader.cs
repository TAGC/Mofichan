using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Mofichan.Core;

namespace Mofichan.Runner
{
    public class ConfigurationLoader : IConfigurationLoader
    {
        private static readonly string DefaultBotName = "Mofichan";
        private static readonly string DefaultDeveloperName = "ThymineC";

        public BotConfiguration LoadConfiguration(string configPath)
        {
            IConfigurationRoot configuration = BuildConfigurationRoot(configPath);

            var backendName = GetRequiredConfigValue("core:backend", configuration);
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
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
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
