using Mofichan.Core;

namespace Mofichan.Runner
{
    public interface IConfigurationLoader
    {
        BotConfiguration LoadConfiguration(string configPath);
    }
}
