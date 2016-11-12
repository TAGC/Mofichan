using System.IO;
using System.Linq;
using Autofac;
using Mofichan.Core;
using YamlDotNet.RepresentationModel;

namespace Mofichan.Runner
{
    public class YamlConfigurationLoader : IConfigurationLoader
    {
        private static readonly string DefaultBotName = "Mofichan";
        private static readonly string DefaultDeveloperName = "ThymineC";

        public BotConfiguration LoadConfiguration(string configPath)
        {
            YamlStream yaml = GetYamlStream(configPath);

            var configBuilder = new BotConfiguration.Builder();
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            var backendNode = rootNode.Children[new YamlScalarNode("backend")];

            // TODO: Inspect YAML tree for possible overrides of these values.
            configBuilder.SetBotName(DefaultBotName);
            configBuilder.SetDeveloperName(DefaultDeveloperName);
            configBuilder.SetSelectedBackend(backendNode.ToString());

            if (backendNode is YamlMappingNode)
            {
                foreach (var childNode in (YamlMappingNode)backendNode)
                {
                    string configKey = childNode.Key.ToString();
                    string configValue = childNode.Value.ToString();
                    configBuilder.WithBackendSetting(configKey, configValue);
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