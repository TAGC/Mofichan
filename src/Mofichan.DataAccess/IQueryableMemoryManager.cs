using System.Collections.Generic;
using Mofichan.Core.BotState;
using Mofichan.DataAccess.Domain;

namespace Mofichan.DataAccess
{
    internal interface IQueryableMemoryManager : IMemoryManager
    {
        IEnumerable<TaggedMessage> LoadAnalyses();
    }
}
