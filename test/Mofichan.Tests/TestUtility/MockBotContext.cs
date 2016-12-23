using Mofichan.Core.BotState;
using Moq;

namespace Mofichan.Tests.TestUtility
{
    public static class MockBotContext
    {
        public static BotContext Instance
        {
            get
            {
                return new BotContext(Mock.Of<IAttentionManager>(), Mock.Of<IMemoryManager>());
            }
        }
    }
}
