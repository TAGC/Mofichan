using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mofichan.Core
{
    /// <summary>
    /// Represents Mofichan's configuration.
    /// </summary>
    public struct BotConfiguration
    {
        /// <summary>
        /// Builds instances of <see cref="BotConfiguration"/>. 
        /// </summary>
        public class Builder
        {
            private readonly IDictionary<string, string> backendConfiguration;

            private BotConfiguration config;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.backendConfiguration = new Dictionary<string, string>();
            }

            /// <summary>
            /// Sets the name of the bot.
            /// </summary>
            /// <param name="botName">Name of the bot.</param>
            /// <returns>This builder.</returns>
            public Builder SetBotName(string botName)
            {
                this.config.BotName = botName;
                return this;
            }

            /// <summary>
            /// Sets the name of the developer.
            /// </summary>
            /// <param name="developerName">Name of the developer.</param>
            /// <returns>This builder.</returns>
            public Builder SetDeveloperName(string developerName)
            {
                this.config.DeveloperName = developerName;
                return this;
            }

            /// <summary>
            /// Sets the selected backend.
            /// </summary>
            /// <param name="selectedBackend">The selected backend.</param>
            /// <returns>This builder.</returns>
            public Builder SetSelectedBackend(string selectedBackend)
            {
                this.config.SelectedBackend = selectedBackend;
                return this;
            }

            /// <summary>
            /// Sets a configuration key-value pair for the selected backend.
            /// <para></para>
            /// These configuration values will be passed to the selected backend
            /// when it's constructed.
            /// </summary>
            /// <param name="configKey">The configuration key.</param>
            /// <param name="configValue">The configuration value.</param>
            /// <returns>This builder.</returns>
            public Builder WithBackendSetting(string configKey, string configValue)
            {
                this.backendConfiguration[configKey] = configValue;
                return this;
            }

            /// <summary>
            /// Builds the <see cref="BotConfiguration"/> instance.
            /// </summary>
            /// <returns>A <code>BotConfiguration</code>.</returns>
            public BotConfiguration Build()
            {
                this.config.BackendConfiguration = new ReadOnlyDictionary<string, string>(
                    this.backendConfiguration);

                return this.config;
            }
        }

        /// <summary>
        /// Gets the name of the bot.
        /// </summary>
        /// <value>
        /// The name of the bot.
        /// </value>
        public string BotName { private set; get; }

        /// <summary>
        /// Gets the name of the developer.
        /// </summary>
        /// <value>
        /// The name of the developer.
        /// </value>
        public string DeveloperName { private set; get; }

        /// <summary>
        /// Gets the selected backend.
        /// </summary>
        /// <value>
        /// The selected backend.
        /// </value>
        public string SelectedBackend { private set; get; }

        /// <summary>
        /// Gets the backend configuration.
        /// </summary>
        /// <value>
        /// The backend configuration.
        /// </value>
        public IReadOnlyDictionary<string, string> BackendConfiguration { private set; get; }
    }
}
