using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mofichan.Core
{
    public struct BotConfiguration
    {
        public class Builder
        {
            private readonly IDictionary<string, string> backendConfiguration;

            private BotConfiguration config;

            public Builder()
            {
                this.backendConfiguration = new Dictionary<string, string>();
            }

            public Builder SetBotName(string botName)
            {
                this.config.BotName = botName;
                return this;
            }

            public Builder SetDeveloperName(string developerName)
            {
                this.config.DeveloperName = developerName;
                return this;
            }

            public Builder SetSelectedBackend(string selectedBackend)
            {
                this.config.SelectedBackend = selectedBackend;
                return this;
            }

            public Builder WithBackendSetting(string configKey, string configValue)
            {
                this.backendConfiguration[configKey] = configValue;
                return this;
            }
            
            public BotConfiguration Build()
            {
                this.config.BackendConfiguration = new ReadOnlyDictionary<string, string>(
                    this.backendConfiguration);

                return this.config;
            }
        }

        public string BotName { private set; get; }
        public string DeveloperName { private set; get; }
        public string SelectedBackend { private set; get; }
        public IReadOnlyDictionary<string, string> BackendConfiguration { private set; get; }
    }
}
