using System;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanIsGreeted : ScenarioFor<Kernel, Specification>
    {
        private const string AddBehaviourTemplate = "Given Mofichan is configured to use behaviour '{0}'";
        private const string WhenGreetingTemplate = "When I say '{0} Mofichan'";

        private Kernel kernel;
        private bool responded = false;

        public MofichanIsGreeted()
        {
            Examples = new ExampleTable("greeting")
            {
                { "Hello" },
                { "Hey"   },
                { "Hi"    },
                { "Yo"    },
                { "Sup"   },
            };
        }

        [RunStepWithArgs("identity", StepTextTemplate = AddBehaviourTemplate)]
        [RunStepWithArgs("greeting", StepTextTemplate = AddBehaviourTemplate)]
        public void Given_Mofichan_is_configured_with_the_following_behaviours(string behaviour)
        {
            this.Container.Set(behaviour);
        }

        public void Given_Mofichan_is_running()
        {
            throw new NotImplementedException();
        }

        public void When_I_say_greeting(string greeting)
        {
            throw new NotImplementedException();
        }

        public void Then_Mofichan_should_greet_me_back()
        {
            throw new NotImplementedException();
        }
    }
}
