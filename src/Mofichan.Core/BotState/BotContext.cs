namespace Mofichan.Core.BotState
{
    /// <summary>
    /// Represents Mofichan's general state.
    /// </summary>
    public class BotContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotContext" /> class.
        /// </summary>
        /// <param name="attentionManager">Manages Mofichan's attention towards other users.</param>
        /// <param name="memoryManager">Manages Mofichan's ability to store and recall information.</param>
        public BotContext(IAttentionManager attentionManager, IMemoryManager memoryManager)
        {
            this.Attention = attentionManager;
            this.Memory = memoryManager;
        }

        /// <summary>
        /// Gets Mofichan's attention manager.
        /// </summary>
        /// <value>
        /// Mofichan's attention manager.
        /// </value>
        public IAttentionManager Attention { get; }

        /// <summary>
        /// Gets Mofichan's memory manager.
        /// </summary>
        /// <value>
        /// Mofichan's memory manager.
        /// </value>
        public IMemoryManager Memory { get; }
    }
}
