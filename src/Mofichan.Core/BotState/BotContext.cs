using Mofichan.Core.Interfaces;

namespace Mofichan.Core.BotState
{
    /// <summary>
    /// Represents Mofichan's general state.
    /// </summary>
    public class BotContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotContext"/> class.
        /// </summary>
        /// <param name="attentionManager">Manages Mofichan's attention towards other users.</param>
        public BotContext(IAttentionManager attentionManager)
        {
            this.Attention = attentionManager;
        }

        /// <summary>
        /// Gets Mofichan's attention manager.
        /// </summary>
        /// <value>
        /// Mofichan's attention manager.
        /// </value>
        public IAttentionManager Attention { get; }
    }
}
