using System.IO;
using Mofichan.Core;
using YamlDotNet.RepresentationModel;

namespace Mofichan.Runner
{
    /// <summary>
    /// Loads Mofichan's configuration from a YAML configuration file.
    /// </summary>
    public class YamlConfigurationLoader : IConfigurationLoader
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
            YamlStream yaml = GetYamlStream(configPath);

            var configBuilder = new BotConfiguration.Builder();
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            var backendNode = rootNode.Children[new YamlScalarNode("backend")];

            // TODO: Inspect YAML tree for possible overrides of these values.
            configBuilder.SetBotName(DefaultBotName);
            configBuilder.SetDeveloperName(DefaultDeveloperName);

            if (backendNode is YamlMappingNode)
            {
                foreach (var childNode in ((YamlMappingNode)backendNode).Children)
                {
                    string configKey = childNode.Key.ToString();
                    string configValue = childNode.Value.ToString();

                    if (configKey == "name")
                    {
                        configBuilder.SetSelectedBackend(configValue.ToLowerInvariant());
                    }
                    else
                    {
                        configBuilder.WithBackendSetting(configKey, configValue);
                    }
                }
            }

            return configBuilder.Build();
        }

        private static YamlStream GetYamlStream(string configPath)
        {
            var input = new StringReader(File.ReadAllText(configPath));

            var yaml = new YamlStream();
            yaml.Load(input);
            return yaml;
        }
    }
}