using System.Collections.Generic;

namespace Mofichan.Core.BotState
{
    public interface IMemoryManager
    {
        void SaveAnalysis(string message, IEnumerable<string> classifications);
    }
}
