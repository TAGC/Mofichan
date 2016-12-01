using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Core.Interfaces;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public abstract class MofichanIsGreeted : BaseScenario
    {
        protected const string WhenGreetingTemplate = "When I say '<greeting>'";

        public MofichanIsGreeted(string scenarioTitle) : base(scenarioTitle)
        {
        }

        protected ExampleTable EmptyExampleTable
        {
            get
            {
                return new ExampleTable("greeting");
            }
        }

        protected void When_I_say_greeting(string greeting)
        {
            this.When_Mofichan_receives_a_message(sender: this.DeveloperUser, message: greeting);
        }
    }

    public class MofichanIsGreetedWithValidGreeting : MofichanIsGreeted
    {
        private readonly string greetingPattern;

        public MofichanIsGreetedWithValidGreeting() : base(scenarioTitle: "Mofichan is greeted with valid greeting")
        {
            this.greetingPattern = string.Format(@"(?i)(hey|hi|hello),? {0}", this.DeveloperUser.Name);

            var greeting = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_say_greeting(greeting), WhenGreetingTemplate)
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(2))
                .Then(s => s.Then_Mofichan_should_greet_me_back())
                .WithExamples(this.Examples)
                .TearDownWith(s => s.TearDown());
        }

        private ExampleTable Examples
        {
            get
            {
                var greetings = from start in new[] { "Hello", "Hey", "Hi", "Sup", "Yo" }
                                from middle in new[] { ",", " " }
                                from name in new[] { "Mofi", "Mofichan" }
                                from end in new[] { string.Empty, " ", "!", "." }
                                let greeting = string.Format("{0}{1}{2}{3}", start, middle, name, end)
                                select greeting;

                var table = this.EmptyExampleTable;
                foreach (var greeting in greetings)
                {
                    table.Add(greeting);
                }

                return table;
            }
        }

        private void Then_Mofichan_should_greet_me_back()
        {
            var message = this.SentMessages.ShouldHaveSingleItem();
            message.Context.To.ShouldBe(this.DeveloperUser);
            message.Context.Body.ShouldMatch(this.greetingPattern);
        }
    }

    public class MofichanIsGreetedWithInvalidGreeting : MofichanIsGreeted
    {
        public MofichanIsGreetedWithInvalidGreeting() : base(scenarioTitle: "Mofichan is greeted with invalid greeting")
        {
            var greeting = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_I_say_greeting(greeting), WhenGreetingTemplate)
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(1))
                .Then(s => s.Then_Mofichan_should_not_have_said_anything())
                .WithExamples(this.Examples)
                .TearDownWith(s => s.TearDown());
        }

        private ExampleTable Examples
        {
            get
            {
                var invalidGreetings = new[]
                {
                    string.Empty,
                    "Foo",
                    "Hello",
                    "Hello Mof",
                    "Goodbye Mofichan",
                    "Mofichan",
                };

                var table = this.EmptyExampleTable;
                foreach (var greeting in invalidGreetings)
                {
                    table.Add(greeting);
                }

                return table;
            }
        }

        private void Then_Mofichan_should_not_have_said_anything()
        {
            this.SentMessages.ShouldBeEmpty();
        }
    }

    public class MofichanGreetingHerself : MofichanIsGreeted
    {
        public MofichanGreetingHerself() : base("Mofichan sees a greeting from herself")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("selfignore"), AddBehaviourTemplate)
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
            .When(s => s.When_Mofichan_receives_a_message(this.MofichanUser, "Hello Mofichan"),
                "When Mofichan receives message: '{1}'")
                .And(s => s.When_flows_are_driven_by__stepCount__steps(1))
            .Then(s => s.Then_Mofichan_should_not_have_said_anything());
        }

        private void Then_Mofichan_should_not_have_said_anything()
        {
            this.SentMessages.ShouldBeEmpty();
        }
    }

    public class MofiRespondsToWellbeingRequest : MofichanIsGreeted
    {
        public MofiRespondsToWellbeingRequest() : base("Mofichan responds to people asking how she is")
        {
            var wellbeingRequest = default(string);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, wellbeingRequest))
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(2))
                .Then(s => s.Then_Mofichan_should_have_responded())
                .WithExamples(this.Examples)
                .TearDownWith(s => s.TearDown());
        }

        private static IEnumerable<string> ExampleMessages
        {
            get
            {
                yield return "How are you doing Mofi?";
                yield return "How r u doing Mofi?";
                yield return "you alright Mofi?";
                yield return "Mofichan, how are you?";
                yield return "Mofi you alright?";
            }
        }

        private ExampleTable Examples
        {
            get
            {
                var table = new ExampleTable("wellbeingRequest");
                foreach (var wellbeingRequest in ExampleMessages)
                {
                    table.Add(wellbeingRequest);
                }

                return table;
            }
        }
    }

    public class MofiRespondsToWellbeingRequestAfterGettingHerAttention : MofichanIsGreeted
    {
        public MofiRespondsToWellbeingRequestAfterGettingHerAttention()
            : base("Mofichan responds to a wellbeing response after her attention is caught")
        {
            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("greeting"), AddBehaviourTemplate)
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "Hey Mofi"))
                    .And(s => s.When_Mofichan_receives_a_message(this.JohnSmithUser, "How are you doing?"))
                    .And(s => s.When_flows_are_driven_by__stepCount__steps(2))
                .Then(s => s.Then_Mofichan_Should_Have_Sent_A_Wellbeing_Response_To_User(this.JohnSmithUser))
                .TearDownWith(s => s.TearDown());
        }

        private void Then_Mofichan_Should_Have_Sent_A_Wellbeing_Response_To_User(IUser recipient)
        {
            var pattern = new Regex(@"I'm.*", RegexOptions.IgnoreCase);

            this.SentMessages.ShouldContain(it =>
                it.Context.To == recipient && pattern.IsMatch(it.Context.Body));
        }
    }
}
