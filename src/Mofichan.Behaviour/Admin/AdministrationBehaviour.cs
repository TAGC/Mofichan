using Mofichan.Behaviour.Base;

namespace Mofichan.Behaviour.Admin
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> augments Mofichan to have administrative capabilities.
    /// <para></para>
    /// Adding this module to the behaviour chain will provide functions that administrators can invoke.
    /// </summary>
    public class AdministrationBehaviour : BaseMultiBehaviour
    {
        internal static string AdministrationBehaviourId = "administration";

        public AdministrationBehaviour() : base(
            new ToggleEnableBehaviour(),
            new DisplayChainBehaviour())
        {
        }

        public override string Id
        {
            get
            {
                return AdministrationBehaviourId;
            }
        }
    }
}
