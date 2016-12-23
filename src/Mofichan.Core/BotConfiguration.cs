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
        /// Gets the name of the bot.
        /// </summary>
        /// <value>
        /// The name of the bot.
        /// </value>
        public string BotName { get; private set; }

        /// <summary>
        /// Gets the name of the developer.
        /// </summary>
        /// <value>
        /// The name of the developer.
        /// </value>
        public string DeveloperName { get; private set; }

        /// <summary>
        /// Gets the selected backend.
        /// </summary>
        /// <value>
        /// The selected backend.
        /// </value>
        public string SelectedBackend { get; private set; }

        /// <summary>
        /// Gets the selected database adapter.
        /// </summary>
        /// <value>
        /// The selected database adapter.
        /// </value>
        public string SelectedDatabaseAdapter { get; private set; }

        /// <summary>
        /// Gets the backend configuration.
        /// </summary>
        /// <value>
        /// The backend configuration.
        /// </value>
        public IReadOnlyDictionary<string, string> BackendConfiguration { get; private set; }

        /// <summary>
        /// Gets the database adapter configuration.
        /// </summary>
        /// <value>
        /// The database adapter configuration.
        /// </value>
        public IReadOnlyDictionary<string, string> DatabaseAdapterConfiguration { get; private set; }

        /// <summary>
        /// Builds instances of <see cref="BotConfiguration"/>. 
        /// </summary>
        public class Builder
        {
            private readonly IDictionary<string, string> backendConfiguration;
            private readonly IDictionary<string, string> databaseAdapterConfiguration;

            private BotConfiguration config;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.backendConfiguration = new Dictionary<string, string>();
                this.databaseAdapterConfiguration = new Dictionary<string, string>();
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
            /// Sets the selected database adapter.
            /// </summary>
            /// <param name="selectedBackend">The selected database adapter.</param>
            /// <returns>This builder.</returns>
            public Builder SetSelectedDatabaseAdapter(string selectedBackend)
            {
                this.config.SelectedDatabaseAdapter = selectedBackend;
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
            /// Sets a configuration key-value pair for the selected database adapter.
            /// <para></para>
            /// These configuration values will be passed to the selected database adapter
            /// when it's constructed.
            /// </summary>
            /// <param name="configKey">The configuration key.</param>
            /// <param name="configValue">The configuration value.</param>
            /// <returns>This builder.</returns>
            public Builder WithDatabaseAdapterSetting(string configKey, string configValue)
            {
                this.databaseAdapterConfiguration[configKey] = configValue;
                return this;
            }

            /// <summary>
            /// Builds the <see cref="BotConfiguration"/> instance.
            /// </summary>
            /// <returns>A <c>BotConfiguration</c>.</returns>
            public BotConfiguration Build()
            {
                this.config.BackendConfiguration = new ReadOnlyDictionary<string, string>(
                    this.backendConfiguration);

                this.config.DatabaseAdapterConfiguration = new ReadOnlyDictionary<string, string>(
                    this.databaseAdapterConfiguration);

                return this.config;
            }
        }
    }
}
