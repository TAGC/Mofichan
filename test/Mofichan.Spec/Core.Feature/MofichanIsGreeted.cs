using System;
using TestStack.BDDfy;
using Xunit;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanIsGreeted : BaseScenario
    {
        private const string WhenGreetingTemplate = "When I say '<greeting> Mofichan'";
        
        public MofichanIsGreeted() : base(scenarioTitle: "Mofichan is greeted")
        {
            var greeting = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("identity"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_say_greeting(greeting), WhenGreetingTemplate)
                .Then(s => s.Then_Mofichan_should_greet_me_back())
                .WithExamples(new ExampleTable("greeting")
                {
                    { "Hello" },
                    { "Hey"   },
                    { "Hi"    },
                    { "Yo"    },
                    { "Sup"   },
                });
        }

        private void When_I_say_greeting(string greeting)
        {
            throw new NotImplementedException();
        }

        private void Then_Mofichan_should_greet_me_back()
        {
            throw new NotImplementedException();
        }
    }
}
