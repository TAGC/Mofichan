using Mofichan.Behaviour.Base;
using Mofichan.Core.BotState;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> augments Mofichan to have administrative capabilities.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will provide functions that administrators can invoke.
    /// </remarks>
    public class AdministrationBehaviour : BaseMultiBehaviour
    {
        internal const string AdministrationBehaviourId = "administration";

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationBehaviour" /> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="chainBuilder">The object to use for composing sub-behaviours into a chain.</param>
        /// <param name="logger">The logger to use.</param>
        public AdministrationBehaviour(
            BotContext botContext,
            IBehaviourChainBuilder chainBuilder,
            ILogger logger)
            : base(chainBuilder,
            new ToggleEnableBehaviour(botContext, logger),
            new DisplayChainBehaviour(botContext, logger))
        {
        }

        /// <summary>
        /// Gets the behaviour module identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public override string Id
        {
            get
            {
                return AdministrationBehaviourId;
            }
        }
    }
}
