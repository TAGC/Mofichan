using Mofichan.Behaviour.Base;

namespace Mofichan.Behaviour.Admin
{
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
