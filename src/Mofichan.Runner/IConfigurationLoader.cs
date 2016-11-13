using Mofichan.Core;

namespace Mofichan.Runner
{
    /// <summary>
    /// Represents an object used to load Mofichan's configuration from
    /// a configuration file.
    /// </summary>
    public interface IConfigurationLoader
    {
        /// <summary>
        /// Loads Mofichan's configuration from a configuration file.
        /// </summary>
        /// <param name="configPath">The path of the configuration file.</param>
        /// <returns>The configuration specified in the file.</returns>
        BotConfiguration LoadConfiguration(string configPath);
    }
}
