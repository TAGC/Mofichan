using System.Collections.Generic;

namespace Mofichan.Core.BotState
{
    /// <summary>
    /// Represents objects that manage Mofichan's acquisition and recollection of information.
    /// </summary>
    public interface IMemoryManager
    {
        /// <summary>
        /// Saves a piece of analysis information.
        /// </summary>
        /// <param name="body">The body of the analysis.</param>
        /// <param name="classifications">The classifications associated with the body.</param>
        void SaveAnalysis(string body, IEnumerable<string> classifications);
    }
}
