using Serilog;

namespace Mofichan.Tests.TestUtility
{
    public static class MockLogger
    {
        public static ILogger Instance
        {
            get
            {
                return new LoggerConfiguration().CreateLogger();
            }
        }
    }
}
